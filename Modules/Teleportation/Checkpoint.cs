using GorillaLocomotion;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Patches;
using Bark.Tools;
using Bark.Modules.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules.Teleportation
{
    public class Checkpoint : BarkModule
    {
        public static Checkpoint Instance;

        private Transform checkpointMarker;
        private LineRenderer bananaLine;
        private Vector3 checkpointPosition, checkpointMarkerPosition;
        private float checkpointRotation;
        private bool pointSet;


        void Awake() { Instance = this; }

        protected override void Start()
        {
            try
            {
                base.Start();
                checkpointMarker = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Checkpoint Banana")).transform;
                checkpointMarker.gameObject.SetActive(false);
                bananaLine = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Banana Line")).GetComponent<LineRenderer>();
                bananaLine.gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }

        void LeftTriggered()
        {
            if (enabled && !NoCollide.active)
                StartCoroutine(GrowBananas());
        }

        void RightTriggered()
        {
            if (enabled && pointSet)
                StartCoroutine(GoBananas());
        }

        // Creates the checkpoint
        IEnumerator GrowBananas()
        {
            checkpointMarker.gameObject.SetActive(true);
            float startTime = Time.time;
            while (GestureTracker.Instance.leftTriggered && !NoCollide.active)
            {
                float scale = Mathf.Lerp(0, Player.Instance.scale, (Time.time - startTime) / 2f);
                checkpointMarker.position = Player.Instance.leftHandTransform.position + Vector3.up * .15f * Player.Instance.scale;
                checkpointMarker.localScale = Vector3.one * scale;
                if (Mathf.Abs(scale - Player.Instance.scale) < .01f)
                {
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(UnityEngine.Random.Range(40, 56), false, 0.1f);
                    GestureTracker.Instance.HapticPulse(true);
                    checkpointPosition = Player.Instance.bodyCollider.transform.position;
                    checkpointRotation = Player.Instance.headCollider.transform.eulerAngles.y;
                    pointSet = true;
                    checkpointMarker.localScale = Vector3.one * Player.Instance.scale;
                    checkpointMarkerPosition = checkpointMarker.position;
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            if (!pointSet)
            {
                checkpointMarker.localScale = Vector3.zero;
                checkpointMarker.gameObject.SetActive(pointSet);
            }
            else
            {
                checkpointMarker.position = checkpointMarkerPosition;
                checkpointMarker.localScale = Vector3.one * Player.Instance.scale;
            }
        }

        // Warps the player to the checkpoint
        IEnumerator GoBananas()
        {
            bananaLine.gameObject.SetActive(true);
            float startTime = Time.time;
            Vector3 startPos, endPos;
            while (GestureTracker.Instance.rightTriggered && pointSet)
            {
                startPos = Player.Instance.rightHandTransform.position;
                bananaLine.SetPosition(1, startPos);
                endPos = Vector3.Lerp(startPos, checkpointMarker.transform.position, (Time.time - startTime) / 2f);
                bananaLine.SetPosition(0, endPos);
                bananaLine.startWidth = bananaLine.endWidth = Player.Instance.scale * .1f;
                bananaLine.material.mainTextureScale = new Vector2(Player.Instance.scale, 1);
                if (Vector3.Distance(endPos, checkpointMarker.transform.position) < .01f)
                {
                    TeleportPatch.TeleportPlayer(checkpointPosition, checkpointRotation);
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            bananaLine.gameObject.SetActive(false);
        }

        void FixedUpdate()
        {
            checkpointMarker.Rotate(Vector3.up, 90 * Time.fixedDeltaTime, Space.World);
        }

        List<GorillaTriggerBox> markedTriggers;
        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            checkpointMarker.gameObject.SetActive(pointSet);
            markedTriggers = new List<GorillaTriggerBox>();
            foreach (var triggerBox in FindObjectsOfType<GorillaTriggerBox>())
            {
                triggerBox.gameObject.AddComponent<CollisionObserver>().OnTriggerStayed += (box, collider) =>
                {
                    if (collider == Player.Instance.bodyCollider)
                    {
                        ClearCheckpoint();
                    }
                };
                markedTriggers.Add(triggerBox);
            }
            GestureTracker.Instance.OnLeftTriggerPressed += LeftTriggered;
            GestureTracker.Instance.OnRightTriggerPressed += RightTriggered;
        }


        public void ClearCheckpoint()
        {
            if (!pointSet) return;
            Logging.LogDebug("Clearing Checkpoint");

            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(68, false, 1f);
            checkpointMarker.gameObject.SetActive(false);
            pointSet = false;
            bananaLine.gameObject.SetActive(false);
        }

        protected override void Cleanup()
        {
            bananaLine.gameObject.SetActive(false);
            checkpointMarker.gameObject.SetActive(false);
            foreach (var triggerBox in markedTriggers)
            {
                triggerBox.GetComponent<CollisionObserver>()?.Obliterate();
            }
            GestureTracker.Instance.OnLeftTriggerPressed -= LeftTriggered;
            GestureTracker.Instance.OnRightTriggerPressed -= RightTriggered;
        }

        public override string DisplayName()
        {
            return "Checkpoint";
        }

        public override string Tutorial()
        {
            return "Hold [Left Trigger] to spawn a checkpoint. Hold [Right Trigger] to return to it.";
        }
    }
}
