using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using GorillaLocomotion;
using BepInEx.Configuration;

namespace Bark.Modules.Movement
{
    public class GrapplingHooks : BarkModule
    {
        public static readonly string DisplayName = "Grappling Hooks";
        private GameObject bananaGunPrefab, bananaGunL, bananaGunR;
        private Transform holsterL, holsterR;
        private Vector3 holsterOffset = new Vector3(0.15f, -0.15f, 0.15f);

        void Awake()
        {
            try
            {
                bananaGunPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Banana Gun");
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        void Setup()
        {
            if (!bananaGunPrefab)
                bananaGunPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Banana Gun");

            holsterL = new GameObject($"Holster (Left)").transform;
            bananaGunL = Instantiate(bananaGunPrefab);
            SetupBananaGun(ref holsterL, ref bananaGunL, true);

            holsterR = new GameObject($"Holster (Right)").transform;
            bananaGunR = Instantiate(bananaGunPrefab);
            SetupBananaGun(ref holsterR, ref bananaGunR, false);
            ReloadConfiguration();
        }

        void SetupBananaGun(ref Transform holster, ref GameObject bananaGun, bool isLeft)
        {
            try
            {
                holster.SetParent(Player.Instance.bodyCollider.transform, false);
                var offset = new Vector3(
                    holsterOffset.x * (isLeft ? -1 : 1),
                    holsterOffset.y,
                    holsterOffset.z
                );
                holster.localPosition = offset;

                var gun = bananaGun.AddComponent<BananaGun>();
                gun.name = isLeft ? "Banana Grapple Left" : "Banana Grapple Right";
                gun.Holster(holster);
                gun.SetupInteraction();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }
        protected override void Cleanup()
        {
            try
            {
                if (bananaGunL)
                    GestureTracker.Instance?.leftPalmInteractor?
                    .RemoveFromValidTargets(bananaGunL.GetComponent<BananaGun>());
                if (bananaGunR)
                    GestureTracker.Instance?.rightPalmInteractor?
                        .RemoveFromValidTargets(bananaGunR.GetComponent<BananaGun>());

                holsterL?.gameObject?.Obliterate();
                holsterR?.gameObject?.Obliterate();
                bananaGunL?.gameObject?.Obliterate();
                bananaGunR?.gameObject?.Obliterate();
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            Setup();
        }

        public static ConfigEntry<int> Spring, Steering, MaxLength;
        public static ConfigEntry<string> RopeType;
        protected override void ReloadConfiguration()
        {
            var guns = new BananaGun[] { bananaGunL?.GetComponent<BananaGun>(), bananaGunR?.GetComponent<BananaGun>() };
            foreach (var gun in guns)
            {
                if (!gun) continue;
                gun.pullForce = Spring.Value * 2;
                gun.ropeType = RopeType.Value == "elastic" ? BananaGun.RopeType.ELASTIC : BananaGun.RopeType.STATIC;
                gun.steerForce = Steering.Value / 2f;
                gun.maxLength = MaxLength.Value * 5;
                Logging.LogDebug(
                    "gun.pullForce:", gun.pullForce,
                    "gun.ropeType:", gun.ropeType,
                    "gun.steerForce:", gun.steerForce,
                    "gun.maxLength:", gun.maxLength
                );
            }
        }

        public static void BindConfigEntries()
        {
            RopeType = Plugin.configFile.Bind(
                section: DisplayName,
                key: "rope type",
                defaultValue: "elastic",
                configDescription: new ConfigDescription(
                    "Whether the rope should pull you to the anchor point or not",
                    new AcceptableValueList<string>("elastic", "rope")
                )
            );

            Spring = Plugin.configFile.Bind(
                section: DisplayName,
                key: "springiness",
                defaultValue: 5,
                description: "If ropes are elastic, this is how springy the ropes are"
            );

            Steering = Plugin.configFile.Bind(
                section: DisplayName,
                key: "steering",
                defaultValue: 5,
                description: "How much influence you have over your velocity"
            );

            MaxLength = Plugin.configFile.Bind(
                section: DisplayName,
                key: "max length",
                defaultValue: 5,
                description: "The maximum distance that the grappling hook can reach"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Grab the grappling hook off of your waist with [Grip]. " +
                "Then fire with [Trigger]. " +
                "You can steer in the air by pointing the guns where you want to go.";
        }
    }

    public class BananaGun : XRGrabInteractable
    {
        public enum RopeType
        {
            ELASTIC, STATIC
        }

        public Transform holster;
        private GameObject openModel, closedModel;
        private LineRenderer rope, laser;
        private bool isGrappling;
        private float baseLaserWidth, baseRopeWidth;
        Vector3 hitPosition,
            baseModelOffsetClosed,
            baseModelOffsetOpen,
            modelOffsetLeft = new Vector3(.025f, 0, .025f),
            modelOffsetRight = new Vector3(-.025f, 0, .025f);
        private XRBaseInteractor interactor;


        public RopeType ropeType;
        public float
            pullForce = 10f,
            steerForce = 5f,
            maxLength = 30f;

        protected override void Awake()
        {
            base.Awake();
            openModel = transform.Find("Banana Gun Open").gameObject;
            closedModel = transform.Find("Banana Gun Closed").gameObject;
            baseModelOffsetClosed = closedModel.transform.localPosition;
            baseModelOffsetOpen = openModel.transform.localPosition;
            rope = openModel.GetComponentInChildren<LineRenderer>();
            rope.useWorldSpace = false;
            baseRopeWidth = rope.startWidth;
            laser = closedModel.GetComponentInChildren<LineRenderer>();
            laser.useWorldSpace = false;
            baseLaserWidth = laser.startWidth;
        }

        public void Holster(Transform holster)
        {
            Close();
            this.holster = holster;
            transform.SetParent(holster, false);
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;
            if (laser)
                laser.enabled = false;
        }

        SpringJoint joint;
        bool activated;
        protected override void OnActivate(XRBaseInteractor interactor)
        {
            base.OnActivate(interactor);
            activated = true;
        }

        protected override void OnDeactivate(XRBaseInteractor interactor)
        {
            base.OnDeactivate(interactor);
            activated = false;
            Close();
        }

        void StartSwing()
        {
            RaycastHit hit;
            Ray ray = new Ray(rope.transform.position, transform.forward);
            UnityEngine.Physics.SphereCast(ray, .5f, out hit, maxLength, Teleport.layerMask);
            if (!hit.transform) return;

            isGrappling = true;
            Open();
            rope.SetPosition(0, rope.transform.position);
            rope.SetPosition(1, hit.point);
            hitPosition = hit.point;

            joint = Player.Instance.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hitPosition;

            float distanceFromPoint = Vector3.Distance(rope.transform.position, hitPosition);

            // the distance grapple will try to keep from grapple point. 
            switch (ropeType)
            {
                case RopeType.ELASTIC:
                    joint.maxDistance = 0.8f;
                    joint.minDistance = 0.25f;
                    joint.spring = pullForce;
                    joint.damper = 7f;
                    joint.massScale = 4.5f;
                    break;
                case RopeType.STATIC:
                    joint.maxDistance = distanceFromPoint;
                    joint.minDistance = distanceFromPoint ;
                    joint.spring = pullForce * 2;
                    joint.damper = 100f;
                    joint.massScale = 4.5f;
                    break;
            }
        }


        void FixedUpdate()
        {
            if (isSelected)
            {
                transform.localScale = Vector3.one * Player.Instance.scale;
                transform.position = selectingInteractor.transform.position;
            }

            if (isSelected && !isGrappling && activated) { StartSwing(); return; }
            if (isGrappling)
            {
                var rigidBody = Player.Instance.bodyCollider.attachedRigidbody;
                rigidBody.velocity +=
                    transform.forward *
                    steerForce * Time.fixedDeltaTime * Player.Instance.scale;
            }
        }

        void UpdateLineRenderer()
        {
            if (!isGrappling && isSelected)
            {
                RaycastHit hit;
                Ray ray = new Ray(rope.transform.position, transform.forward);
                UnityEngine.Physics.SphereCast(ray, .5f, out hit, maxLength, Teleport.layerMask);
                if (!hit.transform)
                {
                    laser.enabled = false;
                    return;
                }
                Vector3
                    start = Vector3.zero,
                    end = laser.transform.InverseTransformPoint(hit.point);

                laser.enabled = true;
                laser.SetPosition(0, start);
                laser.SetPosition(1, end);
                laser.startWidth = baseLaserWidth * Player.Instance.scale;
                laser.endWidth = baseLaserWidth * Player.Instance.scale;
            }
            else if (isGrappling)
            {
                Vector3
                    start = Vector3.zero,
                    end = rope.transform.InverseTransformPoint(hitPosition);
                rope.SetPosition(0, start);
                rope.SetPosition(1, end);
                rope.startWidth = baseRopeWidth * Player.Instance.scale;
                rope.endWidth = baseRopeWidth * Player.Instance.scale;
            }
        }

        protected override void OnSelectEntered(XRBaseInteractor interactor)
        {
            base.OnSelectEntered(interactor);
            this.interactor = interactor;
            if (interactor == GestureTracker.Instance.leftPalmInteractor)
            {
                closedModel.transform.localPosition += modelOffsetLeft;
                openModel.transform.localPosition += modelOffsetLeft;
            }
            else if (interactor == GestureTracker.Instance.rightPalmInteractor)
            {
                closedModel.transform.localPosition += modelOffsetRight;
                openModel.transform.localPosition += modelOffsetRight;
            }
        }

        protected override void OnSelectExited(XRBaseInteractor interactor)
        {
            base.OnSelectEntered(interactor);
            this.interactor = null;

            closedModel.transform.localPosition = baseModelOffsetClosed;
            openModel.transform.localPosition = baseModelOffsetOpen;
            laser.enabled = false;
            Holster(holster);

        }

        public void SetupInteraction()
        {
            gravityOnDetach = false;
            movementType = MovementType.Instantaneous;
            retainTransformParent = true;
            throwOnDetach = false;
            gameObject.layer = BarkInteractor.InteractionLayer;
            if (openModel)
                openModel.layer = BarkInteractor.InteractionLayer;
            if (closedModel)
                closedModel.layer = BarkInteractor.InteractionLayer;
            interactionLayerMask = BarkInteractor.InteractionLayerMask;
            interactionManager = BarkInteractor.manager;
        }

        void Open()
        {
            openModel?.SetActive(true);
            closedModel?.SetActive(false);
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(96, false, 0.05f);
        }

        void Close()
        {
            openModel?.SetActive(false);
            closedModel?.SetActive(true);
            isGrappling = false;
            joint?.Obliterate();
        }

        private void OnEnable()
        {
            Application.onBeforeRender += UpdateLineRenderer;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= UpdateLineRenderer;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            joint?.Obliterate();
        }
    }
}
