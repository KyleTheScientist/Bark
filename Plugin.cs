using BepInEx;
using System;
using UnityEngine;
using Utilla;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using BepInEx.Configuration;
using System.IO;
using Bark.Modules;
using System.Reflection;

namespace Bark
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]

    public class Plugin : BaseUnityPlugin
    {
        public static bool initialized, inRoom;
        bool pluginEnabled = false;
        public static AssetBundle assetBundle;
        public static MenuController menuController;
        public static GameObject monkeMenuPrefab;
        public static ConfigFile configFile;

        public void Setup()
        {
            Logging.Debug("Attempting to set up");
            if (menuController || !pluginEnabled || !inRoom) return;
            Logging.Debug("Menu:", menuController, "Plugin Enabled:", pluginEnabled, "InRoom:", inRoom);

            try
            {
                menuController = Instantiate(monkeMenuPrefab).AddComponent<MenuController>();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        public void Cleanup()
        {
            try
            {
                Logging.Debug("Cleaning up");
                menuController?.gameObject?.Obliterate();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void Awake()
        {
            try
            {
                Logging.Init();
                CI.Init();
                configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginInfo.Name + ".cfg"), true);
                MenuController.BindConfigEntries();
                Logging.Debug("Found", BarkModule.GetBarkModuleTypes().Count, "modules");
                foreach (Type moduleType in BarkModule.GetBarkModuleTypes())
                {
                    MethodInfo bindConfigs = moduleType.GetMethod("BindConfigEntries");
                    if (bindConfigs is null) continue;
                    bindConfigs.Invoke(null, null);
                }
            }
            catch (Exception e) { Logging.Exception(e); }
        }

    void Start()
        {
            try
            {
                Logging.Debug("Start");
                Utilla.Events.GameInitialized += OnGameInitialized;
                assetBundle = AssetUtils.LoadAssetBundle("Bark/Resources/barkbundle");
                monkeMenuPrefab = assetBundle.LoadAsset<GameObject>("Bark Menu");
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }


        void OnEnable()
        {
            try
            {
                Logging.Debug("OnEnable");
                this.pluginEnabled = true;
                HarmonyPatches.ApplyHarmonyPatches();
                if (initialized)
                    Setup();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void OnDisable()
        {
            try
            {
                Logging.Debug("OnDisable");
                this.pluginEnabled = false;
                HarmonyPatches.RemoveHarmonyPatches();
                Cleanup();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            Logging.Debug("OnGameInitialized");
            initialized = true;
        }

        [ModdedGamemodeJoin]
        void RoomJoined(string gamemode)
        {
            Logging.Debug("RoomJoined");
            inRoom = true;
            Setup();
        }

        [ModdedGamemodeLeave]
        void RoomLeft(string gamemode)
        {
            Logging.Debug("RoomLeft");
            inRoom = false;
            Cleanup();
        }
    }
}
