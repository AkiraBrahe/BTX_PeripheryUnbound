using BattleTech;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace BTX_PeripheryUnbound.Tools
{
    internal class StarSystemDefUpdater
    {
        [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        public static class UpdateStarSystemDefsOnLoad
        {
            public static void Postfix(SimGameState __instance)
            {
                if (Main.Settings.Debug.UpdateStarSystemDefsOnLoad)
                {
                    UpdateStarSystemDefFiles(__instance, "Rehydrate");
                }
            }
        }

        public static void UpdateStarSystemDefFiles(SimGameState simGame, string triggerSource)
        {
            Main.Log.LogDebug($"[StarSystemDefUpdater] Updating StarSystemDef files triggered by: {triggerSource}");
            string folderPath = Path.Combine(Main.modDir, "starSystemUpdated");
            int updatedCount = 0;

            JsonSerializerSettings listSerializationSettings = new()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            foreach (StarSystem system in simGame.StarSystems)
            {
                StarSystemDef def = system.Def;
                if (def == null)
                {
                    Main.Log.LogWarning($"[StarSystemDefUpdater] System '{system.Name}' has a null StarSystemDef. Skipping update.");
                    continue;
                }

                string filename = $"{def.CoreSystemID}.json";
                string filePath = Path.Combine(folderPath, filename);

                if (!File.Exists(filePath))
                {
                    Main.Log.LogWarning($"[StarSystemDefUpdater] File not found for '{def.CoreSystemID}' at '{filePath}'. Skipping update for this system.");
                    continue;
                }

                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    JObject jObject = JObject.Parse(fileContent);

                    // --- Update Shop Owner/Items ---
                    jObject["factionShopOwnerID"] = def.factionShopOwnerID != null ? JToken.FromObject(def.factionShopOwnerID) : null;
                    jObject["SystemShopItems"] = def.SystemShopItems != null ? JToken.Parse(JsonConvert.SerializeObject(def.SystemShopItems, listSerializationSettings)) : null;
                    jObject["FactionShopItems"] = def.FactionShopItems != null ? JToken.Parse(JsonConvert.SerializeObject(def.FactionShopItems, listSerializationSettings)) : null;
                    jObject["BlackMarketShopItems"] = def.BlackMarketShopItems != null ? JToken.Parse(JsonConvert.SerializeObject(def.BlackMarketShopItems, listSerializationSettings)) : null;

                    // --- Update Contract Employer/Target IDs ---
                    jObject["contractEmployerIDs"] = def.contractEmployerIDs != null ? JToken.FromObject(def.contractEmployerIDs) : null;
                    jObject["contractTargetIDs"] = def.contractTargetIDs != null ? JToken.FromObject(def.contractTargetIDs) : null;

                    // Write the modified JObject back to the file
                    File.WriteAllText(filePath, jObject.ToString(Formatting.Indented));
                    updatedCount++;
                }
                catch (JsonReaderException jre)
                {
                    Main.Log.LogError($"[StarSystemDefUpdater] JSON parsing error for '{def.CoreSystemID}' in '{filePath}': {jre.Message}");
                }
                catch (Exception ex)
                {
                    Main.Log.LogError($"[StarSystemDefUpdater] Error updating system '{def.CoreSystemID}' in '{filePath}': {ex.Message}");
                }
            }
            Main.Log.LogDebug($"[StarSystemDefUpdater] Updated {updatedCount} StarSystemDef files in {folderPath}");
        }
    }
}