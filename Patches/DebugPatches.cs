using HarmonyLib;
using System;
using Bark.Tools;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Bark.Patches
{
    public static class DebugPatches
    {
        public static string[] ignoreables = new string[]
        {
            "JoinRandomRoom failed.",
            "PhotonView does not exist!",
            "Photon.Pun.PhotonHandler.FixedUpdate ()",
            "Photon.Voice.Unity.VoiceConnection.FixedUpdate ()",
            "GorillaNetworking.PhotonNetworkController.DisconnectCleanup ()",
        };

        public static bool Ignore(string s)
        {
            foreach(var i in  ignoreables)
            {
                if (s.Contains(i)) return true;
            }
            return false;
        }
    }

    //[HarmonyPatch(typeof(Debug))]
    //[HarmonyPatch("LogError", MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(object) })]
    //public class LogErrorPatch
    //{
    //    private static void Postfix(object message) 
    //    {
    //        try
    //        {
    //            var stack = new StackTrace();
    //            if (DebugPatches.Ignore($"{message} {stack}")) return;
    //            Logging.Debug(stack);
    //        }
    //        catch (Exception e) { Logging.Exception(e); }
    //    }
    //}

    //[HarmonyPatch(typeof(Debug))]
    //[HarmonyPatch("LogWarning", MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(object) })]
    //public class LogWarningPatch
    //{
    //    private static void Postfix(object message)
    //    {
    //        try
    //        {
    //            var stack = new StackTrace();
    //            if (DebugPatches.Ignore($"{message} {stack}")) return;
    //            Logging.Debug(stack);
    //        }
    //        catch (Exception e) { Logging.Exception(e); }
    //    }
    //}

    //[HarmonyPatch(typeof(Debug))]
    //[HarmonyPatch("LogError", MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(object), typeof(UnityEngine.Object) })]
    //public class LogError2Patch
    //{
    //    private static void Postfix(object message, Object context)
    //    {
    //        try
    //        {
    //            var stack = new StackTrace();
    //            if (DebugPatches.Ignore($"{message} {context} {stack}")) return;
    //            Logging.Debug(message, context, stack);
    //        }
    //        catch (Exception e) { Logging.Exception(e); }
    //    }
    //}

    //[HarmonyPatch(typeof(Debug))]
    //[HarmonyPatch("LogWarning", MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(object) })]
    //public class LogWarning2Patch
    //{
    //    private static void Postfix(object message)
    //    {
    //        try
    //        {
    //            var stack = new StackTrace();
    //            if (DebugPatches.Ignore($"{message} {stack}")) return;
    //            Logging.Debug(message, stack);
    //        }
    //        catch (Exception e) { Logging.Exception(e); }
    //    }
    //}
}
