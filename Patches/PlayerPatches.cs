using HarmonyLib;
using Bark.Modules;
using GorillaLocomotion;
using System;
using Bark.Tools;
using Bark.Modules.Physics;

namespace Bark.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class LateUpdatePatch
    {
        public static Action<Player> OnLateUpdate;
        private static void Postfix(Player __instance)
        {
            try
            {
                OnLateUpdate?.Invoke(__instance);
            }
            catch(Exception e) { Logging.LogException(e); }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetSlidePercentage", MethodType.Normal)]
    public class SlidePatch
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            if(Slippery.Instance)
            __result = Slippery.Instance.enabled ? 1 : __result;
        }
    }
}
