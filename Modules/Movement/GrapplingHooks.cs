using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Gestures;
using GorillaLocomotion;
using Bark.Tools;
using Bark.Extensions;

namespace Bark.Modules.Movement
{
    public class GrapplingHooks : BarkModule
    {
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
                Logging.LogDebug("Cleaning up...");

                GestureTracker.Instance.leftPalmInteractor
                    .RemoveFromValidTargets(bananaGunL.GetComponent<BananaGun>());
                GestureTracker.Instance.rightPalmInteractor
                    .RemoveFromValidTargets(bananaGunR.GetComponent<BananaGun>());

                holsterL?.Obliterate();
                holsterR?.Obliterate();
                bananaGunL?.Obliterate();
                bananaGunR?.Obliterate();
                Logging.LogDebug("Cleaned up successfully.");
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Setup();
        }

        public override string DisplayName()
        {
            return "Grappling Hooks";
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


        private float
            pullForce = 10f,
            adjustForce = 5f,
            maxLength = 30f;

        protected override void Awake()
        {
            base.Awake();
            GestureTracker.Instance.rightPalmInteractor.onSelectEntered.AddListener((interactable) =>
            {
                Logging.LogDebug("Selected", interactable.name);
            });

            openModel = transform.Find("Banana Gun Open").gameObject;
            closedModel = transform.Find("Banana Gun Closed").gameObject;
            baseModelOffsetClosed = closedModel.transform.localPosition;
            baseModelOffsetOpen = openModel.transform.localPosition;
            rope = openModel.GetComponentInChildren<LineRenderer>();
            baseRopeWidth = rope.startWidth;
            laser = closedModel.GetComponentInChildren<LineRenderer>();
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

        protected override void OnActivate(XRBaseInteractor interactor)
        {
            base.OnActivate(interactor);
            RaycastHit hit;
            Ray ray = new Ray(rope.transform.position, transform.forward);
            UnityEngine.Physics.Raycast(ray, out hit, maxLength, Teleport.layerMask);
            if (!hit.transform) return;

            Open();
            rope.SetPosition(0, rope.transform.position);
            rope.SetPosition(1, hit.point);
            hitPosition = hit.point;
            isGrappling = true;
        }

        protected override void OnDeactivate(XRBaseInteractor interactor)
        {
            base.OnDeactivate(interactor);
            Close();
        }

        void FixedUpdate()
        {
            if (isSelected)
            {
                transform.localScale = Vector3.one * Player.Instance.scale;
                transform.position = selectingInteractor.transform.position;
            }

            if (!isGrappling && isSelected)
            {
                RaycastHit hit;
                Ray ray = new Ray(rope.transform.position, transform.forward);
                UnityEngine.Physics.Raycast(ray, out hit, maxLength, Teleport.layerMask);
                if (!hit.transform)
                {
                    laser.enabled = false;
                    return;
                }
                laser.enabled = true;
                laser.SetPosition(0, rope.transform.position);
                laser.SetPosition(1, hit.point);
                laser.startWidth = baseLaserWidth * Player.Instance.scale;
                laser.endWidth = baseLaserWidth * Player.Instance.scale;
            }
            else if (isGrappling)
            {
                rope.SetPosition(0, rope.transform.position);
                rope.SetPosition(1, hitPosition);
                rope.startWidth = baseRopeWidth * Player.Instance.scale;
                rope.endWidth = baseRopeWidth * Player.Instance.scale;

                var collider = Player.Instance.bodyCollider;
                collider.attachedRigidbody.velocity +=
                    (hitPosition - collider.transform.position).normalized *
                    pullForce * Time.fixedDeltaTime * Player.Instance.scale;
                collider.attachedRigidbody.velocity +=
                    transform.forward *
                    adjustForce * Time.fixedDeltaTime * Player.Instance.scale;
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
            gameObject.layer = 4;
            if (openModel)
                openModel.layer = 4;
            if (closedModel)
                closedModel.layer = 4;
            interactionLayerMask = LayerMask.GetMask("Water");
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
