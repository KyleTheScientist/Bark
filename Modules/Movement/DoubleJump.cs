using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR;

namespace Bark.Modules.Movement
{
    public class DoubleJump : BarkModule
    {
        public static bool canDoubleJump = true, primaryPressed;
        private Rigidbody _rigidbody;
        private Player _player;

        protected override void OnEnable()
        {
            base.OnEnable();
            _player = Player.Instance;
            _rigidbody = _player.bodyCollider.attachedRigidbody;
        }

        Vector3 direction;
        void FixedUpdate()
        {
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out primaryPressed);
            if (_player.wasRightHandTouching || _player.wasLeftHandTouching)
            {
                canDoubleJump = true;
            }
            if (canDoubleJump && primaryPressed && !(_player.wasRightHandTouching || _player.wasLeftHandTouching))
            {
                direction = _player.headCollider.transform.forward;
                _rigidbody.velocity = new Vector3(direction.x, direction.y, direction.z) * _player.maxJumpSpeed * _player.scale;
                canDoubleJump = false;
            }

        }

        protected override void Cleanup() { }

        public override string DisplayName()
        {
            return "Double Jump";
        }

        public override string Tutorial()
        {
            return "Press [A / B] on your right controller to do a double jump in the air.";
        }

    }
}
