using Bark.Extensions;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Bark;
using Bark.Tools;
using Bark.Modules;
using System.Reflection;
using static Bark.Extensions.ConfigExtensions;
using BepInEx.Configuration;
using Bark.Gestures;
using Bark.Interaction;
using Bark.GUI;

public class SettingsPage : MonoBehaviour
{
    BarkOptionWheel modSelector, configSelector;
    BarkSlider valueSlider;
    ConfigEntryBase entry;

    void Awake()
    {
        try
        {
            modSelector = transform.Find("Mod Selector").gameObject.AddComponent<BarkOptionWheel>();
            modSelector.InitializeValues(GetModulesWithSettings());

            configSelector = transform.Find("Config Selector").gameObject.AddComponent<BarkOptionWheel>();
            configSelector.InitializeValues(GetConfigKeys(modSelector.Selected));

            valueSlider = transform.Find("Value Slider").gameObject.AddComponent<BarkSlider>();
            entry = GetEntry(modSelector.Selected, configSelector.Selected);
            var info = entry.ValuesInfo();
            valueSlider.InitializeValues(info.AcceptableValues, info.InitialValue);

            modSelector.OnValueChanged += (mod) =>
            {
                configSelector.InitializeValues(GetConfigKeys(mod));
            };

            configSelector.OnValueChanged += (config) =>
            {
                entry = GetEntry(modSelector.Selected, configSelector.Selected);
                UpdateText();
                var info = entry.ValuesInfo();
                valueSlider.InitializeValues(info.AcceptableValues, info.InitialValue);
            };

            valueSlider.OnValueChanged += (value) =>
            {
                entry.BoxedValue = value;
            };

        }
        catch (Exception e) { Logging.Exception(e); }
    }

    ConfigEntryBase GetEntry(string modName, string key)
    {
        foreach (var definition in Plugin.configFile.Keys)
        {
            if (definition.Section == modName && definition.Key == key)
            {
                return Plugin.configFile[definition];
            }
        }
        throw new Exception($"Could not find config entry for {modName} with key {key}");
    }

    List<string> GetConfigKeys(string modName)
    {
        try
        {
            List<string> configKeys = new List<string>();
            foreach (var definition in Plugin.configFile.Keys)
            {
                if (definition.Section == modName)
                {
                    configKeys.Add(Plugin.configFile[definition].Definition.Key);
                }
            }
            return configKeys;
        }
        catch (Exception e)
        {
            Logging.Exception(e);
            return null;
        }
    }

    List<string> GetModulesWithSettings()
    {
        try
        {
            List<string> modulesWithSettings = new List<string>() { "General" };
            foreach (var type in BarkModule.GetBarkModuleTypes())
            {
                if (type == typeof(BarkModule)) continue;
                MethodInfo bindConfigs = type.GetMethod("BindConfigEntries");
                if (bindConfigs is null) continue;

                FieldInfo nameField = type.GetField("DisplayName");
                string displayName = (string)nameField.GetValue(null);
                modulesWithSettings.Add(displayName);
            }
            return modulesWithSettings;
        }
        catch (Exception e)
        {
            Logging.Exception(e);
            return null;
        }
    }

    public void UpdateText()
    {
        if (entry is null) return;
        MenuController.Instance.helpText.text = 
            $"{modSelector.Selected} > {configSelector.Selected}\n" +
            "-----------------------------------\n" +
            entry.Description.Description + 
            $"\n\nDefault: {entry.DefaultValue}";
    }
}

public class BarkOptionWheel : MonoBehaviour
{
    Transform cylinder;
    Text[] labels;
    int selectedValue = 0, selectedLabel = 0;
    ButtonController upButton, downButton;
    List<string> values;
    public Action<string> OnValueChanged;
    private string _selected;
    public string Selected
    {
        get
        {
            return _selected;
        }
        private set
        {
            _selected = value;
            OnValueChanged?.Invoke(value);
        }
    }

    void Awake()
    {
        try
        {
            cylinder = transform.Find("Cylinder");
            labels = cylinder.GetComponentsInChildren<Text>();
            upButton = transform.Find("Arrow Up").gameObject.AddComponent<ButtonController>();
            downButton = transform.Find("Arrow Down").gameObject.AddComponent<ButtonController>();
            upButton.buttonPushDistance = .01f;
            downButton.buttonPushDistance = .01f;

            upButton.OnPressed += (button, pressed) =>
            {
                Cycle(-1);
                button.IsPressed = false;
            };

            downButton.OnPressed += (button, pressed) =>
            {
                Cycle(1);
                button.IsPressed = false;
            };
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    void Cycle(int direction)
    {
        try
        {
            selectedValue = MathExtensions.Wrap(selectedValue + direction, 0, values.Count);
            selectedLabel = MathExtensions.Wrap(selectedLabel + direction, 0, labels.Length);
            Selected = values[selectedValue];
            int labelToUpdate = MathExtensions.Wrap(selectedLabel + (2 * direction), 0, labels.Length);
            string newLabel = values[MathExtensions.Wrap(selectedValue + (2 * direction), 0, values.Count)];
            labels[labelToUpdate].text = newLabel;
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    public void InitializeValues(List<string> values)
    {
        try
        {
            this.selectedLabel = 0;
            this.selectedValue = 0;
            this.values = values;
            Selected = values[selectedValue];
            for (int i = 0; i < labels.Length; i++)
            {
                int value;
                if (i < labels.Length / 2)
                    value = MathExtensions.Wrap(selectedValue + i, 0, values.Count);
                else
                    value = MathExtensions.Wrap(values.Count - labels.Length + i, 0, values.Count);

                int label = MathExtensions.Wrap(selectedLabel + i, 0, labels.Length);
                labels[label].text = values[value];
            }
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    void FixedUpdate()
    {
        try
        {
            float angle = (selectedLabel % 6) * 60f;
            cylinder.localRotation = Quaternion.Slerp(
                cylinder.localRotation,
                Quaternion.Euler(angle, 0, 0),
                Time.fixedDeltaTime * 10f
            );
        }
        catch (Exception e) { Logging.Exception(e); }
    }
}

public class BarkSlider : MonoBehaviour
{
    Transform knob, sliderStart, sliderEnd;
    Knob _knob;
    Text label;
    object[] values;
    int selectedValue = 0;
    private object _selected;
    public Action<object> OnValueChanged;
    public object Selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
            OnValueChanged?.Invoke(value);
        }
    }


    void Awake()
    {
        try
        {
            sliderStart = transform.Find("Start");
            sliderEnd = transform.Find("End");
            knob = transform.Find("Knob");
            label = GetComponentInChildren<Text>();
            _knob = this.knob.gameObject.AddComponent<Knob>();
            _knob.Initialize(sliderStart, sliderEnd);
            _knob.OnValueChanged += (value) =>
            {
                selectedValue = value;
                Selected = values[selectedValue];
                label.text = Selected.ToString();
            };
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    public void InitializeValues(object[] values, int initialValue)
    {
        try
        {
            this.values = values;
            selectedValue = initialValue;
            Selected = values[initialValue];
            label.text = Selected.ToString();
            _knob.divisions = values.Length - 1;
            _knob.Value = initialValue;
        }
        catch (Exception e) { Logging.Exception(e); }
    }
}

public class Knob : BarkInteractable
{
    public Action<int> OnValueChanged;
    Transform start, end;
    public int divisions;
    private int _value;

    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != _value)
            {
                OnValueChanged?.Invoke(value);
                if (Selected)
                    GestureTracker.Instance.HapticPulse(this.selectors[0].IsLeft);
                Sounds.Play(Sounds.Sound.keyboardclick);
            }
            _value = value;
            this.transform.position = Vector3.Lerp(start.position, end.position, (float)Value / divisions);
        }
    }

    public void Initialize(Transform start, Transform end)
    {
        this.priority = MenuController.Instance.priority;
        this.start = start;
        this.end = end;
    }

    void FixedUpdate()
    {
        if (!Selected) return;

        // Get the length of the projection of the start-to-hand vector onto the start-to-end vector
        Vector3 startToHand = selectors[0].transform.position - start.position;
        Vector3 startToEnd = end.position - start.position;
        float projLength = Vector3.Dot(startToEnd, startToHand) / startToEnd.magnitude;

        // Get the ratio of the projection to the length of the start-to-end vector
        projLength = Mathf.Clamp01(projLength / startToEnd.magnitude);
        // Get the index of the division that the hand is closest to
        Value = Mathf.RoundToInt(projLength * divisions);

    }
}