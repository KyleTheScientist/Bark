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
            catch (Exception e) { Logging.Exception(e); }
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
    }
}
