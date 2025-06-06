using BattleTech;
using ISM3025.Features;
using System.Collections.Generic;

namespace BTX_PeripheryUnbound.Timeline
{
    internal class Contracts
    {
        [HarmonyPatch(typeof(ParticpantGeneration), "TryAddParticipants")]
        public static class AddMoreParticipants
        {
            [HarmonyFinalizer]
            [HarmonyWrapSafe]
            public static void Finalizer(StarSystemDef def)
            {
                SortFactionList(def.ContractEmployerIDList, def.OwnerValue.Name);
                SortFactionList(def.ContractTargetIDList, def.OwnerValue.Name);

                UpdateFactionList(def.ContractEmployerIDList, def.OwnerValue.Name, true);
                UpdateFactionList(def.ContractTargetIDList, def.OwnerValue.Name, false);
            }
        }
        private static void SortFactionList(List<string> factionList, string ownerFactionID)
        {
            HashSet<string> uniqueFactions = [.. factionList];
            List<string> sortedResult = [];
            string pirateFactionID = "AuriganPirates";

            // 1. Add owner first if present
            if (uniqueFactions.Contains(ownerFactionID))
            {
                sortedResult.Add(ownerFactionID);
                uniqueFactions.Remove(ownerFactionID);
            }
            else if (ownerFactionID == "NoFaction" && uniqueFactions.Contains(pirateFactionID))
            {
                // If no owner, but pirates are present, add pirates first
                sortedResult.Add(pirateFactionID);
                uniqueFactions.Remove(pirateFactionID);
            }

            // 2. Add other factions (excluding owner and pirates)
            foreach (string factionID in uniqueFactions)
            {
                if (factionID != pirateFactionID)
                {
                    sortedResult.Add(factionID);
                }
            }

            // 3. Add pirates last if present
            if (uniqueFactions.Contains(pirateFactionID))
            {
                sortedResult.Add(pirateFactionID);
            }

            // Clear the original list and add the sorted results
            factionList.Clear();
            factionList.AddRange(sortedResult);
        }

        private static void UpdateFactionList(List<string> factionList, string ownerFactionID, bool isEmployerIDList)
        {
            // Handle Aurigan Restoration and Directorate
            int OwnerIndex = 0; // Assuming the Owner is always at index 0 in the list
            if (ownerFactionID == "AuriganRestoration" && !factionList.Contains("AuriganDirectorate") && !isEmployerIDList)
            {
                factionList.Insert(OwnerIndex + 1, "AuriganDirectorate");
            }
            else if (ownerFactionID == "AuriganRestoration" && !factionList.Contains("AuriganRestoration"))
            {
                factionList.Insert(OwnerIndex, "AuriganRestoration");
            }
            else if (ownerFactionID == "AuriganDirectorate" && !factionList.Contains("AuriganDirectorate") && !isEmployerIDList)
            {
                factionList.Insert(OwnerIndex, "AuriganDirectorate");
            }

            // Handle Elysian Fields (vassal of Oberon Confederation)
            if (ownerFactionID == "Elysia")
            {
                factionList.Remove("Oberon");
                factionList.Insert(OwnerIndex + 1, "Oberon");
            }
        }
    }
}
