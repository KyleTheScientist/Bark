using Bark.Extensions;
using Bark.Modules.Movement;
using Bark.Tools;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Networking
{
    public class NetworkedPlayer : MonoBehaviour
    {
        public Player owner;
        public VRRig rig;
        List<MonoBehaviour> modManagers = new List<MonoBehaviour>();

        public Action<NetworkedPlayer, bool> OnGripPressed, OnGripReleased;
        public bool hasBark;
        private bool leftGripWasPressed, rightGripWasPressed;
        private bool leftTriggerWasPressed, rightTriggerWasPressed;
        private bool leftThumbWasPressed, rightThumbWasPressed;
        public float LeftGripAmount { get; protected set; }
        public float RightGripAmount { get; protected set; }
        public float LeftTriggerAmount { get; protected set; }
        public float RightTriggerAmount { get; protected set; }
        public float LeftThumbAmount { get; protected set; }
        public float RightThumbAmount { get; protected set; }
        public bool LeftThumbPressed { get; protected set; }
        public bool RightThumbPressed { get; protected set; }
        public bool LeftGripPressed { get; protected set; }
        public bool RightGripPressed { get; protected set; }
        public bool LeftTriggerPressed { get; protected set; }
        public bool RightTriggerPressed { get; protected set; }


        void Start()
        {
            Logging.Debug("Created NP for", owner.NickName);
            NetworkPropertyHandler.Instance.OnPlayerModStatusChanged += OnPlayerModStatusChanged;
        }

        void OnPlayerModStatusChanged(Player player, string mod, bool enabled)
        {
            if (player == owner)
            {
                if (mod == Platforms.DisplayName)
                {
                    var manager = this.gameObject.GetOrAddComponent<NetworkedPlatformsHandler>();
                    if (enabled)
                    {
                        if (!modManagers.Contains(manager))
                            modManagers.Add(manager);
                    }
                    else
                    {
                        manager.Obliterate();
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            LeftThumbAmount = rig.leftThumb.calcT;
            RightThumbAmount = rig.rightThumb.calcT;
            LeftGripAmount = rig.leftMiddle.calcT;
            RightGripAmount = rig.rightMiddle.calcT;
            LeftTriggerAmount = rig.leftIndex.calcT;
            RightTriggerAmount = rig.rightIndex.calcT;

            LeftThumbPressed = LeftThumbAmount > .5f;
            RightThumbPressed = RightThumbAmount > .5f;
            LeftGripPressed = LeftGripAmount > .5f;
            RightGripPressed = RightGripAmount > .5f;
            LeftTriggerPressed = LeftTriggerAmount > .5f;
            RightTriggerPressed = RightTriggerAmount > .5f;

            if (LeftGripPressed && !leftGripWasPressed)
                OnGripPressed?.Invoke(this, true);
            if (!LeftGripPressed && leftGripWasPressed)
                OnGripReleased?.Invoke(this, true);
            if (RightGripPressed && !rightGripWasPressed)
                OnGripPressed?.Invoke(this, false);
            if (!RightGripPressed && rightGripWasPressed)
                OnGripReleased?.Invoke(this, false);

            leftGripWasPressed = LeftGripPressed;
            rightGripWasPressed = RightGripPressed;

        }

        void OnDestroy()
        {
            NetworkPropertyHandler.Instance.OnPlayerModStatusChanged -= OnPlayerModStatusChanged;
            foreach (MonoBehaviour mb in modManagers)
                mb?.Obliterate();
        }
    }
}
