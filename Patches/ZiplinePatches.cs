using HarmonyLib;
using GorillaLocomotion;
using System;
using Bark.Tools;
using Bark.Modules.Physics;
using UnityEngine;
using GorillaLocomotion.Gameplay;
using GorillaLocomotion.Climbing;
using Bark.Modules.Movement;

namespace Bark.Patches
{
    [HarmonyPatch(typeof(GorillaZipline))]
    [HarmonyPatch("Update", MethodType.Normal)]
    public class ZiplineUpdatePatch
    {
        private static void Postfix(GorillaZipline __instance, BezierSpline ___spline, float ___currentT, 
            GorillaHandClimber ___currentClimber)
        {
            if(!Plugin.inRoom) return;
            try
            {
                var rockets = Rockets.Instance;
                if (!rockets || !rockets.enabled || !___currentClimber) return;
                Vector3 curDir = __instance.GetCurrentDirection();
                Vector3 rocketDir = rockets.AddedVelocity();
                var currentSpeed = Traverse.Create(__instance).Property("currentSpeed");
                float speedDelta = Vector3.Dot(curDir, rocketDir) * Time.deltaTime * rocketDir.magnitude * 1000f;
                currentSpeed.SetValue(currentSpeed.GetValue<float>() + speedDelta);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }
}
