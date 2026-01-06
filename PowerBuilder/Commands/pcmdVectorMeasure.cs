using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Infrastructure;
using RevitTaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace PowerBuilder.Commands {
    [Transaction(TransactionMode.Manual)]
    internal class pcmdVectorMeasure: CmdBase{
        public override string DisplayName { get; } = "Vector Measure";
        public override string ShortDesc { get; } = "Display vector components from Start Point to End Point";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            string Message;
            XYZ StartPoint, EndPoint, Displacement;


            PowerDialogResult res = GetInput(uiapp);
            StartPoint = res.SelectionResults[0] as XYZ;
            EndPoint = res.SelectionResults[1] as XYZ;
            Displacement = EndPoint.Subtract(StartPoint);

            Message = $"x: {Displacement.X.ToInches()} in.\ny: {Displacement.Y.ToInches()} in.\nz: {Displacement.Z.ToInches()} in.\n\nlength: {Displacement.GetLength().ToInches()} in.";

            RevitTaskDialog.Show(DisplayName, Message);

            return Result.Succeeded;
        }

        public override PowerDialogResult GetInput(UIApplication uiapp) {
            
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            PowerDialogResult res = new PowerDialogResult();
            XYZ StPt, EdPt;
            Selection PointChoice = uidoc.Selection;

            StPt = PointChoice.PickObject(ObjectType.PointOnElement,"Select Start Point").GlobalPoint;
            EdPt = PointChoice.PickObject(ObjectType.PointOnElement,"Select End Points").GlobalPoint;

            res.AddSelectionResult(StPt);
            res.AddSelectionResult(EdPt);

            return res;
        }

        
    }
}
