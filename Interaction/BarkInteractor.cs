using System;
using UnityEngine;
using Bark.Tools;
using Bark.Interaction;
using System.Collections.Generic;
using UnityEngine.XR;


namespace Bark.Gestures
{
    public class BarkInteractor : MonoBehaviour
    {
        public static string InteractionLayerName = "TransparentFX";
        public static int InteractionLayer = LayerMask.NameToLayer(InteractionLayerName);
        public static int InteractionLayerMask = LayerMask.GetMask(InteractionLayerName);
        public List<BarkInteractable>
            hovered = new List<BarkInteractable>(),
            selected = new List<BarkInteractable>();
        public InputDevice device;
        public XRNode node;
        public bool IsLeft { get; protected set; }
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
                IsLeft = this.name.Contains("Left");
                this.gameObject.AddComponent<SphereCollider>().isTrigger = true;
                this.gameObject.layer = InteractionLayer;

                var gt = GestureTracker.Instance;
                this.device = IsLeft ?
                    gt.leftController :
                    gt.rightController;
                this.node = IsLeft ? XRNode.LeftHand : XRNode.RightHand;

                gt.GetInputTracker("grip", this.node).OnPressed += OnGrip;
                gt.GetInputTracker("grip", this.node).OnReleased += OnGripRelease;
                gt.GetInputTracker("trigger", this.node).OnPressed += OnTrigger;
                gt.GetInputTracker("trigger", this.node).OnReleased += OnTriggerRelease;
                gt.GetInputTracker("primary", this.node).OnPressed += OnPrimary;
                gt.GetInputTracker("primary", this.node).OnReleased += OnPrimaryRelease;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public void Select(BarkInteractable interactable)
        {
            try
            {
                if (!interactable.CanBeSelected(this)) return;
                // Prioritize 
                if (selected.Count > 0)
                {
                    DeselectAll(interactable.priority);
                    if (selected.Count > 0)
                        return;
                }
                interactable.OnSelect(this);
                selected.Add(interactable);
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public void Deselect(BarkInteractable interactable)
        {
            Logging.Debug("Dropped", interactable.name, $"(priority {interactable.priority})");
            interactable.OnDeselect(this);
        }

        public void Hover(BarkInteractable interactable)
        {
            hovered.Add(interactable);
        }

        void OnGrip(InputTracker _)
        {
            if (Selecting) return;
            Selecting = true;
            BarkInteractable selected = null;
            foreach (var interactable in hovered)
            {
                if (interactable.CanBeSelected(this))
                {
                    selected = interactable;
                    break;
                }
            }
            if (!selected) return;
            Select(selected);
        }

        void OnGripRelease(InputTracker _)
        {
            if (!Selecting) return;
            Selecting = false;
            DeselectAll();
        }

        void DeselectAll(int competingPriority = -1)
        {
            foreach (var interactable in selected)
            {
                if (competingPriority < 0 || interactable.priority < competingPriority)
                {
                    Deselect(interactable);
                }
            }
            selected.RemoveAll(g => !g.selectors.Contains(this));
        }

        void OnTrigger(InputTracker _)
        {
            Activating = true;
            foreach (var interactable in selected)
                interactable.OnActivate(this);
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
