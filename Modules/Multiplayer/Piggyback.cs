using Bark.GUI;
using Bark.Gestures;
using Bark.Patches;
using Bark.Modules.Physics;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR;
using Bark.Extensions;
using Bark.Tools;
using System;

namespace Bark.Modules.Multiplayer
{
    public class Piggyback : BarkModule
    {
        public static readonly string DisplayName = "Piggyback";
        public static bool mounted;
        private Transform mount;
        private VRRig mountedRig;
        private bool latchedWithLeft;
        private const float mountDistance = 1f;
        private Vector3 mountOffset = new Vector3(0, .05f, -.5f);
        private Vector3 mountPosition;

        public static Piggyback Instance;
        void Awake() { Instance = this; }
        protected override void Start()
        {
            base.Start();
        }

        void Mount(Transform t, VRRig rig)
        {
            mountPosition = Player.Instance.bodyCollider.transform.position;
            mountedRig = rig;
            mounted = true;
            mount = t;
            EnableNoClip();
        }


        void Unmount()
        {
            mount = null;
            mounted = false;
            mountedRig = null;
            mount = null;
            DisableNoClip();
            Invoke(nameof(WarpBack), .05f);
        }
        void WarpBack()
        {
            TeleportPatch.TeleportPlayer(mountPosition);
        }
        void FixedUpdate()
        {

            if (mounted)
            {
                if (RevokingConsent(mountedRig))
                {
                    Unmount();
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(98, false, 1f);
                }
                else
                {
                    Vector3 position = mount.TransformPoint(mountOffset);
                    TeleportPatch.TeleportPlayer(position);
                }
            }
        }

        public struct RigScanResult
        {
            public Transform transform;
            public VRRig rig;
            public float distance;
        }

        public RigScanResult ClosestRig(Transform hand)
        {
            VRRig closestRig = null;
            Transform closestTransform = null;
            float closestDistance = Mathf.Infinity;
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                try
                {
                    if (rig.PhotonView().Owner.IsLocal)
                    {
                        continue;
                    }
                    var rigTransform = rig.transform.FindChildRecursive("head");
                    float distanceToTarget = Vector3.Distance(hand.position, rigTransform.position);

                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        closestTransform = rigTransform;
                        closestRig = rig;
                    }
                }
                catch (Exception e)
                {
                    Logging.Exception(e);
                }
            }
            return new RigScanResult()
            {
                transform = closestTransform,
                distance = closestDistance,
                rig = closestRig
            };
        }

        bool GivingConsent(VRRig rig)
        {
            return

                    rig.rightIndex.calcT > .5f &&
                    rig.rightMiddle.calcT > .5f &&
                    rig.rightThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.up, rig.leftHandTransform.forward) > 0

                ||

                    rig.leftIndex.calcT > .5f &&
                    rig.leftMiddle.calcT > .5f &&
                    rig.leftThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.up, rig.rightHandTransform.forward) > 0

            ;
        }

        bool RevokingConsent(VRRig rig)
        {
            return
                    rig.rightIndex.calcT > .5f &&
                    rig.rightMiddle.calcT > .5f &&
                    rig.rightThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.down, rig.rightHandTransform.forward) > 0
                ||
                    rig.leftIndex.calcT > .5f &&
                    rig.leftMiddle.calcT > .5f &&
                    rig.leftThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.down, rig.leftHandTransform.forward) > 0
            ;
        }

        void EnableNoClip()
        {
            var noclip = Plugin.menuController.GetComponent<NoCollide>();
            noclip.button.AddBlocker(ButtonController.Blocker.PIGGYBACKING);
            noclip.enabled = true;
        }

        void DisableNoClip()
        {
            var noclip = Plugin.menuController.GetComponent<NoCollide>();
            noclip.button.RemoveBlocker(ButtonController.Blocker.PIGGYBACKING);
            noclip.enabled = false;
        }

        bool TryMount()
        {
            RigScanResult closest = ClosestRig(GestureTracker.Instance.rightHand.transform);
            if (closest.distance < mountDistance && enabled && !mounted)
            {
                if (GivingConsent(closest.rig))
                {
                    if (!PositionValidator.Instance.isValidAndStable)
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(68, false, 1f);
                        return false;
                    }
                    Mount(closest.transform, closest.rig);
                    return true;
                }
            }
            return false;
        }

        void Latch(InputTracker input)
        {
            if (input.node == XRNode.LeftHand)
                latchedWithLeft = TryMount();
            else
                latchedWithLeft = !TryMount();
        }

        void Unlatch(InputTracker input)
        {
            if (!enabled || !mounted) return;
            if (input.node == XRNode.LeftHand && latchedWithLeft ||
                input.node == XRNode.RightHand && !latchedWithLeft)
                Unmount();
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            GestureTracker.Instance.leftGrip.OnPressed += Latch;
            GestureTracker.Instance.leftGrip.OnReleased += Unlatch;
            GestureTracker.Instance.rightGrip.OnPressed += Latch;
            GestureTracker.Instance.rightGrip.OnReleased += Unlatch;
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            if (mounted)
                Unmount();
            if (GestureTracker.Instance is null) return;
            GestureTracker.Instance.leftGrip.OnPressed -= Latch;
            GestureTracker.Instance.leftGrip.OnReleased -= Unlatch;
            GestureTracker.Instance.rightGrip.OnPressed -= Latch;
            GestureTracker.Instance.rightGrip.OnReleased -= Unlatch;

        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "- To mount a player, ask them to give you a thumbs up.\n" +
                "- Hold down [Grip] near their head to hop on.\n" +
                "- If the player gives you a thumbs down you will be dismounted.";
        }


    }
}
