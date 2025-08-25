#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using PowerBuilder.SelectionFilter;
using PowerBuilder.Utils;
using PowerBuilderUI;
using PowerBuilderUI.Forms;
using Serilog;
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
    public class pcmdVerifyAndLog : CmdBase{
        public override string DisplayName { get; } = "Verify and Log";
        public override string ShortDesc { get; } = "Mark verification parameter, Pin, and produce work log for selected elements";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result ComRes;

            PowerDialogResult res = GetInput(uiapp);

            if (res.IsAccepted) {
                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("verify-and-log") == TransactionStatus.Started) {
                        ComRes = VerifyAndLog(res.SelectionResults[0] as List<ElementId>, uiapp);
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }

            return Result.Succeeded;
        }
        /// <summary>
        /// Get user selection with category selection filter
        /// </summary>
        /// <param name="uiapp"></param>
        /// <returns></returns>
        public override PowerDialogResult GetInput(UIApplication uiapp) {

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            PowerDialogResult res = new PowerDialogResult();
            //TODO change this to find categories bound to parameter key "isValid"
            //the idea that a tracking parameter like this should be implemented from the project level makes sense.
            BindingMap ProjectParameters = doc.ParameterBindings;
            List<BuiltInCategory> ModelCategories = new List<BuiltInCategory>();
            
            //TODO: possibly valuable to implement/expect this as a shared parameter
            DefinitionBindingMapIterator dbmIter = ProjectParameters.ForwardIterator();
            dbmIter.Reset();

            while (dbmIter.MoveNext()) {
                
                Definition keyParameter = dbmIter.Key;
                if (keyParameter.Name == "isVerified") {
                    
                    InstanceBinding BindingValue = ProjectParameters.get_Item(keyParameter) as InstanceBinding;
                    foreach (Category cat in BindingValue.Categories) {
                        ModelCategories.Add(cat.BuiltInCategory);
                    }
                    break;
                }
            }
            
            if (ModelCategories.Count == 0) {
                throw new ArgumentException("Project Parameter: isVerified not found");
            }

            CategorySelectionFilter textNoteFilter = new CategorySelectionFilter( ModelCategories );

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

            string logfile = $"{doc.Title}_ValidationLog.txt";
            
            foreach (ElementId eid in sel) {
                Element e = doc.GetElement(eid);
                
                DateTime timestamp = DateTime.Now;
                try {
                    //TODO: determine some behavior for the case where this parameter does not exist.
                    e.LookupParameter("isVerified").Set(1);
                    e.Pinned = true;

                    FileUtils.WriteToFile(ProduceElementLog(e),logfile);

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
        /// <param name="e">Element to log</param>
        /// <returns></returns>
        private string ProduceElementLog(Element e) {
            DateTime timestamp = DateTime.Now;
            Document doc = e.Document;
            Room RoomLoc = null;
            List<string> MessageCollector = new List<string>();
            /*  Contents of the log report
                ElementId
                Date
                Location (Room)
                user id
                comments
             */

            MessageCollector.Add(e.Id.Value.ToString());
            MessageCollector.Add( timestamp.ToString());
            if (e.Location is LocationPoint) {
                LocationPoint locP = e.Location as LocationPoint;
                RoomLoc = doc.GetRoomAtPoint(locP.Point);
            }
            else if (e.Location is LocationCurve) {
                LocationCurve locC = e.Location as LocationCurve;
                Transform MidPoint = locC.Curve.ComputeDerivatives(0.5, true);
                RoomLoc = doc.GetRoomAtPoint(MidPoint.Origin);
            }
            if (RoomLoc != null) {
                MessageCollector.Add($"{RoomLoc.Number} {RoomLoc.Name}");
            }
            else {
                MessageCollector.Add("OTHER LOCATION");
            }
            MessageCollector.Add(doc.Application.Username);
            MessageCollector.Add( e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
            //TODO: implement class based tracking
            //TODO: Where does the log go?
            return String.Join("\t",MessageCollector.ToArray());
        }
    }
}
