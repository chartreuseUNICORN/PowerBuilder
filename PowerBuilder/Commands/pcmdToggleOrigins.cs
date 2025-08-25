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
    public class pcmdToggleOrigins: CmdBase{
        public override string DisplayName { get; } = "Toggle Origins";
        public override string ShortDesc { get; } = "Toggle the Internal Origin, Project Base Point, and Survey Point visibility in the active view";
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
            List<BuiltInCategory> cats = new List<BuiltInCategory> {
                BuiltInCategory.OST_CoordinateSystem,
                BuiltInCategory.OST_ProjectBasePoint,
                BuiltInCategory.OST_SitePoint,
                BuiltInCategory.OST_SharedBasePoint};

            List<ElementId> targets = cats.Select(x => new ElementId(x)).ToList();

            //so there is a question of if the visibility states are not the same.
            using (Transaction T = new Transaction(doc)) {
                if (T.Start("toggle-origins") == TransactionStatus.Started) {
                    foreach(ElementId eid in targets) {
                        ViewUtils.ToggleCategoryVisibility(eid, activeView);
                    }
                    T.Commit();
                }
                else {
                    T.RollBack();
                }
            }
            //check if you can
            //change the visibility
            
            
            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput (UIApplication uiapp) {
            throw new NotImplementedException("Method not used");
        }
    }

}
