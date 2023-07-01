using HarmonyLib;
using System;
using Bark.Tools;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
namespace Bark.Patches
{
    [HarmonyPatch(typeof(Debug))]
    [HarmonyPatch("LogError", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(object) })]
    public class LogErrorPatch
    {
        private static void Postfix(object message)
        {
            try
            {
                var stack = new StackTrace();
                 Logging.Debug(stack);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(Debug))]
    [HarmonyPatch("LogWarning", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(object) })]
    public class LogWarningPatch
    {
        private static void Postfix(object message)
        {
            try
            {
                var stack = new StackTrace();
                 Logging.Debug(stack);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(Debug))]
    [HarmonyPatch("LogError", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(object), typeof(UnityEngine.Object) })]
    public class LogError2Patch
    {
        private static void Postfix(object message, Object context)
        {
            try
            {
                var stack = new StackTrace();
                Logging.Debug(context, stack);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }

    [HarmonyPatch(typeof(Debug))]
    [HarmonyPatch("LogWarning", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(object) })]
    public class LogWarning2Patch
    {
        private static void Postfix(object message)
        {
            try
            {
                var stack = new StackTrace();
                 Logging.Debug(stack);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
    }
}
