using Bark.Patches;
using GorillaLocomotion;
using UnityEngine;

namespace Bark.Modules.Physics
{
    public class Slippery: BarkModule
    {
        public static Slippery Instance;

        void Awake() { Instance = this; }

        protected override void Cleanup() 
        {
            string s = $"The functionality for this module is in {nameof(SlidePatch) }";
        }

        public override string DisplayName()
        {
            return "Slippery Hands";
        }

        public override string Tutorial()
        {
            return "Effect: Surfaces all become slippery.";
        }

    }
}
