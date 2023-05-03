using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.XR;
using Bark.Extensions;
using Bark.Gestures;
using Bark.Modules.Physics;

namespace Bark.Modules.Movement
{
    public class Platforms : BarkModule
    {
        public GameObject platform, ghost;
        private XRNode xrNode;
        private Transform hand;
        private float spawnTime;
        private Material material;

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                if (xrNode == XRNode.LeftHand)
                {
                    GestureTracker.Instance.OnLeftGripPressed += OnGrip;
                    GestureTracker.Instance.OnLeftGripReleased += OnRelease;
                }
                else if (xrNode == XRNode.RightHand)
                {
                    GestureTracker.Instance.OnRightGripPressed += OnGrip;
                    GestureTracker.Instance.OnRightGripReleased += OnRelease;
                }

                platform = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud"));
                platform.name = "Cloud Solid";
                platform.GetComponent<Renderer>().enabled = false;
                platform.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;

                ghost = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud"));
                ghost.name = "Cloud Renderer";
                ghost.GetComponent<Collider>().enabled = false;

                material = ghost.GetComponent<Renderer>().material;
                platform.gameObject.SetActive(false);
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        public void OnGrip()
        {
            if (enabled)
            {
                platform.SetActive(true);
                platform.transform.position = hand.position;
                platform.transform.rotation = hand.rotation;
                float scaleX = xrNode == XRNode.LeftHand ? -1 : 1;
                platform.transform.localScale = new Vector3(scaleX, 1, 1) * Player.Instance.scale;

                ghost.transform.position = hand.position;
                ghost.transform.rotation = hand.rotation;
                ghost.transform.localScale = platform.transform.localScale;

                spawnTime = Time.time;
            }
        }

        public void OnRelease()
        {
            platform?.SetActive(false);
        }

        public Platforms Left()
        {
            hand = Player.Instance.leftHandTransform;
            xrNode = XRNode.LeftHand;

            return this;
        }

        public Platforms Right()
        {
            hand = Player.Instance.rightHandTransform;
            xrNode = XRNode.RightHand;
            return this;
        }

        void FixedUpdate()
        {
            float transparency = (Time.time - spawnTime) / 1f;
            material.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, transparency));
            platform.layer = NoClip.active ? NoClip.layer : 0;
        }

        protected override void Cleanup()
        {
            platform?.Obliterate();
            ghost?.Obliterate();
            if (xrNode == XRNode.LeftHand)
            {

                GestureTracker.Instance.OnLeftGripPressed -= OnGrip;
                GestureTracker.Instance.OnLeftGripReleased -= OnRelease;
            }
            else if (xrNode == XRNode.RightHand)
            {
                GestureTracker.Instance.OnRightGripPressed -= OnGrip;
                GestureTracker.Instance.OnRightGripReleased -= OnRelease;
            }

        }

        public override string DisplayName()
        {
            if (xrNode == XRNode.LeftHand) return "Platforms (Left)";
            return "Platforms (Right)";
        }

        public override string Tutorial()
        {
            return "Press [Grip] to spawn a platform you can stand on. " +
                "Release [Grip] to disable it.";
        }
    }
}
