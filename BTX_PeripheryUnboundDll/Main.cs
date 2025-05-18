using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using HBS.Logging;
using Newtonsoft.Json;
using System;

namespace BTX_PeripheryUnbound
{
    public class Main
    {
        internal static Harmony harmony;
        internal static string modDir;
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public class ModSettings
        {
            public bool HideJumpPointsOnStarMap { get; set; } = true;
            public bool IntelShowTarget { get; set; } = true;
            public bool IntelShowVariant { get; set; } = true;
        }

        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;
            Log = Logger.GetLogger("BTX_PeripheryUnbound");
            Logger.SetLoggerLevel("BTX_PeripheryUnbound", new LogLevel?(LogLevel.Debug));

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settingsJSON) ?? new ModSettings();
                harmony = new Harmony("com.github.AkiraBrahe.BTX_PeripheryUnbound");
                ApplyHarmonyPatches();
                Log.Log($"Periphery Unbound Mod Initialized!");
            
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        static void ApplyHarmonyPatches()
        {
            // Reputation Screen Dupes
            harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets"), HarmonyPatchType.Prefix, "io.github.mpstark.ISM3025");
            harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets"), HarmonyPatchType.Postfix, "io.github.mpstark.ISM3025");

            // Contract Weather Conditions
            harmony.Unpatch(AccessTools.PropertyGetter(typeof(Contract), "ShortDescription"), HarmonyPatchType.Postfix, "BEX.BattleTech.Extended_CE");

            harmony.PatchAll();
        }
    }
}
