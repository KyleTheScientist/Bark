using UnityEngine;

namespace Bark.Modules
{
    public class LowGravity : BarkModule
    {
        Vector3 baseGravity;
        public float gravityScale = .25f;
        private bool active;
        void Awake()
        {
            baseGravity = Physics.gravity;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Physics.gravity = baseGravity * gravityScale;
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
            Physics.gravity = baseGravity;
            active = false;
        }

        public override string DisplayName()
        {
            return "Low Gravity";
        }

        public override string Tutorial()
        {
            return "Effect: Decreases the strength of gravity.";
        }

    }
}
