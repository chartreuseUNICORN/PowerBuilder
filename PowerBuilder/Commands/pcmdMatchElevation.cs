using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using PowerBuilder.Interfaces;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerBuilder.SelectionFilter;
using PowerBuilder.Extensions;
using Nice3point.Revit.Extensions;
using PowerBuilder.Infrastructure;

namespace PowerBuilder.Commands {

    [Transaction(TransactionMode.Manual)]
    internal class pcmdMatchElevation : CmdBase{
        public override string DisplayName { get; } = "Match Service Elevation";
        public override string ShortDesc { get; } = "Set a target service element elevation match the source element elevation";
        public override bool RibbonIncludeFlag { get; set; } = true;

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            
            try {
                
                PowerDialogResult res = GetInput(uiapp);
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("match-service-elevation") == TransactionStatus.Started) {

                        MatchMEPCurveProperties(
                            doc.GetElement(res.SelectionResults[0] as ElementId),
                            doc.GetElement(res.SelectionResults[1] as ElementId),
                            -1);
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }
            catch (OperationCanceledException) {
                return Result.Succeeded;
            }

            return Result.Succeeded;
        }

        public override PowerDialogResult GetInput(UIApplication uiapp) {
            PowerDialogResult res = new PowerDialogResult();
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Selection Sel = uidoc.Selection;
            ClassSelectionFilter MEPCurveClassFilter = new ClassSelectionFilter(typeof(MEPCurve));
            res.AddSelectionResult(Sel.PickObject(ObjectType.Element, MEPCurveClassFilter, "Select source Element").ElementId);
            res.AddSelectionResult(Sel.PickObject(ObjectType.Element, MEPCurveClassFilter, "Select target Element").ElementId);

            return res;
        }

        public void MatchMEPCurveProperties (Element source, Element target, int mode) {
            
            //Should this be by BuiltInParameter?
            string ParameterName;
            switch (mode) {
                case 1:
                    ParameterName = "Upper End Top Elevation";
                    break;
                case 0:
                    ParameterName = "Lower End Bottom Elevation";
                    break;
                default:
                    ParameterName = "Middle Elevation";
                    break;
            }
            Level TargetLevel = target.Document.GetElement(target.LevelId) as Level;
            Level SourceLevel = source.Document.GetElement(source.LevelId) as Level;
            double ValueDifference = (SourceLevel.Elevation + source.LookupParameter(ParameterName).AsDouble()) - (TargetLevel.Elevation + target.LookupParameter(ParameterName).AsDouble());
            target.LookupParameter(ParameterName).Set(target.LookupParameter(ParameterName).AsDouble() + ValueDifference);
        }
    }
}
