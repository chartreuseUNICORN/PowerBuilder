using System;
using System.Linq;
using System.Reflection;
using Serilog;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// DmuManager is a singleton class to consolidate the management and registration of Dynamic Model Updates used in PowerBuilder
    /// </summary>
    public sealed class DmuManager{
        private static readonly DmuManager _instance = new DmuManager();
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
        public void RegisterUpdaters (UIControlledApplication uicap){
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
                    IUpdater iud = Activator.CreateInstance(dmuClass, args) as IUpdater;
                    UpdaterRegistry.RegisterUpdater(iud);
                }
                catch (Exception e){
                    Log.Warning($"COULD NOT REGISTER {dmuClass.FullName} >> {e.Message}");
                }
            }
        }
    }
}