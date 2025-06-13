using BattleTech;
using BEXTimeline;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTX_PeripheryUnbound.Timeline
{
    internal class AbandonedSystems
    {
        [HarmonyPatch(typeof(UpdateOwnership), "UpdateTheMap")]
        public static class AbandonedSystemsSetup
        {
            private static bool HasProcessedAbandonedSystems = false;

            [HarmonyPostfix]
            [HarmonyWrapSafe]
            public static void Postfix(SimGameState simGame)
            {
                if (HasProcessedAbandonedSystems) return;

                Main.Log.LogDebug("--- Abandoned Systems Setup Summary ---");
                List<(StarSystem System, FactionValue NewOwner)> systemsToClaim = [];
                int abandonedCount = 0;

                foreach (StarSystem starSystem in simGame.StarSystems)
                {
                    if (starSystem.Def?.ownerID == "NoFaction")
                    {
                        abandonedCount++;
                        List<StarSystem> neighboringSystems = simGame.Starmap.GetAvailableNeighborSystem(starSystem);

                        if (neighboringSystems == null || !neighboringSystems.Any())
                        {
                            Main.Log.LogDebug($"{starSystem.Name} system has no jump neighbors. Skipping.");
                            continue;
                        }

                        List<FactionValue> influentialNeighbors = [.. neighboringSystems
                            .Where(neighbor => neighbor.Def.ownerID != "NoFaction" &&
                                               neighbor.Def.ownerID != "Locals" &&
                                              !neighbor.Def.Tags.Contains("planet_other_empty"))
                           .Select(neighbor => neighbor.OwnerValue)];

                        if (influentialNeighbors.Count == 0)
                        {
                            Main.Log.LogDebug($"{starSystem.Name} system has no influential neighbors. Skipping.");
                            continue;
                        }

                        double distanceToTerra = GetDistanceToTerra(starSystem.Position);
                        if (influentialNeighbors.Count == 1 && distanceToTerra < 450)
                        {
                            systemsToClaim.Add((starSystem, influentialNeighbors[0]));
                        }
                        else if (influentialNeighbors.Count > 1 && distanceToTerra < 500)
                        {
                            FactionValue mostInfluentialFaction = influentialNeighbors
                                .GroupBy(f => f.FactionDef.ID)
                                .OrderByDescending(g => g.Count())
                                .FirstOrDefault()?.FirstOrDefault();

                            if (mostInfluentialFaction != null)
                            {
                                FactionValue newOwnerValue = mostInfluentialFaction;
                                systemsToClaim.Add((starSystem, newOwnerValue));
                            }
                        }

                        else if (influentialNeighbors.Count == 1 && distanceToTerra >= 450)
                        {
                            Main.Log.LogDebug($"{starSystem.Name} system is outside the Inner Sphere and has only one influential neighbor. Skipping.");
                            continue;
                        }
                        else if (influentialNeighbors.Count > 1 && distanceToTerra >= 450)
                        {
                            {
                                var factionCounts = influentialNeighbors
                                    .GroupBy(f => f.FactionDef.ID)
                                    .Select(g => new { FactionId = g.Key, Count = g.Count(), FactionValue = g.First() })
                                    .OrderByDescending(x => x.Count)
                                    .ToList();

                                var mostInfluential = factionCounts.ElementAtOrDefault(0);
                                var secondMostInfluential = factionCounts.ElementAtOrDefault(1);

                                bool hasDominantInfluence = false;
                                if (mostInfluential != null && mostInfluential.Count >= 1)
                                {
                                    if (secondMostInfluential == null || mostInfluential.Count > secondMostInfluential.Count)
                                    {
                                        hasDominantInfluence = true;
                                    }
                                }

                                if (hasDominantInfluence)
                                {
                                    FactionValue newOwnerValue = mostInfluential.FactionValue;
                                    systemsToClaim.Add((starSystem, newOwnerValue));
                                }
                                else // If no clear dominant faction, calculate nearest faction to break the tie
                                {
                                    double minOverallDistance = double.MaxValue;
                                    FactionValue winningFaction = null;

                                    foreach (var faction in factionCounts)
                                    {
                                        double totalDistance = 0.0;
                                        foreach (StarSystem neighbor in neighboringSystems)
                                        {
                                            if (neighbor.OwnerValue.FactionDef.ID == faction.FactionId)
                                            {
                                                totalDistance += GetDistance(neighbor.Position, starSystem.Position);
                                            }
                                        }
                                        if (totalDistance < minOverallDistance)
                                        {
                                            minOverallDistance = totalDistance;
                                            winningFaction = faction.FactionValue;
                                        }
                                    }

                                    systemsToClaim.Add((starSystem, winningFaction));
                                }
                            }
                        }
                    }
                }

                foreach (var system in systemsToClaim)
                {
                    StarSystem starSystem = system.System;
                    FactionValue newOwner = system.NewOwner;

                    starSystem.Def.ownerID = newOwner.FactionDef.ID;
                    starSystem.Def.OwnerValue = newOwner;
                    //Main.Log.LogDebug($"{starSystem.Name} system has being claimed by {newOwner.FactionDef.Name}.");

                    List<string> tagsToRemove = [.. starSystem.Def.Tags
                        .Where(tag => tag.StartsWith("planet_faction_") || tag.Equals("planet_other_empty"))];

                    foreach (string tag in tagsToRemove)
                    {
                        starSystem.Def.Tags.Remove(tag);
                    }

                    string newFactionTag = $"planet_faction_{newOwner.FactionDef.ID.ToLowerInvariant()}";
                    if (!starSystem.Def.Tags.Contains(newFactionTag))
                    {
                        starSystem.Def.Tags.Add(newFactionTag);
                        starSystem.Def.Tags.Add("planet_other_empty");
                    }
                }

                if (abandonedCount > 0)
                {
                    Main.Log.LogDebug($"Found {abandonedCount} abandoned systems, claimed {systemsToClaim.Count}.");
                    HasProcessedAbandonedSystems = true;
                }

                Main.Log.LogDebug("--- End of Abandoned Systems Setup Summary ---");
            }
        }

        private static double GetDistanceToTerra(FakeVector3 systemPosition)
        {
            return Math.Sqrt(Math.Pow(systemPosition.x, 2.0) + Math.Pow(systemPosition.y, 2.0));
        }

        private static double GetDistance(FakeVector3 systemPosition1, FakeVector3 systemPosition2)
        {
            return Math.Sqrt(Math.Pow(systemPosition2.x - systemPosition1.x, 2.0) + Math.Pow(systemPosition2.y - systemPosition1.y, 2.0));
        }
    }
}