using HarmonyLib;
using GorillaLocomotion;
using System;
using Bark.Tools;
using Bark.Modules.Physics;
using UnityEngine;
using Bark.Extensions;
using System.Runtime.CompilerServices;

namespace Bark.Patches
{
    [HarmonyPatch(typeof(SizeManager))]
    [HarmonyPatch("ControllingChanger", MethodType.Normal)]
    public class SizeChangePatch
    {
        private static void Postfix(ref SizeChanger __result, Transform t)
        {
            if (!Plugin.inRoom) return;
            try
            {
                if (Potions.active && t == Camera.main.transform)
                    __result = Potions.sizeChanger;
                else if 
                (
                    Potions.ShowNetworkedSizes.Value &&
                    t.GetComponentInParent<VRRig>() is VRRig rig && 
                    rig.ModuleEnabled(Potions.DisplayName)
                )
                {
                    Potions.TryGetSizeChangerForRig(rig, out __result);
                }
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(SizeManager))]
    [HarmonyPatch("LerpSizeToNormal", MethodType.Normal)]
    public class SizeLerpPatch
    {
        private static void Postfix(float currentSize, ref float __result)
        {
            if (!Plugin.inRoom) return;
            if (Mathf.Abs(1f - currentSize) < 0.05f)
                __result = 1;
            else
                __result = Mathf.Lerp(currentSize, 1f, .75f * Time.fixedDeltaTime);
        }
    }
}