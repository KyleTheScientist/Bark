using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.XR;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Modules.Physics;
using BepInEx.Configuration;
namespace Bark.Modules.Movement
{
    public class Platforms : BarkModule
    {
        public static readonly string DisplayName = "Platforms";
        public static GameObject platformPrefab;
        public GameObject platform, ghost;
        BoxCollider normalCollider, stickyCollider;
        private XRNode xrNode;
        private Transform hand;
        private float spawnTime;
        private Material material;
        InputTracker input;

        void Awake()
        {
            if (!platformPrefab)
            {
                platformPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Cloud");
                platformPrefab.SetActive(false);
            }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                platform = Instantiate(platformPrefab);
                platform.name = "Cloud Solid";
                platform.GetComponent<Renderer>().enabled = false;
                normalCollider = platform.transform.Find("Normal Collider").GetComponent<BoxCollider>();
                stickyCollider = platform.transform.Find("Sticky Collider").GetComponent<BoxCollider>();
                normalCollider.gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;
                stickyCollider.gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;

                ghost = Instantiate(platformPrefab);
                ghost.name = "Cloud Renderer";
                foreach (var collider in ghost.GetComponentsInChildren<Collider>())
                    collider.enabled = false;

                material = ghost.GetComponent<Renderer>().material;
                platform.gameObject.SetActive(false);
                ReloadConfiguration();
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        bool gripped;
        public void OnGrip()
        {
            if (enabled)
            {
                gripped = true;
                platform.SetActive(true);
                platform.transform.position = hand.position;
                platform.transform.rotation = hand.rotation;
                float scaleX = xrNode == XRNode.LeftHand ? -1 : 1;
                platform.transform.localScale = new Vector3(scaleX, 1, 1) * Player.Instance.scale;

                ghost.SetActive(true);
                ghost.transform.position = hand.position;
                ghost.transform.rotation = hand.rotation;
                ghost.transform.localScale = platform.transform.localScale;

                spawnTime = Time.time;
            }
        }

        public void OnRelease()
        {
            gripped = false;
            platform?.SetActive(false);
        }

        public Platforms Left()
        {
            hand = Player.Instance.leftControllerTransform;
            xrNode = XRNode.LeftHand;

            return this;
        }

        public Platforms Right()
        {
            hand = Player.Instance.rightControllerTransform;
            xrNode = XRNode.RightHand;
            return this;
        }

        void FixedUpdate()
        {
            if (!Sticky.Value && gripped)
                spawnTime = Time.time;
            float transparency = (Time.time - spawnTime) / 1f;
            material.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, transparency));
            normalCollider.gameObject.layer = NoCollide.active ? NoCollide.layer : 0;
            stickyCollider.gameObject.layer = NoCollide.active ? NoCollide.layer : 0;
        }

        protected override void Cleanup()
        {
            platform?.Obliterate();
            ghost?.Obliterate();
            if (input != null)
            {
                input.OnPressed -= OnGrip;
                input.OnReleased -= OnRelease;
            }

        }
        protected override void ReloadConfiguration()
        {
            normalCollider.enabled = !Sticky.Value;
            stickyCollider.enabled = Sticky.Value;

            if (input != null)
            {
                input.OnPressed -= OnGrip;
                input.OnReleased -= OnRelease;
            }
            input = GestureTracker.Instance.GetInputTracker(Input.Value, xrNode);
            input.OnPressed += OnGrip;
            input.OnReleased += OnRelease;
        }

        public static ConfigEntry<bool> Sticky;
        public static ConfigEntry<string> Input;
        public static void BindConfigEntries()
        {
            try
            {
                Sticky = Plugin.configFile.Bind(
                    section: DisplayName,
                    key: "sticky",
                    defaultValue: false,
                    description: "Whether or not your hands stick to the platforms"
                );

                Input = Plugin.configFile.Bind(
                    section: DisplayName,
                    key: "input",
                    defaultValue: "grip",
                    configDescription: new ConfigDescription(
                        "Which button you press to activate the platform",
                        new AcceptableValueList<string>("grip", "trigger", "stick", "a/x", "b/y")
                    )
                );
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        public override string GetDisplayName()
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
