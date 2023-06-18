using Bark.Tools;
using BepInEx.Configuration;
using ComputerInterface;
using ComputerInterface.ViewLib;
using System;
using System.Text;

namespace Bark.GUI.ComputerInterface
{
    public class GeneralSettingsPage : ComputerView
    {
        public static ConfigEntry<string> SummonInput;
        public static ConfigEntry<string> SummonInputHand;

        private StringBuilder content;
        BarkCIList list;
        private static string title = "General";

        public override void OnShow(object[] args)
        {
            try
            {
                base.OnShow(args);
                list = new BarkCIList();                
                foreach (var definition in Plugin.configFile.Keys)
                    if (definition.Section == title)
                        list.lines.Add(BarkCI.ConvertToLine(Plugin.configFile[definition]));
                content = BarkCI.Header(title);
                list.Render(content);
                Text = content.ToString();
            }
            catch (Exception e) { Logging.LogException(e); }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            content = BarkCI.Header(title);
            if (key == EKeyboardKey.Back)
                ShowView(typeof(MainPage));
            else
                list.OnKeyPressed(key);
            list.Render(content);
            Text = content.ToString();
        }

        public static void BindConfigEntries()
        {
            try
            {
                ConfigDescription inputDesc = new ConfigDescription(
                    "Which button you press to open the menu",  
                    new AcceptableValueList<string>("gesture", "stick", "a/x", "b/y")
                );
                SummonInput = Plugin.configFile.Bind(title,
                    "open menu",
                    "gesture",
                    inputDesc
                );
                
                ConfigDescription handDesc = new ConfigDescription(
                    "Which hand can open the menu",
                    new AcceptableValueList<string>("left", "right")
                );
                SummonInputHand = Plugin.configFile.Bind(title,
                    "open hand",
                    "right",
                    handDesc
                );
            }
            catch (Exception e) { Logging.LogException(e); }
        }
    }
}
