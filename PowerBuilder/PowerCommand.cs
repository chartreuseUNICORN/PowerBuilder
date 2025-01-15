using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace PowerBuilder {
    /// <summary>
    /// Base abstract class for PowerBuilder commands
    /// </summary>
    public abstract class PowerCommand : IExternalCommand {
        public abstract string DisplayName { get; }
        public abstract string ShortDescr { get; }
        public abstract bool LoadCommandFlag { get; }

        // Standard implementation of IExternalCommand
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            return Result.Succeeded;
        }
        /// <summary>
        /// Handle UI interactions and element selection. Override this method to implement command-specific UI logic.
        /// </summary>
        /// <param name="uiapp">The Revit UI application</param>
        /// <param name="userInputData">Output parameter for user input data</param>
        /// <returns>True if user input was successfully collected, false if operation was cancelled</returns>
        protected virtual bool GetUserInput(UIApplication uiapp, out object userInputData) {
            userInputData = null;
            return true;
        }

        /// <summary>
        /// Core command logic, isolated for testing. Override this method to implement command-specific business logic.
        /// </summary>
        /// <param name="doc">The active Revit document</param>
        /// <param name="userInputData">Data collected from user input</param>
        protected abstract void ExecuteCommand(Document doc, object userInputData);
    }
}

