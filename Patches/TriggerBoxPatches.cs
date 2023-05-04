using Bark.Modules.Physics;
using Bark.Tools;
using GorillaNetworking;
using HarmonyLib;

namespace Bark.Patches
{
    internal class TriggerBoxPatches
    {
        public static bool triggersEnabled = true;

        [HarmonyPatch(typeof(GorillaGeoHideShowTrigger))]
        [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
        internal class GeoTriggerPatches
        {
            private static bool Prefix(GorillaGeoHideShowTrigger __instance)
            {
                return triggersEnabled;
            }
        }

        [HarmonyPatch(typeof(GorillaNetworkDisconnectTrigger))]
        [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
        internal class DisconnectTriggerPatches
        {
            private static bool Prefix(GorillaNetworkDisconnectTrigger __instance)
            {
                return triggersEnabled;
            }
        }

        [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
        [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
        internal class JoinTriggerPatches
        {
            private static bool Prefix(GorillaNetworkJoinTrigger __instance)
            {
                return triggersEnabled;
            }
        }

        [HarmonyPatch(typeof(GorillaQuitBox))]
        [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
        internal class QuitTriggerPatches
        {
            private static bool Prefix(GorillaQuitBox __instance)
            {
                if (!triggersEnabled)
                {
                    Logging.LogDebug("Player fell out of map, disabling noclip");
                    NoCollide.Instance.enabled = false;
                }
                return triggersEnabled;
            }
        }

    }
}
