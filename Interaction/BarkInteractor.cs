using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Tools;

namespace Bark.Gestures
{
    public class BarkInteractor : XRDirectInteractor
    {
        public static XRInteractionManager manager;
        
        protected override void Awake()
        {
            base.Awake();
            try
            {
                if (!manager)
                    manager = new GameObject("InteractionManager").AddComponent<XRInteractionManager>();
                this.gameObject.AddComponent<SphereCollider>().isTrigger = true; ;
                this.gameObject.layer = LayerMask.NameToLayer("Water");
                this.interactionManager = manager;
                this.enableInteractions = true;
                this.xrController = GetController(this.name.Contains("Left"));
            }
            catch (Exception ex) { Logging.Log(ex.Message); Logging.Log(ex.StackTrace); return; }
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (manager != null)
                Destroy(manager);
        }
    }
}
