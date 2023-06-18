using GorillaLocomotion;
using Bark.Tools;
using UnityEngine;
using System.Reflection;
using Bark.Modules.Physics;
using Bark.GUI;
using BepInEx.Configuration;

namespace Bark.Modules.Movement
{
    public class Wallrun : BarkModule
    {
        public static readonly string DisplayName = "Wall Run";
        private Vector3 baseGravity;
        private RaycastHit hit;
        void Awake()
        {
            baseGravity = UnityEngine.Physics.gravity;
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
        }

        protected void FixedUpdate()
        {
            Player player = Player.Instance;
            if (player.wasLeftHandTouching || player.wasRightHandTouching)
            {
                FieldInfo fieldInfo = typeof(Player).GetField("lastHitInfoHand", BindingFlags.NonPublic | BindingFlags.Instance);
                hit = (RaycastHit)fieldInfo.GetValue(player);
                UnityEngine.Physics.gravity = hit.normal * -baseGravity.magnitude * GravScale();
            }
            else
            {
                if (Vector3.Distance(player.bodyCollider.transform.position, hit.point) > 2)
                    Cleanup();
            }
        }
        public float GravScale()
        {
            return LowGravity.Instance.active ? LowGravity.Instance.gravityScale : 1;
        }

        protected override void Cleanup()
        {
            UnityEngine.Physics.gravity = baseGravity * GravScale();
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Allows you to walk on any surface, no matter the angle.";
        }

    }
}


