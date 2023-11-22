using UnityEngine;
using Player = GorillaLocomotion.Player;
using Bark.GUI;
using BepInEx.Configuration;
using Bark.Extensions;
using Bark.Gestures;

namespace Bark.Modules.Movement
{
    public class Fly : BarkModule
    {
        public static readonly string DisplayName = "Fly";
        float speedScale = 10, acceleration = .01f;
        Vector2 xz;
        float y;
        void FixedUpdate()
        {
            // nullify gravity by adding it's negative value to the player's velocity
            var rb = Player.Instance.bodyCollider.attachedRigidbody;
            if (BarkModule.enabledModules.ContainsKey(Bubble.DisplayName)
                && !BarkModule.enabledModules[Bubble.DisplayName])
                rb.AddForce(-UnityEngine.Physics.gravity * rb.mass * Player.Instance.scale);

            xz = GestureTracker.Instance.leftStickAxis.GetValue();
            y = GestureTracker.Instance.rightStickAxis.GetValue().y;

            Vector3 inputDirection = new Vector3(xz.x, y, xz.y);

            // Get the direction the player is facing but nullify the y axis component
            var playerForward = Player.Instance.bodyCollider.transform.forward;
            playerForward.y = 0;

            // Get the right vector of the player but nullify the y axis component
            var playerRight = Player.Instance.bodyCollider.transform.right;
            playerRight.y = 0;

            var velocity =
                inputDirection.x * playerRight +
                y * Vector3.up +
                inputDirection.z * playerForward;
            velocity *= Player.Instance.scale * speedScale;
            rb.velocity = Vector3.Lerp(rb.velocity, velocity, acceleration);
        }

        public override string GetDisplayName()
        {
            return "Fly";
        }

        public override string Tutorial()
        {
            return "Use left stick to fly on the horizontally, and right stick to fly vertically.";
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            ReloadConfiguration();
        }

        public static ConfigEntry<int> Speed;
        public static ConfigEntry<int> Acceleration;
        protected override void ReloadConfiguration()
        {
            speedScale = Speed.Value * 2;
            acceleration = Acceleration.Value;
            if (acceleration == 10)
                acceleration = 1;
            else
                acceleration = MathExtensions.Map(Acceleration.Value, 0, 10, 0.0075f, .25f);
        }

        public static void BindConfigEntries()
        {
            Speed = Plugin.configFile.Bind(
                section: DisplayName,
                key: "speed",
                defaultValue: 5,
                description: "How fast you fly"
            );

            Acceleration = Plugin.configFile.Bind(
                section: DisplayName,
                key: "acceleration",
                defaultValue: 5,
                description: "How fast you accelerate"
            );
        }

        protected override void Cleanup() { }
    }
}
