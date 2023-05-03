using UnityEngine;

namespace Bark.Modules.Physics
{
    public class LowGravity : BarkModule
    {
        public static LowGravity Instance;
        Vector3 baseGravity;
        public float gravityScale = .25f;
        public bool active { get; private set; }

        void Awake()
        {
            Instance = this;
            baseGravity = UnityEngine.Physics.gravity;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UnityEngine.Physics.gravity = baseGravity * gravityScale;
            active = true;
        }

        protected override void Cleanup()
        {
            if (!active) return;
            UnityEngine.Physics.gravity = baseGravity;
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
