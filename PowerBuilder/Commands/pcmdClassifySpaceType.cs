#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Claude;
using PowerBuilder.Extensions;
using PowerBuilder.Interfaces;
using PowerBuilder.Objects;
using PowerBuilder.SelectionFilter;
using PowerBuilder.Services;
using PowerBuilder.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            SpecCulture spaceTypeDefs = new SpecCulture("Space Types",
                new FilteredElementCollector(doc)
                .OfClass(typeof(HVACLoadSpaceType))
                .ToElements()
                .ToDictionary(x => Convert.ToString(x.Id.Value), x => x.Name));
            ElementClassifier SpaceClassifier = new ElementClassifier(spaceTypeDefs);

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

            ElementClassification elemClass = SpaceClassifier.Classify(target);
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
