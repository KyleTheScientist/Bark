using UnityEngine;

namespace Bark.Modules
{
    public abstract class BarkModule : MonoBehaviour
    {
        public abstract string DisplayName();
        public abstract string Tutorial();
        public ButtonController button;

        protected virtual void Start()
        {
            this.enabled = false;
        }

        protected virtual void OnEnable()
        {
            this.button.IsPressed = true;
        }

        protected virtual void OnDisable()
        {
            this.button.IsPressed = false;
        }

    }
}
