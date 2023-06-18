using GorillaLocomotion;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using UnityEngine;
using BepInEx.Configuration;
using Oculus.Platform;

namespace Bark.Modules.Movement
{
    public class Airplane : BarkModule
    {
        public static readonly string DisplayName = "Airplane";
        float speedScale = 10f, acceleration = .1f;

        protected override void Start()
        {
            base.Start();
        }

        void OnGlide(Vector3 direction)
        {
            if (!enabled) return;
            var tracker = GestureTracker.Instance;
            if (
                tracker.leftTrigger.pressed ||
                tracker.rightTrigger.pressed ||
                tracker.leftGrip.pressed ||
                tracker.rightGrip.pressed) return;

            var player = Player.Instance;
            if (player.wasLeftHandTouching || player.wasRightHandTouching) return;

            if (SteerWith.Value == "head")
                direction = player.headCollider.transform.forward;

            var rigidbody = player.bodyCollider.attachedRigidbody;
            Vector3 velocity = direction * player.scale * speedScale;
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, velocity, acceleration);
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            ReloadConfiguration();
            GestureTracker.Instance.OnGlide += OnGlide;
        }

        protected override void Cleanup()
        {
            GestureTracker.Instance.OnGlide -= OnGlide;
        }

        public static ConfigEntry<int> Speed;
        public static ConfigEntry<string> SteerWith;
        protected override void ReloadConfiguration()
        {
            speedScale = Speed.Value * 2;
        }

        public static void BindConfigEntries()
        {
            Speed = Plugin.configFile.Bind(
                section: DisplayName,
                key: "speed",
                defaultValue: 5,
                description: "How fast you fly"
            );

            SteerWith = Plugin.configFile.Bind(
                section: DisplayName,
                key: "steer with",
                defaultValue: "wrists",
                configDescription: new ConfigDescription(
                    "Which part of your body you use to steer",
                    new AcceptableValueList<string>("wrists", "head")
                )
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "- To fly, do a T-pose (spread your arms out like wings on a plane). \n" +
                "- To fly up, rotate your palms so they face forward. \n" +
                "- To fly down, rotate your palms so they face backward.";
        }

    }
}


