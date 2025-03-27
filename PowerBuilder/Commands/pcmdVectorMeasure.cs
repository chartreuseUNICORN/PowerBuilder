﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using PowerBuilder.Forms;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;

namespace PowerBuilder.Commands {
    [Transaction(TransactionMode.Manual)]
    internal class pcmdVectorMeasure: IPowerCommand {
        public string DisplayName { get; } = "Vector Measure";
        public string ShortDesc { get; } = "Display vector components from Start Point to End Point";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
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

            Message = $"x: {Displacement.X}\ny: {Displacement.Y}\nz: {Displacement.Z}\n\nlength: {Displacement.GetLength()}";

            TaskDialog.Show(DisplayName, Message);

            return Result.Succeeded;
        }

        public PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            PowerDialogResult res = new PowerDialogResult();
            XYZ StPt, EdPt;
            Selection PointChoice = uidoc.Selection;

            StPt = PointChoice.PickPoint("Select Start Point");
            EdPt = PointChoice.PickPoint("Select End Points");

            res.AddSelectionResult(StPt);
            res.AddSelectionResult(EdPt);

            return res;
        }

        
    }
}
