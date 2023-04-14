using Bark.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules
{
    public class XRayMarker : MonoBehaviour
    {
        Material baseMaterial, material;
        VRRig rig;
        void Start()
        {
            this.rig = this.GetComponent<VRRig>();
            this.material = Instantiate(Plugin.assetBundle.LoadAsset<Material>("X-Ray Material"));
            this.Update();
        }

        public void Update()
        {
            if (!rig.mainSkin.material.name.Contains("X-Ray")) {
                this.baseMaterial = rig.mainSkin.material;
                this.material.color = this.baseMaterial.color;
                this.material.mainTexture = this.baseMaterial.mainTexture;
                this.material.SetTexture("_MainTex", this.baseMaterial.mainTexture);
                rig.mainSkin.material = material;
            }
        }

        void OnDestroy()
        {
            rig.mainSkin.material = this.baseMaterial;
            Logging.Log($"Reset material to {this.baseMaterial.name}");
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
            foreach (var marker in markers)
                Destroy(marker);
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
