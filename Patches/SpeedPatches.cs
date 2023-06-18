using HarmonyLib;
using Bark.Modules.Movement;
using Bark.Modules;
using Bark.Tools;
using System;

namespace Bark.Patches
{
    [HarmonyPatch(typeof(GorillaTagManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class TagSpeedPatch
    {
        private static void Postfix(GorillaTagManager __instance, ref float[] __result)
        {
            try
            {
                if (!SpeedBoost.active) return;

                for (int i = 0; i < __result.Length; i++)
                    __result[i] *= SpeedBoost.scale;
            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }

    [HarmonyPatch(typeof(GorillaGameManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class GenericSpeedPatch
    {
        private static void Postfix(GorillaGameManager __instance, ref float[] __result)
        {
            try
            {
                if (!SpeedBoost.active) return;

                for (int i = 0; i < __result.Length; i++)
                    __result[i] *= SpeedBoost.scale;
            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }

    [HarmonyPatch(typeof(GorillaBattleManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class BattleSpeedPatch
    {
        private static void Postfix(GorillaBattleManager __instance, ref float[] __result)
        {
            try
            {
                if (!SpeedBoost.active) return;

                for (int i = 0; i < __result.Length; i++)
                    __result[i] *= SpeedBoost.scale;
            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }

    [HarmonyPatch(typeof(GorillaHuntManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class HuntSpeedPatch
    {
        private static void Postfix(GorillaHuntManager __instance, ref float[] __result)
        {
            try
            {
                if (!SpeedBoost.active) return;

                for (int i = 0; i < __result.Length; i++)
                    __result[i] *= SpeedBoost.scale;
            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }
}
