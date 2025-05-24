using BattleTech;
using BEXTimeline;
using System;

namespace BTX_PeripheryUnbound
{
    internal class Factions
    {
        private const string AuriganRestorationId = "faction_AuriganRestoration";
        private const string AuriganDirectorateId = "faction_AuriganDirectorate";
        private static readonly DateTime PostRestorationDate = new(3025, 12, 30);

        [HarmonyPatch(typeof(UpdateOwnership), "UpdateFactions")]
        public static class UpdateAuriga
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState simGame, DateTime currentDate)
            {
                if (simGame.SimGameMode == SimGameState.SimGameType.CAREER || simGame.CompanyTags.Contains("story_complete"))
                {
                    UpdateFaction(
                        simGame.DataManager.Factions.Get(AuriganRestorationId),
                        currentDate >= PostRestorationDate
                            ? ("the Aurigan Coalition", "the Coalition", "The Aurigan Coalition, re-established after the hard-won victory of the Arano Restoration, struggles to find its footing in the aftermath of a brutal civil war. High Lady Kamea Arano now leads a nation grappling with devastated worlds, unresolved border disputes with the Taurian Concordat, and the lingering threat of Directorate loyalists. While the Espinosa and Karosas houses are gone, the allegiance of other noble families remains uncertain as the Coalition attempts to consolidate its power and secure its future in the Aurigan Reach.", "Aurigan")
                            : ("the Arano Restoration", "the Restoration", "The Arano Restoration is the remnant of the former Aurigan Coalition, driven from power by the treachery of Santiago Espinosa. Those still loyal to the Arano family fled into the Rimward Frontier, the expanse of unsettled systems adjoining the Magistracy of Canopus. Many noble houses of the Aurigan Directorate harbor sympathies for the Restoration and its cause: returning the Arano monarchy to power.", "Restoration")
                    );

                    UpdateFaction(
                        simGame.DataManager.Factions.Get(AuriganDirectorateId),
                        currentDate >= PostRestorationDate
                            ? ("the Directorate Loyalists", "the Directorate", "Following the decisive victory of the Arano Restoration, the self-proclaimed Aurigan Directorate quickly collapsed. While its core leadership, including Director Espinosa, was eliminated, remnants of its authoritarian ideology and loyalist cells persisted, clinging to the dream of reinstating their oppressive rule. However, lacking centralized command or widespread support, the Directorate faded from any true power.", "Directorate")
                            : ("the Aurigan Directorate", "the Directorate", "The Aurigan Directorate replaced the Aurigan Coalition when Santiago Espinosa, adviser to the High Lady Kamea Arano, betrayed her and led a coup against her government. In the years since that coup, Director Espinosa has solidified his hold on the systems of the Directorate, binding the Founding Families to his cause with a mixture of promises and threats. His rule is oppressive and militaristic, with individual liberties sacrificed to the cause of security and stability.", "Directorate")
                    );
                }
            }

            private static void UpdateFaction(FactionDef faction, (string Name, string ShortName, string Description, string Demonym) values)
            {
                if (faction != null)
                {
                    faction.Name = values.Name;
                    faction.ShortName = values.ShortName;
                    faction.Description = values.Description;
                    faction.Demonym = values.Demonym;
                }
            }
        }
    }
}
