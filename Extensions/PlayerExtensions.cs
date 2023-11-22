using Bark.Modules;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Extensions
{
    public static class PlayerExtensions
    {
        public static void AddForce(this Player self, Vector3 v)
        {
            self.bodyCollider.attachedRigidbody.velocity += v;
        }

        public static void SetVelocity(this Player self, Vector3 v)
        {
            self.bodyCollider.attachedRigidbody.velocity = v;
        }

        public static PhotonView PhotonView(this VRRig rig)
        {
            //return rig.photonView;
            return Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
        }

        public static T GetProperty<T>(this VRRig rig, string key)
        {
            if(rig?.PhotonView()?.Owner is Photon.Realtime.Player player)
                return (T)player?.CustomProperties[key];
            return default(T);
        }

        public static bool HasProperty(this VRRig rig, string key)
        {
            if(rig?.PhotonView()?.Owner is Photon.Realtime.Player player)
                return player.HasProperty(key);
            return false;
        }

        public static bool ModuleEnabled(this VRRig rig, string mod)
        {
            if(rig?.PhotonView()?.Owner is Photon.Realtime.Player player)
                return player.ModuleEnabled(mod);
            return false;
        }

        public static T GetProperty<T>(this Photon.Realtime.Player player, string key)
        {
            return (T)player?.CustomProperties[key];
        }

        public static bool HasProperty(this Photon.Realtime.Player player, string key)
        {
            return !(player?.CustomProperties[key] is null);
        }

        public static bool ModuleEnabled(this Photon.Realtime.Player player, string mod)
        {
            if (!player.HasProperty(BarkModule.enabledModulesKey)) return false;
            Dictionary<string, bool> enabledMods = player.GetProperty<Dictionary<string, bool>>(BarkModule.enabledModulesKey);
            if (enabledMods is null || !enabledMods.ContainsKey(mod)) return false;
            return enabledMods[mod];
        }

        public static VRRig Rig(this Photon.Realtime.Player player)
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                if (rig?.PhotonView()?.Owner == player)
                    return rig;
            }
            return null;
        }
    }
}
