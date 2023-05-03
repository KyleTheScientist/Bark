using GorillaLocomotion;

namespace Bark.Modules
{
    public class Speed : BarkModule
    {
        public static float baseVelocityLimit, scale;
        public float _scale = 1.5f;
        public static bool active = false;

        void FixedUpdate()
        {
            var gameMode = GorillaGameManager.instance.GameMode();
            if (active && (gameMode == "NONE" || gameMode == "CASUAL"))
            {
                Player.Instance.jumpMultiplier = 1.3f * _scale;
                Player.Instance.maxJumpSpeed = 8.5f * _scale;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            active = true;
            scale = _scale;
            baseVelocityLimit =  Player.Instance.velocityLimit;
            Player.Instance.velocityLimit = baseVelocityLimit * scale;
        }

        protected override void Cleanup()
        {
            if (active)
            {
                scale = 1;
                Player.Instance.velocityLimit = baseVelocityLimit;
                active = false;
            }
        }

        public override string DisplayName()
        {
            return "Speed Boost";
        }

        public override string Tutorial()
        {
            return "Effect: Increases your jump strength.";
        }

    }
}
