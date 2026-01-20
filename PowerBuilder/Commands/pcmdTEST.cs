#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Extensions;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using PowerBuilder.Services;
using PowerBuilderUI.Forms;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RevitTaskDialog = Autodesk.Revit.UI.TaskDialog;
using RvtView = Autodesk.Revit.DB.View;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdTEST : CmdBase{
        public override string DisplayName { get; } = "TEST FUNCTION";
        public override string ShortDesc { get; } = "Container command for testing logic";
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

            Log.Debug($"{this.GetType()}");
            // TESTS FOR CalloutBlocking
            // 1.   CalloutBlockingManager configuration
            // 1.1      does the fec(doc, viewid) effectively target viewers? they're dependent elements and should be visible in view, but not Owned by the view 
            // 2.   CalloutBlockingTriggers (element, viewer)
            // 3.   Update element visibility
            // 4.   changes to geometry


            Log.Debug("Get Callout viewers or views from active plan view");
            // 
            RvtView activeView = uidoc.ActiveView;
            CalloutBlockingManager cbm = new CalloutBlockingManager(doc, activeView, true);

            List<long> testSelectionIds = new List<long> { 1061508, 1061511, 1061512, 1061513, 1061514, 1061516, 1061517, 1061724, 1061741, 1061743, 1061745, 1061747, 1061749, 1061751, 1333513, 1344719 };
            List<ElementId> testSelection = testSelectionIds.Select(x => new ElementId(x)).ToList();

            using (Transaction T = new Transaction(doc)) {
                try {
                    T.Start("test-callout-blocking-manager");

                    cbm.AssignElementsVisibility(testSelection);
                    T.Commit();
                }
                catch { 
                    T.RollBack();
                }
            }
            

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
