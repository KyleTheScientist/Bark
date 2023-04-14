using GorillaLocomotion;
using Bark.Gestures;
using Bark.Patches;
using Bark.Tools;
using System;
using System.Collections;
using UnityEngine;

namespace Bark.Modules
{
    public class Checkpoint : BarkModule
    {
        private Transform checkpointMarker;
        private LineRenderer bananaLine;
        private Vector3 checkpointPosition;
        private float checkpointRotation, checkpointScale;
        private bool pointSet;

        protected override void Start()
        {
            try
            {
                base.Start();
                GestureTracker.Instance.OnLeftTriggerPressed += LeftTriggered;
                GestureTracker.Instance.OnRightTriggerPressed += RightTriggered;
                checkpointMarker = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Checkpoint Banana")).transform;
                checkpointMarker.gameObject.SetActive(false);
                bananaLine = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Banana Line")).GetComponent<LineRenderer>();
                bananaLine.gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, e.StackTrace);
            }
        }

        void LeftTriggered()
        {
            if (this.enabled)
                StartCoroutine(GrowBananas());
        }

        void RightTriggered()
        {
            if (this.enabled && pointSet)
                StartCoroutine(GoBananas());
        }

        // Creates the checkpoint
        IEnumerator GrowBananas()
        {
            checkpointMarker.gameObject.SetActive(true);
            float startTime = Time.time;
            while (GestureTracker.Instance.leftTriggered)
            {
                float scale = Mathf.Lerp(0, Player.Instance.scale, (Time.time - startTime) / 2f);
                checkpointMarker.position = Player.Instance.leftHandTransform.position + Vector3.up * .15f * Player.Instance.scale;
                checkpointMarker.localScale = Vector3.one * scale;
                if (Mathf.Abs(scale - Player.Instance.scale) < .01f)
                {
                    checkpointPosition = checkpointMarker.position;
                    checkpointRotation = Player.Instance.headCollider.transform.eulerAngles.y;
                    checkpointScale = Player.Instance.scale;
                    pointSet = true;
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            checkpointMarker.localScale = Vector3.one * Player.Instance.scale;
            checkpointMarker.position = checkpointPosition;
        }

        // Warps the player to the checkpoint
        IEnumerator GoBananas()
        {
            bananaLine.gameObject.SetActive(true);
            float startTime = Time.time;
            Vector3 startPos, endPos;
            while (GestureTracker.Instance.rightTriggered)
            {
                startPos = Player.Instance.rightHandTransform.position;
                bananaLine.SetPosition(1, startPos);
                endPos = Vector3.Lerp(startPos, checkpointPosition, (Time.time - startTime) / 2f);
                bananaLine.SetPosition(0, endPos);
                bananaLine.startWidth = bananaLine.endWidth = Player.Instance.scale * .1f;
                bananaLine.material.mainTextureScale = new Vector2(Player.Instance.scale, 1);
                if (Vector3.Distance(endPos, checkpointPosition) < .01f)
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

        protected override void OnEnable()
        {
            base.OnEnable();
            checkpointMarker.gameObject.SetActive(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            bananaLine.gameObject.SetActive(false);
            checkpointMarker.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            Destroy(checkpointMarker.gameObject);
            Destroy(bananaLine.gameObject);
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
