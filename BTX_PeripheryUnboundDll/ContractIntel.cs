using BattleTech;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomUnits;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BTX_PeripheryUnbound
{
    internal class ContractIntel
    {
        [HarmonyPatch(typeof(LanceContractIntelWidget), "Init")]
        public static class AddTargetAndVariant
        {
            private static readonly Dictionary<string, string> Variant = new Dictionary<string, string>()
            {
                { "ThreeWayBattle_SearchDenialCS", "Default" },
                { "ThreeWayBattle_SearchDenialCS_Easy", "Easy (Mixed Level II)" },
                { "ThreeWayBattle_SearchDenialCS_Hard", "Hard (Additional ComStar Forces)" },
                { "ThreeWayBattle_SearchDenialWoB", "Default" },
                { "ThreeWayBattle_SearchDenialWoB_Easy", "Easy (Mixed Level II)" },
                { "ThreeWayBattle_SearchDenialWoB_Hard", "Hard (Additional Blakist Forces)" },
                { "ThreeWayBattle_TagTeam_CS", "Default" },
                { "ThreeWayBattle_TagTeam_CS_Alt", "Alternate (Additional Forces)" },
                { "ThreeWayBattle_TagTeam_CS_Betray", "Betray (Additional ComStar Forces)" }
            };

            [HarmonyPostfix]
            public static void Postfix(LocalizableText ContractDescriptionField, Contract contract)
            {
                try
                {
                    if (contract != null && contract.Override != null)
                    {
                        GameObject parentObject = ContractDescriptionField.transform.parent.gameObject;

                        LocalizableText targetText = parentObject.FindComponent<LocalizableText>("txt_target");
                        LocalizableText variantText = parentObject.FindComponent<LocalizableText>("txt_variant");

                        // Add primary target
                        if (Main.Settings.IntelShowTarget)
                        {
                            if (targetText == null && !string.IsNullOrEmpty(contract.Override.targetTeam.faction))
                            {
                                targetText = UnityEngine.Object.Instantiate(ContractDescriptionField.gameObject).GetComponent<LocalizableText>();
                                if (targetText != null)
                                {
                                    targetText.gameObject.transform.SetParent(parentObject.transform);
                                    targetText.gameObject.transform.SetSiblingIndex(parentObject.transform.GetSiblingIndex() + 1);
                                    targetText.gameObject.transform.localScale = Vector3.one;
                                    targetText.gameObject.name = "txt_target";

                                    string targetFactionName = contract.Override.targetTeam.FactionDef?.Name ?? contract.Override.targetTeam.faction;
                                    if (targetFactionName.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
                                    {
                                        targetFactionName = targetFactionName.Substring(4);
                                    }
                                    targetText.SetText($"Target: <color=#F79B26>{targetFactionName}</color>", Array.Empty<object>());

                                    HBSTooltip targetTooltip = targetText.gameObject.GetComponent<HBSTooltip>() ?? targetText.gameObject.AddComponent<HBSTooltip>();
                                    targetTooltip.SetDefaultStateData(null);

                                    FactionDef targetFactionDef = UnityGameInstance.BattleTechGame.Simulation?.GetFactionDef(contract.Override.targetTeam.faction);
                                    targetTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(targetFactionDef));
                                }
                                else
                                {
                                    Main.Log.LogWarning("Instantiation of target widget failed.");
                                }
                            }
                            else if (targetText != null && !string.IsNullOrEmpty(contract.Override.targetTeam.faction))
                            {
                                string targetFactionName = contract.Override.targetTeam.FactionDef?.Name ?? contract.Override.targetTeam.faction;
                                if (targetFactionName.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
                                {
                                    targetFactionName = targetFactionName.Substring(4);
                                }
                                targetText.SetText($"Target: <color=#F79B26>{targetFactionName}</color>", Array.Empty<object>());

                                HBSTooltip targetTooltip = targetText.gameObject.GetComponent<HBSTooltip>() ?? targetText.gameObject.AddComponent<HBSTooltip>();
                                targetTooltip.SetDefaultStateData(null);

                                FactionDef targetFactionDef = UnityGameInstance.BattleTechGame.Simulation?.GetFactionDef(contract.Override.targetTeam.faction);
                                targetTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(targetFactionDef));
                            }
                        }

                        // Add mission variant (for specific contracts)
                        if (Main.Settings.IntelShowVariant)
                        {
                            if (variantText == null && Variant.TryGetValue(contract.Override.ID, out string variantDescription))
                            {
                                variantText = UnityEngine.Object.Instantiate(ContractDescriptionField.gameObject).GetComponent<LocalizableText>();
                                if (variantText != null)
                                {
                                    variantText.gameObject.transform.SetParent(parentObject.transform);
                                    variantText.gameObject.transform.SetSiblingIndex(parentObject.transform.GetSiblingIndex() + (targetText != null ? 2 : 1));
                                    variantText.gameObject.transform.localScale = Vector3.one;
                                    variantText.gameObject.name = "txt_variant";
                                    variantText.SetText($"Variant: <color=#F79B26>{variantDescription}</color>", Array.Empty<object>());

                                    HBSTooltip variantTooltip = variantText.gameObject.GetComponent<HBSTooltip>();
                                    variantTooltip?.SetDefaultStateData(null);
                                }
                                else
                                {
                                    Main.Log.LogWarning("Instantiation of variant widget failed.");
                                }
                            }
                            else if (variantText != null && Variant.ContainsKey(contract.Override.ID))
                            {
                                variantText.SetText($"Variant: <color=#F79B26>{Variant[contract.Override.ID]}</color>", Array.Empty<object>());

                                HBSTooltip variantTooltip = variantText.gameObject.GetComponent<HBSTooltip>();
                                variantTooltip?.SetDefaultStateData(null);
                            }
                            else if (variantText != null && !Variant.ContainsKey(contract.Override.ID))
                            {
                                UnityEngine.Object.Destroy(variantText.gameObject);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.Log.LogException(ex);
                }
            }
        }
    }
}