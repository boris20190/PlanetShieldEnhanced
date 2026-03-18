using HarmonyLib;

namespace PlanetShieldEnhanced
{
    public static class GroundShieldPatch
    {
        private static long GetEnergyPerDamage()
        {
            return GameMain.history.planetaryATFieldEnergyRate;
        }

        public static void RegisterPatches(Harmony harmony)
        {
            var localMethod = AccessTools.Method(typeof(SkillSystem), "DamageGroundObjectByLocalCaster");
            var remoteMethod = AccessTools.Method(typeof(SkillSystem), "DamageGroundObjectByRemoteCaster");
            var localPrefix = AccessTools.Method(typeof(GroundShieldPatch), "DamageGroundLocal");
            var remotePrefix = AccessTools.Method(typeof(GroundShieldPatch), "DamageGroundRemote");

            if (localMethod != null)
                harmony.Patch(localMethod, prefix: new HarmonyMethod(localPrefix));
            else
                Plugin.Log.LogError("[PlanetShield] Could not find DamageGroundObjectByLocalCaster");

            if (remoteMethod != null)
                harmony.Patch(remoteMethod, prefix: new HarmonyMethod(remotePrefix));
            else
                Plugin.Log.LogError("[PlanetShield] Could not find DamageGroundObjectByRemoteCaster");
        }

        public static void DamageGroundLocal(
            PlanetFactory factory, ref int damage, ref int slice,
            ref SkillTargetLocal target, ref SkillTargetLocal caster)
        {
            TryAbsorbDamage(factory, ref damage, target.type);
        }

        public static void DamageGroundRemote(
            PlanetFactory factory, ref int damage, ref int slice,
            ref SkillTargetLocal target, ref SkillTarget caster)
        {
            TryAbsorbDamage(factory, ref damage, target.type);
        }

        private static void TryAbsorbDamage(PlanetFactory factory, ref int damage, ETargetType targetType)
        {
            if (damage <= 0) return;
            if (targetType == ETargetType.Enemy) return;

            PlanetATField atField = factory?.planetATField;
            if (atField == null || atField.generatorCount <= 0 || atField.energy <= 0) return;

            long energyPerDamage = GetEnergyPerDamage();
            if (energyPerDamage <= 0) energyPerDamage = 1;

            long energyCost = (long)damage * energyPerDamage;

            if (atField.energy >= energyCost)
            {
                atField.energy -= energyCost;
                damage = 0;
            }
            else
            {
                int absorbed = (int)(atField.energy / energyPerDamage);
                atField.energy = 0;
                damage -= absorbed;
                if (damage < 0) damage = 0;
            }
        }
    }
}
