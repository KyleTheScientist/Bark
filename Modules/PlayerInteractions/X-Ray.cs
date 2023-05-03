using Bark.Extensions;
using Bark.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules.PlayerInteractions
{
    public class XRayMarker : MonoBehaviour
    {
        Material baseMaterial, material;
        VRRig rig;
        void Start()
        {
            rig = GetComponent<VRRig>();
            material = Instantiate(Plugin.assetBundle.LoadAsset<Material>("X-Ray Material"));
            Update();
        }

        public void Update()
        {
            if (!rig.mainSkin.material.name.Contains("X-Ray"))
            {
                baseMaterial = rig.mainSkin.material;
                material.color = baseMaterial.color;
                material.mainTexture = baseMaterial.mainTexture;
                material.SetTexture("_MainTex", baseMaterial.mainTexture);
                rig.mainSkin.material = material;
            }
        }

        void OnDestroy()
        {
            rig.mainSkin.material = baseMaterial;
            Logging.LogDebug($"Reset material to {baseMaterial.name}");
        }
    }

    public class XRay : BarkModule
    {
        List<XRayMarker> markers;
        void ApplyMaterial()
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                if (rig.photonView.Owner.IsLocal) continue;

                var marker = rig.gameObject.GetComponent<XRayMarker>();
                if (marker)
                    marker.Update();
                else
                    markers.Add(rig.gameObject.AddComponent<XRayMarker>());
            }
        }

        void FixedUpdate()
        {
            if (Time.frameCount % 300 == 0) ApplyMaterial();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            markers = new List<XRayMarker>();
            ApplyMaterial();
        }

        protected override void Cleanup()
        {
            foreach (var marker in markers)
                marker?.Obliterate();
        }

        public override string DisplayName()
        {
            return "X-Ray";
        }

        public override string Tutorial()
        {
            return "Effect: Allows you to see other players through walls.";
        }

    }
}
