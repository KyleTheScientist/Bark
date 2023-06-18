using Bark.GUI;
using BepInEx.Configuration;
using UnityEngine;

namespace Bark.Modules.Physics
{
    public class LowGravity : BarkModule
    {
        public static readonly string DisplayName = "Gravity";
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
            if (!MenuController.Instance.Built) return;
            base.OnEnable();
            ReloadConfiguration();
            active = true;
        }

        protected override void Cleanup()
        {
            if (!active) return;
            UnityEngine.Physics.gravity = baseGravity;
            active = false;
        }

        protected override void ReloadConfiguration()
        {
            gravityScale = GravityMultiplier.Value / 5f;
            gravityScale = Mathf.Pow(gravityScale, 2f);
            UnityEngine.Physics.gravity = baseGravity * gravityScale;
        }

        public static ConfigEntry<int> GravityMultiplier;
        public static void BindConfigEntries()
        {
            GravityMultiplier = Plugin.configFile.Bind(
                section: DisplayName,
                key: "gravityMultiplier",
                defaultValue: 2,
                description: "How strong gravity will be (0=No gravity, 5=Normal gravity, 10=2x Jupiter Gravity)"
            );
        }

        public override string GetDisplayName()
        {
            return DisplayName;
        }

        public override string Tutorial()
        {
            return "Effect: Decreases the strength of gravity.";
        }

    }
}
