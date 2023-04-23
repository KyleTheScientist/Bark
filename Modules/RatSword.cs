using GorillaLocomotion;
using Bark.Gestures;
using Bark.Patches;
using Bark.Tools;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Bark.Extensions;
using Photon.Pun;

namespace Bark.Modules
{
    public class RatSword : BarkModule
    {
        private GameObject sword;
        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                sword = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Rat Sword"));
                sword.transform.SetParent(GestureTracker.Instance.rightHand.transform, true);
                sword.transform.localPosition = new Vector3(-0.4782f, 0.1f, 0.4f);
                sword.transform.localRotation = Quaternion.Euler(9, 0, 0);
                sword.transform.localScale /= 2;
            }
            catch(Exception e) { Logging.LogException(e); }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            sword?.Obliterate();
        }

        public override string DisplayName()
        {
            return "Rat Sword";
        }

        public override string Tutorial()
        {
            return "I met a lil' kid in canyons who wanted me to make him a sword.";
        }
    }
}
