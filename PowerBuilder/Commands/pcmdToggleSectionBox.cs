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
using PowerBuilder.Interfaces;
using PowerBuilder.Utils;
using PowerBuilder.Infrastructure;

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleSectionBox : CmdBase{
        public override string DisplayName { get; } = "Toggle Section Box";
        public override string ShortDesc { get; } = "Toggle the Section Box visibility in the active view";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            ElementId sectionBoxCategoryId = new ElementId(BuiltInCategory.OST_SectionBox);
            
            //check if you can
            //change the visibility
            if (activeView.IsCategoryOverridable(sectionBoxCategoryId)) {
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("toggle-section-box") == TransactionStatus.Started) {

                        ViewUtils.ToggleCategoryVisibility(sectionBoxCategoryId, activeView);
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }
            
            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput (UIApplication uiapp) {
            throw new NotImplementedException("Method not used");
        }
    }
}
