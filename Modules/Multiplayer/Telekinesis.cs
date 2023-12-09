using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
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
        ParticleSystem playerParticles, sithlordHandParticles;
        AudioSource sfx;
        TKMarker sithLord;
        void Awake() { Instance = this; }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                ReloadConfiguration();
                var prefab = Plugin.assetBundle.LoadAsset<GameObject>("TK Hitbox");
                var hitbox = Instantiate(prefab);
                hitbox.name = "Bark TK Hitbox";
                hitbox.transform.SetParent(Player.Instance.bodyCollider.transform, false);
                hitbox.layer = BarkInteractor.InteractionLayer;
                tkCollider = hitbox.GetComponent<SphereCollider>();
                tkCollider.isTrigger = true;
                playerParticles = hitbox.GetComponent<ParticleSystem>();
                playerParticles.Stop();
                playerParticles.Clear();
                sfx = hitbox.GetComponent<AudioSource>();

                var sithlordEffect = Instantiate(prefab);
                sithlordEffect.name = "Bark Sithlord Particles";
                sithlordEffect.transform.SetParent(Player.Instance.bodyCollider.transform, false);
                sithlordEffect.layer = BarkInteractor.InteractionLayer;
                sithlordHandParticles = sithlordEffect.GetComponent<ParticleSystem>();
                var shape = sithlordHandParticles.shape;
                shape.radius = .2f;
                shape.position = Vector3.zero;
                Destroy(sithlordEffect.GetComponent<SphereCollider>());
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
                    sfx.Stop();
                    sithlordHandParticles.Stop();
                    sithlordHandParticles.Clear();
                    playerParticles.Stop();
                    playerParticles.Clear();
                    rb.velocity = Player.Instance.bodyVelocityTracker.GetAverageVelocity(true, 0.15f, false) * 2;
                    return;
                }

                Vector3 end = sithLord.controllingHand.position + sithLord.controllingHand.up * 3 * sithLord.rig.scaleFactor;
                Vector3 direction = end - Player.Instance.bodyCollider.transform.position;
                rb.AddForce(direction * 10, ForceMode.Impulse);
                float dampingThreshold = direction.magnitude * 10;
                //if (rb.velocity.magnitude > dampingThreshold)
                //if(direction.magnitude < 1)
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, .1f);
            }

        }

        void TryGetSithLord()
        {
            foreach (var tk in markers)
            {
                try
                {
                    if (tk && tk.IsGripping() && tk.PointingAtMe())
                    {
                        sithLord = tk;
                        playerParticles.Play();
                        sithlordHandParticles.transform.SetParent(tk.controllingHand);
                        sithlordHandParticles.transform.localPosition = Vector3.zero;
                        sithlordHandParticles.Play();
                        sfx.Play();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Exception(e);
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
            tkCollider?.gameObject?.Obliterate();
            sithlordHandParticles?.gameObject?.Obliterate();
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
                    Logging.Debug("DOING THE THING WITH THE COLLIDER");
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
