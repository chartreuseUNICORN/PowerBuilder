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
using PowerBuilder.Services;
using Serilog;

#endregion

namespace PowerBuilder
{
    public class PowerBuilderApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            DateTime Start = DateTime.Now;
            string LogFileName = $"PowerBuilderLog_{Start.Year}.{Start.Month}.{Start.Day}.{Start.Hour}.{Start.Minute}.txt";
            string LOG_FILE_PATH = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData) + "\\PowerBuilder\\"+LogFileName;
            string FILE_OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            string JOURNAL_OUTPUT_TEMPLATE = "PBA:{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Debug.WriteLine($"++Initialize Logger at: {LOG_FILE_PATH}");
            /*
             * I guess it's reasonable to have a logger for the application, and a different logger for the other components like Business Logic and UI
             */
            ConfigureTheme();

            #region Initialize Add-in Logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(LOG_FILE_PATH, outputTemplate: FILE_OUTPUT_TEMPLATE)
                .CreateLogger();
            Debug.WriteLine($"++Logger Initialized");
            #endregion

            #region Initialize Singletons
            Log.Debug("INITIALIZE SINGLETONS");
            ViewSynchronizationService Vss = ViewSynchronizationService.Instance;
            #endregion

            #region Assemble Ribbon Components
            Log.Debug("ASSEMBLE RIBBON COMPONENTS");
            RibbonPanel ribbonPanel = a.CreateRibbonPanel("PowerBuilder");
            PulldownButtonData pullDownData = new PulldownButtonData("pldbPBCommands", "Power Tools");
            PulldownButton pullDownButton = ribbonPanel.AddItem(pullDownData) as PulldownButton;

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

            #region Register Dynamic Model Updates
            //initialize updaters
            /*
             * TODO: use reflection to register updaters and events from base class DocumentScopeUpdater
             */
            Log.Debug("REGISTER UPDATERS");
            VerifyAndLogUpdater VaLUpdater = new VerifyAndLogUpdater(a.ActiveAddInId);
            SpaceUpdater SpaceDms = new SpaceUpdater(a.ActiveAddInId);
            ParameterLinkUpdater ParameterLinkUpdater = new ParameterLinkUpdater(a.ActiveAddInId);
            SystemNameUpdater SystemNameUpdater = new SystemNameUpdater(a.ActiveAddInId);
            ControlSystemUpdater ControlSystemUpdater = new ControlSystemUpdater(a.ActiveAddInId);

            //register updaters
            UpdaterRegistry.RegisterUpdater(VaLUpdater);
            UpdaterRegistry.RegisterUpdater(SpaceDms);
            UpdaterRegistry.RegisterUpdater(ParameterLinkUpdater);
            UpdaterRegistry.RegisterUpdater(SystemNameUpdater);
            UpdaterRegistry.RegisterUpdater(ControlSystemUpdater);
            #endregion

            #region Register Event Handlers
            Log.Debug("REGISTER EVENT HANDLERS");
            try {
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(VaLUpdater.updater_OnDocumentOpened);
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(SpaceDms.updater_OnDocumentOpened);
                //a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(ParameterLinkUpdater.updater_OnDocumentOpened);
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(SystemNameUpdater.updater_OnDocumentOpened);
                a.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(ControlSystemUpdater.updater_OnDocumentOpened);

                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(VaLUpdater.updater_OnDocumentClosing);
                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(SpaceDms.updater_OnDocumentClosing);
                //a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(ParameterLinkUpdater.updater_OnDocumentClosing);
                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(SystemNameUpdater.updater_OnDocumentClosing);
                a.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(ControlSystemUpdater.updater_OnDocumentClosing);
                Log.Debug("REGISTER EVENT HANDLERS: COMPLETE");
            }
            catch (Exception) {
                Log.Warning("Event Handler Registration FAILED");
                return Result.Failed;
            }
            #endregion
            Log.Debug("STARTUP COMPLETE");
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            ViewSynchronizationService Vss = ViewSynchronizationService.Instance;

            VerifyAndLogUpdater VaLUpdater = new VerifyAndLogUpdater(a.ActiveAddInId);
            SpaceUpdater SpaceDms = new SpaceUpdater(a.ActiveAddInId);
            ParameterLinkUpdater ParameterLinkUpdater = new ParameterLinkUpdater(a.ActiveAddInId);
            SystemNameUpdater SystemNameUpdater = new SystemNameUpdater(a.ActiveAddInId);
            ControlSystemUpdater ControlSystemUpdater = new ControlSystemUpdater(a.ActiveAddInId);
            
            #region Unregister Event Handlers
            a.ControlledApplication.DocumentOpened -= VaLUpdater.updater_OnDocumentOpened;
            a.ControlledApplication.DocumentOpened -= SpaceDms.updater_OnDocumentOpened;
            //a.ControlledApplication.DocumentOpened -= ParameterLinkUpdater.updater_OnDocumentOpened;
            a.ControlledApplication.DocumentOpened -= SystemNameUpdater.updater_OnDocumentOpened;
            a.ControlledApplication.DocumentOpened -= ControlSystemUpdater.updater_OnDocumentOpened;

            a.ControlledApplication.DocumentClosing-= VaLUpdater.updater_OnDocumentClosing;
            a.ControlledApplication.DocumentClosing -= SpaceDms.updater_OnDocumentClosing;
            //a.ControlledApplication.DocumentClosing -= ParameterLinkUpdater.updater_OnDocumentClosing;
            a.ControlledApplication.DocumentClosing -= SystemNameUpdater.updater_OnDocumentClosing;
            a.ControlledApplication.DocumentClosing -= ControlSystemUpdater.updater_OnDocumentClosing;

            if (Vss.Status) {
                a.GetUIApplication().ViewActivated -= Vss.onViewActivated;
            }
            #endregion

            #region Unregister Dynamic Model Updates
            UpdaterRegistry.UnregisterUpdater(VaLUpdater.GetUpdaterId());
            UpdaterRegistry.UnregisterUpdater(SpaceDms.GetUpdaterId());
            UpdaterRegistry.UnregisterUpdater(ParameterLinkUpdater.GetUpdaterId());
            UpdaterRegistry.UnregisterUpdater(SystemNameUpdater.GetUpdaterId());
            #endregion

            #region Close logger
            Log.CloseAndFlush();
            #endregion

            return Result.Succeeded;
        }
        private void ConfigureTheme() {
            UITheme SetTheme;
            if (DateTime.Now.TimeOfDay > new TimeSpan(17, 0, 0)) {
                SetTheme = UITheme.Dark;
            }
            else {
                SetTheme = UITheme.Light;
            }
            UIThemeManager.CurrentTheme = SetTheme;
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
        /*
        private void SetupDynamicModelUpdates (UIControlledApplication a, string NameSpace) {
            Assembly asm = Assembly.GetExecutingAssembly();
            var UpdaterTypes = asm.GetTypes().Where(t => typeof(IDocumentUpdater).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            foreach (System.Type Updater in UpdaterTypes) {
                DocumentScopeUpdater currentUpdater = new IDocumentUpdater(a.ActiveAddInId);
            }
        }*/
    }
}
