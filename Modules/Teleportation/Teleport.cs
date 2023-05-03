using GorillaLocomotion;
using Bark.Gestures;
using Bark.Patches;
using Bark.Tools;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Bark.Extensions;
using Bark.Modules.Physics;

namespace Bark.Modules
{
    public class Teleport : BarkModule
    {
        private Transform teleportMarker, window;
        private bool isTeleporting;
        private const float teleportWindupTime = 1f;
        public static int layerMask = LayerMask.GetMask("Default", "Gorilla Object");
        private DebugPoly poly;
        private SphereCollider windowCollider;

        protected override void Start()
        {
            try
            {
                base.Start();
            }
            catch (Exception e) { Logging.LogException(e); }

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                teleportMarker = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Checkpoint Banana")).transform;
                teleportMarker.gameObject.SetActive(false);
                window = new GameObject("Teleport Window").transform;
                windowCollider = window.gameObject.AddComponent<SphereCollider>();
                windowCollider.isTrigger = true;
                window.gameObject.layer = NoClip.layer;
                poly = window.gameObject.AddComponent<DebugPoly>();
                GestureTracker.Instance.OnIlluminati += OnIlluminati;
            } catch (Exception e) { Logging.LogException(e); }
        }

        private void OnIlluminati()
        {
            if (this.enabled)
            {
                if (!isTeleporting)
                    StartCoroutine(GrowBananas());
            }
        }

        IEnumerator GrowBananas()
        {
            isTeleporting = true;
            teleportMarker.gameObject.SetActive(true);
            float startTime = Time.time;
            Transform
                leftHand = GestureTracker.Instance.leftPalmInteractor.transform,
                rightHand = GestureTracker.Instance.rightPalmInteractor.transform;
            bool playedSound = false;
            Player player = Player.Instance;
            while (GestureTracker.Instance.isIlluminatiing)
            {
                RenderTriangle();
                window.transform.position = (leftHand.position + rightHand.position) / 2;
                if(Vector3.Distance(window.transform.position, player.headCollider.transform.position) > .2f)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                RaycastHit hit;
                var forward = GestureTracker.Instance.headVectors.pointerDirection;
                Ray ray = new Ray(
                    player.headCollider.transform.position,
                    forward
                );
                UnityEngine.Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
                if (!hit.transform)
                {
                    startTime = Time.time;
                    teleportMarker.position = Vector3.zero;
                    if (playedSound)
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(98, false, .1f);
                        playedSound = false;
                    }
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                if (!playedSound)
                {
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(Random.Range(40, 56), false, 0.1f);
                    playedSound = true;
                }

                float scale = Mathf.Lerp(0, player.scale, (Time.time - startTime) / teleportWindupTime);
                teleportMarker.position = hit.point - forward * player.scale;
                teleportMarker.localScale = Vector3.one * scale;
                if (Mathf.Abs(scale - player.scale) < .01f)
                {
                    TeleportPatch.TeleportPlayer(teleportMarker.position, player.bodyCollider.transform.eulerAngles.y);
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
            teleportMarker.gameObject.SetActive(false);
            isTeleporting = false;
            poly.renderer.enabled = false;
        }

        void RenderTriangle()
        {
            poly.renderer.enabled = true;
            var gt = GestureTracker.Instance;
            Vector3 a = gt.leftThumbTransform.position - gt.leftThumbTransform.up * .03f + gt.leftThumbTransform.right * -.02f;
            Vector3 b = gt.rightThumbTransform.position - gt.rightThumbTransform.up * .03f + gt.rightThumbTransform.right * .02f;
            Vector3 c = (gt.rightPointerTransform.position + gt.leftPointerTransform.position) / 2f;

            a = poly.transform.InverseTransformPoint(a);
            b = poly.transform.InverseTransformPoint(b);
            c = poly.transform.InverseTransformPoint(c);

            window.localRotation = Quaternion.identity;
            windowCollider.center = (a + b + c) / 3;
            windowCollider.radius = Vector3.Distance(a, b) / 2;
            poly.vertices = new Vector3[] { a, b, c };
        }

        void FixedUpdate()
        {
            teleportMarker.Rotate(Vector3.up, 90 * Time.fixedDeltaTime, Space.World);
        }

        protected override void Cleanup()
        {
            GestureTracker.Instance.OnIlluminati -= OnIlluminati;
            teleportMarker?.gameObject?.Obliterate();
            window?.gameObject?.Obliterate();
        }

        public override string DisplayName()
        {
            return "Teleport";
        }

        public override string Tutorial()
        {
            return
                "To teleport, make a triangle with your thumbs and index fingers " +
                "and hold it close to your face.";
        }

    }
}
