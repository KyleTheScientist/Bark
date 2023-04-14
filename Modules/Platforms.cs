using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace Bark.Modules
{
    public class Platforms : BarkModule
    {
        public GameObject platform, ghost;
        private bool isPressed;
        private Vector3 dimensions = new Vector3(1, 1, 1);
        private XRNode xrNode;
        private Transform hand;
        private float spawnTime;
        private Material material;

        void Awake()
        {
            try
            {
                platform = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud"));
                platform.name = "Cloud Solid";
                platform.GetComponent<Renderer>().enabled = false;
                platform.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;

                ghost = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud"));
                ghost.name = "Cloud Renderer";
                ghost.GetComponent<Collider>().enabled = false;

                material = ghost.GetComponent<Renderer>().material;
                platform.gameObject.SetActive(false);
            } catch (Exception e)
            {
                Logging.Log(e, e.StackTrace);
            }
        }

        void FixedUpdate()
        {

            InputDevices.GetDeviceAtXRNode(xrNode).TryGetFeatureValue(CommonUsages.gripButton, out isPressed);

            if (!platform.gameObject.activeSelf && isPressed)
            {
                platform.transform.position = hand.position;
                ghost.transform.position = hand.position;
                platform.transform.rotation = hand.rotation;
                ghost.transform.rotation = hand.rotation;
                spawnTime = Time.time;
            }

            try
            {
                platform.SetActive(enabled && isPressed);
                float transparency = (Time.time - spawnTime) / 1f;
                material.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, transparency));
                platform.transform.localScale = dimensions * Player.Instance.scale;
                ghost.transform.localScale = platform.transform.localScale;
                platform.layer = NoClip.active ? NoClip.layer : 0;
            }
            catch (Exception e)
            {
                Logging.Log(e, e.StackTrace);
            }
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

        protected override void OnEnable()
        {
            base.OnEnable();
            ghost.SetActive(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            platform.SetActive(false);
            ghost.SetActive(false);
        }

        void OnDestroy()
        {
            Destroy(platform);
            Destroy(ghost);
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
