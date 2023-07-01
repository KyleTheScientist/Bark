using Bark.Modules;
using Bark.Tools;
using ComputerInterface;
using ComputerInterface.ViewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bark.GUI.ComputerInterface
{
    public class MainPage : ComputerView
    {
        private StringBuilder content;
        private BarkCIList moduleList;

        public override void OnShow(object[] args)
        {
            try
            {
                base.OnShow(args);
                content = BarkCI.Header($"{PluginInfo.Name} v{PluginInfo.Version}");
                moduleList = new BarkCIList();
                moduleList.lines.Add(new BarkCILink(
                    "General", 
                    typeof(GeneralSettingsPage), 
                    this, 
                    BarkCI.MainColor
                ));
                foreach (var type in BarkModule.GetBarkModuleTypes())
                {
                    if (type == typeof(BarkModule)) continue;
                    MethodInfo bindConfigs = type.GetMethod("BindConfigEntries");
                    if (bindConfigs is null) continue;

                    FieldInfo nameField = type.GetField("DisplayName");
                    string displayName = (string)nameField.GetValue(null);
                    moduleList.lines.Add(
                        new BarkCILink(
                            displayName,
                            typeof(ModulePage),
                            this,
                            BarkCI.MainColor,
                            displayName
                        )
                    );
                }

                moduleList.Render(content);
                Text = content.ToString();
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            content = BarkCI.Header($"{PluginInfo.Name}-v{PluginInfo.Version}");
            if (key == EKeyboardKey.Back)
                ReturnToMainMenu();
            else
                moduleList.OnKeyPressed(key);
            moduleList.Render(content);
            Text = content.ToString();
        }
    }
}
