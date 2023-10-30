using System;
using UnityEngine;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using GorillaLocomotion;
using BepInEx.Configuration;
using Bark.Interaction;

namespace Bark.Modules.Teleportation
{
    public class Pearl : BarkModule
    {
        public static readonly string DisplayName = "Pearl";
        public static Pearl Instance;
        private GameObject pearlPrefab;
        ThrowablePearl pearl;

        void Awake()
        {
            try
            {
                Instance = this;
                pearlPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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
                pearl = SetupPearl(Instantiate(pearlPrefab), true);
                ReloadConfiguration();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        ThrowablePearl SetupPearl(GameObject rocketObj, bool isLeft)
        {
            try
            {
                rocketObj.name = "Bark Pearl";
                var pearl = rocketObj.AddComponent<ThrowablePearl>();
                pearl.LocalPosition = new Vector3(0.51f, 0, 0f);
                //pearl.LocalRotation = new Vector3(0, 0, -90);
                return pearl;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
            return null;
        }

        protected override void Cleanup()
        {
            try
            {
                pearl?.gameObject?.Obliterate();
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

    public class ThrowablePearl : BarkGrabbable
    {
        GestureTracker gt;

        protected override void Awake()
        {
            base.Awake();
            this.throwOnDetach = true;
            gameObject.layer = BarkInteractor.InteractionLayer;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            gt = GestureTracker.Instance;
            gt.leftGrip.OnPressed += Attach;
            gt.rightGrip.OnPressed += Attach;
        }

        void Attach(InputTracker tracker)
        {
            var parent = (tracker == gt.leftGrip ? gt.leftPalmInteractor : gt.rightPalmInteractor);
            if (!this.CanBeSelected(parent)) return;
            this.transform.parent = null;
            this.transform.localScale = Vector3.one * Player.Instance.scale * .1f;
            parent.Select(this);
        }

        //public override void OnDeselect(BarkInteractor interactor)
        //{
        //    base.OnDeselect(interactor);

        //}

        public void SetupInteraction()
        {
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
