using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Bark.Modules
{
    public abstract class BarkModule : MonoBehaviour
    {
        public List<ConfigEntryBase> ConfigEntries;
        protected virtual void ReloadConfiguration() { }

        public abstract string GetDisplayName();
        protected void SettingsChanged(object sender, SettingChangedEventArgs e)
        {
            foreach (var field in this.GetType().GetFields())
                if (e.ChangedSetting == field.GetValue(this))
                    ReloadConfiguration();
        }

        public abstract string Tutorial();
        public ButtonController button;

        protected abstract void Cleanup();

        protected virtual void Start()
        {
            this.enabled = false;

        }

        protected virtual void OnEnable()
        {
            Plugin.configFile.SettingChanged += SettingsChanged;
            if (this.button)
                this.button.IsPressed = true;
        }

        protected virtual void OnDisable()
        {
            Plugin.configFile.SettingChanged -= SettingsChanged;
            if (this.button)
                this.button.IsPressed = false;
            this.Cleanup();
        }
        protected virtual void OnDestroy()
        {
            this.Cleanup();
        }

        public static List<Type> GetBarkModuleTypes()
        {

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(BarkModule).IsAssignableFrom(t)).ToList();
            types.Sort((x, y) =>
            {
                FieldInfo xField = x.GetField("DisplayName", BindingFlags.Public | BindingFlags.Static);
                FieldInfo yField = y.GetField("DisplayName", BindingFlags.Public | BindingFlags.Static);

                if (xField == null || yField == null)
                    return 0;

                string xValue = (string)xField.GetValue(null);
                string yValue = (string)yField.GetValue(null);

                return string.Compare(xValue, yValue);
            });
            return types;
        }

    }
}
