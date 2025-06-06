using BattleTech;
using BattleTech.UI;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace BTX_PeripheryUnbound
{
    public class Main
    {
        private const string ModName = "BTX_PeripheryUnbound";
        private const string HarmonyInstanceId = "com.github.AkiraBrahe.BTX_PeripheryUnbound";

        internal static Harmony harmony;
        internal static string modDir;
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public class ModSettings
        {
            public bool HideJumpPointsOnStarMap { get; set; } = true;
            public bool IntelShowTarget { get; set; } = true;
            public bool IntelShowVariant { get; set; } = true;
            public bool UpdateStarSystemDefsOnLoad { get; set; } = false;
        }

        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;
            Log = Logger.GetLogger(ModName);
            Logger.SetLoggerLevel(ModName, LogLevel.Debug);

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settingsJSON) ?? new ModSettings();
                harmony = new Harmony(HarmonyInstanceId);
                ApplyHarmonyPatches();
                Log.Log($"{ModName} Initialized!");
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        internal static void ApplyHarmonyPatches()
        {
            // Reputation Screen Dupes
            harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets"), HarmonyPatchType.Prefix, "io.github.mpstark.ISM3025");
            harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets"), HarmonyPatchType.Postfix, "io.github.mpstark.ISM3025");

            // Contract Weather Conditions
            harmony.Unpatch(AccessTools.PropertyGetter(typeof(Contract), "ShortDescription"), HarmonyPatchType.Postfix, "BEX.BattleTech.Extended_CE");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
