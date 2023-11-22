using Bark.Tools;
using Bark.Modules;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Networking
{
    public class NetworkPropertyHandler : MonoBehaviourPunCallbacks
    {

        public static NetworkPropertyHandler Instance;
        public static string versionKey = "BarkVersion";
        public Action<Player> OnPlayerJoined, OnPlayerLeft;
        public Action<Player, string, bool> OnPlayerModStatusChanged;

        void Awake()
        {
            Instance = this;
            ChangeProperty(versionKey, PluginInfo.Version);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer == PhotonNetwork.LocalPlayer) return;
            if (changedProps.ContainsKey(BarkModule.enabledModulesKey))
            {
                var enabledModules = (Dictionary<string, bool>)changedProps[BarkModule.enabledModulesKey];
                //Logging.Debug(targetPlayer.NickName, "toggled mods:");
                foreach (var mod in enabledModules)
                {
                    //Logging.Debug(mod.Value ? "  +" : "  -", mod.Key, mod.Value);
                    OnPlayerModStatusChanged?.Invoke(targetPlayer, mod.Key, mod.Value);
                }
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            OnPlayerLeft?.Invoke(otherPlayer);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            OnPlayerJoined?.Invoke(newPlayer);
        }

        float lastPropertyUpdate;
        const float refreshRate = 1f;
        Hashtable properties = new Hashtable();
        void FixedUpdate()
        {
            if (properties.Count == 0 || Time.time - lastPropertyUpdate < refreshRate) return;
            Logging.Debug($"Updated properties ({properties.Count}):");
            foreach (var property in properties)
            {
                Logging.Debug(property.Key, ":", property.Value);
                if ((string)property.Key == BarkModule.enabledModulesKey)
                    foreach (var mod in (Dictionary<string, bool>)property.Value)
                        if (mod.Value)
                            Logging.Debug("    ", property.Key, "is enabled");
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            properties.Clear();
            lastPropertyUpdate = Time.time;
        }

        public void ChangeProperty(string key, object value)
        {
            if (properties.ContainsKey(key))
                properties[key] = value;
            else
                properties.Add(key, value);
        }

        //public void ChangeProperties(Player player, Dictionary<string, object> properties)
        //{

        //}
    }
}
