#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.SelectionHelpers;
using PowerBuilderUI;
using PowerBuilderUI.Forms;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;


#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdVerifyAndLog : IPowerCommand
    {
        string IPowerCommand.DisplayName { get; } = "Verify and Log as-built element status";
        string IPowerCommand.ShortDesc { get; } = "Mark verification parameter, Pin, and produce work log for selected elements";
        bool IPowerCommand.RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result ComRes;

            PowerDialogResult res = GetInput(uiapp);
            
            using (Transaction T = new Transaction(doc)) {
                if (T.Start("verify-and-log")== TransactionStatus.Started) {
                    ComRes = VerifyAndLog(res.SelectionResults[0] as List<ElementId>, uiapp);
                }
                else {
                    T.RollBack();
                }
            }
            
            return Result.Succeeded;
        }
        /// <summary>
        /// Get user selection with category selection filter
        /// </summary>
        /// <param name="uiapp"></param>
        /// <returns></returns>
        public PowerDialogResult GetInput(UIApplication uiapp) {
            
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            PowerDialogResult res = new PowerDialogResult();
            List<BuiltInCategory> ModelCategories = new FilteredElementCollector(doc)
                .OfClass(typeof(Category))
                .Cast<Category>()
                .Where(x => (x.BuiltInCategory != BuiltInCategory.INVALID) && (x.CategoryType == CategoryType.Model))
                .Select(x => x.BuiltInCategory).ToList();

            CategorySelectionFilter textNoteFilter = new CategorySelectionFilter(ModelCategories);

            try {
                IList<Reference> refs = uidoc.Selection.PickObjects(ObjectType.Element, textNoteFilter, "Select Elements to Verify.");
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
        /// Set Verify parameter, Pin, and compose element log
        /// </summary>
        /// <param name="sel"></param>
        /// <returns></returns>
        public Result VerifyAndLog (List<ElementId> sel, UIApplication uiapp) {
            Result res = Result.Succeeded;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            string msg;
            /*  Contents of the log report
                ElementId
                Date
                Location (Room)
                user id
                comments
             */
            foreach (ElementId eid in sel) {
                Element e = doc.GetElement(eid);
                msg = "";
                
                DateTime timestamp = DateTime.Now;
                try {
                    e.LookupParameter("isVerified").Set(0);
                    e.Pinned = true;
                    
                    e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(msg);

                }
                catch {
                    res = Result.Failed;
                }
            }

            return res;
        }
        /// <summary>
        /// Produce element verification log
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Dictionary<string, string> ProduceElementLog(Element e) {
            Dictionary<string, string> log = new Dictionary<string, string>();
            DateTime timestamp = DateTime.Now;
            Document doc = e.Document;

            log["ElementId"] = e.Id.Value.ToString();
            log["Date"] = timestamp.ToString();
            log["Location"] = "TODO: IMPLEMENT LOCATION";
            log["User"] = doc.Application.Username;
            log["Comments"] = e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
            //TODO: implement class based tracking
            //TODO: Where does the log go?
            return log;
        }
    }
}
