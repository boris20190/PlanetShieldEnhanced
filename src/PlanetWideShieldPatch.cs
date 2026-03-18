using HarmonyLib;
using System;
using UnityEngine;

namespace PlanetShieldEnhanced
{
    /// <summary>
    /// A single shield generator covers the entire planet surface.
    /// Original concept by lltcggie (MIT license).
    /// </summary>
    public static class PlanetWideShieldPatch
    {
        public static void RegisterPatch(Harmony harmony)
        {
            var targetMethod = AccessTools.Method(typeof(PlanetATField), "UpdateGeneratorMatrix");
            var postfix = AccessTools.Method(typeof(PlanetWideShieldPatch), "Postfix");

            if (targetMethod != null)
            {
                harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
                Plugin.Log.LogInfo("[PlanetShield] Patched: PlanetATField.UpdateGeneratorMatrix");
            }
            else
            {
                Plugin.Log.LogError("[PlanetShield] Could not find PlanetATField.UpdateGeneratorMatrix");
            }
        }

        public static void Postfix(PlanetATField __instance)
        {
            int count = __instance.generatorCount;
            if (count <= 0) return;

            float maxW = 0.0f;
            for (int i = 0; i < count; i++)
                maxW = Math.Max(__instance.generatorMatrix[i].w, maxW);

            __instance.generatorCount = 0;
            Array.Clear(__instance.generatorMatrix, 0, PlanetATField.MAX_GENERATOR_COUNT);

            double shieldRadius = 80.0;
            double realRadius = __instance.planet.realRadius;
            int rCount = (int)Math.Ceiling((Math.PI * realRadius) / (2.0 * shieldRadius));

            Vector4 vec;
            vec.x = 0.0f;
            vec.y = (float)realRadius;
            vec.z = 0.0f;
            vec.w = maxW;
            __instance.generatorMatrix[__instance.generatorCount++] = vec;

            for (int i = 1; i <= rCount; i++)
            {
                double sita = (double)i * Math.PI / (double)rCount;
                vec.x = 0.0f;
                vec.y = (float)(realRadius * Math.Cos(sita));
                vec.z = (float)(realRadius * Math.Sin(sita));
                vec.w = maxW;
                __instance.generatorMatrix[__instance.generatorCount++] = vec;

                double r2 = realRadius * Math.Sin(sita);
                int r2Count = (int)Math.Ceiling((Math.PI * r2 * 2.0) / (2.0 * shieldRadius));
                for (int j = 1; j < r2Count; j++)
                {
                    double sita2 = (double)j * 2.0 * Math.PI / (double)r2Count;
                    vec.x = (float)(r2 * Math.Sin(sita2));
                    vec.z = (float)(r2 * Math.Cos(sita2));
                    __instance.generatorMatrix[__instance.generatorCount++] = vec;
                }
            }
        }
    }
}
