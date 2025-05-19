using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BTX_PeripheryUnbound
{
    internal class RepScreen
    {
        private static readonly List<string> FixedFactions = new List<string>()
        {
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
        };

        [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        public static class ReorderFactions
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState __instance)
            {
                if (__instance != null && __instance.displayedFactions != null)
                {
                    __instance.displayedFactions.Clear();
                    __instance.displayedFactions.AddRange(FixedFactions);
                }
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class MoveAuriganWidget
        {
            [HarmonyPostfix]
            [HarmonyAfter("BattleTech.Haree.BEXTimeline")]
            public static void Postfix(SGCaptainsQuartersReputationScreen __instance, List<SGFactionReputationWidget> ___FactionPanelWidgets, SimGameState ___simState)
            {
                try
                {
                    SGFactionReputationWidget auriganWidget = ___FactionPanelWidgets[12]; // AuriganDirectorate
                    auriganWidget.Init(___simState, FactionEnumeration.GetAuriganRestorationFactionValue(), new UnityAction(__instance.RefreshWidgets), false);
                    auriganWidget.gameObject.SetActive(true);
                    ___FactionPanelWidgets[___FactionPanelWidgets.Count - 1].gameObject.SetActive(false);
                }
                catch (Exception ex)
                {
                    Main.Log.LogException(ex);
                }
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class AddHeaders
        {
            public static void Postfix(List<SGFactionReputationWidget> ___FactionPanelWidgets)
            {
                if (___FactionPanelWidgets == null || ___FactionPanelWidgets.Count == 0) return;

                var factionTypeHeaders = new[]
                {
                    new { Type = "Successor States", StartIndex = 0 },
                    new { Type = "Minor States", StartIndex = 5 },
                    new { Type = "Periphery States", StartIndex = 10 },
                    new { Type = "Bandit Kingdoms", StartIndex = 20 },
                    new { Type = "Criminals / Pirates / Mercs", StartIndex = 25 },
                };

                int headerIndex = 0;

                foreach (var headerInfo in factionTypeHeaders)
                {
                    if (headerInfo.StartIndex < ___FactionPanelWidgets.Count && ___FactionPanelWidgets[headerInfo.StartIndex] != null)
                    {
                        SGFactionReputationWidget firstWidget = ___FactionPanelWidgets[headerInfo.StartIndex];
                        string headerName = $"FactionHeader{headerIndex}";

                        if (firstWidget.transform.Find(headerName) == null)
                        {
                            GameObject headerWidget = new GameObject(headerName);
                            headerWidget.transform.SetParent(firstWidget.transform, false);

                            TextMeshProUGUI headerText = headerWidget.AddComponent<TextMeshProUGUI>();
                            headerText.text = $"<b>{headerInfo.Type}</b>";
                            headerText.alignment = TextAlignmentOptions.Left;
                            headerText.fontSize = 18;
                            headerText.overflowMode = TextOverflowModes.Overflow;

                            RectTransform headerRectTransform = headerText.GetComponent<RectTransform>();
                            if (headerRectTransform != null)
                            {
                                headerRectTransform.sizeDelta = new Vector2(275f, 20f);
                                headerRectTransform.anchoredPosition = new Vector2(-55f, 55f);
                                headerRectTransform.localScale = Vector3.one;
                            }
                        }
                        headerIndex++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
        public static class CheckActivity
        {
            public static void Postfix(List<SGFactionReputationWidget> ___FactionPanelWidgets, ref SimGameState ___simState)
            {
                if (___FactionPanelWidgets == null || ___FactionPanelWidgets.Count == 0) return;

                DateTime currentDate = ___simState.CurrentDate;
                if (currentDate == FactionActivityTracker.LastDayUpdated)
                {
                    return;
                }

                FactionActivityTracker.LastDayUpdated = currentDate;

                for (int i = 0; i < FixedFactions.Count; i++)
                {
                    SGFactionReputationWidget factionWidget = ___FactionPanelWidgets[i];
                    string factionName = FixedFactions[i];

                    bool isActive = FactionActivityTracker.IsFactionActive(factionName, currentDate);

                    Transform overlay = factionWidget.transform.Find("noRep-overlay");
                    if (overlay != null)
                    {
                        overlay.gameObject.SetActive(!isActive);
                    }

                    Transform logo = factionWidget.transform.Find("LOGO/factionLogo");
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
}