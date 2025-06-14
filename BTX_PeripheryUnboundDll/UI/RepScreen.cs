using BattleTech;
using BattleTech.UI;
using BEXTimeline;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BTX_PeripheryUnbound.UI
{
    internal class RepScreen
    {
        private static readonly List<string> FixedFactions =
        [
            // --- Successor States ---
            "Kurita", "Davion", "Liao", "Marik", "Steiner",
            // --- Minor States ---
            "Rasalhague", "Tikonov", "Ives", "Andurien", "Arc-RoyalDC",
            // --- Periphery States ---
            "Outworld", "TaurianConcordat", "AuriganDirectorate", "MagistracyOfCanopus", "Rim",
            "Elysia", "Calderon", "NewColonyRegion", "Illyrian", "Lothian",
            // --- Bandit Kingdoms ---
            "Tortuga", "Marian", "Circinus", "Valkyrate", "Oberon",
            // --- Criminal Organizations / Pirate Affiliates / Mercenaries ---
            "CriminalYakuza", "CriminalCloak", "CriminalBeroskiFamily", "CriminalYizhiTong", "CriminalRostakovTong",
            "CriminalManTLE", "CriminalRedCobraTriad", "CriminalMalthus", "PiratesSantander", "PiratesDamned",
            "PiratesTortuga", "PiratesMarch", "PiratesAurigan", "PiratesMarian", "PiratesCircinus",
            "PiratesExtractor", "PiratesValkyrate", "PiratesOberon", "PiratesBelt", "WolfsDragoons"
        ];

        [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        public static class ReorderFactions
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState __instance)
            {
                if (__instance?.displayedFactions != null)
                {
                    __instance.displayedFactions.Clear();
                    __instance.displayedFactions.AddRange(FixedFactions);
                }
            }
        }

        [HarmonyPatch(typeof(SGNavigationActiveFactionWidget), "ActivateFactions")]
        public static class EnableAuriganWidget
        {
            [HarmonyPostfix]
            public static void Postfix(SGNavigationActiveFactionWidget __instance)
            {
                foreach (HBSButton button in __instance.FactionButtons)
                {
                    int num = __instance.FactionButtons.IndexOf(button);
                    string factionId = __instance.FactionIDList[num];

                    if (factionId == "AuriganRestoration")
                    {
                        // Force the button state to Enabled, regardless of other conditions.
                        button.SetState(ButtonState.Enabled, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class MoveAuriganWidget
        {
            private const int AuriganDirectorateIndex = 12;

            [HarmonyPostfix]
            [HarmonyWrapSafe]
            [HarmonyAfter("BattleTech.Haree.BEXTimeline")]
            public static void Postfix(SGCaptainsQuartersReputationScreen __instance, List<SGFactionReputationWidget> ___FactionPanelWidgets, SimGameState ___simState)
            {
                if (___FactionPanelWidgets == null || ___FactionPanelWidgets.Count <= AuriganDirectorateIndex) return;

                //DateTime currentDate = ___simState.CurrentDate;
                //if (currentDate > Factions.PostRestorationDate)

                SGFactionReputationWidget auriganWidget = ___FactionPanelWidgets[AuriganDirectorateIndex];
                auriganWidget.Init(___simState, FactionEnumeration.GetAuriganRestorationFactionValue(), new UnityAction(__instance.RefreshWidgets), false);
                auriganWidget.gameObject.SetActive(true);
                ___FactionPanelWidgets[___FactionPanelWidgets.Count - 1].gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class AddFactionHeaders
        {
            private readonly struct FactionHeader
            {
                public string Type { get; } public int StartIndex { get; } 
                public FactionHeader(string type, int startIndex) => (Type, StartIndex) = (type, startIndex);
            }
            private static readonly FactionHeader[] factionTypeHeaders =
            [
                new FactionHeader("Successor States", 0),
                new FactionHeader("Minor States", 5),
                new FactionHeader("Periphery States", 10),
                new FactionHeader("Bandit Kingdoms", 20),
                new FactionHeader("Criminals / Pirates / Mercs", 25),
            ];

            [HarmonyPostfix]
            [HarmonyWrapSafe]
            public static void Postfix(List<SGFactionReputationWidget> ___FactionPanelWidgets)
            {
                if (___FactionPanelWidgets == null || ___FactionPanelWidgets.Count == 0) return;

                int headerIndex = 0;
                foreach (var headerInfo in factionTypeHeaders)
                {
                    if (headerInfo.StartIndex < ___FactionPanelWidgets.Count && ___FactionPanelWidgets[headerInfo.StartIndex] != null)
                    {
                        AddHeaderToWidget(___FactionPanelWidgets[headerInfo.StartIndex], headerInfo.Type, headerIndex);
                        headerIndex++;
                    }
                }
            }

            private static void AddHeaderToWidget(SGFactionReputationWidget widget, string headerType, int headerIndex)
            {
                string headerName = $"FactionHeader{headerIndex}";
                if (widget.transform.Find(headerName) != null) return;

                GameObject headerWidget = new(headerName);
                headerWidget.transform.SetParent(widget.transform, false);

                TextMeshProUGUI headerText = headerWidget.AddComponent<TextMeshProUGUI>();
                headerText.font = Resources.Load<TMP_FontAsset>("UnitedSansReg-Medium SDF");
                headerText.text = headerType;
                headerText.alignment = TextAlignmentOptions.Left;
                headerText.fontSize = 18;
                headerText.overflowMode = TextOverflowModes.Overflow;

                RectTransform headerRectTransform = headerText.GetComponent<RectTransform>();
                headerRectTransform.sizeDelta = new Vector2(275f, 20f);
                headerRectTransform.anchoredPosition = new Vector2(-55f, 55f);
                headerRectTransform.localScale = Vector3.one;
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class CheckFactionActivity
        {

            [HarmonyPostfix]
            [HarmonyWrapSafe]
            public static void Postfix(List<SGFactionReputationWidget> ___FactionPanelWidgets, ref SimGameState ___simState)
            {
                if (___FactionPanelWidgets == null || ___FactionPanelWidgets.Count == 0) return;

                DateTime currentDate = ___simState.CurrentDate;
                if (currentDate > UpdateOwnership.LastDayUpdated)
                {
                    for (int i = 0; i < FixedFactions.Count && i < ___FactionPanelWidgets.Count; i++)
                    {
                        SGFactionReputationWidget factionWidget = ___FactionPanelWidgets[i];
                        string factionName = FixedFactions[i];

                        bool isActive = IsFactionActive(factionName, currentDate);

                        SetWidgetActivity(factionWidget, isActive);
                    }
                }
            }

            private static bool IsFactionActive(string factionID, DateTime currentDate)
            {
                return factionID switch
                {
                    "Rasalhague" => currentDate >= new DateTime(3034, 03, 14) && currentDate < new DateTime(3152, 12, 31),
                    "Tikonov" => currentDate >= new DateTime(3029, 03, 03) && currentDate < new DateTime(3031, 09, 01),
                    "Ives" => currentDate >= new DateTime(3029, 01, 01) && currentDate < new DateTime(3063, 06, 10),
                    "Andurien" => (currentDate >= new DateTime(3030, 09, 11) && currentDate < new DateTime(3040, 01, 31)) ||
                                  (currentDate >= new DateTime(3079, 01, 25) && currentDate < new DateTime(3152, 12, 31)),
                    "Arc-RoyalDC" => currentDate >= new DateTime(3057, 12, 01) && currentDate < new DateTime(3067, 04, 24),
                    "Rim" => currentDate >= new DateTime(3048, 01, 24) && currentDate < new DateTime(3148, 06, 24),
                    "Elysia" => currentDate >= new DateTime(2900, 09, 28) && currentDate < new DateTime(3049, 09, 18),
                    "Calderon" => currentDate >= new DateTime(3066, 04, 01) && currentDate < new DateTime(3152, 12, 31),
                    "NewColonyRegion" => currentDate >= new DateTime(3057, 01, 09) && currentDate < new DateTime(3152, 12, 31),
                    "Illyrian" => currentDate >= new DateTime(2444, 02, 22) && currentDate < new DateTime(3063, 06, 30),
                    "Lothian" => currentDate >= new DateTime(2821, 12, 27) && currentDate < new DateTime(3054, 12, 31),
                    "Circinus" => currentDate >= new DateTime(2785, 03, 14) && currentDate < new DateTime(3081, 05, 01),
                    "Valkyrate" => currentDate >= new DateTime(3021, 04, 14) && currentDate < new DateTime(3049, 09, 27),
                    "Oberon" => currentDate >= new DateTime(2783, 01, 07) && currentDate < new DateTime(3049, 09, 02),
                    _ => true,
                };
            }

            private static void SetWidgetActivity(SGFactionReputationWidget widget, bool isActive)
            {
                Transform overlay = widget.transform.Find("noRep-overlay");
                overlay?.gameObject.SetActive(!isActive);

                Transform logo = widget.transform.Find("LOGO/factionLogo");
                if (logo != null)
                {
                    var imageComponent = logo.GetComponent<UnityEngine.UI.Image>();
                    if (imageComponent != null)
                    {
                        Color currentColor = imageComponent.color;
                        currentColor.a = isActive ? 1f : 0.25f;
                        imageComponent.color = currentColor;
                    }
                }
            }
        }
    }
}