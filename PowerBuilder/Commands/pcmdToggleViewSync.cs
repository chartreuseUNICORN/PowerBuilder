#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using PowerBuilder.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleViewSync : CmdBase{
        public override string DisplayName { get; } = "Toggle View Sync";
        public override string ShortDesc { get; } = "Active View Synchronization";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            ViewSynchronizationService Vss = ViewSynchronizationService.Instance;

            bool VssInitial = Vss.Status;
            
            if (!Vss.Status) {
                Vss.ActivateService(uiapp);
                Vss.Status = true;
            }
            else {
                Vss.DeactivateService(uiapp);
                Vss.Status = false;
            }
            Log.Information("Vss State change {initial} -> {final}", VssInitial, Vss.Status);
                return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
