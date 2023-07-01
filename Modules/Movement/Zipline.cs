using Bark.Tools;
using System;
using UnityEngine;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using GorillaLocomotion.Gameplay;
using GorillaLocomotion.Climbing;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine.XR;
using GorillaLocomotion;

namespace Bark.Modules.Movement
{
    public class Zipline : BarkModule
    {
        public static readonly string DisplayName = "Zipline";
        public static GameObject launcherPrefab, ziplinePrefab;
        public GameObject launcher;
        public GameObject[] ziplines = new GameObject[0];

        public static AudioClip ziplineAudioLoop;
        Transform climbOffsetHelper;
        GorillaClimbable climbable;
        AudioSource audioSlide, audioFire;
        ParticleSystem[] smokeSystems;
        GameObject gunStartHook, gunEndHook;
        XRNode hand;
        GorillaZiplineSettings settings;

        void Awake()
        {
            Logging.Debug("Zipline Awake");
            settings = ScriptableObject.CreateInstance<GorillaZiplineSettings>();
            settings.gravityMulti = 0;
            settings.maxFriction = 0;
            settings.friction = 0;
            settings.maxSpeed *= 2;
        }


        protected override void OnEnable()
        {

            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                if (!launcherPrefab)
                {
                    launcherPrefab = Plugin.assetBundle.LoadAsset<GameObject>("Zipline Launcher");
                    ziplinePrefab = Plugin.assetBundle.LoadAsset<GameObject>("Zipline Rope");
                    ziplineAudioLoop = Plugin.assetBundle.LoadAsset<AudioClip>("Zipline Loop");
                }

                launcher = Instantiate(launcherPrefab);
                audioFire = launcher.GetComponent<AudioSource>();
                gunStartHook = launcher.transform.Find("Start Hook").gameObject;
                gunEndHook = launcher.transform.Find("End Hook").gameObject;

                ReloadConfiguration();

                smokeSystems = launcher.GetComponentsInChildren<ParticleSystem>();
                foreach (var system in smokeSystems)
                    system.gameObject.SetActive(false);

                HideLauncher();

            }
            catch (Exception e) { Logging.Exception(e); }
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

        void Fire()
        {
            if(!launcher.activeSelf) return;
            audioFire.enabled = true;
            audioFire.Play();
            
            GestureTracker.Instance.HapticPulse(hand == XRNode.LeftHand, 1, .25f);
            foreach (var system in smokeSystems)
            {
                system.gameObject.SetActive(true);
                system.Clear();
                system.Play();
            }

            ziplines[nextZipline]?.Obliterate();
            ziplines[nextZipline] = MakeZipline();
            nextZipline = MathExtensions.Wrap(nextZipline + 1, 0, ziplines.Length - 1);
            gunStartHook.SetActive(false);
            gunEndHook.SetActive(false);
        }


        int nextZipline;
        void ResetHooks()
        {
            if(!launcher.activeSelf) return;
            GestureTracker.Instance.HapticPulse(hand == XRNode.LeftHand);
            gunStartHook.SetActive(true);
            gunEndHook.SetActive(true);
        }

        GameObject MakeZipline()
        {
            try
            {
                GameObject zipline = Instantiate(ziplinePrefab);
                // Figure out where the ends of the rope will be
                Vector3[] endpoints = GetEndpoints(gunStartHook.transform.position, gunStartHook.transform.up);
                Vector3 start = endpoints[0];
                Vector3 end = endpoints[1];
                zipline.transform.position = start;

                Transform startHook = zipline.transform.Find("Start Hook");
                Transform endHook = zipline.transform.Find("End Hook");

                startHook.transform.position = start;
                endHook.transform.position = end;
                startHook.localScale *= Player.Instance.scale;
                endHook.localScale *= Player.Instance.scale;
                startHook.rotation = gunStartHook.transform.rotation;
                endHook.rotation = gunEndHook.transform.rotation;

                LineRenderer ropeRenderer = zipline.GetComponent<LineRenderer>();
                ropeRenderer.positionCount = 2;
                ropeRenderer.SetPosition(0, startHook.GetChild(0).position);
                ropeRenderer.SetPosition(1, endHook.GetChild(0).position);
                ropeRenderer.enabled = true;
                ropeRenderer.startWidth = 0.05f * Player.Instance.scale;
                ropeRenderer.endWidth = 0.05f * Player.Instance.scale;

                // Set up the segment objects
                MakeSlideHelper(zipline.transform);
                Transform segments = MakeSegments(start, end);
                segments.parent = zipline.transform;
                segments.localPosition = Vector3.zero;
                // Create the spline which dictates the path you follow
                BezierSpline spline = zipline.AddComponent<BezierSpline>();
                spline.Reset();
                Traverse.Create(spline).Field("points").SetValue(
                     SplinePoints(zipline.transform, start, end)
                );
                //This thing does something important, I don't know what.
                climbOffsetHelper = new GameObject("Climb Offset Helper").transform;
                //Time to put it all together! Create the zipline controller
                GorillaZipline gorillaZipline = zipline.AddComponent<GorillaZipline>();
                //Assign everything to the zipline controller
                climbOffsetHelper.SetParent(zipline.transform, false);
                Traverse traverse = Traverse.Create(gorillaZipline);
                traverse.Field("spline").SetValue(spline);
                traverse.Field("segmentsRoot").SetValue(segments);
                traverse.Field("slideHelper").SetValue(climbable);
                traverse.Field("audioSlide").SetValue(audioSlide);
                traverse.Field("climbOffsetHelper").SetValue(climbOffsetHelper);
                traverse.Field("settings").SetValue(settings);
                float length = (end - start).magnitude;
                traverse.Field("ziplineDistance").SetValue(length);
                traverse.Field("segmentDistance").SetValue(length);

                return zipline;
            }
            catch (Exception e) { Logging.Exception(e); }
            return null;
        }

        Vector3[] SplinePoints(Transform parent, Vector3 start, Vector3 end)
        {
            return new Vector3[]
            {
                parent.InverseTransformPoint(start),
                parent.InverseTransformPoint(start + (end - start) / 4f),
                parent.InverseTransformPoint(end - (end - start) / 4f),
                parent.InverseTransformPoint(end)
            };
        }

        Vector3[] GetEndpoints(Vector3 origin, Vector3 forward)
        {

            Vector3 start, end;
            Ray ray = new Ray(origin, forward);
            RaycastHit hit;

            // Shoot a ray forward
            UnityEngine.Physics.Raycast(ray, out hit, Mathf.Infinity, Teleport.layerMask);
            if (!hit.collider) return null; //if it hits nothing, return null
            end = hit.point;

            // Shoot a ray backward
            ray.direction *= -1;
            UnityEngine.Physics.Raycast(ray, out hit, Mathf.Infinity, Teleport.layerMask);
            if (!hit.collider) return null; //if it hits nothing, return null
            start = hit.point;

            return new Vector3[] { start, end };
        }

        void MakeSlideHelper(Transform parent)
        {
            GameObject slideHelper = new GameObject("SlideHelper");
            slideHelper.transform.SetParent(parent, false);
            slideHelper.AddComponent<GorillaSurfaceOverride>().overrideIndex = 89;
            climbable = slideHelper.AddComponent<GorillaClimbable>();
            climbable.snapX = true;
            climbable.snapY = true;
            climbable.snapZ = true;

            audioSlide = slideHelper.AddComponent<AudioSource>(); // add an audio clip to this somehow
            audioSlide.clip = ziplineAudioLoop;
        }

        Transform MakeSegments(Vector3 start, Vector3 end)
        {
            float distance = (end - start).magnitude;
            Transform segments = new GameObject("Segments").transform;
            segments.position = start;

            //for (int i = 0; i < distance; i++)
            //{
            //    Vector3 position = Vector3.Lerp(start, end, i / distance);
            //    GameObject segment = MakeSegment(position, start, end);
            //    segment.transform.SetParent(segments);
            //}

            Vector3 position = Vector3.Lerp(start, end, 0.5f);
            GameObject segment = MakeSegment(position, start, end);
            segment.transform.SetParent(segments);

            return segments;
        }

        GameObject MakeSegment(Vector3 position, Vector3 start, Vector3 end)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.transform.position = position;
            segment.AddComponent<GorillaClimbableRef>().climb = climbable;
            segment.GetComponent<BoxCollider>().isTrigger = true;
            segment.layer = LayerMask.NameToLayer("GorillaInteractable");
            float distance = (end - start).magnitude;
            segment.transform.localScale = new Vector3(0.05f * Player.Instance.scale, 0.05f * Player.Instance.scale, distance);
            segment.transform.LookAt(end, Vector3.up);
            segment.GetComponent<MeshRenderer>().enabled = false;

            return segment;
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            UnsubscribeFromEvents();
            foreach (var zipline in ziplines)
                zipline?.Obliterate();
            launcher?.Obliterate();
            gunStartHook?.gameObject?.Obliterate();
            gunEndHook?.gameObject?.Obliterate();
        }

        public static ConfigEntry<int> MaxZiplines;
        public static ConfigEntry<string> LauncherHand;
        public static ConfigEntry<int> GravityMultiplier;
        protected override void ReloadConfiguration()
        {
            settings.gravityMulti = GravityMultiplier.Value / 5f;
            ResizeArray(MaxZiplines.Value);
            
            UnsubscribeFromEvents();
            
            hand = LauncherHand.Value == "left"
                ? XRNode.LeftHand : XRNode.RightHand;

            Parent();

            InputTracker grip = GestureTracker.Instance.GetInputTracker("grip", hand);
            InputTracker trigger = GestureTracker.Instance.GetInputTracker("trigger", hand);

            grip.OnPressed += ShowLauncher;
            grip.OnReleased += HideLauncher;
            trigger.OnPressed += ResetHooks;
            trigger.OnReleased += Fire;
        }

        public void ResizeArray(int newLength)
        {
            if (newLength < 0)
            {
                Logging.LogWarning("Cannot resize array to a negative length.");
                return;
            }

            // Check if the new length is smaller than the current length
            if (newLength < ziplines.Length)
                for (int i = newLength; i < ziplines.Length; i++)
                    ziplines[i]?.Obliterate();

            if (nextZipline >= ziplines.Length)
                nextZipline = 0;

            // Resize the array
            Array.Resize(ref ziplines, newLength);
        }

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
            launcher.transform.localRotation = Quaternion.Euler(20, 0, 180);
            launcher.transform.localScale = Vector3.one * 18;
        }

        void UnsubscribeFromEvents()
        {
            InputTracker grip = GestureTracker.Instance.GetInputTracker("grip", hand);
            InputTracker trigger = GestureTracker.Instance.GetInputTracker("trigger", hand);
            grip.OnPressed -= ShowLauncher;
            grip.OnReleased -= HideLauncher;
            trigger.OnPressed -= ResetHooks;
            trigger.OnReleased -= Fire;
        }

        public static void BindConfigEntries()
        {
            MaxZiplines = Plugin.configFile.Bind(
                section: DisplayName,
                key: "max ziplines",
                defaultValue: 3,
                description: "Maximum number of ziplines that can exist at one time"
            );

            LauncherHand = Plugin.configFile.Bind(
                section: DisplayName,
                key: "launcher hand",
                defaultValue: "right",
                configDescription: new ConfigDescription(
                    "Which hand holds the launcher",
                    new AcceptableValueList<string>("left", "right")
                )
            );

            GravityMultiplier = Plugin.configFile.Bind(
                section: DisplayName,
                key: "gravity multiplier",
                defaultValue: 5,
                description: "Gravity multiplier while on the zipline"
            );
        }

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
