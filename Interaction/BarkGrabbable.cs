using Bark.Gestures;
using Bark.Tools;
using GorillaLocomotion;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Bark.Interaction
{
    public class BarkGrabbable : MonoBehaviour
    {
        private Vector3 _localPos, _mirrorPos;
        Vector3 mirrorScale = new Vector3(-1, 1, 1);
        public Vector3 LocalPosition
        {
            get { return _localPos; }
            set
            {
                _localPos = value;
                _mirrorPos = Vector3.Scale(value, mirrorScale);
            }
        }

        public Vector3 MirroredLocalPosition
        {
            get { return _mirrorPos; }
        }

        public Vector3 LocalRotation = Vector3.zero;
        GestureTracker gt;
        public Action<BarkGrabbable, BarkInteractor> OnHoverEnter, OnHoverExit;
        public Action<BarkGrabbable, BarkInteractor> OnSelectEnter, OnSelectExit;
        public Action<BarkGrabbable, BarkInteractor> OnActivateEnter, OnActivateExit;
        public Action<BarkGrabbable, BarkInteractor> OnPrimaryEnter, OnPrimaryExit;
        public InputTracker[] grips, triggers;
        public BarkInteractor[] validSelectors;
        public List<BarkInteractor> selectors = new List<BarkInteractor>();
        public List<BarkInteractor> hoverers = new List<BarkInteractor>();
        public bool throwOnDetach;
        bool kinematicCache;
        GorillaVelocityEstimator velEstimator;

        public bool Activated;
        public bool Primary;
        public bool Selected
        {
            get { return !selectors.IsEmpty(); }
        }

        protected virtual void Awake()
        {
            gt = GestureTracker.Instance;
            grips = new InputTracker[] { gt.leftGrip, gt.rightGrip };
            triggers = new InputTracker[] { gt.leftTrigger, gt.rightTrigger };
            validSelectors = new BarkInteractor[] { gt.leftPalmInteractor, gt.rightPalmInteractor };
            velEstimator = this.gameObject.AddComponent<GorillaVelocityEstimator>();
        }

        public virtual bool CanBeSelected(BarkInteractor interactor)
        {
            return enabled && !Selected && validSelectors.Contains(interactor);
        }

        public virtual void OnSelect(BarkInteractor interactor)
        {
            if (this.GetComponent<Rigidbody>() is Rigidbody rb)
            {
                kinematicCache = rb.isKinematic;
                rb.isKinematic = true;
            }
            selectors.Add(interactor);
            OnSelectEnter?.Invoke(this, interactor);
        }

        Vector3 velocity, angularVelocity;
        public virtual void OnDeselect(BarkInteractor interactor)
        {
            if (this.GetComponent<Rigidbody>() is Rigidbody rb)
            {
                if (throwOnDetach)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    // Apply the force to the rigidbody
                    rb.velocity = velEstimator.linearVelocity + Player.Instance.currentVelocity;
                    rb.angularVelocity = velEstimator.angularVelocity;
                }
                else
                    rb.isKinematic = kinematicCache;
            }
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

        void OnTriggerEnter(Collider collider)
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

        void OnTriggerExit(Collider collider)
        {
            if (!enabled || Selected) return;
            if (collider.GetComponent<BarkInteractor>() is BarkInteractor interactor)
            {
                if (!validSelectors.Contains(interactor) || !interactor.hovered.Contains(this)) return;
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