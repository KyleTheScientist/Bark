using System;
using UnityEngine;
using Bark.Tools;
using Bark.Interaction;
using System.Collections.Generic;
using UnityEngine.XR;
using ModestTree;

namespace Bark.Gestures
{
    public class BarkInteractor : MonoBehaviour
    {
        public static string InteractionLayerName = "TransparentFX";
        public static int InteractionLayer = LayerMask.NameToLayer(InteractionLayerName);
        public static int InteractionLayerMask = LayerMask.GetMask(InteractionLayerName);
        public List<BarkGrabbable>
            hovered = new List<BarkGrabbable>(),
            selected = new List<BarkGrabbable>();
        public InputDevice device;
        public XRNode node;
        public bool Selecting { get; protected set; }
        public bool Activating { get; protected set; }
        public bool PrimaryPressed { get; protected set; }

        public bool IsSelecting
        {
            get { return selected.Count > 0; }
        }

        protected void Awake()
        {
            try
            {
                bool isLeft = this.name.Contains("Left");
                this.gameObject.AddComponent<SphereCollider>().isTrigger = true;
                this.gameObject.layer = InteractionLayer;

                var gt = GestureTracker.Instance;
                this.device = isLeft ?
                    gt.leftController :
                    gt.rightController;
                this.node = isLeft ? XRNode.LeftHand : XRNode.RightHand;

                gt.GetInputTracker("grip", this.node).OnPressed += OnGrip;
                gt.GetInputTracker("grip", this.node).OnReleased += OnGripRelease;
                gt.GetInputTracker("trigger", this.node).OnPressed += OnTrigger;
                gt.GetInputTracker("trigger", this.node).OnReleased += OnTriggerRelease;
                gt.GetInputTracker("primary", this.node).OnPressed += OnPrimary;
                gt.GetInputTracker("primary", this.node).OnReleased += OnPrimaryRelease;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        

        public void Select(BarkGrabbable grabbable)
        {
            if (!grabbable.CanBeSelected(this)) return;

            // Prioritize the menu over regular grabbables
            if (!selected.IsEmpty())
            {
                if (grabbable == Plugin.menuController)
                    DeselectAll();
                else return;
            }

            grabbable.transform.SetParent(this.transform);

            if ((((uint)device.characteristics) & ((uint)InputDeviceCharacteristics.Right)) == ((uint)InputDeviceCharacteristics.Right))
                grabbable.transform.localPosition = grabbable.MirroredLocalPosition;
            else
                grabbable.transform.localPosition = grabbable.LocalPosition;
            grabbable.transform.localRotation = Quaternion.Euler(grabbable.LocalRotation);
            grabbable.OnSelect(this);
            selected.Add(grabbable);
        }

        public void Deselect(BarkGrabbable grabbable, bool inPlace = true)
        {
            grabbable.transform.SetParent(null);
            if(inPlace)
                selected.Remove(grabbable);
            grabbable.OnDeselect(this);
        }

        public void Hover(BarkGrabbable grabbable)
        {
            hovered.Add(grabbable);
        }

        void OnGrip(InputTracker _)
        {
            if (Selecting) return;
            Selecting = true;
            BarkGrabbable grabbed = null;
            foreach (var grabbable in hovered)
            {
                if (grabbable.CanBeSelected(this))
                {
                    grabbed = grabbable;
                    break;
                }
            }
            if (!grabbed) return;
            Select(grabbed);
        }

        void OnGripRelease(InputTracker _)
        {
            if (!Selecting) return;
            Selecting = false;
            DeselectAll();
        }

        void DeselectAll()
        {
            foreach (var grabbable in selected)
                Deselect(grabbable, inPlace: false);
            selected.RemoveAll(g => !g.selectors.Contains(this));
        }

        void OnTrigger(InputTracker _)
        {
            Activating = true;
            foreach (var grabbable in selected)
                grabbable.OnActivate(this);
        }

        void OnTriggerRelease(InputTracker _)
        {
            Activating = false;
            foreach (var grabbable in selected)
                grabbable.OnDeactivate(this);
        }

        void OnPrimary(InputTracker _)
        {
            Activating = true;
            foreach (var grabbable in selected)
                grabbable.OnPrimary(this);
        }

        void OnPrimaryRelease(InputTracker _)
        {
            Activating = false;
            foreach (var grabbable in selected)
                grabbable.OnPrimaryReleased(this);
        }
    }
}
