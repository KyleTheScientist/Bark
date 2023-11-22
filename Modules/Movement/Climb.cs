using Bark.Tools;
using System;
using UnityEngine;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using GorillaLocomotion.Climbing;
using Sound = Bark.Tools.Sounds.Sound;

namespace Bark.Modules.Movement
{
    public class Climb : BarkModule
    {
        public static readonly string DisplayName = "Climb";
        public GameObject climbableLeft, climbableRight;
        private InputTracker<float> leftGrip, rightGrip;
        private Transform leftHand, rightHand;

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                leftGrip = GestureTracker.Instance.leftGrip;
                rightGrip = GestureTracker.Instance.rightGrip;

                leftHand = GestureTracker.Instance.leftHand.transform;
                rightHand = GestureTracker.Instance.rightHand.transform;
                climbableLeft = CreateClimbable(leftGrip);
                climbableRight = CreateClimbable(rightGrip);
                ReloadConfiguration();
            }
            catch (Exception e) { Logging.Exception(e); }
        }
        public GameObject CreateClimbable(InputTracker<float> grip)
        {
            var climbable = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            climbable.name = "Bark Climb Obj";
            climbable.AddComponent<GorillaClimbable>();
            climbable.layer = LayerMask.NameToLayer("GorillaInteractable");
            climbable.GetComponent<Renderer>().enabled = false;
            climbable.transform.localScale = Vector3.one * .25f;
            climbable.SetActive(false);
            grip.OnPressed += OnGrip;
            grip.OnReleased += OnRelease;
            return climbable;
        }

        public void OnGrip(InputTracker tracker)
        {
            if (enabled)
            {
                GameObject climbable;
                Transform hand;
                if (tracker == leftGrip)
                {
                    climbable = climbableLeft;
                    hand = leftHand;
                }
                else
                {
                    climbable = climbableRight;
                    hand = rightHand;
                }

                Collider[] colliders = UnityEngine.Physics.OverlapSphere(
                    hand.position,
                    0.25f,
                    LayerMask.GetMask("Gorilla Body Collider", "Gorilla Tag Collider", "Gorilla Head")
                );

                if (colliders.Length > 0)
                {
                    foreach(var collider in colliders)
                    {
                        Logging.Debug("Hit", collider.gameObject.name);
                    }
                    colliders[0].gameObject.AddComponent<GorillaClimbable>();
                    Sounds.Play(Sound.DragonSqueeze, 1f);
                }
            }
        }

        public void OnRelease(InputTracker tracker)
        {
            if (tracker == GestureTracker.Instance.leftGrip)
                climbableLeft.SetActive(false);
            else
                climbableRight.SetActive(false);
        }

        protected override void Cleanup()
        {
            climbableLeft?.Obliterate();
            climbableRight?.Obliterate();
            if (leftGrip != null)
            {
                leftGrip.OnPressed -= OnGrip;
                leftGrip.OnReleased -= OnRelease;
            }
            if (rightGrip != null)
            {
                rightGrip.OnPressed -= OnGrip;
                rightGrip.OnReleased -= OnRelease;
            }
        }

        public override string GetDisplayName()
        {
            return "Climb";
        }

        public override string Tutorial()
        {
            return "Press [Grip] with either hand to stick to a surface.";
        }
    }
}
