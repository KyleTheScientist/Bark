using Bark.GUI;
using Bark.Patches;

namespace Bark.Modules.Physics
{
    public class SlipperyHands : BarkModule
    {
        public static SlipperyHands Instance;

        void Awake() { Instance = this; }

        protected override void OnEnable()
        {
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            if (NoSlip.Instance)
                NoSlip.Instance.enabled = false;
        }

        protected override void Cleanup()
        {
            string s = $"The functionality for this module is in {nameof(SlidePatch)}";
        }

        public override string DisplayName()
        {
            return "Slippery Hands";
        }

        public override string Tutorial()
        {
            return "Effect: All surfaces become slippery.";
        }

    }
}
