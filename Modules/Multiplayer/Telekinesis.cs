using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using BepInEx.Configuration;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules.Multiplayer
{
    public class Telekinesis : BarkModule
    {
        public static readonly string DisplayName = "Telekinesis";
        public static Telekinesis Instance;
        private List<TKMarker> markers = new List<TKMarker>();
        public SphereCollider tkCollider;
        TKMarker sithLord;
        void Awake() { Instance = this; }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                ReloadConfiguration();
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "BarkTKDetector";
                sphere.transform.SetParent(Player.Instance.bodyCollider.transform, false);
                sphere.layer = BarkInteractor.InteractionLayer;
                sphere.GetComponent<MeshRenderer>().enabled = false;

                tkCollider = sphere.GetComponent<SphereCollider>();
                tkCollider.isTrigger = true;

                DistributeMidichlorians();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }
        Joint joint;
        void FixedUpdate()
        {
            if (Time.frameCount % 300 == 0)
                DistributeMidichlorians();

            if (!sithLord) TryGetSithLord();

            if (sithLord)
            {
                var rb = Player.Instance.bodyCollider.attachedRigidbody;
                if (!sithLord.IsGripping())
                {
                    sithLord = null;
                    rb.velocity *= 3f;
                    return;
                }

                Vector3 end = sithLord.controllingHand.position + sithLord.controllingHand.up * 3 * sithLord.rig.scaleFactor;
                Vector3 direction = end - Player.Instance.bodyCollider.transform.position;
                rb.AddForce(direction * 3, ForceMode.Impulse);
                float dampingThreshold = direction.magnitude * 6;
                if (rb.velocity.magnitude > dampingThreshold)
                    rb.velocity = rb.velocity.normalized * dampingThreshold;
            }

        }

        void TryGetSithLord()
        {
            foreach (var tk in markers)
            {
                if (tk.IsGripping() && tk.PointingAtMe())
                {
                    sithLord = tk;
                    break;
                }
            }
        }

        void DistributeMidichlorians()
        {

            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                try
                {
                    if (rig.PhotonView().Owner.IsLocal ||
                        rig.gameObject.GetComponent<TKMarker>()) continue;

                    markers.Add(rig.gameObject.AddComponent<TKMarker>());
                }
                catch (Exception e)
                {
                    Logging.Exception(e);
                }
            }
        }

        protected override void Cleanup()
        {
            foreach (TKMarker m in markers)
            {
                m?.Obliterate();
            }
            tkCollider?.gameObject.Obliterate();
            joint?.Obliterate();
            sithLord = null;
            markers.Clear();
            tkCollider = null;
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: If another player points their index finger at you, they can pick you up with telekinesis.";
        }

        public class TKMarker : MonoBehaviour
        {
            public VRRig rig;
            bool grippingRight, grippingLeft;
            public Transform leftHand, rightHand, controllingHand;
            public Rigidbody controllingBody;
            DebugRay dr;

            public static int count;
            int uuid;
            void Awake()
            {
                this.rig = GetComponent<VRRig>();
                this.uuid = count++;
                leftHand = SetupHand("L");
                rightHand = SetupHand("R");
                dr = new GameObject($"{uuid} (Debug Ray)").AddComponent<DebugRay>();
            }

            public Transform SetupHand(string hand)
            {
                var handTransform = transform.Find(
                    string.Format(GestureTracker.palmPath, hand).Substring(1)
                );
                var rb = handTransform.gameObject.AddComponent<Rigidbody>();

                rb.isKinematic = true;
                return handTransform;
            }

            public bool IsGripping()
            {
                grippingRight =
                    rig.rightIndex.calcT < .5f &&
                    rig.rightMiddle.calcT > .5f;
                //rig.rightThumb.calcT > .5f;

                grippingLeft =
                    rig.leftIndex.calcT < .5f &&
                    rig.leftMiddle.calcT > .5f;
                //rig.leftThumb.calcT > .5f;
                return grippingRight || grippingLeft;
            }

            public bool PointingAtMe()
            {
                try
                {
                    if (!(grippingRight || grippingLeft)) return false;
                    Transform hand = grippingRight ? rightHand : leftHand;
                    controllingHand = hand;
                    if (!hand) return false;
                    controllingBody = hand?.GetComponent<Rigidbody>();
                    if (!controllingBody) return false;
                    RaycastHit hit;
                    Ray ray = new Ray(hand.position, hand.up);
                    var collider = Instance.tkCollider;
                    UnityEngine.Physics.SphereCast(ray, .2f * Player.Instance.scale, out hit, collider.gameObject.layer);
                    return hit.collider == collider;
                }
                catch (Exception e) { Logging.Exception(e); }
                return false;
            }

            void OnDestroy()
            {
                dr?.gameObject?.Obliterate();
                leftHand?.GetComponent<Rigidbody>()?.Obliterate();
                rightHand?.GetComponent<Rigidbody>()?.Obliterate();
            }
        }
    }
}
