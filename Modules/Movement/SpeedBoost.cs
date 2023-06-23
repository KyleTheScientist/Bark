using Bark.GUI;
using Bark.Tools;
using BepInEx.Configuration;
using GorillaLocomotion;
using System;

namespace Bark.Modules
{
    public class SpeedBoost : BarkModule
    {
        public static readonly string DisplayName = "Speed Boost";
        public static float baseVelocityLimit, scale = 1.5f;
        public static bool active = false;

        void FixedUpdate()
        {
            string progress = "";
            try
            {
                progress = "Getting Gamemode\n";
                var gameMode = GorillaGameManager.instance.GameMode();
                progress = "Checking status\n";
                if (active && (gameMode == "NONE" || gameMode == "CASUAL"))
                {
                    progress = "Setting multiplier\n";
                    Player.Instance.jumpMultiplier = 1.3f * scale;
                    Player.Instance.maxJumpSpeed = 8.5f * scale;
                }
            }
            catch (Exception e)
            {
                Logging.LogDebug("GorillaGameManager.instance is null:", GorillaGameManager.instance is null);
                Logging.LogDebug("GorillaGameManager.instance.GameMode() is null:", GorillaGameManager.instance?.GameMode() is null);
                Logging.LogDebug(progress);
                Logging.LogException(e);
            }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            active = true;
            baseVelocityLimit = Player.Instance.velocityLimit;
            ReloadConfiguration();
        }

        protected override void Cleanup()
        {
            if (active)
            {
                scale = 1;
                Player.Instance.velocityLimit = baseVelocityLimit;
                active = false;
            }
        }
        protected override void ReloadConfiguration()
        {
            scale = 1 + (Speed.Value / 10f);
            if(this.enabled)
                Player.Instance.velocityLimit = baseVelocityLimit * scale;
        }

        public static ConfigEntry<int> Speed;
        public static void BindConfigEntries()
        {
            Speed = Plugin.configFile.Bind(
                section: DisplayName,
                key: "speed",
                defaultValue: 5,
                description: "How fast you run while speed boost is active"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }
        public override string Tutorial()
        {
            return "Effect: Increases your jump strength.";
        }

    }
}
