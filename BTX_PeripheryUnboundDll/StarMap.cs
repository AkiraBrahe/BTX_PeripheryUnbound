using BattleTech;
using BEXTimeline;
using ISM3025.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BTX_PeripheryUnbound
{
    internal class StarMap
    {
        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class SetJumpPointVisibility
        {
            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance)
            {
                if (Main.Settings.HideJumpPointsOnStarMap &&
                    __instance.system.System.Def.Tags != null &&
                    __instance.system.System.Def.Tags.Contains("planet_other_jumppoint"))
                {
                    __instance.starInner.gameObject.SetActive(false);
                    __instance.starInnerUnvisited.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(PirateHelper), "GetClosestPirate")]
        public static class AddDarkCaste
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result)
            {
                SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
                if (simulation.CurrentDate > new DateTime(3050, 1, 1) &&
                    (__result == "PiratesValkyrate" || __result == "PiratesOberon"))
                {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                    {
                        __result = "DarkCaste";
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FactionValue), "GetMapBorderColor")]
        public static class OverrideBorderColor
        {
            [HarmonyFinalizer]
            public static void Finalizer(FactionValue __instance, ref Color __result)
            {
                FactionDef factionDef = __instance?.FactionDef;
                if (factionDef != null && !string.IsNullOrEmpty(factionDef.factionMapBorderOverrideColor))
                {
                    if (factionDef.GetFactionMapBorderColor(out Color outColor))
                    {
                        __result = outColor;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DynamicLogos), "PlaceLogos")]
        public static class CleanUpLogos
        {
            [HarmonyPostfix]
            public static void Postfix(StarmapRenderer renderer)
            {
                Transform logosParent = renderer.restorationLogo.transform.parent;
                List<Transform> logoList = [.. logosParent.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.gameObject.name.EndsWith("Logo"))
                    .OrderBy(t => t.gameObject.name)];

                var seen = new HashSet<string>();
                foreach (var logo in logoList)
                {
                    if (!seen.Add(logo.gameObject.name))
                    {
                        UnityEngine.Object.Destroy(logo.gameObject);
                    }
                    else
                    {
                        logo.SetSiblingIndex(seen.Count - 1);
                    }
                }
            }
        }
    }
}
