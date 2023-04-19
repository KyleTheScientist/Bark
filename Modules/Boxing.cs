using GorillaLocomotion;
using Bark.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Bark.Modules
{

    public class BoxingMarker : MonoBehaviour { }

    public class Boxing : BarkModule
    {
        public float forceMultiplier = 100f;
        private PunchTracker tracker;
        private Collider punchCollider;
        private List<GameObject> gloves = new List<GameObject>();
        private List<BoxingMarker> markers = new List<BoxingMarker>();

        public class PunchTracker
        {
            public Collider collider;
            public Vector3 lastPos;
            public int punchFrame = 0;
        }

        void CreateGloves()
        {
            try
            {
                foreach (var rig in GorillaParent.instance.vrrigs)
                {
                    if (rig.photonView.Owner.IsLocal ||
                        rig.gameObject.GetComponent<BoxingMarker>()) continue;

                    markers.Add(rig.gameObject.AddComponent<BoxingMarker>());
                    gloves.Add(CreateGlove(rig.leftHandTransform));
                    gloves.Add(CreateGlove(rig.rightHandTransform, false));
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        private GameObject CreateGlove(Transform parent, bool isLeft = true)
        {
            var glove = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Boxing Glove"));
            string side = isLeft ? "Left" : "Right";
            glove.name = $"Boxing Glove ({side})";
            glove.transform.SetParent(parent, false);
            float x = isLeft ? 1 : -1;
            glove.transform.localScale = new Vector3(x, 1, 1);
            glove.layer = 4;
            foreach (Transform child in glove.transform)
                child.gameObject.layer = 4;
            return glove;
        }

        void FixedUpdate()
        {
            if (Time.frameCount % 300 == 0) CreateGloves();

            if (!(tracker is null) && Time.frameCount - tracker.punchFrame > 5)
            {
                DoPunch();
            }

        }

        private void DoPunch()
        {
            Vector3 force = (tracker.collider.transform.position - tracker.lastPos) * forceMultiplier;
            if (force.magnitude > 20) force = force.normalized * 20;
            Player.Instance.bodyCollider.attachedRigidbody.velocity += force;
            tracker.punchFrame = Time.frameCount;
        }

        private void RegisterTracker(Collider collider)
        {
            tracker = new PunchTracker()
            {
                collider = collider,
                lastPos = collider.transform.position + Vector3.zero
            };
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "MonkeMenuPunchDetector";
                capsule.transform.SetParent(Player.Instance.bodyCollider.transform, false);
                capsule.layer = 4;
                capsule.GetComponent<MeshRenderer>().enabled = false;

                punchCollider = capsule.GetComponent<Collider>();
                punchCollider.isTrigger = true;

                var observer = capsule.AddComponent<CollisionObserver>();
                observer.OnTriggerEntered += (obj, collider) =>
                {
                    if (collider.name != "MM Glove" || collider == tracker?.collider) return;
                    RegisterTracker(collider);
                };

                observer.OnTriggerExited += (obj, collider) =>
                {
                    Logging.LogDebug(collider.name);
                    if (collider.name != "MM Glove") return;
                    if (collider == tracker.collider) 
                        tracker = null;
                };

                CreateGloves();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        void Cleanup()
        {
            punchCollider?.gameObject?.Obliterate();
            foreach (GameObject g in gloves)
            {
                g?.Obliterate();
            }
            foreach (BoxingMarker m in markers)
            {
                m?.Obliterate();
            }
        }

        public override string DisplayName()
        {
            return "Boxing";
        }

        public override string Tutorial()
        {
            return "Effect: Other players can punch you around.";
        }
    }
}
