using ComputerInterface;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;
using Bark.Extensions;
using Bark.Tools;
using BepInEx.Configuration;
using ModestTree;

namespace Bark.GUI.ComputerInterface
{
    public class ModEntry : IComputerModEntry
    {
        public string EntryName => "Bark";
        public Type EntryViewType => typeof(MainPage);
    }


    public class BarkCI : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<IComputerModEntry>().To<ModEntry>().AsSingle();
        }

        public static readonly string MainColor = "ffffffff";
        public static readonly string AccentColor = "00ffffff";
        public static readonly string PositiveColor = "00ff00ff";
        public static readonly string NegativeColor = "ff0000ff";

        public static StringBuilder Header(string text)
        {
            StringBuilder str = new StringBuilder();
            str.BeginCenter()
                .MakeBar('=', ComputerView.SCREEN_WIDTH, 0, MainColor)
                .AppendClr(text, AccentColor).AppendLine()
                .MakeBar('=', ComputerView.SCREEN_WIDTH, 0, MainColor)
                .AppendLine()
                .EndAlign();
            return str;
        }

        public static BarkCILine ConvertToLine(ConfigEntryBase entry)
        {
            if (entry.SettingType == typeof(bool))
            {
                var radio = new BarkCISwitch(entry.Definition.Key);
                radio.OnChange += (value) =>
                {
                    entry.BoxedValue = value;
                };
                return radio;
            }
            else if (entry.SettingType == typeof(int))
            {
                var slider = new BarkCISlider(entry.Definition.Key, 0, 10, (int)entry.BoxedValue);
                slider.OnChange += (value) =>
                {
                    entry.BoxedValue = value;
                };
                return slider;
            }
            else if (entry.SettingType == typeof(string))
            {
                string[] options = ((AcceptableValueList<string>)entry.Description.AcceptableValues).AcceptableValues;
                string initialValue = (string)entry.BoxedValue;

                var slider = new BarkCIRadio<string>(entry.Definition.Key, options.IndexOf(initialValue), options);
                slider.OnChange += (value) =>
                {
                    entry.BoxedValue = value;
                };
                return slider;
            }

            return null;
        }

    }

    public class BarkCIList
    {
        public List<BarkCILine> lines = new List<BarkCILine>();
        int selected, page;
        int capacity = 8;

        public void Render(StringBuilder str)
        {
            try
            {
                lines[selected].Hover();
                int start = page * capacity;
                for (int i = start; i < Mathf.Clamp(start + capacity, 0, lines.Count); i++)
                {
                    BarkCILine line = lines[i];
                    line.Render(str);
                }
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        public void OnKeyPressed(EKeyboardKey key)
        {
            try
            {
                switch (key)
                {
                    case EKeyboardKey.Down:
                        Navigate(1);
                        break;
                    case EKeyboardKey.Up:
                        Navigate(-1);
                        break;
                    default:
                        lines[selected].OnKeyPressed(key);
                        break;
                }
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        public void Navigate(int y)
        {
            lines[selected].Unhover();
            selected = MathExtensions.Wrap(selected + (int)Mathf.Sign(y), 0, lines.Count - 1);
            int pages = (lines.Count - 1) / capacity;
            page = (selected / capacity);
        }
    }

    public abstract class BarkCILine
    {
        public enum CILineType
        {
            TEXT, BUTTON, RADIO, CHECKBOX, SWITCH, SLIDER, NUMBER_INPUT
        }

        public CILineType type;
        protected string color = BarkCI.MainColor;
        protected bool isHovered;

        public abstract void Render(StringBuilder str);
        public abstract void OnKeyPressed(EKeyboardKey key);

        public virtual void Hover()
        {
            isHovered = true;
            this.color = BarkCI.AccentColor;
        }
        public virtual void Unhover()
        {
            isHovered = false;
            this.color = BarkCI.MainColor;
        }
    }

    public class BarkCILabel : BarkCILine
    {
        public string text;

        public BarkCILabel(string text, string color = "ffffffff")
        {
            this.text = text;
            this.color = color;
            this.type = CILineType.TEXT;
        }

        public override void OnKeyPressed(EKeyboardKey key) { }

        public override void Render(StringBuilder str)
        {
            str.AppendClr(text, color).AppendLine();
        }
    }

    public class BarkCILink : BarkCILine
    {
        private ComputerView parent;
        private Type link;
        private string text;
        private object[] args;

        public BarkCILink(string text, Type link, ComputerView parent, string color = "ffffffff", params object[] args)
        {
            this.color = color;
            this.link = link;
            this.text = text;
            this.type = CILineType.TEXT;
            this.parent = parent;
            this.args = args;
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Enter)
            {
                parent.ShowView(link, args);
            }
        }

        public override void Render(StringBuilder str)
        {
            str.AppendClr(text, color).AppendLine();
        }
    }


    public class BarkCISlider : BarkCILine
    {
        private int min, max, value;
        public Action<int> OnChange;
        private string label;

        public BarkCISlider(string label, int min, int max, int initialValue)
        {
            this.label = char.ToUpper(label[0]) + label.Substring(1);
            this.min = min;
            this.max = max;
            this.value = initialValue;
        }

        public override void Render(StringBuilder str)
        {
            str.AppendClr(label, color)
               .AppendClr(": |", BarkCI.MainColor);
            for (int i = min; i <= max; i++)
            {
                if (i == value)
                    str.AppendClr("+", BarkCI.PositiveColor);
                else
                    str.AppendClr("-", BarkCI.MainColor);
            }
            str.AppendLine($"| {value}");
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Left) Cycle(-1);
            if (key == EKeyboardKey.Right) Cycle(1);
        }

        public void Cycle(int direction)
        {
            direction = (int)Mathf.Sign(direction);
            value += direction;
            value = Mathf.Clamp(value, min, max);
            OnChange?.Invoke(value);
        }
    }

    public class BarkCIRadio<T> : BarkCILine
    {
        private T[] options;
        private int selected;
        public Action<T> OnChange;
        private string label;

        public BarkCIRadio(string label, int initialValue, params T[] options)
        {
            this.label = char.ToUpper(label[0]) + label.Substring(1);
            this.options = options;
            this.selected = initialValue;
        }

        public override void Render(StringBuilder str)
        {
            str.AppendClr(label + ": ", color);
            for (int i = 0; i < options.Length; i++)
            {
                bool isSelected = i == selected;
                str.AppendClr(
                    options[i].ToString(),
                    isSelected ? BarkCI.PositiveColor : BarkCI.MainColor
                );
                str.Append(" | ");
            }
            str.AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Left) Cycle(-1);
            if (key == EKeyboardKey.Right) Cycle(1);
        }

        public void Cycle(int direction)
        {
            direction = (int)Mathf.Sign(direction);
            selected = MathExtensions.Wrap(selected + direction, 0, options.Length - 1);
            OnChange?.Invoke(options[selected]);
        }
    }

    public class BarkCISwitch : BarkCILine
    {
        private bool enabled;
        public Action<bool> OnChange;
        private string label;

        public BarkCISwitch(string label)
        {
            this.label = char.ToUpper(label[0]) + label.Substring(1);
            this.type = CILineType.SWITCH;
        }

        public override void Render(StringBuilder str)
        {
            str.AppendClr(label + ": ", color)
                .AppendClr(
                    enabled ? "On" : "Off",
                    enabled ? BarkCI.PositiveColor : BarkCI.NegativeColor
                )
                .AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Left ||
                key == EKeyboardKey.Right ||
                key == EKeyboardKey.Enter)
                Toggle();
        }

        public void Toggle()
        {
            enabled = !enabled;
            OnChange?.Invoke(enabled);
        }
    }


}
