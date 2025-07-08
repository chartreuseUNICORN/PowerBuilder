#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Claude;
using PowerBuilder.Extensions;
using PowerBuilder.Interfaces;
using PowerBuilder.SelectionFilter;
using PowerBuilder.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using PowerBuilder.Objects;
using System.Text.Json;
using PowerBuilder.Services;
using PowerBuilder.Enums;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Architecture;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdClassifySpaceType : IPowerCommand {
        public string DisplayName { get; } = "Classify Space";
        public string ShortDesc { get; } = "Connect with Claude to Classify the Space Type";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Log.Debug($"{this.GetType()}");
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Selection sel = uidoc.Selection;
            Element target = null;
            if (sel.GetElementIds().Count == 0) {
                // Nothing selected, let user pick
                Reference reference = sel.PickObject(ObjectType.Element, new ClassSelectionFilter(typeof(Room)));
                target = doc.GetElement(reference.ElementId);
            }
            else {
                // Something already selected
                target = doc.GetElement(sel.GetElementIds().First());
            }
            ElementClassification elemClass = ClassifierSpace.ClassifySpaceTypeByRoom(target, doc);
            string report = $@"Element: {target.Id}
    Classification System:  Spaces
    Specification Number:  {elemClass.ClassificationNumber}
    Specification Name: {elemClass.ClassificationName}

    Confidence: {elemClass.Confidence}";

            TaskDialog.Show("Element Classification", report);

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
