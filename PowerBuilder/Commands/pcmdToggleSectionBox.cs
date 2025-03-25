using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UIFramework.Widget.CustomControls.NativeMethods;
using Autodesk.Revit.Attributes;

namespace PowerBuilder.Commands {
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleSectionBox : IPowerCommand {
        public string DisplayName { get; } = "Toggle Section Box";
        public string ShortDesc { get; } = "Toggle the Section Box visibility in the active view";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Autodesk.Revit.DB.View ActiveView = doc.ActiveView;
            ElementId SectionBoxCategoryId = new ElementId((Int64)(-2000301));
            bool setState;
            //check if you can
            //change the visibility
            if (ActiveView.IsCategoryOverridable(SectionBoxCategoryId)) {
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("toggle-section-box") == TransactionStatus.Started) {
                        
                        //TODO: how does this interact with different visibility modes
                        if (ActiveView.GetCategoryHidden(SectionBoxCategoryId)) {
                            setState = false;
                        }
                        else {
                            setState = true;
                        }
                        ActiveView.SetCategoryHidden(SectionBoxCategoryId, setState);
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }
            
            return Result.Succeeded;
        }
        public PowerDialogResult GetInput (UIApplication uiapp) {
            throw new NotImplementedException("Method not used");
        }
    }

}
