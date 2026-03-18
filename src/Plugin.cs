using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace PlanetShieldEnhanced
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "boris.dsp.PlanetShieldEnhanced";
        public const string PluginName = "PlanetShieldEnhanced";
        public const string PluginVersion = "1.0.0";

        public static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;

            try
            {
                var harmony = new Harmony(PluginGuid);
                PlanetWideShieldPatch.RegisterPatch(harmony);
                GroundShieldPatch.RegisterPatches(harmony);
            }
            catch (Exception e)
            {
                Log.LogError($"[PlanetShield] Init failed: {e}");
            }

            Log.LogInfo("PlanetShieldEnhanced v1.0.0 loaded.");
        }
    }
}
