#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PowerBuilder.Interfaces;
using PowerBuilder.IUpdaters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using PowerBuilder.Extensions;
using Autodesk.Revit.DB.Events;

#endregion

namespace PowerBuilder
{
    public class PowerBuilderApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonPanel ribbonPanel = a.CreateRibbonPanel("PowerBuilder");
            PulldownButtonData pullDownData = new PulldownButtonData("pldbPBCommands", "Power Tools");
            PulldownButton pullDownButton = ribbonPanel.AddItem(pullDownData) as PulldownButton;

            #region Assemble Ribbon Components
            //Collect Commands and compose into RibbonItems
            //TODO: port this over to its own class RibbonBuilder or something
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            Debug.WriteLine($"PATH: {thisAssemblyPath}");
            List<(string fullName, string displayName, string shortDesc)> commArgs = GetCommandClasses("PowerBuilder").OrderBy(x => x.displayName).ToList();
            for (int i = 0; i < commArgs.Count; i++) {
                Debug.WriteLine($"DisplayName: {commArgs[i].displayName}\t\t\tFullName {commArgs[i].fullName}");
                PushButtonData CurrentPushButton = new PushButtonData($"PBCOM{i}", commArgs[i].displayName, thisAssemblyPath, commArgs[i].fullName);
                CurrentPushButton.ToolTip = commArgs[i].shortDesc;
                pullDownButton.AddPushButton(CurrentPushButton);
            }
            #endregion

            #region Configure IUpdaters
            //initialize updaters
            VerifyAndLogUpdater VaLUpdater = new VerifyAndLogUpdater(a.ActiveAddInId);
            SpaceUpdater SpaceDms = new SpaceUpdater(a.ActiveAddInId);

            //register updaters
            UpdaterRegistry.RegisterUpdater(VaLUpdater);
            UpdaterRegistry.RegisterUpdater(SpaceDms);
            #endregion

            #region Register Event Handlers
            try {
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(VaLUpdater.updater_OnDocumentOpened);
                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(VaLUpdater.updater_OnDocumentClosing);
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(SpaceDms.updater_OnDocumentOpened);
                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(SpaceDms.updater_OnDocumentClosing);
            }
            catch (Exception) {
                return Result.Failed;
            }
            #endregion

            return Result.Succeeded;
        }
        private List<(string fullName,string displayName, string tooltip)> GetCommandClasses(string sNameSpace) {

            Assembly asm = Assembly.GetExecutingAssembly();
            var commandTypes = asm.GetTypes().Where(t => typeof(IExternalCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            List<(string fullName, string displayName, string tooltip)> CommandData = new List<(string fullName, string displayName, string tooltip)>();
            
            foreach (System.Type Command in commandTypes) {
                object instance = Activator.CreateInstance(Command);
                
                if ((bool)Command.GetProperty("RibbonIncludeFlag").GetValue(instance)) {
                    CommandData.Add((Command.FullName,
                    Command.GetProperty("DisplayName")?.GetValue(instance) as string ?? Command.Name,
                    Command.GetProperty("ShortDesc")?.GetValue(instance) as string ?? "")
                    );
                }
            }
            return CommandData;
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            VerifyAndLogUpdater VaLUpdater = new VerifyAndLogUpdater(a.ActiveAddInId);
            SpaceUpdater SpaceDms = new SpaceUpdater(a.ActiveAddInId);

            #region Unregister Event Handlers
            a.ControlledApplication.DocumentOpened -= VaLUpdater.updater_OnDocumentOpened;
            a.ControlledApplication.DocumentClosing-= VaLUpdater.updater_OnDocumentClosing;
            a.ControlledApplication.DocumentOpened -= SpaceDms.updater_OnDocumentOpened;
            a.ControlledApplication.DocumentClosing -= SpaceDms.updater_OnDocumentClosing;
            #endregion

            #region Unregister IUpdaters
            UpdaterRegistry.UnregisterUpdater(VaLUpdater.GetUpdaterId());
            UpdaterRegistry.UnregisterUpdater(SpaceDms.GetUpdaterId());
            #endregion


            return Result.Succeeded;
        }
    }
}
