using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Logging = Bark.Tools.Logging;

namespace Bark.Gestures
{
    //TODO - Add a timeout on meat beat actions so you can't slowly accumulate them and accidentally trigger the menu
    public class GestureTracker : MonoBehaviour
    {
        public static GestureTracker Instance;

        public Action OnMeatBeat;
        public GameObject
            chest,
            leftPointerObj, rightPointerObj,
            leftHand, rightHand;

        public bool isIlluminatiing = false;

        public BarkInteractor
            leftPalmInteractor, rightPalmInteractor,
            leftPointerInteractor, rightPointerInteractor;

        private Transform leftPointerTransform, rightPointerTransform, leftThumbTransform, rightThumbTransform;

        public const string pointerFingerPath =
            "Global/Local VRRig/Local Gorilla Player/rig/body/shoulder.{0}/upper_arm.{0}/forearm.{0}/hand.{0}/palm.01.{0}/f_index.01.{0}/f_index.02.{0}/f_index.03.{0}/";
        public const string thumbPath =
            "Global/Local VRRig/Local Gorilla Player/rig/body/shoulder.{0}/upper_arm.{0}/forearm.{0}/hand.{0}/palm.01.{0}/thumb.01.{0}/thumb.02.{0}/thumb.03.{0}/";

        public bool leftGripped, rightGripped, leftWasGripped, rightWasGripped;
        public Action
            OnLeftGripPressed, OnRightGripPressed,
            OnLeftGripReleased, OnRightGripReleased;

        public bool leftTriggered, rightTriggered, leftWasTriggered, rightWasTriggered;
        public Action
            OnLeftTriggerPressed, OnRightTriggerPressed,
            OnLeftTriggerReleased, OnRightTriggerReleased;
        public Action<Vector3> OnGlide, OnIlluminati;

        public BodyVectors leftHandVectors, rightHandVectors, headVectors;


        public struct BodyVectors
        {
            public Vector3 forward, right, up;
        }

        void Awake() { Instance = this; }

        void Start()
        {
            try
            {
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

        void Update()
        {
            TrackButtonPresses();
            TrackBodyVectors();
            TrackGlideGesture();
            isIlluminatiing = TrackIlluminatiGesture();
            
            
        }

        void TrackBodyVectors()
        {
            var left = Player.Instance.leftHandTransform;
            leftHandVectors = new BodyVectors()
            {
                forward = left.forward,
                right = left.right,
                up = left.up
            };
            var right = Player.Instance.rightHandTransform;
            rightHandVectors = new BodyVectors()
            {
                forward = right.forward,
                right = right.right * -1,
                up = right.up
            };

            var head = Player.Instance.headCollider.transform;
            headVectors = new BodyVectors()
            {
                forward = head.forward,
                right = head.right,
                up = head.up
            };
        }

        float illProximityThreshold = .1f;
        bool TrackIlluminatiGesture()
        {
            var scale = Player.Instance.scale;
            // Check if thumb and pointer are touching
            if (Vector3.Distance(leftPointerTransform.position, rightPointerTransform.position) > illProximityThreshold * scale) return false;
            if (Vector3.Distance(leftThumbTransform.position, rightThumbTransform.position) > illProximityThreshold * scale) return false;

            // Check if the two hands are facing in the same direction on their local up axes with plus or minus 20 degrees accuracy
            float angle = Vector3.Angle(leftHandVectors.right, rightHandVectors.right);
            if (angle > 45)
                return false;

            // Check if the indicated directions is facing in the same direction as the eyes
            Vector3 direction = (leftHandVectors.right + rightHandVectors.right) / 2;
            angle = Vector3.Angle(direction, headVectors.forward);
            if (angle > 45)
                return false;

            OnIlluminati?.Invoke(direction);
            return true;
        }

        void TrackGlideGesture()
        {
            // Check if the two transforms are facing away from each other on their local forward axes
            if (Vector3.Dot(leftHandVectors.forward, rightHandVectors.forward) > 0f)
                return;

            // Check if the two transforms are facing in the same direction on their local up axes with plus or minus 20 degrees accuracy
            float angle = Vector3.Angle(leftHandVectors.right, rightHandVectors.right);
            if (angle > 45)
                return;

            // Check that the glide direction is toward where the player is facing
            Vector3 direction = (leftHandVectors.up + rightHandVectors.up) / 2;
            if (Vector3.Dot(direction, headVectors.forward) > 0f)
                OnGlide?.Invoke(direction);
        }

        void TrackButtonPresses()
        {
            leftWasGripped = leftGripped;
            rightWasGripped = rightGripped;
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.gripButton, out leftGripped);
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out rightGripped);
            if (!leftWasGripped && leftGripped)
                OnLeftGripPressed?.Invoke();
            if (leftWasGripped && !leftGripped)
                OnLeftGripReleased?.Invoke();
            if (!rightWasGripped && rightGripped)
                OnRightGripPressed?.Invoke();
            if (rightWasGripped && !rightGripped)
                OnRightGripReleased?.Invoke();

            leftWasTriggered = leftTriggered;
            rightWasTriggered = rightTriggered;
            float lTriggerAmount, rTriggerAmount;
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.trigger, out lTriggerAmount);
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.trigger, out rTriggerAmount);
            leftTriggered = lTriggerAmount > .9f;
            rightTriggered = rTriggerAmount > .9f;
            if (!leftWasTriggered && leftTriggered)
                OnLeftTriggerPressed?.Invoke();
            if (leftWasTriggered && !leftTriggered)
            {
                OnLeftTriggerReleased?.Invoke();
            }
            if (!rightWasTriggered && rightTriggered)
                OnRightTriggerPressed?.Invoke();
            if (rightWasTriggered && !rightTriggered)
            {
                OnRightTriggerReleased?.Invoke();
            }
        }

        Queue<int> collisions = new Queue<int>();
        void OnChestBeat(GameObject obj, Collider collider)
        {
            //Logging.Log($"[{obj.name}] was triggered by [{collider.gameObject.name}]");
            if (collisions.Count > 3)
                collisions.Dequeue();
            if (collider.gameObject == leftHand)
                collisions.Enqueue(0);
            else if (collider.gameObject == rightHand)
                collisions.Enqueue(1);
            if (collisions.Count < 4) return;
            int current, last = -1;
            for (int i = 0; i < collisions.Count; i++)
            {
                current = collisions.ElementAt(i);
                if (last == current) return;
                last = current;
            }
            collisions.Clear();
            OnMeatBeat?.Invoke();
        }

        void BuildColliders()
        {
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

            var leftHandAttach = player.transform.FindChildRecursive("[LeftHand Controller] Attach");
            leftPalmInteractor = CreateInteractor("Left Palm Interactor", leftHandAttach, 1 / 16f);
            leftHand = leftPalmInteractor.gameObject;

            var rightHandAttach = player.transform.FindChildRecursive("[RightHand Controller] Attach");
            rightPalmInteractor = CreateInteractor("Right Palm Interactor", rightHandAttach, 1 / 16f);
            rightHand = rightPalmInteractor.gameObject;

            leftPointerTransform = GameObject.Find(string.Format(pointerFingerPath, "L")).transform;
            leftPointerInteractor = CreateInteractor("Left Pointer Interactor", leftPointerTransform, 1 / 32f);
            leftPointerInteractor.xrController = null;
            leftPointerObj = leftPointerInteractor.gameObject;

            rightPointerTransform = GameObject.Find(string.Format(pointerFingerPath, "R")).transform;
            rightPointerInteractor = CreateInteractor("Right Pointer Interactor", rightPointerTransform, 1 / 32f);
            rightPointerInteractor.xrController = null;
            rightPointerObj = rightPointerInteractor.gameObject;

            leftThumbTransform = GameObject.Find(string.Format(thumbPath, "L")).transform;
            rightThumbTransform = GameObject.Find(string.Format(thumbPath, "R")).transform;
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
            Logging.Log("Gesture Tracker Destroy");
            Destroy(leftHand);
            Destroy(rightHand);
            Destroy(leftPointerObj);
            Destroy(rightPointerObj);
            Instance = null;
            OnMeatBeat = null;
        }

    }
}
