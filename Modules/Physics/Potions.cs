using System;
using UnityEngine;
using Bark.Gestures;
using Bark.GUI;
using Bark.Tools;
using Bark.Extensions;
using GorillaLocomotion;
using BepInEx.Configuration;
using Bark.Interaction;
using System.Collections.Generic;
using static SizeManager;
using Bark.Networking;

namespace Bark.Modules.Physics
{
    public class Potions : BarkModule
    {
        public static readonly string DisplayName = "Potions";
        private GameObject bottlePrefab, shrinkPotion, growPotion;
        private Material shrinkMaterial, growMaterial;
        private Transform holsterL, holsterR;
        private Vector3 holsterOffset = new Vector3(0.15f, -0.15f, 0.15f);
        public static SizeChanger sizeChanger;
        public static Potions Instance;
        public static bool active;

        // Networking
        public static readonly string playerSizeKey = "BarkPlayerSize";
        public static Dictionary<VRRig, SizeChanger> sizeChangers = new Dictionary<VRRig, SizeChanger>();

        void Awake()
        {
            try
            {
                Instance = this;
                NetworkPropertyHandler.Instance?.ChangeProperty(playerSizeKey, Player.Instance.scale);
                bottlePrefab = Plugin.assetBundle.LoadAsset<GameObject>("Potion Bottle");
                shrinkMaterial = Plugin.assetBundle.LoadAsset<Material>("Portal A Material");
                growMaterial = Plugin.assetBundle.LoadAsset<Material>("Portal B Material");
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        void Setup()
        {
            try
            {
                if (!bottlePrefab)
                    bottlePrefab = Plugin.assetBundle.LoadAsset<GameObject>("Potion Bottle");

                NetworkPropertyHandler.Instance?.ChangeProperty(playerSizeKey, Player.Instance.scale);
                sizeChanger = new GameObject("Bark Size Changer").AddComponent<SizeChanger>();
                sizeChanger.myType = SizeChanger.ChangerType.Static;
                sizeChanger.minScale = Player.Instance.scale;
                sizeChanger.maxScale = Player.Instance.scale;

                holsterL = new GameObject($"Holster (Left)").transform;
                shrinkPotion = Instantiate(bottlePrefab);
                SetupPotion(ref holsterL, ref shrinkPotion, true);

                holsterR = new GameObject($"Holster (Right)").transform;
                growPotion = Instantiate(bottlePrefab);
                SetupPotion(ref holsterR, ref growPotion, false);
                ReloadConfiguration();
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void SetupPotion(ref Transform holster, ref GameObject potion, bool isLeft)
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

                var sizePotion = potion.AddComponent<SizePotion>();
                sizePotion.name = isLeft ? "Bark Shrink Potion" : "Bark Grow Potion";
                sizePotion.Holster(holster);
                sizePotion.OnDrink += DrinkPotion;
                sizePotion.GetComponent<Renderer>().material = isLeft ? shrinkMaterial : growMaterial;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void DrinkPotion(SizePotion potion)
        {
            if (potion.gameObject == growPotion && !PositionValidator.Instance.isValidAndStable) return;
            float delta = potion.gameObject == shrinkPotion ? .99f : 1.01f;
            delta = Mathf.Clamp(sizeChanger.minScale * delta, .03f, 20f);
            sizeChanger.minScale = delta;
            sizeChanger.maxScale = delta;
            active = true;
        }

        float cachedSize;
        void FixedUpdate()
        {
            if (cachedSize == Player.Instance.scale) return;
            NetworkPropertyHandler.Instance.ChangeProperty(playerSizeKey, Player.Instance.scale);
            cachedSize = Player.Instance.scale;
        }

        protected override void Cleanup()
        {
            try
            {
                active = false;
                holsterL?.gameObject?.Obliterate();
                holsterR?.gameObject?.Obliterate();
                shrinkPotion?.gameObject?.Obliterate();
                growPotion?.gameObject?.Obliterate();
                sizeChanger?.gameObject.Obliterate();
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (VRRig rig in FindObjectsOfType<VRRig>())
            {
                try
                {
                    rig.transform.localScale = Vector3.one;
                    rig.scaleFactor = 1;
                }
                catch (Exception e) { Logging.Exception(e); };
            }
            foreach (SizeManager manager in FindObjectsOfType<SizeManager>())
            {
                try
                {
                    if (manager.myType != SizeManager.SizeChangerType.LocalOffline)
                    {
                        var t = manager.targetRig?.transform;
                        Logging.Debug($"Resizing {manager.name}");
                        if (!t) continue;
                        float scale = manager.ScaleFromChanger(manager.ControllingChanger(t), t);
                        t.localScale = Vector3.one * scale;
                        manager.targetRig.scaleFactor = scale;
                        Logging.Debug($"Resized {t.name} to {t.localScale}");
                        NetworkPropertyHandler.Instance?.ChangeProperty(playerSizeKey, Player.Instance.scale);
                    }
                    else
                    {
                        var player = manager.targetPlayer;
                        float scale = manager.ScaleFromChanger(
                                manager.ControllingChanger(manager.mainCameraTransform),
                                manager.mainCameraTransform
                            );
                        player.turnParent.transform.localScale = Vector3.one * scale;
                        player.scale = scale;
                    }
                }
                catch (Exception e) { Logging.Exception(e); };
            }
        }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            NetworkPropertyHandler.Instance.ChangeProperty(playerSizeKey, Player.Instance.scale);
            base.OnEnable();
            active = false;
            Setup();

        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return string.Format("- Grab the potion off of your waist with [Grip].\n" +
                "- Pop the cork with the other [Grip].\n" +
                "- Tilt the potion to drink it.\n\n" +
                "Current size: {0:0.##}x", Player.Instance.scale);
        }

        public static ConfigEntry<bool> ShowNetworkedSizes;
        protected override void ReloadConfiguration() { }

        public static void BindConfigEntries()
        {
            ShowNetworkedSizes = Plugin.configFile.Bind(
                section: DisplayName,
                key: "show networked size",
                defaultValue: true,
                description: "Whether or not to show how big other players using the Potions module are"
            );
        }
        public static void TryGetSizeChangerForRig(VRRig rig, out SizeChanger sc)
        {
            if (!rig.HasProperty(Potions.playerSizeKey))
            {
                sc = null;
                return;
            }
            if (sizeChangers.ContainsKey(rig))
            {
                sc = sizeChangers[rig];
            }
            else
            {
                sc = new GameObject("Bark Size Changer").AddComponent<SizeChanger>();
                sc.transform.SetParent(rig.transform);
                sc.myType = SizeChanger.ChangerType.Static;
                sc.minScale = rig.scaleFactor;
                sc.maxScale = rig.scaleFactor;
                sizeChangers.Add(rig, sc);
            }
            float size = rig.GetProperty<float>(Potions.playerSizeKey);
            size = Mathf.Lerp(sc.minScale, size, .75f * Time.fixedDeltaTime);
            sc.minScale = size;
            sc.maxScale = size;
        }
    }


    public class SizePotion : BarkGrabbable
    {
        public Transform holster;
        Vector3 corkOffset, corkScale;
        Cork cork;
        ParticleSystem drip;
        public Action<SizePotion> OnDrink;

        protected override void Awake()
        {
            base.Awake();
            cork = this.transform.Find("Cork").gameObject.AddComponent<Cork>();
            cork.enabled = false;
            drip = this.transform.Find("Drip").GetComponent<ParticleSystem>();
            try
            {
                drip.gameObject.GetComponent<ParticleSystemRenderer>().material =
                    this.GetComponent<Renderer>().material;
            }
            catch (Exception e) { Logging.Exception(e); }
            corkOffset = cork.transform.localPosition;
            corkScale = cork.transform.localScale;
            this.LocalPosition = new Vector3(0.55f, 0, 0.425f);
            this.LocalRotation = new Vector3(8, 0, 0);
            this.throwOnDetach = false;
        }

        bool isFlipped, wasFlipped, inRange;
        Vector3 mouthPosition, bottlePosition;
        void FixedUpdate()
        {
            try
            {
                if (IsCorked())
                {
                    wasFlipped = false;
                    return;
                }
                isFlipped = Vector3.Dot(transform.up, Vector3.down) > 0;
                if (!wasFlipped && isFlipped)
                    drip.Play();
                if (!isFlipped && wasFlipped)
                    drip.Stop();
                wasFlipped = isFlipped;


                mouthPosition = Player.Instance.headCollider.transform.TransformPoint(new Vector3(0, -.05f, .1f));
                bottlePosition = transform.position;

                float range = .15f;
                Vector3 delta = bottlePosition - mouthPosition;
                inRange = Vector3.Dot(delta, Vector3.up) > 0f && delta.magnitude < range * Player.Instance.scale;
                if (isFlipped && inRange)
                    OnDrink?.Invoke(this);
                //Logging.Debugger(
                //    "isFlipped:", Vector3.Dot(transform.up, Vector3.down),
                //    "atOrAboveHead:", Vector3.Dot(delta, Vector3.up),
                //    "Distance:", delta.magnitude,
                //    "inRange:", inRange
                //);
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        bool IsCorked()
        {
            return cork.transform.parent == this.transform;
        }

        public override void OnSelect(BarkInteractor interactor)
        {
            base.OnSelect(interactor);
            if (cork)
                cork.enabled = true;
        }

        public override void OnDeselect(BarkInteractor interactor)
        {
            base.OnDeselect(interactor);
            Holster(this.holster);
        }

        public override void OnPrimaryReleased(BarkInteractor interactor)
        {
            base.OnPrimaryReleased(interactor);
            if (IsCorked())
            {
                cork.Pop();
                cork.enabled = false;
            }
        }

        public override void OnActivate(BarkInteractor interactor)
        {
            base.OnActivate(interactor);
        }

        public void Holster(Transform holster)
        {
            drip.Stop();
            this.holster = holster;
            this.GetComponent<Rigidbody>().isKinematic = true;
            transform.SetParent(holster);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            cork.enabled = false;
            cork.rb.isKinematic = true;
            cork.transform.SetParent(this.transform);
            cork.transform.localPosition = corkOffset;
            cork.transform.localScale = corkScale;
            cork.transform.localRotation = Quaternion.identity;
            cork.shouldPlayPopSound = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            cork?.gameObject.Obliterate();
        }
    }

    public class Cork : BarkGrabbable
    {

        public Rigidbody rb;
        AudioSource popSource;
        public bool shouldPlayPopSound = true;
        protected override void Awake()
        {
            base.Awake();
            this.LocalPosition = new Vector3(0.5f, .5f, 0.425f);
            this.LocalRotation = new Vector3(8, 0, 0);
            this.throwOnDetach = true;
            rb = this.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            popSource = GetComponent<AudioSource>();
        }

        public override void OnSelect(BarkInteractor interactor)
        {
            base.OnSelect(interactor);
            if (shouldPlayPopSound)
                popSource.Play();
            shouldPlayPopSound = false;
        }

        public void Pop()
        {
            transform.SetParent(null);
            rb.isKinematic = false;
            rb.velocity = this.transform.up * 2.5f;
            popSource.Play();
        }
    }
}
