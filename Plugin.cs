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
                Logging.LogFatal(error, error.StackTrace);
            }
        }

        public void Cleanup()
        {
            try
            {
                Logging.LogDebug(menuController is null);
                Logging.LogDebug(menuController?.gameObject is null);
                menuController?.gameObject?.Obliterate();
            }
            catch (Exception error)
            {
                Logging.LogFatal(error, error.StackTrace);
            }
        }

        void Awake()
        {
            Logging.Init();
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
                Logging.LogFatal(e, e.StackTrace);
            }
        }


        void OnEnable()
        {

            try
            {
                Logging.LogWarning("Debugmessage");
                Logging.LogInfo("Info message");
                Logging.LogWarning("Warning message");
                Logging.LogFatal("Fatal message");
                this.pluginEnabled = true;
                HarmonyPatches.ApplyHarmonyPatches();
                if (initialized)
                    Setup();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void OnDisable()
        {
            try
            {
                this.pluginEnabled = false;
                HarmonyPatches.RemoveHarmonyPatches();
                Cleanup();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

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

        void OnGameInitialized(object sender, EventArgs e)
        {
            initialized = true;
        }
    }
}
