#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            List<(string fullName, string displayName)> commArgs = GetCommandClasses("PowerBuilder");
            for (int i = 0; i < commArgs.Count; i++) {
                Debug.WriteLine($"DisplayName: {commArgs[i].displayName}\t|FullName {commArgs[i].fullName}");
                pullDownButton.AddPushButton(new PushButtonData($"PBCOM{i}", commArgs[i].displayName, thisAssemblyPath, commArgs[i].fullName));
            }

            Debug.WriteLine($"+{thisAssemblyPath}");
            
            return Result.Succeeded;
        }
        private List<(string fullName,string displayName)> GetCommandClasses(string sNameSpace) {

            Assembly asm = Assembly.GetExecutingAssembly();
            var commandTypes = asm.GetTypes().Where(t => typeof(IExternalCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            return commandTypes.Select(t => {
                var displayNameProperty = t.GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Static);
                string displayName = displayNameProperty?.GetValue(null) as string ?? t.Name;

                return (t.FullName, displayName);
            }).ToList();
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            Debug.WriteLine("APPLICATION SHUTDOWN");
            return Result.Succeeded;
        }
    }
}
