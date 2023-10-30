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
                    Logging.Exception(e);
                    Logging.Debug("rig is null:", rig is null);
                    Logging.Debug("rig?.PhotonView() is null:", rig?.PhotonView() is null);
                    Logging.Debug("rig?.PhotonView()?.Owner is null:", rig?.PhotonView()?.Owner is null);
                    Logging.Debug("rig?.gameObject is null:", rig?.gameObject is null);
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
            if (!MenuController.Instance.Built) return;
            if (markers is null) return;
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
        Material 
            baseSkin, baseFace, baseChest, 
            skinMaterial, faceMaterial, chestMaterial;
        VRRig rig;
        Renderer face, chest;

        void Start()
        {
            try
            {
                rig = GetComponent<VRRig>();
                var xray = Plugin.assetBundle.LoadAsset<Material>("X-Ray Material");
                skinMaterial = Instantiate(xray);
                skinMaterial.renderQueue = 5000;
                faceMaterial = Instantiate(xray);
                faceMaterial.renderQueue = 5000;
                chestMaterial = Instantiate(xray);
                chestMaterial.renderQueue = 5000;
                Update();
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public void Update()
        {
            if (!rig.mainSkin.material.name.Contains("X-Ray"))
            {
                baseSkin = ReplaceMaterial(rig.mainSkin, skinMaterial);
                chest = rig.transform.Find("rig/body/gorillachest").GetComponent<Renderer>();
                baseChest = ReplaceMaterial(chest, chestMaterial);
                face = rig.transform.Find("rig/body/head/gorillaface").GetComponent<Renderer>();
                baseFace = ReplaceMaterial(face, faceMaterial);
            }
        }

        //method that replaces a material with a new one and copies the mainTexture and color
        Material ReplaceMaterial(Renderer renderer, Material newMaterial)
        {
            
            var oldMaterial = renderer.material;
            newMaterial.color = oldMaterial.color;
            Logging.Debug("Texture name:", oldMaterial.mainTexture?.name);
            newMaterial.mainTexture = oldMaterial.mainTexture;
            newMaterial.mainTextureScale = oldMaterial.mainTextureScale;
            newMaterial.mainTextureOffset = oldMaterial.mainTextureOffset;
            newMaterial.SetTexture("_BaseMap", oldMaterial.mainTexture);
            renderer.material = newMaterial;
            return oldMaterial;
        }

        void OnDestroy()
        {
            rig.mainSkin.material = baseSkin;
            face.material = baseFace;
            chest.material = baseChest;
            Logging.Debug($"Reset material to {baseSkin.name}");
        }
    }
}
