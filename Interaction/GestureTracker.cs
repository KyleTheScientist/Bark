using Bark.Extensions;
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
    public class GestureTracker : MonoBehaviour
    {
        public static GestureTracker Instance;

        public InputDevice leftController, rightController;

        public InputTracker
            leftGrip, rightGrip,
            leftTrigger, rightTrigger,
            leftStick, rightStick,
            leftPrimary, rightPrimary,
            leftSecondary, rightSecondary;

        public List<InputTracker> inputs;

        public GameObject
            chest,
            leftPointerObj, rightPointerObj,
            leftHand, rightHand;

        public BodyVectors leftHandVectors, rightHandVectors, headVectors;

        public BarkInteractor
            leftPalmInteractor, rightPalmInteractor,
            leftPointerInteractor, rightPointerInteractor;

        public Transform leftPointerTransform, rightPointerTransform, leftThumbTransform, rightThumbTransform;

        public const string localRigPath =
            "Global/Local VRRig/Local Gorilla Player";
        public const string palmPath =
            "/rig/body/shoulder.{0}/upper_arm.{0}/forearm.{0}/hand.{0}/palm.01.{0}";
        public const string pointerFingerPath =
            palmPath + "/f_index.01.{0}/f_index.02.{0}/f_index.03.{0}";
        public const string thumbPath =
            palmPath + "/thumb.01.{0}/thumb.02.{0}/thumb.03.{0}";


        // Gesture Actions
        public Action OnMeatBeat;
        private Queue<int> meatBeatCollisions = new Queue<int>();
        private float lastBeat;
        private GameObject lastBeatCollider;

        public Action<Vector3> OnGlide;
        public Action OnIlluminati, OnKamehameha;
        public bool isIlluminatiing = false, isChargingKamehameha;


        public struct BodyVectors
        {
            public Vector3 pointerDirection, palmNormal, thumbDirection;
        }

        void Awake()
        {
            Instance = this;

            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            leftGrip = new InputTracker(CommonUsages.gripButton, leftController);
            rightGrip = new InputTracker(CommonUsages.gripButton, rightController);

            leftTrigger = new InputTracker(CommonUsages.trigger, leftController);
            rightTrigger = new InputTracker(CommonUsages.trigger, rightController);

            leftStick = new InputTracker(CommonUsages.primary2DAxisClick, leftController);
            rightStick = new InputTracker(CommonUsages.primary2DAxisClick, rightController);

            leftPrimary = new InputTracker(CommonUsages.primaryButton, leftController);
            rightPrimary = new InputTracker(CommonUsages.primaryButton, rightController);

            leftSecondary = new InputTracker(CommonUsages.secondaryButton, leftController);
            rightSecondary = new InputTracker(CommonUsages.secondaryButton, rightController);

            inputs = new List<InputTracker>()
            {
                leftGrip, rightGrip,
                leftTrigger, rightTrigger,
                leftStick, rightStick,
                leftPrimary, rightPrimary,
                leftSecondary, rightSecondary
            };
        }

        void Start()
        {

            try
            {
                Logging.LogDebug("Start");
                BuildColliders();
                var observer = chest.AddComponent<CollisionObserver>();
                observer.OnTriggerEntered += OnChestBeat;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        void FixedUpdate()
        {
            // If it's been more than one second since you last beat your chest, 
            if (Time.time - lastBeat > 1f)
                meatBeatCollisions.Clear();
        }

        void Update()
        {
            TrackButtonPresses();
            TrackBodyVectors();
            TrackGlideGesture();
            isIlluminatiing = TrackIlluminatiGesture();
            //isChargingKamehameha = TrackKamehamehaGesture();
        }

        void TrackBodyVectors()
        {
            var left = leftHand.transform;
            leftHandVectors = new BodyVectors()
            {
                pointerDirection = left.forward,
                palmNormal = left.right,
                thumbDirection = left.up
            };
            var right = rightHand.transform;
            rightHandVectors = new BodyVectors()
            {
                pointerDirection = right.forward,
                palmNormal = right.right * -1,
                thumbDirection = right.up
            };

            var head = Player.Instance.headCollider.transform;
            headVectors = new BodyVectors()
            {
                pointerDirection = head.forward,
                palmNormal = head.right,
                thumbDirection = head.up
            };
        }

        float illProximityThreshold = .1f;
        bool TrackIlluminatiGesture()
        {
            var scale = Player.Instance.scale;
            // Check if thumb and pointer are touching
            if (Vector3.Distance(
                    leftPointerTransform.position, rightPointerTransform.position
                ) > illProximityThreshold * scale) return false;
            if (Vector3.Distance(
                    leftThumbTransform.position, rightThumbTransform.position
                ) > illProximityThreshold * scale) return false;

            if (PalmsFacingSameWay())
            {
                OnIlluminati?.Invoke();
                return true;
            }
            return false;
        }

        bool TrackKamehamehaGesture()
        {
            var scale = Player.Instance.scale;
            // Check if palms are too far away. If so, leave.
            if (
                Vector3.Distance(
                    leftPalmInteractor.transform.position,
                    rightPalmInteractor.transform.position
                ) > .25f * scale)
                return false;

            if (PalmsFacingEachOther() && FingersFacingAway())
            {
                OnKamehameha?.Invoke();
                return true;
            }
            return false;
        }

        void TrackGlideGesture()
        {
            if (FingersFacingAway() && PalmsFacingSameWay())
            {
                // Check that the glide direction is toward where the player is facing
                Vector3 direction = (leftHandVectors.thumbDirection + rightHandVectors.thumbDirection) / 2;
                if (Vector3.Dot(direction, headVectors.pointerDirection) > 0f)
                    OnGlide?.Invoke(direction);
            }
        }
        public bool PalmsFacingEachOther()
        {
            Vector3 relativePosition = leftHand.transform.InverseTransformPoint(rightHand.transform.position);
            if (relativePosition.x < 0f) return false;
            return Vector3.Dot(leftHandVectors.palmNormal, rightHandVectors.palmNormal) < -.5f;
        }

        public bool PalmsFacingSameWay()
        {
            return Vector3.Dot(leftHandVectors.palmNormal, rightHandVectors.palmNormal) > .5f;
        }

        public bool FingersFacingAway()
        {
            Vector3 relativePosition = leftHand.transform.InverseTransformPoint(rightHand.transform.position);
            if (relativePosition.z > 0f) return false;
            return Vector3.Dot(leftHandVectors.pointerDirection, rightHandVectors.pointerDirection) < -.5f;
        }
        void OnChestBeat(GameObject obj, Collider collider)
        {
            if (collider.gameObject != leftHand && 
                collider.gameObject != rightHand) return;
            
            lastBeat = Time.time;
            //if (collider.gameObject != lastBeatCollider)
                //Sounds.Play(81, .1f);
            lastBeatCollider = collider.gameObject;


            if (meatBeatCollisions.Count > 3)
                meatBeatCollisions.Dequeue();
            if (collider.gameObject == leftHand)
                meatBeatCollisions.Enqueue(0);
            else if (collider.gameObject == rightHand)
                meatBeatCollisions.Enqueue(1);
            if (meatBeatCollisions.Count < 4) return;
            int current, last = -1;
            for (int i = 0; i < meatBeatCollisions.Count; i++)
            {
                current = meatBeatCollisions.ElementAt(i);
                if (last == current) return;
                last = current;
            }
            meatBeatCollisions.Clear();
            OnMeatBeat?.Invoke();
        }

        void BuildColliders()
        {
            Logging.LogDebug("BuildColliders");

            var player = Player.Instance;
            chest = new GameObject("Body Gesture Collider");
            chest.AddComponent<CapsuleCollider>().isTrigger = true;
            chest.AddComponent<Rigidbody>().isKinematic = true;
            chest.transform.SetParent(player.transform.FindChildRecursive("Body Collider"), false);
            chest.layer = LayerMask.NameToLayer("Water");
            float
                height = 1 / 8f,
                radius = 1 / 4f;
            chest.transform.localScale = new Vector3(radius, height, radius);

            var leftPalm = GameObject.Find(string.Format(localRigPath + palmPath, "L")).transform;
            leftPalmInteractor = CreateInteractor("Left Palm Interactor", leftPalm, 1 / 16f);
            leftHand = leftPalmInteractor.gameObject;
            leftHand.transform.localRotation = Quaternion.Euler(-90, -90, 0);

            var rightPalm = GameObject.Find(string.Format(localRigPath + palmPath, "R")).transform;
            rightPalmInteractor = CreateInteractor("Right Palm Interactor", rightPalm, 1 / 16f);
            rightHand = rightPalmInteractor.gameObject;
            rightHand.transform.localRotation = Quaternion.Euler(-90, 0, 0);

            leftPointerTransform = GameObject.Find(string.Format(localRigPath + pointerFingerPath, "L")).transform;
            leftPointerInteractor = CreateInteractor("Left Pointer Interactor", leftPointerTransform, 1 / 32f);
            leftPointerInteractor.xrController = null;
            leftPointerObj = leftPointerInteractor.gameObject;

            rightPointerTransform = GameObject.Find(string.Format(localRigPath + pointerFingerPath, "R")).transform;
            rightPointerInteractor = CreateInteractor("Right Pointer Interactor", rightPointerTransform, 1 / 32f);
            rightPointerInteractor.xrController = null;
            rightPointerObj = rightPointerInteractor.gameObject;

            leftThumbTransform = GameObject.Find(string.Format(localRigPath + thumbPath, "L")).transform;
            rightThumbTransform = GameObject.Find(string.Format(localRigPath + thumbPath, "R")).transform;
        }

        public BarkInteractor CreateInteractor(string name, Transform parent, float scale)
        {
            var obj = new GameObject(name);
            var interactor = obj.AddComponent<BarkInteractor>();
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = Vector3.one * scale;
            return interactor;
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

        public void OnDestroy()
        {
            Logging.LogDebug("Gesture Tracker Destroy");
            leftHand?.Obliterate();
            rightHand?.Obliterate();
            leftPointerObj?.Obliterate();
            rightPointerObj?.Obliterate();
            Instance = null;
            OnMeatBeat = null;
        }

        public void HapticPulse(bool isLeft, float strength = .5f, float duration = .05f)
        {
            var hand = isLeft ? leftController : rightController;
            hand.SendHapticImpulse(0u, strength, duration);
        }

        public InputTracker GetInputTracker(string name, XRNode node)
        {
            switch (name)
            {
                case "grip":
                    return node == XRNode.LeftHand ? leftGrip : rightGrip;
                case "trigger":
                    return node == XRNode.LeftHand ? leftTrigger : rightTrigger;
                case "stick":
                    return node == XRNode.LeftHand ? leftStick : rightStick;
                case "a/x":
                    return node == XRNode.LeftHand ? leftPrimary : rightPrimary;
                case "b/y":
                    return node == XRNode.LeftHand ? leftSecondary : rightSecondary;
                case "a":
                    return rightPrimary;
                case "b":
                    return rightSecondary;
                case "x":
                    return leftPrimary;
                case "y":
                    return leftSecondary;
            }
            return null;
        }

        void TrackButtonPresses()
        {
            foreach (var input in inputs)
            {
                input.wasPressed = input.pressed;
                if (input.type == typeof(bool))
                {
                    input.device.TryGetFeatureValue(input.usageBool, out input.pressed);
                }
                else
                {
                    float amount;
                    input.device.TryGetFeatureValue(input.usageFloat, out amount);
                    input.pressed = amount > .5f;
                }

                if (!input.wasPressed && input.pressed)
                    input.OnPressed?.Invoke();
                if (input.wasPressed && !input.pressed)
                    input.OnReleased?.Invoke();
            }
        }
    }

    public class InputTracker
    {
        public bool pressed, wasPressed;
        public Action OnPressed, OnReleased;
        public Type type;
        public InputFeatureUsage<bool> usageBool;
        public InputFeatureUsage<float> usageFloat;
        public InputDevice device;
        public InputTracker(InputFeatureUsage<bool> usage, InputDevice device)
        {
            this.usageBool = usage;
            this.device = device;
            type = typeof(bool);
        }

        public InputTracker(InputFeatureUsage<float> usage, InputDevice device)
        {
            this.usageFloat = usage;
            this.device = device;
            type = typeof(float);
        }
    }
}
