using HarmonyLib;
using GorillaLocomotion;
using System;
using Bark.Tools;
using Bark.Modules.Physics;
using UnityEngine;
using Bark.Gestures;

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
                if (Potions.active) 
                    Camera.main.farClipPlane = 500;
            }
            catch(Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetSlidePercentage", MethodType.Normal)]
    public class SlidePatch
    {
        private static void Postfix(Player __instance, ref float __result)
        {
            try
            {
                if (SlipperyHands.Instance)
                    __result = SlipperyHands.Instance.enabled ? 1 : __result;
                if (NoSlip.Instance)
                    __result = NoSlip.Instance.enabled ? 0 : __result;
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class VRRigLateUpdatePatch
    {
        private static void Postfix(VRRig __instance, ref AudioSource ___voiceAudio)
        {
            if(!Plugin.inRoom || !___voiceAudio) return;
            try
            {
                ___voiceAudio.pitch = Mathf.Clamp(___voiceAudio.pitch, .8f, 1.2f);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    //[HarmonyPatch(typeof(ControllerInputPoller))]
    //[HarmonyPatch("Update", MethodType.Normal)]
    //public class ControllerUpdatePatch
    //{
    //    private static void Prefix(ControllerInputPoller __instance)
    //    {
    //        Debug.Log("Lol");
    //        GestureTracker.Instance.UpdateValues();
    //    }
    //}
}
