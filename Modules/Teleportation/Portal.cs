using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using BepInEx.Configuration;
using UnityEngine.XR;
using Bark.Patches;

namespace Bark.Modules.Teleportation
{
    public class Portal : BarkModule
    {
        public static readonly string DisplayName = "Portals";
        public static GameObject launcherPrefab, portalPrefab;
        public GameObject launcher;
        public GameObject[] portals = new GameObject[2];
        AudioSource audioFire;
        ParticleSystem[] smokeSystems;
        XRNode hand;
        Material passthroughMaterialPrefab;
        Material[] portalMaterials;
        RenderTexture[] portalRenderTextures;



        void Awake()
        {
        }


        protected override void OnEnable()
        {

            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            Transform head = Player.Instance.headCollider.transform;
            try
            {
                if (!launcherPrefab)
                {
                    launcherPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Zipline Launcher");
                    portalPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Portal");
                    portalMaterials = new Material[2] {
                        Plugin.assetBundle.LoadAsset<Material>("Portal A Material"),
                        Plugin.assetBundle.LoadAsset<Material>("Portal B Material"),
                    };
                    portalRenderTextures = new RenderTexture[2] {
                        Plugin.assetBundle.LoadAsset<RenderTexture>("Portal Render Texture A"),
                        Plugin.assetBundle.LoadAsset<RenderTexture>("Portal Render Texture B"),
                    };
                    passthroughMaterialPrefab = Plugin.assetBundle.LoadAsset<Material>("Portal Passthrough Material");
                }

                launcher = Instantiate(launcherPrefab);
                audioFire = launcher.GetComponent<AudioSource>();
                launcher.transform.Find("Start Hook").gameObject.SetActive(false);
                launcher.transform.Find("End Hook").gameObject.SetActive(false);

                ReloadConfiguration();

                smokeSystems = launcher.GetComponentsInChildren<ParticleSystem>();
                foreach (var system in smokeSystems)
                    system.gameObject.SetActive(false);

                HideLauncher();

            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void Update()
        {
            UpdateCameraPortals();
        }

        void ShowLauncher()
        {
            foreach (var system in smokeSystems)
                system.gameObject.SetActive(false);
            launcher.SetActive(true);
            audioFire.enabled = false;
        }

        void HideLauncher()
        {
            launcher.SetActive(false);
            foreach (var system in smokeSystems)
                system.gameObject.SetActive(false);
            audioFire.enabled = false;
        }

        void Fire(int portal)
        {
            if (!launcher.activeSelf) return;
            audioFire.enabled = true;
            audioFire.Play();
            GestureTracker.Instance.HapticPulse(false, 1, .25f);
            foreach (var system in smokeSystems)
            {
                system.gameObject.SetActive(true);
                system.Clear();
                system.Play();
            }
            MakePortal(portal);
        }

        void ResetHooks()
        {
            if (!launcher.activeSelf) return;
            GestureTracker.Instance.HapticPulse(hand == XRNode.LeftHand);

        }

        Color[] portalColors = new Color[]
        {
            Color.red, Color.blue
        };
        void MakePortal(int portal)
        {
            try
            {
                RaycastHit hit = Raycast(launcher.transform.position, launcher.transform.forward);
                if (!hit.collider) return;

                portals[portal]?.Obliterate();
                portals[portal] = MakePortal(hit.point, hit.normal, portal);

            }
            catch (Exception e) { Logging.Exception(e); }
        }

        GameObject MakePortal(Vector3 position, Vector3 normal, int index)
        {
            GameObject portal = Instantiate(portalPrefab);
            portal.GetComponent<Renderer>().materials[1] = portalMaterials[index];
            portal.transform.position = position;
            portal.transform.LookAt(position + normal);
            portal.layer = LayerMask.GetMask("GorillaInteractable");
            portal.AddComponent<CollisionObserver>().OnTriggerEntered += (self, collider) =>
            {
                if (collider.gameObject.GetComponentInParent<Player>() ||
                    collider == GestureTracker.Instance.leftPalmInteractor ||
                    collider == GestureTracker.Instance.rightPalmInteractor)
                    OnPlayerEntered(self);
            };

            SetupPassthrough(portal, index);

            return portal;
        }

        void SetupPassthrough(GameObject inPortal, int index)
        {
            var outPortal = GetConnectedPortal(inPortal);
            Renderer inPortalRenderer = inPortal.GetComponent<Renderer>();
            if (!outPortal)
            {
                inPortalRenderer.materials[0] = portalMaterials[index];
                return;
            }

            Renderer outPortalRenderer = inPortal.GetComponent<Renderer>();
            outPortalRenderer.materials[0] = Instantiate(passthroughMaterialPrefab);
            inPortalRenderer.materials[0] = Instantiate(passthroughMaterialPrefab);
            
            inPortalRenderer.materials[0].mainTexture = portalRenderTextures[index];
            outPortalRenderer.materials[0].mainTexture = portalRenderTextures[(index + 1) % 2];

            var camIn = inPortal.GetComponentInChildren<Camera>();
            var camOut = outPortal.GetComponentInChildren<Camera>();

            camIn.targetDisplay = 5;
            camOut.targetDisplay = 5;
            camIn.targetTexture = portalRenderTextures[index];
            camOut.targetTexture = portalRenderTextures[(index + 1) % 2];
        }

        void UpdateCameraPortals()
        {
            if (!(portals[0] && portals[1])) return;
            Transform p = Camera.main.transform;
            Transform camA = portals[0].transform.GetChild(0);
            Transform camB = portals[1].transform.GetChild(0);
            Transform portalA = portals[0].transform;
            Transform portalB = portals[1].transform;

            var camIn = portalA.GetComponentInChildren<Camera>();
            var camOut = portalB.GetComponentInChildren<Camera>();
            System.Reflection.FieldInfo[] fields = typeof(Camera).GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(camIn, field.GetValue(Camera.main));
                field.SetValue(camOut, field.GetValue(Camera.main));
            }
            MovePortalCamera(portalA, portalB, p, camB);
            MovePortalCamera(portalB, portalA, p, camA);

        }

        void MovePortalCamera(Transform portalA, Transform portalB, Transform observer, Transform portalCam)
        {
            var relativePosition = portalA.InverseTransformPoint(observer.position);
            relativePosition = Vector3.Scale(relativePosition, new Vector3(-1, 1, -1));
            portalCam.position = portalB.TransformPoint(relativePosition);

            var relativeRotation = portalA.InverseTransformDirection(observer.forward);
            relativeRotation = Vector3.Scale(relativeRotation, new Vector3(-1, 1, -1));
            portalCam.forward = portalB.TransformDirection(relativeRotation);
        }

        void OnPlayerEntered(GameObject inPortal)
        {
            GameObject outPortal = GetConnectedPortal(inPortal);
            if (!outPortal) return;
            float p = Player.Instance.currentVelocity.magnitude;
            TeleportPatch.TeleportPlayer(outPortal.transform.position + (outPortal.transform.forward * 1f), false);
            Player.Instance.SetVelocity(p * outPortal.transform.forward);
        }

        GameObject GetConnectedPortal(GameObject portal)
        {
            return portal == portals[0] ? portals[1] : portals[0];
        }

        RaycastHit Raycast(Vector3 origin, Vector3 forward)
        {
            Ray ray = new Ray(origin, forward);
            RaycastHit hit;

            // Shoot a ray forward
            UnityEngine.Physics.Raycast(ray, out hit, Mathf.Infinity, Teleport.layerMask);
            return hit;
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            UnsubscribeFromEvents();
            launcher?.Obliterate();
            portals[0]?.Obliterate();
            portals[1]?.Obliterate();
        }

        public static ConfigEntry<string> LauncherHand;
        protected override void ReloadConfiguration()
        {
            UnsubscribeFromEvents();

            hand = LauncherHand.Value == "left"
                ? XRNode.LeftHand : XRNode.RightHand;

            Parent();

            InputTracker grip = GestureTracker.Instance.GetInputTracker("grip", hand);
            InputTracker primary = GestureTracker.Instance.GetInputTracker("primary", hand);
            InputTracker secondary = GestureTracker.Instance.GetInputTracker("secondary", hand);

            grip.OnPressed += ShowLauncher;
            grip.OnReleased += HideLauncher;
            primary.OnPressed += FireA;
            secondary.OnPressed += FireB;
        }

        void FireA() { Fire(0); }
        void FireB() { Fire(1); }

        void Parent()
        {
            Transform parent = GestureTracker.Instance.rightHand.transform;
            float x = -1;
            if (hand == XRNode.LeftHand)
            {
                parent = GestureTracker.Instance.leftHand.transform;
                x = 1;
            }

            launcher.transform.SetParent(parent, true);
            launcher.transform.localPosition = new Vector3(0.4782f * x, 0.1f, 0.4f);
            launcher.transform.localRotation = Quaternion.Euler(20, 0, 0);
        }

        void UnsubscribeFromEvents()
        {
            InputTracker grip = GestureTracker.Instance.GetInputTracker("grip", hand);
            InputTracker primary = GestureTracker.Instance.GetInputTracker("primary", hand);
            InputTracker secondary = GestureTracker.Instance.GetInputTracker("secondary", hand);
            grip.OnPressed -= ShowLauncher;
            grip.OnReleased -= HideLauncher;
            primary.OnPressed -= FireA;
            secondary.OnPressed -= FireB;
        }

        //public static void BindConfigEntries()
        //{
        //    LauncherHand = Plugin.configFile.Bind(
        //        section: DisplayName,
        //        key: "launcher hand",
        //        defaultValue: "right",
        //        configDescription: new ConfigDescription(
        //            "Which hand holds the launcher",
        //            new AcceptableValueList<string>("left", "right")
        //        )
        //    );
        //}

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            string h = LauncherHand.Value.Substring(0, 1).ToUpper() + LauncherHand.Value.Substring(1);
            return $"Hold [{h} Grip] to summon the zipline cannon. Press and release [{h} Trigger] to fire a zipline.";
        }
    }
}
