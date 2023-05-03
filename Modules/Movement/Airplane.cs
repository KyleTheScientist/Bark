using GorillaLocomotion;
using Bark.Gestures;
using Bark.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules.Movement
{
    public class Airplane : BarkModule
    {
        float speedScale = 10f, acceleration = .1f;

        protected override void Start()
        {
            base.Start();
        }

        void OnGlide(Vector3 direction)
        {
            if (!enabled) return;
            var tracker = GestureTracker.Instance;
            if (
                tracker.leftTriggered ||
                tracker.rightTriggered ||
                tracker.leftGripped ||
                tracker.rightGripped) return;

            var player = Player.Instance;
            if (player.wasLeftHandTouching || player.wasRightHandTouching) return;

            var rigidbody = player.bodyCollider.attachedRigidbody;
            Vector3 velocity = direction * player.scale * speedScale;
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, velocity, acceleration);
        }

        List<LineRenderer> lines = new List<LineRenderer>();
        private void DrawRay(Vector3 position, Vector3 direction, Color color, int index)
        {
            try
            {
                LineRenderer renderer;
                if (index >= lines.Count)
                {
                    renderer = new GameObject($"Debug Line ({index})").AddComponent<LineRenderer>();
                    renderer.startWidth = .01f;
                    renderer.endWidth = .01f;
                    lines.Add(renderer);
                }
                else
                {
                    renderer = lines[index];
                }

                renderer.material.color = color;
                renderer.SetPosition(0, position);
                renderer.SetPosition(1, position + direction);
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GestureTracker.Instance.OnGlide += OnGlide;
        }

        protected override void Cleanup()
        {
            GestureTracker.Instance.OnGlide -= OnGlide;
        }

        public override string DisplayName()
        {
            return "Airplane";
        }

        public override string Tutorial()
        {
            return "- To fly, do a T-pose (spread your arms out like wings on a plane). \n" +
                "- To fly up, rotate your palms so they face forward. \n" +
                "- To fly down, rotate your palms so they face backward.";
        }

    }
}


