using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Tools;

namespace Bark.Gestures
{
    public class BarkInteractor : XRDirectInteractor
    {
        public static XRInteractionManager manager;
        public static string InteractionLayerName = "TransparentFX";
        public static int InteractionLayer = LayerMask.NameToLayer(InteractionLayerName);
        public static int InteractionLayerMask = LayerMask.GetMask(InteractionLayerName);

        protected override void Awake()
        {
            base.Awake();
            try
            {
                if (!manager)
                    manager = new GameObject("InteractionManager").AddComponent<XRInteractionManager>();
                this.gameObject.AddComponent<SphereCollider>().isTrigger = true; ;
                this.gameObject.layer = InteractionLayer;
                this.interactionManager = manager;
                this.enableInteractions = true;
                this.xrController = GetController(this.name.Contains("Left"));
            }
            catch (Exception e) { Logging.LogException(e); }
        }
        public XRController GetController(bool isLeft)
        {
            foreach (var controller in FindObjectsOfType<XRController>())
            {
                if (isLeft && controller.name.ToLowerInvariant().Contains("left"))
                {
                    return controller;
                }
                if (!isLeft && controller.name.ToLowerInvariant().Contains("right"))
                {
                    return controller;
                }
            }
            return null;
        }
        
        public void RemoveFromValidTargets(XRBaseInteractable interactable)
        {
            if (validTargets.Contains(interactable))
                this.validTargets.Remove(interactable);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (manager)
                Destroy(manager);
        }
    }
}
