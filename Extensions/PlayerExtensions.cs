using Bark.Tools;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
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

        public static PhotonView PhotonView(this VRRig rig)
        {
            return Traverse.Create(rig).Field("photonView").GetValue<PhotonView>();
        }
    }
}
