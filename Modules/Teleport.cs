using GorillaLocomotion;
using Bark.Gestures;
using Bark.Patches;
using Bark.Tools;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Bark.Extensions;

namespace Bark.Modules
{
    public class Teleport : BarkModule
    {
        private Transform teleportMarker, window;
        private bool isTeleporting;
        private const float teleportWindupTime = 1f;
        public static int layerMask = LayerMask.GetMask("Default", "Gorilla Object");

        protected override void Start()
        {
            try
            {
                base.Start();
                teleportMarker = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Checkpoint Banana")).transform;
                window = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                window.localScale *= .1f;
                window.gameObject.layer = NoClip.layer;
                window.GetComponent<Collider>().isTrigger = true;
                teleportMarker.gameObject.SetActive(false);
                GestureTracker.Instance.OnIlluminati += OnIlluminati;
            }
            catch (Exception e) { Logging.LogException(e); }

        }

        private void OnIlluminati()
        {
            if (this.enabled && !isTeleporting)
                StartCoroutine(GrowBananas());
        }

        IEnumerator GrowBananas()
        {
            isTeleporting = true;
            teleportMarker.gameObject.SetActive(true);
            float startTime = Time.time;
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(Random.Range(40, 56), false, 0.1f);
            Transform
                leftHand = GestureTracker.Instance.leftPalmInteractor.transform,
                rightHand = GestureTracker.Instance.rightPalmInteractor.transform;
            while (GestureTracker.Instance.isIlluminatiing)
            {
                window.transform.position = (leftHand.position + rightHand.position) / 2;

                RaycastHit hit, windowHit;
                var forward = GestureTracker.Instance.headVectors.pointerDirection;
                Ray ray = new Ray(
                    Player.Instance.headCollider.transform.position,
                    forward
                );
                Physics.Raycast(ray, out windowHit, 2);
                Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
                if (!hit.transform || windowHit.transform != window)
                {
                    startTime = Time.time;
                    teleportMarker.position = Vector3.zero;
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                float scale = Mathf.Lerp(0, Player.Instance.scale, (Time.time - startTime) / teleportWindupTime);
                teleportMarker.position = hit.point - forward * Player.Instance.scale;
                teleportMarker.localScale = Vector3.one * scale;
                if (Mathf.Abs(scale - Player.Instance.scale) < .01f)
                {
                    TeleportPatch.TeleportPlayer(teleportMarker.position, Player.Instance.bodyCollider.transform.eulerAngles.y);
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            teleportMarker.gameObject.SetActive(false);
            isTeleporting = false;
        }

        void FixedUpdate()
        {
            teleportMarker.Rotate(Vector3.up, 90 * Time.fixedDeltaTime, Space.World);
        }

        void OnDestroy()
        {
            teleportMarker?.gameObject?.Obliterate();
        }

        public override string DisplayName()
        {
            return "Teleport";
        }

        public override string Tutorial()
        {
            return "To teleport, make a triangle with your thumbs and index fingers and hold it up to your eyes. " +
                "You will be teleported to the indicator that appears where your head is pointing.";
        }

    }
}
