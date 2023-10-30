using GorillaLocomotion;
using Bark.Extensions;
using Bark.Gestures;
using Bark.GUI;
using Bark.Patches;
using Bark.Tools;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using BepInEx.Configuration;

namespace Bark.Modules
{
    public class Teleport : BarkModule
    {
        public static readonly string DisplayName = "Teleport";
        public static readonly int layerMask = LayerMask.GetMask("Default", "Gorilla Object");

        private Transform teleportMarker, window;
        private bool isTeleporting;
        private DebugPoly poly;

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            try
            {
                teleportMarker = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Checkpoint Banana")).transform;
                teleportMarker.gameObject.SetActive(false);
                window = new GameObject("Teleport Window").transform;
                poly = window.gameObject.AddComponent<DebugPoly>();
                GestureTracker.Instance.OnIlluminati += OnIlluminati;
                Application.onBeforeRender += RenderTriangle;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        private void OnIlluminati()
        {
            if (this.enabled)
            {
                if (!isTeleporting)
                {
                    StartCoroutine(GrowBananas());
                }
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
                window.transform.position = (leftHand.position + rightHand.position) / 2;
                if (!TriangleInRange())
                {
                    teleportMarker.position = Vector3.up * 100000;
                    startTime = Time.time;
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
                    teleportMarker.position = Vector3.up * 100000;
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

                float chargeScale = MathExtensions.Map(ChargeTime.Value, 0, 10, .25f, 1.5f);
                float t = Mathf.Lerp(0, 1, (Time.time - startTime) / chargeScale);
                teleportMarker.position = hit.point - forward * player.scale;
                teleportMarker.localScale = Vector3.one * Player.Instance.scale * t;
                if (t >= 1)
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


        bool TriangleInRange()
        {
            return Vector3.Distance(
                window.transform.position,
                Player.Instance.headCollider.transform.position
            ) <= .2f * Player.Instance.scale;
        }

        void RenderTriangle()
        {
            if (!GestureTracker.Instance.isIlluminatiing) return;
            poly.renderer.enabled = true;
            var gt = GestureTracker.Instance;
            Vector3 a = gt.leftThumbTransform.position - gt.leftThumbTransform.up * .03f + gt.leftThumbTransform.right * -.02f;
            Vector3 b = gt.rightThumbTransform.position - gt.rightThumbTransform.up * .03f + gt.rightThumbTransform.right * .02f;
            Vector3 c = (gt.rightPointerTransform.position + gt.leftPointerTransform.position) / 2f;

            a = poly.transform.InverseTransformPoint(a);
            b = poly.transform.InverseTransformPoint(b);
            c = poly.transform.InverseTransformPoint(c);

            poly.vertices = new Vector3[] { a, b, c };
        }

        void FixedUpdate()
        {
            teleportMarker.Rotate(Vector3.up, 90 * Time.fixedDeltaTime, Space.World);
        }

        protected override void Cleanup()
        {
            if (!MenuController.Instance.Built) return;
            Application.onBeforeRender -= RenderTriangle;
            teleportMarker?.gameObject?.Obliterate();
            window?.gameObject?.Obliterate();
            isTeleporting = false;
            if (GestureTracker.Instance is null) return;
            GestureTracker.Instance.OnIlluminati -= OnIlluminati;
        }

        public static ConfigEntry<int> ChargeTime;
        public static void BindConfigEntries()
        {
            ChargeTime = Plugin.configFile.Bind(
                section: DisplayName,
                key: "charge time",
                defaultValue: 5,
                description: "How long it takes to charge the teleport"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return
                "To teleport, make a triangle with your thumbs and index fingers " +
                "and hold it close to your face.";
        }

    }
}
