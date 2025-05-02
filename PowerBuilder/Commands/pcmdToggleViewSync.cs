#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Interfaces;
using PowerBuilder.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleViewSync : IPowerCommand {
        public string DisplayName { get; } = "Toggle View Sync";
        public string ShortDesc { get; } = "Active View Synchronization";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            ViewSynchronizationService Vss = ViewSynchronizationService.Instance;

            Debug.WriteLine($"pcmd-ToggleViewSync:\tVSS Status:{Vss.Status}");
            
            if (!Vss.Status) {
                Vss.ActivateService(uiapp);
                Vss.Status = true;
            }
            else {
                Vss.DeactivateService(uiapp);
                Vss.Status = false;
            }

                return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
