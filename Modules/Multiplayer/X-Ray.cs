using Bark.Extensions;
using Bark.GUI;
using Bark.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules.Multiplayer
{

    public class XRay : BarkModule
    {
        public static readonly string DisplayName = "X-Ray";
        List<XRayMarker> markers;
        void ApplyMaterial()
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                try
                {
                    if (rig.PhotonView().Owner.IsLocal) continue;

                    var marker = rig.gameObject.GetComponent<XRayMarker>();
                    if (marker)
                        marker.Update();
                    else
                        markers.Add(rig.gameObject.AddComponent<XRayMarker>());
                }
                catch (Exception e)
                {
                    Logging.LogException(e);
                    Logging.LogDebug("rig is null:", rig is null);
                    Logging.LogDebug("rig?.PhotonView() is null:", rig?.PhotonView() is null);
                    Logging.LogDebug("rig?.PhotonView()?.Owner is null:", rig?.PhotonView()?.Owner is null);
                    Logging.LogDebug("rig?.gameObject is null:", rig?.gameObject is null);
                }
            }
        }

        void FixedUpdate()
        {
            if (Time.frameCount % 300 == 0) ApplyMaterial();
        }
        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            markers = new List<XRayMarker>();
            ApplyMaterial();
        }

        protected override void Cleanup()
        {
            foreach (var marker in markers)
                marker?.Obliterate();
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }
        public override string Tutorial()
        {
            return "Effect: Allows you to see other players through walls.";
        }

    }

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
}
