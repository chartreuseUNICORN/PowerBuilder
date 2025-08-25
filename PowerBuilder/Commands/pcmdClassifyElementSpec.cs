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
using PowerBuilder.Infrastructure;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdClassifyElementSpec : CmdBase {
        public override string DisplayName { get; } = "Classify Element Specification";
        public override string ShortDesc { get; } = "Connect with Claude to report estimated Element Clasification";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Log.Debug($"{this.GetType()}");
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            SpecCulture specCulture = new SpecCulture("MasterFormat",@"C:\Users\mclough\source\repos\PowerBuilder\PowerBuilder\ReferenceFiles\MasterFormat.csv");
            ElementClassifier SpecificationClassifier = new ElementClassifier(specCulture);

            Selection sel = uidoc.Selection;
            Element target = null;
            if (sel.GetElementIds().Count == 0) {
                // Nothing selected, let user pick
                Reference reference = sel.PickObject(ObjectType.Element, new ClassSelectionFilter(typeof(FamilyInstance)));
                target = doc.GetElement(reference.ElementId);
            }
            else {
                // Something already selected
                target = doc.GetElement(sel.GetElementIds().First());
            }

            ElementClassification elemClass = SpecificationClassifier.Classify(target);
            string report = $@"Element: {target.Id}
    Classification System:  {specCulture}
    Specification Number:  {elemClass.ClassificationNumber}
    Specification Name: {elemClass.ClassificationName}

    Confidence: {elemClass.Confidence}";

            TaskDialog.Show("Element Classification", report);

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
