using System;
using UnityEngine;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using GorillaLocomotion;
using BepInEx.Configuration;
using Bark.Interaction;
using Random = UnityEngine.Random;

namespace Bark.Modules.Movement
{
    public class Rockets : BarkModule
    {
        public static readonly string DisplayName = "Rockets";
        public static Rockets Instance;
        private GameObject rocketPrefab;
        Rocket rocketL, rocketR;

        void Awake()
        {
            try
            {
                Instance = this;
                rocketPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Rocket");
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        void Setup()
        {
            try
            {
                if (!rocketPrefab)
                    rocketPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Rocket");

                rocketL = SetupRocket(Instantiate(rocketPrefab), true);
                rocketR = SetupRocket(Instantiate(rocketPrefab), false);
                ReloadConfiguration();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        Rocket SetupRocket(GameObject rocketObj, bool isLeft)
        {
            try
            {
                rocketObj.name = isLeft ? "Bark Rocket Left" : "Bark Rocket Right";
                var rocket = rocketObj.AddComponent<Rocket>().Init(isLeft);
                rocket.LocalPosition = new Vector3(0.51f, -3, 0f);
                rocket.LocalRotation = new Vector3(0, 0, -90);
                return rocket;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
            return null;
        }

        public Vector3 AddedVelocity()
        {
            return rocketL.force + rocketR.force;
        }

        protected override void Cleanup()
        {
            try
            {
                rocketL?.gameObject?.Obliterate();
                rocketR?.gameObject?.Obliterate();
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            Setup();
        }

        public static ConfigEntry<int> Power;
        protected override void ReloadConfiguration()
        {
            var guns = new Rocket[] { rocketL?.GetComponent<Rocket>(), rocketR?.GetComponent<Rocket>() };
            foreach (var gun in guns)
            {
                if (!gun) continue;
                gun.power = Power.Value * 2f;
            }
        }

        public static void BindConfigEntries()
        {
            Power = Plugin.configFile.Bind(
                section: DisplayName,
                key: "power",
                defaultValue: 5,
                description: "The power of each rocket"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return $"Hold either [Grip] to summon a rocket.";

        }
    }

    public class Rocket : BarkGrabbable
    {
        public float power = 5f;
        public Vector3 force { get; private set; }
        bool isLeft;
        GestureTracker gt;
        Rigidbody rb;
        AudioSource exhaustSound;

        protected override void Awake()
        {
            base.Awake();
            this.rb = GetComponent<Rigidbody>();
            this.exhaustSound = GetComponent<AudioSource>();
            this.exhaustSound.Stop();
        }

        public Rocket Init(bool isLeft)
        {
            this.isLeft = isLeft;
            gt = GestureTracker.Instance;

            if(isLeft)
                gt.leftGrip.OnPressed += Attach;
            else
                gt.rightGrip.OnPressed += Attach;
            return this;
        }

        void Attach(InputTracker _)
        {
            var parent = (isLeft ? gt.leftPalmInteractor : gt.rightPalmInteractor);
            if (!this.CanBeSelected(parent)) return;
            this.transform.parent = null;
            this.transform.localScale = Vector3.one * Player.Instance.scale;
            parent.Select(this);
            exhaustSound.time = Random.Range(0, exhaustSound.clip.length);
            exhaustSound.Play();
        }

        void FixedUpdate()
        {
            Player player = Player.Instance;
            force = this.transform.forward * this.power * Time.fixedDeltaTime * Player.Instance.scale;
            if (Selected)
                player.AddForce(force);
            else
            {
                rb.velocity += force * 10;
                force = Vector3.zero;
                transform.Rotate(Random.insideUnitSphere);
            }
            this.exhaustSound.volume = Mathf.Lerp(.5f, 0, Vector3.Distance(
                player.headCollider.transform.position, 
                transform.position
            ) / 20f);
        }

        public override void OnDeselect(BarkInteractor interactor)
        {
            base.OnDeselect(interactor);
            rb.velocity = Player.Instance.currentVelocity;
        }

        public void SetupInteraction()
        {
            this.throwOnDetach = true;
            gameObject.layer = BarkInteractor.InteractionLayer;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!gt) return;
            gt.leftGrip.OnPressed -= Attach;
            gt.rightGrip.OnPressed -= Attach;
        }
    }
}
