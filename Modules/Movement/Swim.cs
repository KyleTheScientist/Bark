using Bark.Extensions;
using Bark.GUI;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using HarmonyLib;
using UnityEngine;

namespace Bark.Modules
{
    public class Swim : BarkModule
    {
        public static readonly string DisplayName = "Swim";
        public GameObject waterVolume;
        public WaterParameters settings;

        void Awake()
        {
            settings = ScriptableObject.CreateInstance<WaterParameters>();
            settings.playSplashEffect = false;
        }

        void LateUpdate()
        {
            //waterVolume.transform.position = Player.Instance.bodyCollider.transform.position;
        }

        Traverse volumeTraverse;
        WaterOverlappingCollider collider;
        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            waterVolume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            waterVolume.GetComponent<Collider>().isTrigger = true;
            waterVolume.transform.localScale *= 2;
            waterVolume.transform.position = Player.Instance.bodyCollider.transform.position;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(waterVolume.transform, false);
            plane.transform.localPosition = Vector3.up;
            //waterVolume.GetComponent<Renderer>().enabled = false;
            waterVolume.layer = LayerMask.NameToLayer("Water");
            
            var volume = waterVolume.AddComponent<WaterVolume>();
            volumeTraverse = Traverse.Create(volume);
            volumeTraverse.Field("waterParams").SetValue(settings);
            volume.surfacePlane = plane.transform;
            collider = new WaterOverlappingCollider() { collider = Player.Instance.headCollider };
            volumeTraverse.Method("OnWaterSurfaceEnter", collider);
            ReloadConfiguration();
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            volumeTraverse?.Method("OnWaterSurfaceExit", collider, Time.time);
            waterVolume?.Obliterate();
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }
        public override string Tutorial()
        {
            return "Effect: Surrounds you with invisible water.";
        }

    }
}
