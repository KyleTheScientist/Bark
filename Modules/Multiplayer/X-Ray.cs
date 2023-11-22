using Bark.Extensions;
using Bark.GUI;
using Bark.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using GorillaLocomotion;

namespace Bark.Modules.Multiplayer
{

    public class XRay : BarkModule
    {
        public static readonly string DisplayName = "X-Ray";
        List<XRayMarker> markers;
        public static Dictionary<VRRig, bool> trackedStatus = new Dictionary<VRRig, bool>();
        void ApplyMaterial()
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                try
                {
                    if (!rig?.PhotonView()) continue;
                    if ((bool)rig?.PhotonView()?.Owner?.IsLocal) continue;

                    var marker = rig.gameObject.GetComponent<XRayMarker>();
                    if (!marker)
                        markers.Add(rig.gameObject.AddComponent<XRayMarker>());

                    trackedStatus[rig] = true;
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

        void Debug()
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
            {
                var chest = rig.transform.Find("rig/body/gorillachest").GetComponent<Renderer>();
                var face = rig.transform.Find("rig/body/head/gorillaface").GetComponent<Renderer>();
                Material[] materials = new Material[] { rig.mainSkin.material, chest.material, face.material };
                bool xray = false;
                foreach (var material in materials)
                    if (material.name.Contains("X-Ray"))
                        xray = true;

                if (trackedStatus.ContainsKey(rig))
                {
                    if (xray != trackedStatus[rig])
                    {
                        Logging.Debugger("Mistracked rig:",
                            "expected:", trackedStatus[rig],
                            ",", rig.mainSkin.material.name.Contains("X-Ray") ? "x-ray" : "default",
                            ",", chest.material.name.Contains("X-Ray") ? "x-ray" : "default",
                            ",", face.material.name.Contains("X-Ray") ? "x-ray" : "default"
                        );
                    }
                }
                else if (xray)
                {
                    Logging.Debugger("Untracked rig has xray:",
                        "", rig.mainSkin.material.name.Contains("X-Ray") ? "x-ray" : "default",
                        ",", chest.material.name.Contains("X-Ray") ? "x-ray" : "default",
                        ",", face.material.name.Contains("X-Ray") ? "x-ray" : "default"
                    );
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
            Patches.VRRigCachePatches.OnRigCached += OnRigCached;
            base.OnEnable();
            markers = new List<XRayMarker>();
            ApplyMaterial();
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            Patches.VRRigCachePatches.OnRigCached += OnRigCached;
            if (markers is null) return;
            foreach (var marker in markers)
            {
                marker?.Obliterate();
                trackedStatus[marker.rig] = false;
            }
        }

        void OnRigCached(Player player, VRRig rig)
        {
            try
            {
                DestroyImmediate(rig.GetComponent<XRayMarker>());
                trackedStatus[rig] = false;
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
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
        public VRRig rig;
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
                
                baseSkin = ReplaceMaterial(rig.mainSkin, skinMaterial);
                chest = rig.transform.Find("rig/body/gorillachest").GetComponent<Renderer>();
                baseChest = ReplaceMaterial(chest, chestMaterial);
                face = rig.transform.Find("rig/body/head/gorillaface").GetComponent<Renderer>();
                baseFace = ReplaceMaterial(face, faceMaterial);
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public void FixedUpdate()
        {
            if (Time.frameCount % 100 == 0)
            {
                if (!rig.mainSkin.material == skinMaterial)
                    ReplaceMaterial(rig.mainSkin, skinMaterial);
            }
        }

        //method that replaces a material with a new one and copies the mainTexture and color
        Material ReplaceMaterial(Renderer renderer, Material newMaterial)
        {

            var oldMaterial = renderer.material;
            newMaterial.color = oldMaterial.color;
            newMaterial.mainTexture = oldMaterial.mainTexture;
            newMaterial.mainTextureScale = oldMaterial.mainTextureScale;
            newMaterial.mainTextureOffset = oldMaterial.mainTextureOffset;
            renderer.material = newMaterial;
            return oldMaterial;
        }

        void OnDestroy()
        {
            string step = "";
            try
            {

                step = "Resetting face";
                face.material = baseFace;
                step = "Resetting chest";
                chest.material = baseChest;
                step = "Resetting skin";
                rig.mainSkin.material = baseSkin;
                Logging.Debug($"Reset materials successfully");
            }
            catch (Exception e)
            {
                Logging.Fatal("Failed to reset material at step:", step, e);
                Logging.Fatal("Face is null: ", face is null);
                Logging.Fatal("Chest is null: ", chest is null);
                Logging.Exception(e);
            }
        }
    }
}
