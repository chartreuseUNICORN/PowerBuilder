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
using PowerBuilder.Infrastructure;

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleViewProjection : CmdBase{
        public override string DisplayName { get; } = "Toggle View Projection";
        public override string ShortDesc { get; } = "Toggle the View Projection state in the active view";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Autodesk.Revit.DB.View ActiveView = doc.ActiveView;
            
            if (ActiveView is View3D) {
                if (ActiveView.Cast<View3D>().CanToggleBetweenPerspectiveAndIsometric())
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("toggle-view-projection") == TransactionStatus.Started) {

                        ActiveView.get_Parameter(BuiltInParameter.VIEWER_PERSPECTIVE).Set(!ActiveView.Cast<View3D>().IsPerspective);
                        
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
                return Result.Succeeded;
            }
            else {
                return Result.Failed;
            }
            
        }
        public override PowerDialogResult GetInput (UIApplication uiapp) {
            throw new NotImplementedException("Method not used");
        }
    }

}
