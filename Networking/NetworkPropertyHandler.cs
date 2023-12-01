using Bark.Tools;
using Bark.Modules;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Bark.Extensions;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Bark.Networking
{
    public class NetworkPropertyHandler : MonoBehaviourPunCallbacks
    {

        public static NetworkPropertyHandler Instance;
        public static string versionKey = "BarkVersion";
        public Action<Player> OnPlayerJoined, OnPlayerLeft;
        public Action<Player, string, bool> OnPlayerModStatusChanged;
        public Dictionary<Player, NetworkedPlayer> networkedPlayers = new Dictionary<Player, NetworkedPlayer>();

        void Awake()
        {
            Instance = this;
            ChangeProperty(versionKey, PluginInfo.Version);
        }

        void Start()
        {
            Logging.Debug("Found", GorillaParent.instance.vrrigs.Count, "vrrigs and ", PhotonNetwork.PlayerList.Length, "players.");
            foreach (var player in PhotonNetwork.PlayerList)
            {
                StartCoroutine(CreateNetworkedPlayer(player));
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer == PhotonNetwork.LocalPlayer) return;
            if (changedProps.ContainsKey(BarkModule.enabledModulesKey))
            {
                networkedPlayers[targetPlayer].hasBark = true;
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
            if (networkedPlayers.ContainsKey(otherPlayer))
            {
                Destroy(networkedPlayers[otherPlayer]);
                networkedPlayers.Remove(otherPlayer);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            try
            {
                base.OnPlayerEnteredRoom(newPlayer);
                OnPlayerJoined?.Invoke(newPlayer);
                StartCoroutine(CreateNetworkedPlayer(newPlayer));
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        IEnumerator CreateNetworkedPlayer(Player player = null, VRRig rig = null)
        {
            if (player is null && rig is null)
                throw new Exception("Both player and rig are null");

            if (player is null)
                player = rig.myPlayer;
            else if (rig is null)
            {
                for (int i = 0; i < 10; i++)
                {
                    rig = player.Rig();
                    if (rig is null)
                    {
                        yield return new WaitForSeconds(.1f);
                        continue;
                    }
                }
            }
            var np = rig.gameObject.GetOrAddComponent<NetworkedPlayer>();
            np.owner = player;
            np.rig = rig;
            networkedPlayers.Add(player, np);
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
    }
}
