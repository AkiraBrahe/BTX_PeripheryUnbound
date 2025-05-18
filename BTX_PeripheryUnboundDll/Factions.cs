using BattleTech;
using BEXTimeline;
using HarmonyLib;
using System;

namespace BTX_PeripheryUnbound
{
    internal class Factions
    {
        [HarmonyPatch(typeof(UpdateOwnership), "UpdateFactions")]
        public static class UpdateAuriga
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState simGame, DateTime currentDate)
            {
                if (simGame.SimGameMode == SimGameState.SimGameType.CAREER || simGame.CompanyTags.Contains("story_complete"))
                {
                    if (currentDate >= new DateTime(3025, 12, 30))
                    {
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Name", null).SetValue("the Aurigan Coalition");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("ShortName", null).SetValue("the Coalition");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Description", null).SetValue("The Aurigan Coalition, re-established after the hard-won victory of the Arano Restoration, struggles to find its footing in the aftermath of a brutal civil war. High Lady Kamea Arano now leads a nation grappling with devastated worlds, unresolved border disputes with the Taurian Concordat, and the lingering threat of Directorate loyalists. While the Espinosa and Karosas houses are gone, the allegiance of other noble families remains uncertain as the Coalition attempts to consolidate its power and secure its future in the Aurigan Reach.");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Demonym", null).SetValue("Aurigan");
                    }
                    else
                    {
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Name", null).SetValue("the Arano Restoration");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("ShortName", null).SetValue("the Restoration");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Description", null).SetValue("The Arano Restoration is the remnant of the former Aurigan Coalition, driven from power by the treachery of Santiago Espinosa. Those still loyal to the Arano family fled into the Rimward Frontier, the expanse of unsettled systems adjoining the Magistracy of Canopus. Many noble houses of the Aurigan Directorate harbor sympathies for the Restoration and its cause: returning the Arano monarchy to power.");
                        Traverse.Create(simGame.DataManager.Factions.Get("faction_AuriganRestoration")).Property("Demonym", null).SetValue("Restoration");
                    }
                }
            }
        }
    }
}
