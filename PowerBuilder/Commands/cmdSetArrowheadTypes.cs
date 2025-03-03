#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilderUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSetArrowheadTypes : IExternalCommand
    {
        public static string DisplayName { get; } = "Set Arrowhead by Type";
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            /*
             This is a good example of a utility that doesn't necessarily want to be Select->Accept->Execute
             This could be really nice with <Selection>->Apply + Done.  Make multiple assignments from the same interface
             */
            // Get input selection objects
            // Get existing Arrowhead Types
            List<ElementType> ArrowheadTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(ElementType))
                .WhereElementIsElementType()
                .Cast<ElementType>()
                .Where(x => x.FamilyName == "Arrowhead")
                .ToList();

            // Get ElementTypes that have Arrowhead properties
            /*
            TODO: condense this search
            IEnumerable<ElementType> TagTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(ElementType))
                .WhereElementIsElementType()
                .Cast<ElementType>()
                .Where(x => doc.Settings.Categories.Cast<Category>()
                    .Where(c => c.IsTagCategory)
                    .Select(c => c.Id.IntegerValue)
                    .Contains((int)x.Category.Id.IntegerValue));

             */
            Categories cats = doc.Settings.Categories;
            
            List<BuiltInCategory> TagCats = cats.Cast<Category>()
                .Where(x => x.IsTagCategory)
                .Select(x => x.BuiltInCategory)
                .ToList();

            FilteredElementCollector TagTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericAnnotation);
            TagTypes.UnionWith(new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)));

            foreach (BuiltInCategory c in TagCats) {
                FilteredElementCollector cTypes = new FilteredElementCollector(doc).OfCategory(c);
                TagTypes.UnionWith(cTypes);
            }
            TagTypes.WhereElementIsElementType()
                .WherePasses(new ElementParameterFilter(
                    ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(BuiltInParameter.LEADER_ARROWHEAD))
                    )
                );

            //Collect Inputs
            object[] ArrowNames = ArrowheadTypes.Cast<Element>().Select(x => x.Name).ToArray<object>();
            object[] TagTypeNames = TagTypes.WhereElementIsElementType()
                .Cast<ElementType>()
                .Select(x => x.FamilyName)
                .ToArray<object>();

            frmSetArrowheadTypes commandUI = new frmSetArrowheadTypes();
            commandUI.AddArrowTypes(ArrowNames);
            commandUI.AddTargets(TagTypeNames);
            PowerDialogResult res = commandUI.ShowDialogWithResult();

            if (res.IsAccepted)
            {
                // how does unwrapping the inputs need to look
                // res.MapToElements(List<List<Element>> source_objects)
                // where each element in source_objects corresponds to the targets of the associated control
                // it's just 1:1 wiring.  maybe you pass everything as a boolean mask? seems unecessary
                Debug.WriteLine("Set Arrowhead Types SELECTION COMPLETE");
                Debug.WriteLine(res.SelectionResults);
                int srArrowHeadNameInd = (int)res.SelectionResults[0];
                
                string selectedArrowHeadName = (string)ArrowNames[srArrowHeadNameInd];
                
                Element selectedArrowHead = ArrowheadTypes[srArrowHeadNameInd];
                

                //this string is only for input configuration validation
                StringBuilder selectedTypeString = new StringBuilder();
                List<Element> selectedElementTypes = new List<Element>();

                foreach (int idx in (CheckedListBox.CheckedIndexCollection)res.SelectionResults[1]) {
                    selectedTypeString.AppendLine((string)TagTypeNames.ElementAt(idx));
                    selectedElementTypes.Add(TagTypes.ElementAt(idx));
                }

                pcdrSetArrowheadByRefTargets(selectedArrowHead, selectedElementTypes, doc);
                
                MessageBox.Show($"Apply {selectedArrowHeadName} to\n\n" +
                    $"{selectedTypeString.ToString()}");
                Debug.WriteLine("Set Arrowhead Types COMPLETE");
            }
            else {
                Debug.WriteLine("Command ABORTED");
            }

            return Result.Succeeded;
        }
        private void pcdrSetArrowheadByRefTargets(Element source, List<Element> targets, Document doc) {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("SetArrowheadTypes");
                foreach (Element e in targets)
                {
                    e.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).Set(source.Id);
                }
                tx.Commit();
            }
        }
    }

}
