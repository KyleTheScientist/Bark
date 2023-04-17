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
        public bool inRoom;
        public bool initialized;

        public static AssetBundle assetBundle;
        public static MenuController menuController;
        public static GameObject monkeMenuPrefab;

        public void Setup()
        {
            if (menuController || !enabled) return;

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

        public void Awake()
        {
            try
            {
                Events.GameInitialized += OnGameInitialized;
                assetBundle = AssetUtils.LoadAssetBundle("Bark/Resources/barkbundle");
                monkeMenuPrefab = assetBundle.LoadAsset<GameObject>("MonkeMenu");
            }
            catch (Exception e)
            {
                Logging.Log(e, e.StackTrace);
            }
        }

        // Enable mod when we load in
        public void OnGameInitialized(object sender, EventArgs e)
        {
            initialized = true;
        }

        public void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            if (initialized && inRoom)
                Setup();
        }

        public void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
            if (initialized && inRoom)
                Cleanup();
        }

        [ModdedGamemodeJoin]
        public void RoomJoined()
        {
            inRoom = true;

            if (!enabled) return;
            Setup();
        }

        [ModdedGamemodeLeave]
        public void RoomLeft()
        {
            inRoom = false;

            if (!enabled) return;
            Cleanup();
        }
    }
}
