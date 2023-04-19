using Bark.Gestures;
using Bark.Patches;
using UnityEngine;

namespace Bark.Modules
{
    public class Piggyback : BarkModule
    {
        public static bool mounted;
        private Transform mount;
        private VRRig mountedRig;
        private bool latchedWithLeft;
        private const float mountDistance = 1f;
        private Vector3 mountOffset = new Vector3(0, .05f, -.5f);

        protected override void Start()
        {
            base.Start();
            GestureTracker.Instance.OnRightGripReleased += () =>
            {
                if (enabled && mounted && !latchedWithLeft)
                    Unmount();
            };
            GestureTracker.Instance.OnRightGripPressed += () =>
            {
                RigScanResult closest = ClosestRig(GestureTracker.Instance.rightHand.transform);
                if (closest.distance < mountDistance && enabled && !mounted)
                {
                    if (GivingConsent(closest.rig))
                    {
                        Mount(closest.transform, closest.rig);
                        latchedWithLeft = false;
                    }
                }
            };

            GestureTracker.Instance.OnLeftGripReleased += () =>
            {
                if (enabled && mounted && latchedWithLeft)
                    Unmount();
            };
            GestureTracker.Instance.OnLeftGripPressed += () =>
            {
                RigScanResult closest = ClosestRig(GestureTracker.Instance.leftHand.transform);
                if (closest.distance < mountDistance && enabled && !mounted)
                {
                    if (GivingConsent(closest.rig))
                    {
                        Mount(closest.transform, closest.rig);
                        latchedWithLeft = true;
                    }
                }
            };


        }

        void Mount(Transform t, VRRig rig)
        {
            mountedRig = rig;
            mounted = true;
            mount = t;
            EnableNoClip();
        }


        void Unmount()
        {
            mount = null;
            mounted = false;
            DisableNoClip();
        }
        
        void FixedUpdate()
        {
            if (RevokingConsent(mountedRig))
                Unmount();

            if (mounted)
            {
                Vector3 position = mount.TransformPoint(mountOffset);
                TeleportPatch.TeleportPlayer(position, mount.transform.eulerAngles.y);
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
                if (rig.photonView.Owner.IsLocal)
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
            return new RigScanResult()
            {
                transform = closestTransform,
                distance = closestDistance,
                rig = closestRig
            };
        }

        bool GivingConsent(VRRig rig)
        {
            return (
                (
                    rig.rightIndex.calcT > .5f &&
                    rig.rightMiddle.calcT > .5f &&
                    rig.rightThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.up, rig.leftHandTransform.forward) > 0
                )
                ||
                (
                    rig.leftIndex.calcT > .5f &&
                    rig.leftMiddle.calcT > .5f &&
                    rig.leftThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.up, rig.rightHandTransform.forward) > 0
                )
            );
        }

        bool RevokingConsent(VRRig rig)
        {
            return (
                (
                    rig.rightIndex.calcT > .5f &&
                    rig.rightMiddle.calcT > .5f &&
                    rig.rightThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.down, rig.rightHandTransform.forward) > 0
                )
                ||
                (
                    rig.leftIndex.calcT > .5f &&
                    rig.leftMiddle.calcT > .5f &&
                    rig.leftThumb.calcT < .25f &&
                    Vector3.Dot(Vector3.down, rig.leftHandTransform.forward) > 0
                )
            );
        }

        void EnableNoClip()
        {
            Plugin.menuController.GetComponent<NoClip>().enabled = true;
        }

        void DisableNoClip()
        {
            Plugin.menuController.GetComponent<NoClip>().enabled = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (mounted)
                Unmount();
        }

        void OnDestroy()
        {
            if (mounted)
                Unmount();
        }

        public override string DisplayName()
        {
            return "Piggyback";
        }

        public override string Tutorial()
        {
            return "- To mount a player, ask them to give you a thumbs up.\n" +
                "- Hold down [Grip] near their head to hop on.\n" +
                "- If the player gives you a thumbs down you will be dismounted.";
        }


    }
}
