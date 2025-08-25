using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FilterTreeControlWPF;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// CmdRegistry is a singleton class to track command state and related methods for composing the Ribbona
    /// </summary>
    public sealed class CmdRegistry {
        private static readonly CmdRegistry _instance = new CmdRegistry();
        private static Dictionary<string, CmdSignature> _commandRegistry = new Dictionary<string, CmdSignature>();
        //do we need to provide a static reference to the application resources? is this controller more than just the command registry?
        static CmdRegistry() {
            
        }
        private CmdRegistry() {
                     
        }
        public static CmdRegistry Instance {
            get {
                return _instance;
            }
        }
        public void ComposeRibbon(UIControlledApplication uicapp) {
            //TOOD: these concerns should be separated. this is probably fine, but this should be some other sort of deal RibbonManager
            //that contains the Ribbon manipulation logic and only gets the command registry from CmdRegistry
            RibbonPanel ribbonPanel = uicapp.CreateRibbonPanel("PowerBuilder");
            PulldownButtonData pullDownData = new PulldownButtonData("pldbPBCommands", "Power Tools");
            PulldownButton pullDownButton = ribbonPanel.AddItem(pullDownData) as PulldownButton;

            //Collect Commands and compose into RibbonItems
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            Debug.WriteLine($"PATH: {thisAssemblyPath}");
            List<string> registrySequence = _commandRegistry.Keys.OrderBy(x => _commandRegistry[x].DisplayName).ToList();
            
            for (int i = 0; i < _commandRegistry.Count; i++) {
                string ir = registrySequence[i];
                if (_commandRegistry[ir].RibbonIncludeFlag) {
                    Debug.WriteLine($"DisplayName: {_commandRegistry[ir].DisplayName}\t\t\tFullName {_commandRegistry[ir]}");
                    PushButtonData CurrentPushButton = new PushButtonData($"PBCOM{i}", _commandRegistry[ir].DisplayName, thisAssemblyPath, ir);
                    CurrentPushButton.ToolTip = _commandRegistry[ir].ShortDesc;
                    pullDownButton.AddPushButton(CurrentPushButton);
                }
            }
        }
        public void InitializeRegistry() {

            Assembly asm = Assembly.GetExecutingAssembly();
            List<Type> commandTypes = asm.GetTypes()
                .Where(t => typeof(CmdBase)
                .IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (System.Type Command in commandTypes) {
                _commandRegistry.Add(Command.FullName, GetCommandData(Command));
            }
        }
        private CmdSignature GetCommandData(Type cmdType) {
            CmdBase cmd = Activator.CreateInstance(cmdType) as CmdBase;
            return cmd.GetCommandSignature();
        }
        //these should really have error handling like if (isRegistered) else throw new ..
        public void RegisterCmd (Type cmdType) {
            _commandRegistry[cmdType.FullName].RibbonIncludeFlag = true;
        }
        public void UnregisterCmd(Type cmdType) {
            _commandRegistry[cmdType.FullName].RibbonIncludeFlag = false;
        }
        //TODO: build document context aware event handlers
    }
}
