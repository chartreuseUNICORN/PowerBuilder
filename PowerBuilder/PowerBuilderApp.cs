#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace PowerBuilder
{
    public class PowerBuilderApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            Debug.WriteLine("APPLICATION STARTUP");
            RibbonPanel ribbonPanel = a.CreateRibbonPanel("PowerBuilder");
            PulldownButtonData pullDownData = new PulldownButtonData("pldbPBCommands", "Power Tools");
            PulldownButton pullDownButton = ribbonPanel.AddItem(pullDownData) as PulldownButton;

            //TODO: procedurally generate buttons from loaded command DLLs
            //last arg targets the command in the compiled DLL
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            List<(string fullName, string displayName, string shortDesc)> commArgs = GetCommandClasses("PowerBuilder");
            for (int i = 0; i < commArgs.Count; i++) {
                Debug.WriteLine($"DisplayName: {commArgs[i].displayName}\t|FullName {commArgs[i].fullName}");
                PushButtonData CurrentPushButton = new PushButtonData($"PBCOM{i}", commArgs[i].displayName, thisAssemblyPath, commArgs[i].fullName);
                CurrentPushButton.ToolTip = commArgs[i].shortDesc;
                pullDownButton.AddPushButton(CurrentPushButton);
            }

            Debug.WriteLine($"+{thisAssemblyPath}");
            
            return Result.Succeeded;
        }
        private List<(string fullName,string displayName, string tooltip)> GetCommandClasses(string sNameSpace) {

            Assembly asm = Assembly.GetExecutingAssembly();
            var commandTypes = asm.GetTypes().Where(t => typeof(IPowerCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            /*return commandTypes.Select(t => {
                var DisplayNameProperty = t.GetProperty("IPowerCommand.DisplayName", BindingFlags.Public );
                var TooltipProperty = t.GetProperty("PowerBuilder.IPowerCommand.ShortDesc");
                string displayName = DisplayNameProperty?.GetValue(null) as string ?? t.Name;
                string tooltip = TooltipProperty?.GetValue(null) as string ?? "";
                return (t.FullName, displayName, tooltip);
            }).ToList();*/
            List<(string fullName, string displayName, string tooltip)> CommandData = new List<(string fullName, string displayName, string tooltip)>();
            foreach (System.Type Command in commandTypes) {
                object instance = Activator.CreateInstance(Command);
                CommandData.Add((Command.Name, 
                    Command.GetProperty("DisplayName")?.GetValue(instance) as string ?? Command.Name, 
                    Command.GetProperty("ShortDesc")?.GetValue(instance) as string ?? "")
                    );
            }
            return CommandData;
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            Debug.WriteLine("APPLICATION SHUTDOWN");
            return Result.Succeeded;
        }
    }
}
