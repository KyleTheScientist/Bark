using GorillaLocomotion;
using Bark.Tools;
using UnityEngine;
using System.Reflection;
using Bark.Modules.Physics;
using Bark.GUI;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Bark.Modules.Movement
{
    public class Wallrun : BarkModule
    {
        public static readonly string DisplayName = "Wall Run";
        private Vector3 baseGravity;
        private RaycastHit hit;

        void Awake()
        {
            // Debug.Log("Calling awake for Wall run");
            baseGravity = UnityEngine.Physics.gravity;
        }

        protected override void OnEnable()
        {
            // Debug.Log("Calling OnEnable for Wall run");
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
        }

        protected void FixedUpdate()
        {
            // Debug.Log("Calling FixedUpdate for Wall run");

            bool isgripping;
            Player player = Player.Instance;
            List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, inputDevices);
            inputDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out isgripping);

            if (isgripping)
            {
                // Debug.Log("Gripping");
                if (player.wasLeftHandTouching || player.wasRightHandTouching)
                {
                    FieldInfo fieldInfo = typeof(Player).GetField("lastHitInfoHand", BindingFlags.NonPublic | BindingFlags.Instance);
                    hit = (RaycastHit)fieldInfo.GetValue(player);
                    UnityEngine.Physics.gravity = hit.normal * -baseGravity.magnitude * GravScale();
                }
                else
                {
                    if (Vector3.Distance(player.bodyCollider.transform.position, hit.point) > 2 * Player.Instance.scale)
                        Cleanup();
                }
            } else
            {
                // Debug.Log("Not Gripping");
                if (Vector3.Distance(player.bodyCollider.transform.position, hit.point) > 2 * Player.Instance.scale)
                    Cleanup();
            }


        }
        public float GravScale()
        {
            // Debug.Log("Calling GravScale for Wall run");
            return 1;
        }

        protected override void Cleanup()
        {
            // Debug.Log("Calling cleanup for Wall run");
            UnityEngine.Physics.gravity = baseGravity * GravScale();
        }

        public override string GetDisplayName()
        {
            // Debug.Log("Calling GetDisplayName for Wall run");
            return DisplayName;
        }

        public override string Tutorial()
        {
            // Debug.Log("Calling Tutorials for Wall run");
            return "Effect: Allows you to walk on any surface, no matter the angle.";
        }

    }
}


