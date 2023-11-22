using Bark.Gestures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Bark.Interaction
{
    public class BarkInteractable : MonoBehaviour
    {
        GestureTracker gt;
        public Action<BarkInteractable, BarkInteractor> OnHoverEnter, OnHoverExit;
        public Action<BarkInteractable, BarkInteractor> OnSelectEnter, OnSelectExit;
        public Action<BarkInteractable, BarkInteractor> OnActivateEnter, OnActivateExit;
        public Action<BarkInteractable, BarkInteractor> OnPrimaryEnter, OnPrimaryExit;
        public BarkInteractor[] validSelectors;
        public List<BarkInteractor> selectors = new List<BarkInteractor>();
        public List<BarkInteractor> hoverers = new List<BarkInteractor>();
        public int priority;
        public bool Activated;
        public bool Primary;
        public bool Selected
        {
            get { return selectors.Count > 0; }
        }

        protected virtual void Awake()
        {
            gt = GestureTracker.Instance;
            this.gameObject.layer = BarkInteractor.InteractionLayer;
            validSelectors = new BarkInteractor[] { gt.leftPalmInteractor, gt.rightPalmInteractor };
        }

        public virtual bool CanBeSelected(BarkInteractor interactor)
        {
            return enabled && !Selected && validSelectors.Contains(interactor);
        }

        public virtual void OnSelect(BarkInteractor interactor)
        {
            selectors.Add(interactor);
            OnSelectEnter?.Invoke(this, interactor);
        }

        public virtual void OnDeselect(BarkInteractor interactor)
        {
            selectors.Remove(interactor);
            OnSelectExit?.Invoke(this, interactor);
        }

        public virtual void OnActivate(BarkInteractor interactor)
        {
            Activated = true;
            OnActivateEnter?.Invoke(this, interactor);
        }

        public virtual void OnDeactivate(BarkInteractor interactor)
        {
            Activated = false;
            OnActivateExit?.Invoke(this, interactor);
        }

        public virtual void OnPrimary(BarkInteractor interactor)
        {
            Primary = true;
            OnPrimaryEnter?.Invoke(this, interactor);
        }

        public virtual void OnPrimaryReleased(BarkInteractor interactor)
        {
            Primary = false;
            OnPrimaryExit?.Invoke(this, interactor);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (collider.GetComponent<BarkInteractor>() is BarkInteractor interactor)
            {
                if (!CanBeSelected(interactor) || interactor.hovered.Contains(this)) return;
                if (interactor.Selecting)
                {
                    interactor.Select(this);
                }
                else
                {
                    interactor.Hover(this);
                    hoverers.Add(interactor);
                    OnHoverEnter?.Invoke(this, interactor);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            if (!enabled) return;
            if (collider.GetComponent<BarkInteractor>() is BarkInteractor interactor)
            {
                if (!interactor.hovered.Contains(this)) return;
                interactor.hovered.Remove(this);
                hoverers.Remove(interactor);
                OnHoverExit?.Invoke(this, interactor);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var hoverer in hoverers)
                hoverer?.hovered.Remove(this);
            foreach (var selector in selectors)
                selector?.selected.Remove(this);
        }
    }
}