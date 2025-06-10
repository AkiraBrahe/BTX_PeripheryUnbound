using BattleTech;
using ISM3025.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BTX_PeripheryUnbound.UI
{
    internal class StarMap
    {
        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class HighlightInhabitedSystems
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __runOriginal, StarmapSystemRenderer __instance)
            {
                if (!(__runOriginal || !Main.Settings.MapVisuals.HighlightInhabitedSystems)) return true;

                bool isAbandoned = __instance.system.System.Def.Tags.Contains("planet_other_empty");
                __instance.starInner.gameObject.SetActive(!isAbandoned);
                __instance.starInnerUnvisited.gameObject.SetActive(isAbandoned);

                __runOriginal = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class HighlightStarClusters
        {
            public class ClusterData
            {
                public string Name { get; set; } public float Width { get; set; } public float Height { get; set; }
            }

            private static readonly Dictionary<string, ClusterData> StarClusterSizes = new()
            {
                { "starsystemdef_BrocchisCluster", new ClusterData { Name = "Brocchi's Cluster", Width = 6.884f, Height = 7.451f } },
                { "starsystemdef_ChaineCluster", new ClusterData { Name = "Chaine Cluster", Width = 5.634f, Height = 5.637f } },
                { "starsystemdef_EndersCluster", new ClusterData { Name = "Enders Cluster", Width = 4.365f, Height = 4.365f } },
                //{ "starsystemdef_EritCluster", new ClusterData { Name = "Erit Cluster", Width = ???, Height = ??? } },
                { "starsystemdef_HyadesCluster", new ClusterData { Name = "Hyades Cluster", Width = 13.819f, Height = 14.107f } },
                { "starsystemdef_PiratesHavenCluster", new ClusterData { Name = "Pirates Haven Cluster", Width = 27.424f, Height = 27.422f } },
                { "starsystemdef_PleiadesCluster", new ClusterData { Name = "Pleiades Cluster", Width = 14.153f, Height = 14.218f } }
                //{ "starsystemdef_ThetaCarinaeCluster", new ClusterData { Name = "Theta Carinae Cluster", Width = ???, Height = ??? } },
                //{ "starsystemdef_TrznadelCluster", new ClusterData { Name = "Trznadel Cluster", Width = ???, Height = ??? } },
            };

            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance)
            {
                if (Main.Settings.MapVisuals.HighlightStarClusters &&
                    __instance.system.System.Def.Tags.Contains("planet_type_starcluster"))
                {
                    string systemDefId = __instance.system.System.ID;
                    if (StarClusterSizes.TryGetValue(systemDefId, out ClusterData clusterData))
                    {
                        float scaleFactor = 0.7f;
                        float targetScale = Mathf.Max(clusterData.Width, clusterData.Height) * scaleFactor; ;
                        __instance.starInnerUnvisited.gameObject.transform.localScale = Vector3.one * targetScale;
                    }
                    else
                    {
                        float defaultScale = 2f;
                        __instance.starInnerUnvisited.gameObject.transform.localScale = Vector3.one * defaultScale;
                    }

                    __instance.starInner.gameObject.SetActive(false);
                    __instance.starInnerUnvisited.gameObject.SetActive(true);

                    float clusterBrightness = 0.25f;

                    MaterialPropertyBlock BrightnessMpb = new();
                    BrightnessMpb.Clear();
                    BrightnessMpb.SetColor("_Color", __instance.systemColor * clusterBrightness);
                    __instance.starInner.SetPropertyBlock(BrightnessMpb);
                    __instance.starInnerUnvisited.SetPropertyBlock(BrightnessMpb);
                    __instance.transform.SetAsLastSibling();
                }
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "Init")]
        public static class ReduceStarBrightness
        {
            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance)
            {
                if (!Main.Settings.MapVisuals.HighlightInhabitedSystems ||
                   (Main.Settings.MapVisuals.HighlightStarClusters && 
                   __instance.system.System.Def.Tags.Contains("planet_type_starcluster"))) return;

                bool isAbandoned = __instance.system.System.Def.Tags.Contains("planet_other_empty");
                float starBrightness = isAbandoned ? 1f : 1.5f;

                MaterialPropertyBlock BrightnessMpb = new();
                BrightnessMpb.Clear();
                BrightnessMpb.SetColor("_Color", __instance.systemColor * starBrightness);
                __instance.starInner.SetPropertyBlock(BrightnessMpb);
                __instance.starInnerUnvisited.SetPropertyBlock(BrightnessMpb);
            }
        }

        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class HideJumpPoints
        {
            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance)
            {
                if (Main.Settings.MapVisuals.HideJumpPoints &&
                    __instance.system.System.Def.Tags.Contains("planet_other_jumppoint"))
                {
                    __instance.starInner.gameObject.SetActive(false);
                    __instance.starInnerUnvisited.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(FactionValue), "GetMapBorderColor")]
        public static class UseMapBorderOverrideColor
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
                    if (seen.Add(logo.gameObject.name))
                    {
                        logo.SetSiblingIndex(seen.Count - 1);
                    }
                    else
                    {
                        Object.Destroy(logo.gameObject);
                    }
                }
            }
        }
    }
}
