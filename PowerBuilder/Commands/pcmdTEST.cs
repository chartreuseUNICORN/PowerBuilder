#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Extensions;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
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
            
            Log.Debug("Get Callout viewers or views from active plan view");
            // 
            RvtView activeView = uidoc.ActiveView;
            ElementCategoryFilter getViewerFilter = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);

            FilteredElementCollector checkViewerCollocetor = new FilteredElementCollector(doc).WherePasses(getViewerFilter);
            Log.Debug($"found {checkViewerCollocetor.Count()} viewers from filtered element collector");

            List<ElementId> callouts = activeView.GetDependentElements(getViewerFilter).ToList();
            Log.Debug($"found {callouts.Count} from get dependent elements");

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
