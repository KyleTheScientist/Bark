using GorillaLocomotion;
using Bark.Gestures;
using Bark.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.InputSystem.HID;
using Bark.Modules.Physics;

namespace Bark.Modules.Movement
{
    public class Wallrun : BarkModule
    {
        private Vector3 baseGravity;
        private RaycastHit hit;
        void Awake()
        {
            baseGravity = UnityEngine.Physics.gravity;
        }

        protected override void OnEnable()
        {
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
                if(Vector3.Distance(player.bodyCollider.transform.position, hit.point) > 2)
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

        public override string DisplayName()
        {
            return "Wall Run";
        }

        public override string Tutorial()
        {
            return "Effect: Allows you to walk on any surface, no matter the angle.";
        }

    }
}


