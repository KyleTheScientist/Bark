using BepInEx;
using System;
using UnityEngine;
using Utilla;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using Bark.GUI.ComputerInterface;
using Bepinject;
using BepInEx.Configuration;
using System.Runtime.InteropServices;
using System.IO;
using Bark.Modules;
using System.Reflection;
using ModestTree;

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
            if (menuController || !pluginEnabled || !inRoom) return;
            Logging.LogDebug("Menu:", menuController, "Plugin Enabled:", pluginEnabled, "InRoom:", inRoom);

            try
            {
                menuController = Instantiate(monkeMenuPrefab).AddComponent<MenuController>();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        public void Cleanup()
        {
            try
            {
                Logging.LogDebug("Cleaning up");
                menuController?.gameObject?.Obliterate();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        void Awake()
        {
            try
            {
                Logging.Init();
                Zenjector.Install<BarkCI>().OnProject();
                configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, PluginInfo.Name + ".cfg"), true);

                GeneralSettingsPage.BindConfigEntries();
                Logging.LogDebug("Found", BarkModule.GetBarkModuleTypes().Count, "modules");
                foreach (Type moduleType in BarkModule.GetBarkModuleTypes())
                {
                    MethodInfo bindConfigs = moduleType.GetMethod("BindConfigEntries");
                    if (bindConfigs is null) continue;
                    bindConfigs.Invoke(null, null);
                }
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        void Start()
        {
            try
            {
                Logging.LogDebug("Start");
                Utilla.Events.GameInitialized += OnGameInitialized;
                assetBundle = AssetUtils.LoadAssetBundle("Bark/Resources/barkbundle");
                Logging.LogDebug(assetBundle.GetAllAssetNames().Join("\n"));
                monkeMenuPrefab = assetBundle.LoadAsset<GameObject>("Bark Menu");
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }


        void OnEnable()
        {
            try
            {
                Logging.LogDebug("OnEnable");
                this.pluginEnabled = true;
                HarmonyPatches.ApplyHarmonyPatches();
                if (initialized)
                    Setup();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        void OnDisable()
        {
            try
            {
                Logging.LogDebug("OnDisable");
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
            Logging.LogDebug("RoomJoined");
            inRoom = true;
            Setup();
        }

        [ModdedGamemodeLeave]
        void RoomLeft(string gamemode)
        {
            Logging.LogDebug("RoomLeft");
            inRoom = false;
            Cleanup();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            Logging.LogDebug("OnGameInitialized");
            initialized = true;
        }
    }
}
