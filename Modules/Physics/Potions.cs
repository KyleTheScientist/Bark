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
using Bark.Networking;
using HarmonyLib;

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
        public static Traverse sizeChangerTraverse, minScale, maxScale;
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
                Patches.VRRigCachePatches.OnRigCached += OnRigCached;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void OnRigCached(Player player, VRRig rig)
        {
            try
            {
                rig.transform.localScale = Vector3.one;
                rig.scaleFactor = 1;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        void Setup()
        {
            try
            {
                if (!bottlePrefab)
                    bottlePrefab = Plugin.assetBundle.LoadAsset<GameObject>("Potion Bottle");

                NetworkPropertyHandler.Instance?.ChangeProperty(playerSizeKey, Player.Instance.scale);
                sizeChanger = new GameObject("Bark Size Changer").AddComponent<SizeChanger>();
                sizeChangerTraverse = Traverse.Create(sizeChanger);
                minScale = sizeChangerTraverse.Field("minScale");
                maxScale = sizeChangerTraverse.Field("maxScale");
                sizeChangerTraverse.Field("myType").SetValue(SizeChanger.ChangerType.Static);
                sizeChangerTraverse.Field("staticEasing").SetValue(.5f);
                minScale.SetValue(Player.Instance.scale);
                maxScale.SetValue(Player.Instance.scale);

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
                holster.localScale = Vector3.one;
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
            bool shrink = potion.gameObject == shrinkPotion;
            if (!shrink && !PositionValidator.Instance.isValidAndStable) return;
            float delta = shrink ? .99f : 1.01f;
            delta = Mathf.Clamp(sizeChanger.MinScale * delta, .03f, 20f);
            if(delta < 1)
                potion.gulp.pitch = MathExtensions.Map(Player.Instance.scale, 0, 1, 1.5f, 1);
            else
                potion.gulp.pitch = MathExtensions.Map(Player.Instance.scale, 1, 20, 1, .5f);
            minScale.SetValue(delta);
            maxScale.SetValue(delta);
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
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
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
                Traverse managerTraverse = Traverse.Create(manager);
                Traverse scaleFromChanger = managerTraverse.Method("ScaleFromChanger");
                Traverse controllingChanger = managerTraverse.Method("ControllingChanger");
                try
                {
                    if (manager.myType != SizeManager.SizeChangerType.LocalOffline)
                    {
                        var t = manager.targetRig?.transform;
                        if (!t) continue;
                        float scale = scaleFromChanger.GetValue<float>(controllingChanger.GetValue<SizeChanger>(t), t);
                        t.localScale = Vector3.one * scale;
                        manager.targetRig.scaleFactor = scale;
                        NetworkPropertyHandler.Instance?.ChangeProperty(playerSizeKey, Player.Instance.scale);
                    }
                    else
                    {
                        var t = manager.mainCameraTransform;
                        var player = manager.targetPlayer;
                        float scale = scaleFromChanger.GetValue<float>(controllingChanger.GetValue<SizeChanger>(t), t);
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
            float size = rig.GetProperty<float>(Potions.playerSizeKey);
            if (!rig.HasProperty(Potions.playerSizeKey))
            {
                sc = null;
                return;
            }
            if (sizeChangers.ContainsKey(rig))
            {
                sc = sizeChangers[rig];
                var sizeChangerTraverse = Traverse.Create(sc);
                var minScale = sizeChangerTraverse.Field("minScale");
                var maxScale = sizeChangerTraverse.Field("maxScale");

                size = Mathf.Lerp(sc.MinScale, size, .75f * Time.fixedDeltaTime);
                minScale.SetValue(size);
                maxScale.SetValue(size);
            }
            else
            {
                size = Mathf.Lerp(rig.scaleFactor, size, .75f * Time.fixedDeltaTime);
                sc = CreateSizeChanger(size);
                sizeChangers.Add(rig, sc);
            }
        }

        public static SizeChanger CreateSizeChanger(float scale)
        {
            var sizeChanger = new GameObject("Bark Size Changer").AddComponent<SizeChanger>();
            var sizeChangerTraverse = Traverse.Create(sizeChanger);
            var minScale = sizeChangerTraverse.Field("minScale");
            var maxScale = sizeChangerTraverse.Field("maxScale");
            sizeChangerTraverse.Field("myType").SetValue(SizeChanger.ChangerType.Static);
            sizeChangerTraverse.Field("staticEasing").SetValue(.5f);
            minScale.SetValue(scale);
            maxScale.SetValue(scale);
            return sizeChanger;
        }
    }


    public class SizePotion : BarkGrabbable
    {
        public Transform holster;
        Vector3 corkOffset, corkScale;
        Cork cork;
        ParticleSystem drip;
        public AudioSource gulp;
        public Action<SizePotion> OnDrink;

        protected override void Awake()
        {
            try
            {
                base.Awake();
                gulp = this.GetComponent<AudioSource>();
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
                this.OnSelectExit += (_, __) =>
                {
                    gulp.Stop();
                };
            }
            catch (Exception e) { Logging.Exception(e); }
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
                {
                    if(!gulp.isPlaying)
                        gulp.Play();
                    OnDrink?.Invoke(this);
                }
                else
                {
                    if(gulp.isPlaying)
                        gulp.Stop();
                }
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
            transform.localScale = Vector3.one;

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
