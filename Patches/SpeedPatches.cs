using HarmonyLib;
using Bark.Modules.Movement;
using Bark.Modules;
using Bark.Tools;

namespace Bark.Patches
{
    [HarmonyPatch(typeof(GorillaTagManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class TagSpeedPatch
    {
        private static void Postfix(GorillaTagManager __instance, ref float[] __result)
        {

            if (!Speed.active) return;

            for (int i = 0; i < __result.Length; i++)
                __result[i] *= Speed.scale;
        }
    }

    [HarmonyPatch(typeof(GorillaGameManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class GenericSpeedPatch
    {
        private static void Postfix(GorillaGameManager __instance, ref float[] __result)
        {
            if (!Speed.active) return;

            for (int i = 0; i < __result.Length; i++)
                __result[i] *= Speed.scale;
        }
    }

    [HarmonyPatch(typeof(GorillaBattleManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class BattleSpeedPatch
    {
        private static void Postfix(GorillaBattleManager __instance, ref float[] __result)
        {
            if (!Speed.active) return;

            for (int i = 0; i < __result.Length; i++)
                __result[i] *= Speed.scale;
        }
    }

    [HarmonyPatch(typeof(GorillaHuntManager))]
    [HarmonyPatch("LocalPlayerSpeed", MethodType.Normal)]
    internal class HuntSpeedPatch
    {
        private static void Postfix(GorillaHuntManager __instance, ref float[] __result)
        {
            if (!Speed.active) return;

            for (int i = 0; i < __result.Length; i++)
                __result[i] *= Speed.scale;
        }
    }
}
