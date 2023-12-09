using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.XR;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using BepInEx.Configuration;
using GorillaLocomotion.Climbing;
using HarmonyLib;
using Bark.Modules.Physics;
using Bark.Networking;

namespace Bark.Modules.Movement
{
    public class Platform : MonoBehaviour
    {
        public GorillaHandClimber climber;
        public bool isSticky, isActive, isLeft;
        Transform hand;
        private Material cloudMaterial;
        float spawnTime;
        Collider collider;
        Vector3 scale;
        string modelName;
        GameObject model;
        Transform wings;
        ParticleSystem rain;

        public void Initialize(bool isLeft)
        {
            try
            {
                this.isLeft = isLeft;
                this.name = "Bark Platform " + (isLeft ? "Left" : "Right");
                this.Scale = 1;
                foreach (Transform child in this.transform)
                    child.gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;
                var cloud = this.transform.Find("cloud");
                cloudMaterial = cloud.GetComponent<Renderer>().material;
                cloudMaterial.color = new Color(1, 1, 1, 0);
                rain = cloud.GetComponent<ParticleSystem>();
                wings = this.transform.Find("doug/wings");

                var handObj = isLeft ? Player.Instance.leftControllerTransform : Player.Instance.rightControllerTransform;
                this.hand = handObj.transform;

                string climberName = isLeft ? "leftClimber" : "rightClimber";
                climber = Traverse.Create(EquipmentInteractor.instance).Field<GorillaHandClimber>(climberName).Value;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        public void Activate()
        {
            isActive = true;
            this.spawnTime = Time.time;
            this.transform.position = hand.transform.position;
            this.transform.rotation = hand.transform.rotation;
            this.transform.localScale = scale * Player.Instance.scale;
            collider.gameObject.layer = NoCollide.active ? NoCollide.layer : 0;
            collider.gameObject.layer = NoCollide.active ? NoCollide.layer : 0;
            collider.enabled = !isSticky;
            if (isSticky)
                Sounds.Play(111, .1f, this.isLeft);
            this.model.SetActive(true);
            if (modelName == "storm cloud")
                rain.Play();
        }

        public void Deactivate()
        {
            isActive = false;
            collider.enabled = false;
            if (!this.model.name.Contains("cloud"))
                this.model.SetActive(false);
            rain.Stop();

        }

        void FixedUpdate()
        {
            if (isActive)
                spawnTime = Time.time;

            float transparency = Mathf.Clamp((Time.time - spawnTime) / 1f, 0.2f, 1);
            float c = modelName == "storm cloud" ? .2f : 1;
            cloudMaterial.color = new Color(c, c, c, Mathf.Lerp(1, 0, transparency));
            if (model.name == "doug")
            {
                wings.transform.localRotation = Quaternion.Euler(Time.frameCount % 2 == 0 ? -30 : 0, 0, 0);
            }
        }


        public bool Sticky
        {
            set
            {
                this.isSticky = value;
                if (isActive)
                    collider.enabled = !isSticky;
            }
        }

        public float Scale
        {
            set
            {
                this.scale = new Vector3(isLeft ? -1 : 1, 1, 1) * value;
            }
        }


        public string Model
        {
            get
            {
                return this.modelName;
            }
            set
            {
                this.modelName = value;
                var path = this.modelName;
                if (modelName.Contains("cloud"))
                    path = "cloud";
                this.model = this.transform.Find(path).gameObject;
                this.transform.Find("cloud").gameObject.SetActive(path == "cloud");
                this.transform.Find("doug").gameObject.SetActive(path == "doug");
                this.transform.Find("invisible").gameObject.SetActive(path == "invisible");
                collider = model.GetComponent<BoxCollider>();
            }
        }
    }

    public class Platforms : BarkModule
    {
        public static readonly string DisplayName = "Platforms";
        public static GameObject platformPrefab;
        public Platform left, right, main;
        InputTracker inputL, inputR;

        void Awake()
        {
            if (!platformPrefab)
            {
                platformPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Bark Platform");
            }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                left = CreatePlatform(true);
                right = CreatePlatform(false);
                ReloadConfiguration();
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public Platform CreatePlatform(bool isLeft)
        {
            var platformObj = Instantiate(platformPrefab);
            var platform = platformObj.AddComponent<Platform>();
            platform.Initialize(isLeft);
            return platform;
        }

        public void OnActivate(InputTracker tracker)
        {
            if (!enabled) return;
            bool isLeft = (tracker == inputL);
            main = isLeft ? left : right;
            var other = !isLeft ? left : right;
            main.Activate();
            if (Sticky.Value)
            {
                Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                other.Deactivate();
            }
        }

        public void OnDeactivate(InputTracker tracker)
        {
            bool isLeft = tracker == inputL;
            var platform = isLeft ? left : right;
            platform.Deactivate();
            if (Sticky.Value && platform == main)
            {
                var rb = Player.Instance.bodyCollider.attachedRigidbody;
                rb.velocity = Player.Instance.bodyVelocityTracker.GetAverageVelocity(true, 0.15f, false);
            }
        }

        void LateUpdate()
        {
            if (Sticky.Value && main.isActive)
            {
                Player.Instance.isClimbing = true;
                Vector3 offset = main.climber.transform.position - main.transform.position;
                var rb = Player.Instance.bodyCollider.attachedRigidbody;
                rb.velocity = Vector3.zero;
                rb.useGravity = false;
                rb.MovePosition(rb.position - offset);
            }
        }

        protected override void Cleanup()
        {
            left.gameObject?.Obliterate();
            right.gameObject?.Obliterate();
            Unsub();

        }
        protected override void ReloadConfiguration()
        {
            left.Model = Model.Value;
            right.Model = Model.Value;
            left.Sticky = Sticky.Value;
            right.Sticky = Sticky.Value;

            float scale = MathExtensions.Map(Scale.Value, 0, 10, .5f, 1.5f);
            left.Scale = scale;
            right.Scale = scale;

            Unsub();
            inputL = GestureTracker.Instance.GetInputTracker(Input.Value, XRNode.LeftHand);
            inputL.OnPressed += OnActivate;
            inputL.OnReleased += OnDeactivate;

            inputR = GestureTracker.Instance.GetInputTracker(Input.Value, XRNode.RightHand);
            inputR.OnPressed += OnActivate;
            inputR.OnReleased += OnDeactivate;
        }

        void Unsub()
        {
            if (inputL != null)
            {
                inputL.OnPressed -= OnActivate;
                inputL.OnReleased -= OnDeactivate;
            }
            if (inputR != null)
            {
                inputR.OnPressed -= OnActivate;
                inputR.OnReleased -= OnDeactivate;
            }
        }

        public static ConfigEntry<bool> Sticky;
        public static ConfigEntry<int> Scale;
        public static ConfigEntry<string> Input;
        public static ConfigEntry<string> Model;
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

                Scale = Plugin.configFile.Bind(
                    section: DisplayName,
                    key: "size",
                    defaultValue: 5,
                    description: "The size of the platforms"
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

                Model = Plugin.configFile.Bind(
                    section: DisplayName,
                    key: "model",
                    defaultValue: "cloud",
                    configDescription: new ConfigDescription(
                        "Which button you press to activate the platform",
                        new AcceptableValueList<string>("cloud", "storm cloud", "doug", "invisible")
                    )
                );

            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public override string GetDisplayName()
        {
            return "Platforms";
        }

        public override string Tutorial()
        {

            return $"Press [{Input.Value}] to spawn a platform you can stand on. " +
                $"Release [{Input.Value}] to disable it.";
        }
    }

    public class NetworkedPlatformsHandler : MonoBehaviour
    {
        public GameObject platformLeft, platformRight;
        public NetworkedPlayer networkedPlayer;

        void Start()
        {
            try
            {
                networkedPlayer = this.gameObject.GetComponent<NetworkedPlayer>();
                Logging.Debug("Networked player", networkedPlayer.owner.NickName, "turned on platforms");
                platformLeft = Instantiate(Platforms.platformPrefab);
                platformRight = Instantiate(Platforms.platformPrefab);
                SetupPlatform(platformLeft);
                SetupPlatform(platformRight);
                platformLeft.name = networkedPlayer.owner.NickName + "'s Left Platform";
                platformRight.name = networkedPlayer.owner.NickName + "'s Right Platform";
                networkedPlayer.OnGripPressed += OnGripPressed;
                networkedPlayer.OnGripReleased += OnGripReleased;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void SetupPlatform(GameObject platform)
        {
            try
            {
                platform.SetActive(false);

                foreach (Transform child in platform.transform)
                {
                    if (!child.name.Contains("cloud"))
                    {
                        child.gameObject.Obliterate();
                    }
                    else
                    {
                        child.GetComponent<Collider>()?.Obliterate();
                        child.GetComponent<ParticleSystem>()?.Obliterate();
                    }
                }
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void OnGripPressed(NetworkedPlayer player, bool isLeft)
        {
            if (isLeft)
            {
                var leftHand = networkedPlayer.rig.leftHandTransform;
                platformLeft.SetActive(true);
                platformLeft.transform.position = leftHand.TransformPoint(new Vector3(-12, 18, -10) / 200f);
                platformLeft.transform.rotation = leftHand.transform.rotation * Quaternion.Euler(215, 0, -15);
                platformLeft.transform.localScale = Vector3.one * networkedPlayer.rig.scaleFactor;
            }
            else
            {
                var rightHand = networkedPlayer.rig.rightHandTransform;
                platformRight.SetActive(true);
                platformRight.transform.localPosition = rightHand.TransformPoint(new Vector3(12, 18, 10) / 200f);
                platformRight.transform.localRotation = rightHand.transform.rotation * Quaternion.Euler(-45, -25, -190);
                platformLeft.transform.localScale = Vector3.one * networkedPlayer.rig.scaleFactor;
            }
        }

        void OnGripReleased(NetworkedPlayer player, bool isLeft)
        {
            if (isLeft)
                platformLeft.SetActive(false);
            else
                platformRight.SetActive(false);
        }

        void OnDestroy()
        {
            Logging.Debug("Networked player", networkedPlayer.owner.NickName, "turned off platforms");
            platformLeft?.Obliterate();
            platformRight?.Obliterate();
            networkedPlayer.OnGripPressed -= OnGripPressed;
            networkedPlayer.OnGripReleased -= OnGripReleased;
        }
    }
}
