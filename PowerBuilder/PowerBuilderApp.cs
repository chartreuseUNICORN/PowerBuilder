#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //TODO: procedurally generate buttons from loaded command DLLs
            //last arg targets the command in the compiled DLL
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            Debug.WriteLine($"+{thisAssemblyPath}");
            //TODO: change these to just dropdown buttons using command names
            PushButtonData pbdC2 = new PushButtonData("cmd2-test","Command2", thisAssemblyPath, "PowerBuilder.Commands.Command2");
            PushButtonData pbdC3 = new PushButtonData("cmd3-test", "Command3", thisAssemblyPath, "PowerBuilder.Commands.Command3");
            PushButtonData pbdCSAT = new PushButtonData("cmdSetArrowheadTypes","Set Arrowhead Types", thisAssemblyPath, "PowerBuilder.Commands.cmdSetArrowheadTypes");

            PushButton pbC2 = ribbonPanel.AddItem(pbdC2) as PushButton;
            PushButton pbC3 = ribbonPanel.AddItem(pbdC3) as PushButton;
            PushButton pbCSAT = ribbonPanel.AddItem(pbdCSAT) as PushButton;
            pbC2.ToolTip = "test cmd2";
            pbC3.ToolTip = "test cmd3";
            pbdCSAT.ToolTip = "Set Arrowhead Type for multiple Annotation Objects";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            Debug.WriteLine("APPLICATION SHUTDOWN");
            return Result.Succeeded;
        }
    }
}
