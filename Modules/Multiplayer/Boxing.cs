using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Networking;
using Bark.Tools;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using NetworkPlayer = Photon.Realtime.Player;
using Player = GorillaLocomotion.Player;
namespace Bark.Modules.Multiplayer
{
    public class BoxingGlove : MonoBehaviour
    {
        public VRRig rig;
        public AudioSource punchSound;
        public GorillaVelocityEstimator velocity;
        public static int uuid;
        
        void Start()
        {
            punchSound = GetComponent<AudioSource>();
            velocity = this.gameObject.AddComponent<GorillaVelocityEstimator>();
        }
    }

    public class Boxing : BarkModule
    {
        public static readonly string DisplayName = "Boxing";
        public float forceMultiplier = 50;
        private Collider punchCollider;
        private List<BoxingGlove> gloves = new List<BoxingGlove>();
        private List<VRRig> glovedRigs = new List<VRRig>();

        private float lastPunch;

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                ReloadConfiguration();
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "BarkPunchDetector";
                capsule.transform.SetParent(Player.Instance.bodyCollider.transform, false);
                capsule.layer = BarkInteractor.InteractionLayer;
                capsule.GetComponent<MeshRenderer>().enabled = false;

                punchCollider = capsule.GetComponent<Collider>();
                punchCollider.isTrigger = true;
                punchCollider.transform.localScale = new Vector3(.5f, .35f, .5f);
                punchCollider.transform.localPosition += new Vector3(0, .3f, 0);

                var observer = capsule.AddComponent<CollisionObserver>();
                observer.OnTriggerEntered += (obj, collider) =>
                {
                    if (collider.GetComponentInParent<BoxingGlove>() is BoxingGlove glove)
                    {
                        DoPunch(glove);
                    }
                };
                NetworkPropertyHandler.Instance.OnPlayerLeft += OnPlayerLeft;
                NetworkPropertyHandler.Instance.OnPlayerJoined += OnPlayerJoined;
                CreateGloves();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        protected override void Cleanup()
        {
            punchCollider?.gameObject?.Obliterate();
            glovedRigs.Clear();
            if (NetworkPropertyHandler.Instance is NetworkPropertyHandler nph)
            {
                nph.OnPlayerLeft -= OnPlayerLeft;
                nph.OnPlayerJoined -= OnPlayerJoined;
            }
            if (!(gloves is null))
            {
                foreach (BoxingGlove g in gloves)
                    g?.gameObject.Obliterate();
                gloves.Clear();
            }
        }

        void OnPlayerLeft(NetworkPlayer player)
        {
            Logging.Debug(player.NickName, "left. Destroying gloves.");

            foreach (BoxingGlove g in gloves)
            {
                if (g && g?.rig?.PhotonView()?.Owner == player)
                {
                    g?.gameObject.Obliterate();
                    Logging.Debug($"Destroyed {player.NickName}'s gloves.");
                }
            }
            gloves.RemoveAll(g => g is null);
        }

        Queue<NetworkPlayer> gloveQueue = new Queue<NetworkPlayer>();
        void OnPlayerJoined(NetworkPlayer player)
        {
            Logging.Debug(player.NickName, "joined. Giving them gloves.");
            gloveQueue.Enqueue(player);
            Invoke(nameof(ClearQueue), 1f);
        }

        void ClearQueue()
        {
            Logging.Debug("Clearing queue");
            while (gloveQueue.Count > 0)
            {
                NetworkPlayer player = gloveQueue.Dequeue();
                Logging.Debug($"Giving gloves to {player.NickName}");
                foreach (var rig in GorillaParent.instance.vrrigs)
                {
                    Logging.Debug($"    {rig?.PhotonView()?.Owner?.NickName} == {player?.NickName}: {rig?.PhotonView()?.Owner?.UserId == player.UserId}");
                    if (rig?.PhotonView() && rig.PhotonView().Owner == player)
                    {
                        GiveGlovesTo(rig);
                    }
                }
            }
        }

        void CreateGloves()
        {

            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                try
                {
                    if (rig.PhotonView().Owner.IsLocal || glovedRigs.Contains(rig)) continue;
                    GiveGlovesTo(rig);
                }
                catch (Exception e)
                {
                    Logging.Exception(e);
                }
            }

        }

        void GiveGlovesTo(VRRig rig)
        {
            glovedRigs.Add(rig);
            var lefty = CreateGlove(rig.leftHandTransform, true);
            lefty.rig = rig;
            gloves.Add(lefty);
            var righty = CreateGlove(rig.rightHandTransform, false);
            righty.rig = rig;
            gloves.Add(righty);
            Logging.Debug("Gave gloves to", rig.PhotonView().Owner.NickName);

        }

        private BoxingGlove CreateGlove(Transform parent, bool isLeft = true)
        {
            var glove = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Boxing Glove"));
            string side = isLeft ? "Left" : "Right";
            glove.name = $"Boxing Glove ({side})";
            glove.transform.SetParent(parent, false);
            float x = isLeft ? 1 : -1;
            glove.transform.localScale = new Vector3(x, 1, 1);
            glove.layer = BarkInteractor.InteractionLayer;
            foreach (Transform child in glove.transform)
                child.gameObject.layer = BarkInteractor.InteractionLayer;
            return glove.AddComponent<BoxingGlove>();
        }

        void FixedUpdate()
        {
            //if (Time.frameCount % 300 == 0) CreateGloves();
        }

        private void DoPunch(BoxingGlove glove)
        {
            if (Time.time - lastPunch < 1) return;
            Vector3 force = glove.velocity.linearVelocity;
            Logging.Debug("Raw Force", force.magnitude);
            if (force.magnitude < .5f * Player.Instance.scale) return;
            if (force.magnitude > 5)
                force.Normalize();
            force *= forceMultiplier;
            Logging.Debug("Force multiplier", forceMultiplier);
            Logging.Debug("Actual Force", force.magnitude);
            Player.Instance.bodyCollider.attachedRigidbody.velocity += force;
            lastPunch = Time.time;
            GestureTracker.Instance.HapticPulse(false);
            GestureTracker.Instance.HapticPulse(true);
            glove.punchSound.pitch = UnityEngine.Random.Range(.8f, 1.2f);
            glove.punchSound.Play();

        }

        protected override void ReloadConfiguration()
        {
            forceMultiplier = (PunchForce.Value);
        }

        public static ConfigEntry<int> PunchForce;
        public static void BindConfigEntries()
        {
            Logging.Debug("Binding", DisplayName, "to config");
            PunchForce = Plugin.configFile.Bind(
                section: DisplayName,
                key: "punch force",
                defaultValue: 5,
                description: "How much force will be applied to you when you get punched"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Other players can punch you around.";
        }
    }
}
