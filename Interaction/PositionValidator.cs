using Bark.Extensions;
using Bark.Modules;
using Bark.Tools;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Bark.Gestures
{
    //TODO - Add a timeout on meat beat actions so you can't slowly accumulate them and accidentally trigger the menu
    public class PositionValidator : MonoBehaviour
    {
        public static PositionValidator Instance;
        public bool isValid, isValidAndStable, hasValidPosition;
        public Vector3 lastValidPosition;
        private float stabilityPeriod = 1f;
        private float stabilityPeriodStart;
        void Awake() { Instance = this; }

        void FixedUpdate()
        {
            try
            {

                if (Time.frameCount % 120 == 0)
                {
                    Collider[] debugCollisions = Physics.OverlapSphere(
                        Player.Instance.leftHandTransform.position,
                        .1f,
                        Player.Instance.locomotionEnabledLayers
                    );
                    foreach (var c in debugCollisions)
                    {
                        Logging.LogDebug(c.name);
                    }
                }

                Collider[] collisions = Physics.OverlapSphere(
                    Player.Instance.lastHeadPosition,
                    .25f,
                    Player.Instance.locomotionEnabledLayers
                );

                bool wasValid = isValid;
                isValid = collisions.Length == 0;
                if (!wasValid && isValid)
                {
                    stabilityPeriodStart = Time.time;
                }
                else if (isValid && Time.time - stabilityPeriodStart > stabilityPeriod)
                {
                    lastValidPosition = Player.Instance.bodyCollider.transform.position;
                    hasValidPosition = true;
                    isValidAndStable = true;
                    if (NoClip.Instance?.button)
                        NoClip.Instance.button.RemoveBlocker(ButtonController.Blocker.NOCLIP_BOUNDARY);
                    if (Piggyback.Instance?.button)
                        Piggyback.Instance.button.RemoveBlocker(ButtonController.Blocker.NOCLIP_BOUNDARY);
                }
                else if (!isValid)
                {
                    isValidAndStable = false;
                    if (NoClip.Instance?.button)
                        NoClip.Instance.button.AddBlocker(ButtonController.Blocker.NOCLIP_BOUNDARY);
                    if (Piggyback.Instance?.button)
                        Piggyback.Instance.button.AddBlocker(ButtonController.Blocker.NOCLIP_BOUNDARY);
                }

            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }
}
