using Bark.Patches;
using Bark.Tools;
using GorillaLocomotion;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Modules
{
    public class NoClip : BarkModule
    {
        private LayerMask baseMask;
        private bool baseHeadIsTrigger, baseBodyIsTrigger;
        public static bool active;
        public static int layer = 29, layerMask = 1 << layer;
        private Vector3 activationLocation;
        private float activationAngle;
        private List<GorillaTriggerInfo> disabledTriggers;

        private struct GorillaTriggerInfo
        {
            public GorillaTriggerBox trigger;
            public bool wasEnabled;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            disabledTriggers = new List<GorillaTriggerInfo>();
            foreach(var trigger in FindObjectsOfType<GorillaTriggerBox>())
            {
                if (!trigger.gameObject.activeSelf) continue;
                disabledTriggers.Add(new GorillaTriggerInfo()
                {
                    trigger = trigger,
                    wasEnabled = trigger.gameObject.activeSelf
                });
                trigger.gameObject.SetActive(false);
                Logging.LogDebug("Disabled", trigger.name);
            }

            activationLocation = Player.Instance.transform.position;
            activationAngle = Player.Instance.bodyCollider.transform.eulerAngles.y;
            if (!Piggyback.mounted)
            {
                foreach (var platformModule in Plugin.menuController.GetComponents<Platforms>())
                {
                    platformModule.enabled = true;
                }
            }
            baseMask = Player.Instance.locomotionEnabledLayers;
            Player.Instance.locomotionEnabledLayers = layerMask;

            baseBodyIsTrigger = Player.Instance.bodyCollider.isTrigger;
            Player.Instance.bodyCollider.isTrigger = true;

            baseHeadIsTrigger = Player.Instance.headCollider.isTrigger;
            Player.Instance.headCollider.isTrigger = true;
            active = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        void Cleanup()
        {
            if (!active) return;
            Player.Instance.locomotionEnabledLayers = baseMask;
            Player.Instance.bodyCollider.isTrigger = baseBodyIsTrigger;
            Player.Instance.headCollider.isTrigger = baseHeadIsTrigger;
            TeleportPatch.TeleportPlayer(activationLocation, activationAngle);
            active = false;
            Invoke(nameof(EnableTriggerBoxes), .1f);
        }

        void EnableTriggerBoxes()
        {
            foreach (var triggerInfo in disabledTriggers)
            {
                if (triggerInfo.wasEnabled && triggerInfo.trigger?.gameObject)
                {
                    triggerInfo.trigger.gameObject.SetActive(true);
                    Logging.LogDebug("Enabled", triggerInfo.trigger.name);
                }
            }
        }

        public override string DisplayName()
        {
            return "No Collide";
        }

        public override string Tutorial()
        {
            return "Effect: Disables collisions. Automatically enables platforms.";
        }

    }
}
