﻿using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using PowerBuilder.Interfaces;
using PowerBuilder.SelectionFilter;

namespace PowerBuilder.Commands
{


    [Transaction(TransactionMode.Manual)]
    public class pcmdTextUnion : IPowerCommand {
        public string DisplayName { get; } = "Text Union";
        public string ShortDesc { get; } =  "Concatenate TextNote contents into one TextNote";
        public bool RibbonIncludeFlag { get; } = true;

        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            PowerDialogResult res = GetInput(uiapp);
            if (res.IsAccepted) {
                TextUnion(doc, res.SelectionResults[0] as ICollection<ElementId>);
            }

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            PowerDialogResult res = new PowerDialogResult();
            CategorySelectionFilter textNoteFilter = new CategorySelectionFilter(BuiltInCategory.OST_TextNotes);
            
            try {
                IList<Reference> refs = uidoc.Selection.PickObjects(ObjectType.Element, textNoteFilter, "Select Text Notes to combine.");
                List<ElementId> sel = new List<ElementId>();
                foreach (Reference r in refs) {
                    sel.Add(r.ElementId);
                }
                res.IsAccepted = true;
                res.SelectionResults.Add(sel);
            }            
            catch {
                res.IsAccepted = false;
            }
            
            return res;
        }

        /// <summary>
        /// In the doc, combine the text contents of the selection sel into one TextNote placed at the top-left most XYZ of the selection
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="sel"></param>
        public void TextUnion (Document doc, ICollection<ElementId> sel) {
            
            IList<Element> selectedText = new FilteredElementCollector(doc, sel).OfCategory(BuiltInCategory.OST_TextNotes).ToElements();
            selectedText.OrderByDescending(x => ((TextElement)x).Coord.Y);
            StringBuilder newCopy = new StringBuilder();

            XYZ newPoint = ((TextElement)selectedText.First()).Coord;
            ElementId newType = selectedText.First().GetTypeId();
            ElementId thisView = selectedText.First().OwnerViewId;

            foreach (TextNote e in selectedText) {
                newCopy.Append(e.GetFormattedText().GetPlainText().Trim());
                newCopy.Append(' ');
            }
            using (Transaction tx = new Transaction(doc)) {
                tx.Start("text-union");
                try {
                    TextNote.Create(doc, thisView, newPoint, newCopy.ToString(), newType);
                    doc.Delete(sel);
                    tx.Commit();
                }
                catch {
                    tx.Dispose();
                }

            }
        }
    }
}
