using BepInEx;
using System;
using UnityEngine;
using Utilla;
using Bark.GUI;
using Bark.Tools;

namespace Bark
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]

    public class Plugin : BaseUnityPlugin
    {
        bool initialized, inRoom;
        bool pluginEnabled = false;
        public static AssetBundle assetBundle;
        public static MenuController menuController;
        public static GameObject monkeMenuPrefab;
        
        public void Setup()
        {
            if (menuController || !pluginEnabled || !inRoom) return;

            try
            {
                menuController = Instantiate(monkeMenuPrefab).AddComponent<MenuController>();
            }
            catch (Exception error)
            {
                Logging.Log(error, error.StackTrace);
            }
        }

        public void Cleanup()
        {
            try
            {
                Destroy(menuController?.gameObject);
            }
            catch (Exception error)
            {
                Logging.Log(error, error.StackTrace);
            }
        }

        void Start()
        {
            try
            {
                Utilla.Events.GameInitialized += OnGameInitialized;
                assetBundle = AssetUtils.LoadAssetBundle("Bark/Resources/barkbundle");
                monkeMenuPrefab = assetBundle.LoadAsset<GameObject>("MonkeMenu");
            }
            catch (Exception e)
            {
                Logging.Log(e, e.StackTrace);
            }
        }


        void OnEnable()
        {
            this.pluginEnabled = true;
            HarmonyPatches.ApplyHarmonyPatches();
            if (initialized)
                Setup();
        }

        void OnDisable()
        {
            this.pluginEnabled = false; 
            HarmonyPatches.RemoveHarmonyPatches();
            Cleanup();
        }

        // Disable mod if we join a public lobby
        [ModdedGamemodeJoin]
        void RoomJoined(string gamemode)
        {
            inRoom = true;
            Setup();
        }

        [ModdedGamemodeLeave]
        void RoomLeft(string gamemode)
        {
            inRoom = false;
            Cleanup();
        }

        // Enable mod when we load in
        void OnGameInitialized(object sender, EventArgs e)
        {
            initialized = true;
        }
    }
}
