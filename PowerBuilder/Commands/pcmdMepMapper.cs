#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdMepMapper : IPowerCommand {
        public string DisplayName { get; } = "MEP Map";
        public string ShortDesc { get; } = "Produce a system graphic for based on the selection.";
        public bool RibbonIncludeFlag { get; } = false;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            
            

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PowerDialogResult res = new PowerDialogResult();
            Selection sel = uidoc.Selection;
            if (sel == null) {
                //get selection from model selection
                sel.PickObject(ObjectType.Element);
            }
            return res;
        }
    }
}
