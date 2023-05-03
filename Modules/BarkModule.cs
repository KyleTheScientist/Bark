using UnityEngine;

namespace Bark.Modules
{
    public abstract class BarkModule : MonoBehaviour
    {
        public abstract string DisplayName();
        public abstract string Tutorial();
        public ButtonController button;

        protected abstract void Cleanup();

        protected virtual void Start()
        {
            this.enabled = false;
        }

        protected virtual void OnEnable()
        {
            if(this.button)
                this.button.IsPressed = true;
        }

        protected virtual void OnDisable()
        {
            if(this.button)
                this.button.IsPressed = false;
            this.Cleanup();
        }
        protected virtual void OnDestroy()
        {
            this.Cleanup();
        }

    }
}
