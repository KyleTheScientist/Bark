using Bark.Tools;
using GorillaLocomotion;
using UnityEngine;

namespace Bark.Extensions
{
    public static class PlayerExtensions
    {
        public static void AddForce(this Player self, Vector3 v)
        {
            self.bodyCollider.attachedRigidbody.velocity += v;
        }

        public static void SetVelocity(this Player self, Vector3 v)
        {
            self.bodyCollider.attachedRigidbody.velocity = v;
        }
    }
}
