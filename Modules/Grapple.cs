using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Gestures;
using GorillaLocomotion;
using Bark.Tools;
using UnityEngine.InputSystem.HID;

namespace Bark.Modules
{
    public class Grapple : BarkModule
    {
        private GameObject bananaGunPrefab, bananaGunL, bananaGunR;
        private Transform holsterL, holsterR;
        private Vector3 holsterOffset = new Vector3(0.15f, -0.15f, 0.15f);

        void Awake()
        {
            try
            {
                Application.logMessageReceived += (logString, stackTrace, type) =>
                {
                    //Logging.LogFatal(logString, stackTrace);
                };
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


        void Cleanup()
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
            } catch (Exception e) { Logging.LogException(e); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Setup();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Cleanup();
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
        bool isGrappling;
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

            this.openModel = transform.Find("Banana Gun Open").gameObject;
            this.closedModel = transform.Find("Banana Gun Closed").gameObject;
            this.baseModelOffsetClosed = closedModel.transform.localPosition;
            this.baseModelOffsetOpen = openModel.transform.localPosition;
            this.rope = openModel.GetComponentInChildren<LineRenderer>();
            this.laser = closedModel.GetComponentInChildren<LineRenderer>();
        }

        public void Holster(Transform holster)
        {
            Close();
            this.holster = holster;
            this.transform.SetParent(holster, false);
            this.transform.localPosition = new Vector3(0, 0, 0);
            this.transform.localRotation = Quaternion.identity;
            laser.enabled = false;
        }

        protected override void OnActivate(XRBaseInteractor interactor)
        {
            base.OnActivate(interactor);
            RaycastHit hit;
            Ray ray = new Ray(rope.transform.position, this.transform.forward);
            Physics.Raycast(ray, out hit, maxLength, Teleport.layerMask);
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
            if (!isGrappling) return;
            var collider = Player.Instance.bodyCollider;
            collider.attachedRigidbody.velocity +=
                (hitPosition - collider.transform.position).normalized *
                pullForce * Time.fixedDeltaTime;
            collider.attachedRigidbody.velocity +=
                (transform.forward) *
                adjustForce * Time.fixedDeltaTime;
        }

        void Update()
        {
            if (!isGrappling && isSelected)
            {
                RaycastHit hit;
                Ray ray = new Ray(rope.transform.position, this.transform.forward);
                Physics.Raycast(ray, out hit, maxLength, Teleport.layerMask);
                if (!hit.transform)
                {
                    laser.enabled = false;
                    return;
                }
                laser.enabled = true;
                laser.SetPosition(0, rope.transform.position);
                laser.SetPosition(1, hit.point);
            }
            else
            {
                rope.SetPosition(0, rope.transform.position);
                rope.SetPosition(1, hitPosition);
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
            this.gravityOnDetach = false;
            this.movementType = XRBaseInteractable.MovementType.Instantaneous;
            this.retainTransformParent = true;
            this.throwOnDetach = false;
            this.gameObject.layer = 4;
            this.openModel.layer = 4;
            this.closedModel.layer = 4;
            this.interactionLayerMask = LayerMask.GetMask("Water");
            this.interactionManager = BarkInteractor.manager;
        }

        void Open()
        {
            openModel.SetActive(true);
            closedModel.SetActive(false);
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(96, false, 0.05f);
        }

        void Close()
        {
            openModel.SetActive(false);
            closedModel.SetActive(true);
            isGrappling = false;
        }

        protected override void OnDestroy()
        {
            Logging.LogDebug("Destroying...");
            base.OnDestroy();
            Logging.LogDebug("Destroy Successful");
        }
    }
}
