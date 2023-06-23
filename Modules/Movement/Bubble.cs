using GorillaLocomotion;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.XR;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Modules.Physics;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;

namespace Bark.Modules.Movement
{
    public class Bubble : BarkModule
    {
        public static readonly string DisplayName = "Bubble";
        public static GameObject bubblePrefab;
        public GameObject bubble;
        public GameObject colliderObject;
        public Vector3 targetPosition;
        public Vector3 bubbleOffset = new Vector3(0, .2f, 0);
        

        void Awake()
        {
            if (!bubblePrefab)
            {
                bubblePrefab = Plugin.assetBundle.LoadAsset<GameObject>("Bubble");
            }
        }

        Rigidbody rb;
        void FixedUpdate()
        {
            if(!rb )
                rb = Player.Instance.bodyCollider.attachedRigidbody;
            rb.AddForce(-UnityEngine.Physics.gravity * rb.mass);
        }

        bool leftWasTouching, rightWasTouching;
        float lastTouchLeft, lastTouchRight;
        float cooldown = .1f;
        void LateUpdate()
        {
            bubble.transform.position = Player.Instance.headCollider.transform.position - bubbleOffset;
            Vector3 leftPos = GestureTracker.Instance.leftHand.transform.position,
                rightPos = GestureTracker.Instance.rightHand.transform.position;

            if (Touching(leftPos))
            {
                if (!leftWasTouching && Time.time - lastTouchLeft > cooldown)
                {
                    OnTouch(leftPos, true);
                    lastTouchLeft = Time.time;
                }
                leftWasTouching = true;
            }
            else
            {
                leftWasTouching = false;
            }

            if(Touching(rightPos))
            {
                if (!rightWasTouching && Time.time - lastTouchRight > cooldown)
                {
                    OnTouch(rightPos, false);
                    lastTouchRight = Time.time;
                }
                rightWasTouching = true;
            }
            else
            {
                rightWasTouching = false;
            }
        }

        bool Touching(Vector3 position)
        {
            float radius = bubble.transform.localScale.x;
            float margin = .1f;
            float d = Vector3.Distance(position, bubble.transform.position);
            return d > radius - margin && d < radius + margin;
        }

        void OnTouch(Vector3 position, bool left)
        {
            Sounds.Play(110);
            position -= bubble.transform.position;
            GestureTracker.Instance.HapticPulse(left);
            Player.Instance.AddForce(position.normalized);
        }


        float baseDrag = 0;
        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                bubble = Instantiate(bubblePrefab);
                bubble.AddComponent<GorillaSurfaceOverride>().overrideIndex = 110;
                bubble.GetComponent<Collider>().enabled = false;
                rb = Player.Instance.bodyCollider.attachedRigidbody;
                baseDrag = rb.drag;
                rb.drag = 1;
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        protected override void Cleanup()
        {
            if (bubble)
                Sounds.Play(84, 2);
            bubble?.Obliterate();
            if(rb)
                rb.drag = baseDrag;
        }
        protected override void ReloadConfiguration()
        {
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Creates a bubble around you so you can float. "+
                "Punch the sides to move around";
        }
    }
}
