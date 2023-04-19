using BepInEx.Logging;
using System;
using System.Diagnostics;

namespace Bark.Tools
{
    public static class Logging
    {
        public static ManualLogSource logger;
        public static void Init()
        {
            logger = Logger.CreateLogSource("Bark");
            
        }

        public static void LogException(Exception e)
        {

            LogWarning(e.Message, e.StackTrace);
        }

        public static void LogFatal(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogFatal($"({methodInfo.ReflectedType.Name})s " + string.Join(" ", content));
        }

        public static void LogWarning(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogWarning($"({methodInfo.ReflectedType.Name})s " + string.Join(" ", content));
        }

        public static void LogInfo(params object[] content)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            logger.LogInfo($"({methodInfo.ReflectedType.Name})s " + string.Join(" ", content));
        }

        public static void LogDebug(params object[] content)
        {
            //var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            //logger.LogInfo($"*** Debug *** ({methodInfo.ReflectedType.Name}] )s + string.Join(" ", content));
        }
    }
}
