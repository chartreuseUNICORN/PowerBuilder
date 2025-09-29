using System;
using System.Linq;
using System.Reflection;
using Serilog;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using PowerBuilder.IUpdaters;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// DmuManager is a singleton class to consolidate the management and registration of Dynamic Model Updates used in PowerBuilder
    /// </summary>
    public sealed class DmuManager{
        private static readonly DmuManager _instance = new DmuManager();
        private static List<IUpdater> _registeredUpdaters = new List<IUpdater>();
        //private static Dictionary<string, DmuSignature> _commandRegistry = new Dictionary<string, DmuSignature>();
        //do we need to provide a static reference to the application resources? is this controller more than just the command registry?
        static DmuManager() {
            
        }
        private DmuManager() {
                     
        }
        public static DmuManager Instance {
            get {
                return _instance;
            }
        }
        /// <summary>
        /// Use reflection to register all updaters flagged for registration on startup.
        /// </summary>
        /// <param name="uicap">UIControlledApplication from IExternalApplication</param>
        public void RegisterUpdaters (UIControlledApplication uicap){
            Log.Information("Register Updaters via DmuManager");
            Assembly asm = Assembly.GetExecutingAssembly();
            AddInId aId = uicap.ActiveAddInId;
            object[] args = [aId];
            List<Type> commandTypes = asm.GetTypes()
                .Where(t => typeof(IUpdater)
                .IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (System.Type dmuClass in commandTypes) {
                // how do you get this to do the thing here
                /*
                it's something like this
                all updaters
                create instances
                    if instances error out with exceptions, don't register
                    register
                    subscribe to required events
                */
                try{
                    IUpdater updater = Activator.CreateInstance(dmuClass, args) as IUpdater;
                    UpdaterRegistry.RegisterUpdater(updater);
                    _registeredUpdaters.Add(updater);
                    Log.Information($"\tRegistered {updater.GetUpdaterName()}");
                }
                catch (Exception e){
                    Log.Warning($"COULD NOT REGISTER {dmuClass.FullName} >> {e.Message}");
                }
            }
        }
        /// <summary>
        /// Access internally tracked updater status and subscribe Event Handlers to the current ControlledApplication.
        /// </summary>
        /// <param name="capp"></param>
        public void SubscribeEventHandlers(ControlledApplication capp) {
            Log.Information("Subscribe Event Handlers via DmuManager");
            foreach (IUpdater updater in _registeredUpdaters) {
                
                if (updater is DocumentScopeUpdater) {
                    DocumentScopeUpdater dsu = updater as DocumentScopeUpdater;
                    capp.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(dsu.updater_OnDocumentOpened);
                    capp.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(dsu.updater_OnDocumentClosing);
                    Log.Information($"Bind Event Handler: {dsu.GetUpdaterName()}");
                }
            }
        }
        /// <summary>
        /// Access internally tracked updaters and unregister any currently registered IUpdaters
        /// </summary>
        public void UnregisterUpdaters() {
            Log.Information("Unregister Updaters via DmuManager");
            foreach (IUpdater updater in _registeredUpdaters) {
                if (UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId())) {
                    UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
                    Log.Information($"\tUnregistering {updater.GetUpdaterName()}");
                }
            }
        }
        /// <summary>
        /// Access internally tracked IUpdaters and unsubscribe any active Event Handlers
        /// </summary>
        /// <param name="capp"></param>
        public void UnsubscribeEventHandlers(ControlledApplication capp) {
            Log.Information("Unsubscribe Event Handlers via DmuManager");
            foreach (IUpdater updater in _registeredUpdaters) {
                if (updater is DocumentScopeUpdater) {
                    DocumentScopeUpdater dsu = updater as DocumentScopeUpdater;
                    capp.DocumentOpened -= dsu.updater_OnDocumentOpened;
                    capp.DocumentClosing -= dsu.updater_OnDocumentClosing;
                    Log.Information($"\tRemoved Event Handlers from {dsu.GetUpdaterName()}");
                }
            }
        }
        public List<UpdaterId> GetRegisteredUpdaterIds() {
            return _registeredUpdaters.Select(x => x.GetUpdaterId()).ToList();
        }
    }
}