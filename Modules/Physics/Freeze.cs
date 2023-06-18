using Bark.GUI;
using BepInEx.Configuration;
using GorillaLocomotion;
using UnityEngine;

namespace Bark.Modules.Physics
{
    public class Freeze : BarkModule
    {
        public static readonly string DisplayName = "Freeze";
        public static Freeze Instance;

        void Awake() { Instance = this; }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            Player.Instance.bodyCollider.attachedRigidbody.isKinematic = true;
        }

        protected override void Cleanup()
        {
            Player.Instance.bodyCollider.attachedRigidbody.isKinematic = false;
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Freezes you in place.";
        }

    }
}
