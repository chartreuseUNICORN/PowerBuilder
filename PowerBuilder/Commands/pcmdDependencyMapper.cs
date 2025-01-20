﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Forms;
using PowerBuilder.SelectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace PowerBuilder.Commands {
    [Transaction(TransactionMode.Manual)]
    public class pcmdDependencyMapper : IPowerCommand {
        string IPowerCommand.DisplayName { get; } = "Dependency Mapper";
        string IPowerCommand.ShortDesc { get; } = "Graphically Display the element dependency of a selected Element or Type";
        bool IPowerCommand.RibbonIncludeFlag { get; } = true;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            PBDialogResult res = GetInput(uiapp);
            ElementId target = res.SelectionResults[0] as ElementId;
            frmDependencyMapper form = new frmDependencyMapper();
            Dictionary<ElementId, List<ElementId>> map = BuildDependencyMap(doc, target);

            form.AddItemsToTreeView(doc, target, map);
            form.ShowDialog();
            
            
            return Result.Succeeded;
        }
        public PBDialogResult GetInput(UIApplication uiapp) {
            // so how does this actually want to work. This is usually type based, except for classes that don't have types
            // line styles
            // text styles
            // materials (this is the only one you would have to target differently)
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            PBDialogResult res = new PBDialogResult();

            try {
                ElementId sel = uidoc.Selection.PickObject(ObjectType.Element, "Select Element to evaluate.").ElementId;
                
                res.IsAccepted = true;
                res.SelectionResults.Add(sel);
            }
            catch {
                res.IsAccepted = false;
            }

            return res;
        }
        public Dictionary<ElementId, List<ElementId>> BuildDependencyMap (Document doc,ElementId target) {
            //
            Dictionary<ElementId,List < ElementId >> depMap = new Dictionary<ElementId, List<ElementId>>();
            IList<ElementId> dependents;
            Element elem = doc.GetElement(target);
            
            if (elem is CurveElement) {
                Element lStyle = ((CurveElement)elem).LineStyle;
                dependents = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Lines).ToElementIds().Cast<CurveElement>().Where(x => x.LineStyle == lStyle) as IList<ElementId>;
            }
            else if (elem is Material) {
                throw new NotImplementedException("Please Implement CurveElement mode");
            }
            else if (elem is TextNote) {
                throw new NotImplementedException("Please Implement TextNote mode");
            }
            else {
                Element eType = doc.GetElement(elem.GetTypeId());
                dependents = eType.GetDependentElements(new ElementCategoryFilter(eType.Category.BuiltInCategory));

                // i think there's a really tidy way to do this using linq and .GroupBy
                
            }

            foreach (ElementId eid in dependents) {
                Element depElement = doc.GetElement(eid);
                ElementId branchEid;
                if (depElement.OwnerViewId.Value != -1) {
                    branchEid = depElement.OwnerViewId;
                }
                else {
                    branchEid = depElement.Id;
                }
                if (depMap.ContainsKey(branchEid)) {
                    depMap[branchEid].Add(eid);
                }
                else {
                    depMap.Add(branchEid, new List<ElementId>() { eid });
                }
            }
            return depMap;
        }
        
    }
}
