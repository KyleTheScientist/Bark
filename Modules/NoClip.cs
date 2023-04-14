using GorillaLocomotion;
using UnityEngine;

namespace Bark.Modules
{
    public class NoClip : BarkModule
    {
        private LayerMask baseMask;
        private bool baseHeadIsTrigger, baseBodyIsTrigger;
        public static bool active;
        public static int layer = 29, layerMask = 1 << layer;

        protected override void OnEnable()
        {
            base.OnEnable();
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
            active = false;
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
