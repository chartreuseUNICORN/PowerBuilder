#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdTrimElementsToScopeBox : CmdBase{
        public override string DisplayName { get; } = "Trim Elements to Beyond View";
        public override string ShortDesc { get; } = "Delete all elements not visible in the Active View.  Use this to generate partial plans for Renovation scope of work";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Log.Information($"IExternalCommand: {DisplayName}");
            if (!(doc.IsFamilyDocument)){
                using (Transaction T = new Transaction(doc)) {
                    T.Start("delete-elements-beyond-view");
                    try {
                        TrimModelElementsBeyondActiveView(doc);
                        T.Commit();
                    }
                    catch (Exception Ex) {
                        Log.Error(Ex.Message);
                        T.RollBack(); }
                }
            }
            else {
                TaskDialog notice = new TaskDialog(DisplayName);
                notice.MainContent = "Command cannot function in a Family Document";
                notice.Show();
            }
            
            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
        public void TrimModelElementsBeyondActiveView(Document doc) {
            Autodesk.Revit.DB.View ActiveView = doc.ActiveView;

            HashSet<BuiltInCategory> ExcludeCats = new HashSet<BuiltInCategory>() 
                {   BuiltInCategory.OST_Site, 
                    BuiltInCategory.OST_ProjectBasePoint,
                    BuiltInCategory.OST_SitePoint,
                    BuiltInCategory.OST_ProjectInformation,
                    BuiltInCategory.OST_Materials
                };

            HashSet<ElementId> ElementsInView = new FilteredElementCollector(doc, ActiveView.Id)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .ToElementIds()
                    .ToHashSet<ElementId>();
            
            foreach (Category cat in doc.Settings.Categories) {
                
                if (!ExcludeCats.Contains(cat.BuiltInCategory)) {
                    HashSet<ElementId> ModelElements = new FilteredElementCollector(doc)
                    .OfCategory(cat.BuiltInCategory)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .ToElementIds()
                    .ToHashSet<ElementId>();

                    HashSet<ElementId> ElementsNotInView = ModelElements.Except(ElementsInView).ToHashSet();
                    
                    foreach (ElementId eid in ElementsNotInView) {
                        Element e = doc.GetElement(eid);
                        e.Pinned = false;
                    }
                    //TODO: there's an issue deleting all Groups: https://forums.autodesk.com/t5/revit-api-forum/can-t-delete-a-view-last-copy-of-group-quot-quot-deleted-group/m-p/10718977
                    Log.Debug($"{cat.Name}\t{cat.get_Visible(ActiveView)}\t{ElementsNotInView.Count()}");
                    doc.Delete(ElementsNotInView);
                }
            }
        }
        
    }
}
