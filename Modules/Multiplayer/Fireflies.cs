using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Patches;
using Bark.Tools;
using GorillaLocomotion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Bark.Modules.Multiplayer
{
    public class Firefly : MonoBehaviour
    {
        public VRRig rig;
        public GameObject _object;
        public ParticleSystem particles, trail;
        public Transform leftWing, rightWing;
        public float startTime;
        public static float duration = 10f;
        public ParticleSystemRenderer particleRenderer, trailRenderer;
        Renderer modelRenderer;
        public bool seek = false;
        public Transform hand;
        void Awake()
        {
            try
            {
                rig = this.gameObject.GetComponent<VRRig>();
                _object = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Firefly")).gameObject;
                modelRenderer = _object.transform.Find("Model").GetComponent<Renderer>();
                leftWing = _object.transform.Find("Model/Wing L");
                rightWing = _object.transform.Find("Model/Wing R");
                particles = _object.GetComponent<ParticleSystem>();
                particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
                particleRenderer.material = Instantiate(particleRenderer.material);
                trail = _object.transform.Find("Trail").GetComponent<ParticleSystem>();
                trailRenderer = trail.GetComponent<ParticleSystemRenderer>();
                trailRenderer.trailMaterial = Instantiate(trailRenderer.trailMaterial);
                particles.Play();
                trail.Play();
                startTime = Time.time;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void FixedUpdate()
        {
            try
            {
                int y = Time.frameCount % 2 == 0 ? 30 : 0;
                leftWing.transform.localRotation = Quaternion.Euler(0, -y, 0);
                rightWing.transform.localRotation = Quaternion.Euler(0, y, 0);
                Transform target = rig?.transform;
                if (target != null)
                {
                    Color color = rig.mainSkin.material.color;
                    modelRenderer.materials[1].color = color;
                    //flyRenderer.material.SetColor("_EmissionColor", color);
                    particleRenderer.material.color = color;
                    //particleRenderer.material.SetColor("_EmissionColor", color);
                    trailRenderer.trailMaterial.color = color;
                    //trailRenderer.trailMaterial.SetColor("_EmissionColor", color);
                    if (seek)
                    {
                        _object.transform.position = Vector3.Slerp(
                                         _object.transform.position,
                                         target.position,
                                         (Time.time - startTime) / duration);
                        _object.transform.localScale = Vector3.Lerp(_object.transform.localScale, rig.scaleFactor * Vector3.one, .1f);
                    }
                }
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void OnDestroy()
        {
            _object?.Obliterate();
        }
    }

    public class Fireflies : BarkModule
    {
        public static readonly string DisplayName = "Fireflies";
        public static List<Firefly> fireflies = new List<Firefly>();

        bool charging = false;
        Transform hand;

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                ReloadConfiguration();
                GestureTracker.Instance.leftGrip.OnPressed += OnGrip;
                GestureTracker.Instance.rightGrip.OnPressed += OnGrip;
                GestureTracker.Instance.leftGrip.OnReleased += OnGripReleased;
                GestureTracker.Instance.rightGrip.OnReleased += OnGripReleased;
                VRRigCachePatches.OnRigCached += OnRigCached;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void OnGrip(InputTracker tracker)
        {
            foreach (var firefly in fireflies)
            {
                if (firefly.rig is null)
                {
                    firefly.Obliterate();
                }
            }
            fireflies.RemoveAll(fly => fly is null);
            bool isLeft = tracker == GestureTracker.Instance.leftGrip;
            var interactor = isLeft ? GestureTracker.Instance.leftPalmInteractor : GestureTracker.Instance.rightPalmInteractor;
            hand = interactor.transform;
            StartCoroutine(SpawnFireflies(hand, isLeft));
            charging = true;
        }

        void FixedUpdate()
        {
            if (!charging || !hand) return;
            for(int i = 0; i < fireflies.Count; i++)
            {
                float angle = (i * Mathf.PI * 2 / fireflies.Count) + Time.time;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                Vector3 offset = new Vector3(x, z, 0);
                var fly = fireflies[i]._object;
                fly.transform.position = hand.transform.TransformPoint(offset * 2);
                fly.transform.LookAt(fireflies[i].rig.transform);
            }
        }

        void OnGripReleased(InputTracker _)
        {
            StartCoroutine(ReleaseFireflies());
        }

        IEnumerator ReleaseFireflies()
        {
            charging = false;
            foreach (var firefly in fireflies)
            {
                firefly.hand = null;
            }

            foreach (var firefly in fireflies)
            {
                firefly.startTime = Time.time;
                firefly.seek = true;
                firefly._object.transform.localScale = Vector3.zero;
                firefly.particles.Play();
                firefly.trail.Play();
                Sounds.Play(Sounds.Sound.BeeSqueeze, .1f, hand == GestureTracker.Instance.leftPalmInteractor.transform); 
                yield return new WaitForSeconds(.05f);
            }
        }

        IEnumerator SpawnFireflies(Transform hand, bool isLeft)
        {
            var rigs = GorillaParent.instance.vrrigs;
            int count = rigs.Count;
            Sounds.Play(Sounds.Sound.BeeSqueeze, .1f, isLeft);
            for (int i = 0; i < count; i++)
            {
                VRRig rig = rigs[i];
                try
                {
                    if (rig != null && !rig.isOfflineVRRig)
                    {
                        var firefly = rig.gameObject.GetOrAddComponent<Firefly>();
                        firefly.particles.Stop();
                        firefly.trail.Stop();
                        firefly.particles.Clear();
                        firefly.trail.Clear();
                        firefly.rig = rig;
                        firefly.hand = hand;
                        firefly.seek = false;
                        firefly.particles.transform.localScale = Vector3.one * Player.Instance.scale;
                        firefly.particles.transform.position = hand.position;
                        if (!fireflies.Contains(firefly)) 
                            fireflies.Add(firefly);
                    }
                }
                catch (Exception e) { Logging.Exception(e); }
                yield return new WaitForFixedUpdate();
            }
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            try
            {
                if (!(fireflies is null))
                {
                    foreach (Firefly s in fireflies)
                        s?.Obliterate();
                    fireflies.Clear();
                }
                VRRigCachePatches.OnRigCached -= OnRigCached;

                if (GestureTracker.Instance)
                {
                    GestureTracker.Instance.leftGrip.OnPressed -= OnGrip;
                    GestureTracker.Instance.rightGrip.OnPressed -= OnGrip;
                    GestureTracker.Instance.leftGrip.OnReleased -= OnGripReleased;
                    GestureTracker.Instance.rightGrip.OnReleased -= OnGripReleased;
                }
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        private void OnRigCached(Player player, VRRig rig)
        {
            Firefly target = null;
            foreach (Firefly fly in fireflies)
            {
                if (fly && fly?.rig == rig)
                {
                    target = fly;
                    break;
                }
            }
            if (!target) return;
            fireflies.Remove(target);
            target.Obliterate();
        }

        //public static ConfigEntry<int> PunchForce;
        public static void BindConfigEntries()
        {
            //Logging.Debug("Binding", DisplayName, "to config");
            //PunchForce = Plugin.configFile.Bind(
            //    section: DisplayName,
            //    key: "punch force",
            //    defaultValue: 5,
            //    description: "How much force will be applied to you when you get punched"
            //);
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Press [Grip] to summon trails that will follow each player upon release";
        }
    }
}
