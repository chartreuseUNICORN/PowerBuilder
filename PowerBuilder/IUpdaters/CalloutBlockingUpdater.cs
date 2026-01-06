using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using PowerBuilder.Utils;
using PowerBuilder.Exceptions;

namespace PowerBuilder.IUpdaters {
    internal class CalloutBlockingUpdater : DocumentScopeUpdater {

        protected override string _name => "Verification Logger";
        protected override string _description => "Update pin status and log entry on verification";
        public override bool LoadOnStartup => false;
        
        static private Definition _KeyParameter;
        
        public CalloutBlockingUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("4D7EC7FB-A211-44B9-8F0B-5BA675475F81"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();
            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            //get callouts visible in the active view
            ElementFilter viewerFilter = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);
            List<Element> callouts = activeView.GetDependentElements(viewerFilter) as List<Element>;

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                Element e = doc.GetElement(ChangedElement);
                foreach (Element cv in callouts) {
                    BoundingBoxXYZ bbox = cv.get_BoundingBox(activeView);
                    Location loc = e.Location;
                    XYZ checkPoint;
                    if (loc is LocationPoint) {
                         checkPoint = loc.Cast<LocationPoint>().Point;
                    }
                    else if (loc is LocationCurve) {
                        checkPoint = loc.Cast<LocationCurve>().Curve.Evaluate(0.5, false);
                    }
                    else {
                        continue;
                    }
                        BoundingBoxContainsPointFilter bbcpf = new BoundingBoxContainsPointFilter(checkPoint);
                    if (bbcpf.PassesFilter(cv)) {
                        int calloutScale = cv.LookupParameter("View Scale").AsInteger();
                        string message = $"{calloutScale}";
                        e.LookupParameter("Comments").Set(message);
                    }
                }
                
            }
            Debug.WriteLine($"IUpdater COMPLETE: {data.GetModifiedElementIds().Count} items changed");
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");

            try {
                if (!args.Document.IsFamilyDocument) {
                    Log.Debug($"initialized SpaceCalculationService");

                    ElementMulticategoryFilter ModelElementFilter = new ElementMulticategoryFilter(CategoryUtils.GetCategoriesByType(args.Document, CategoryType.Model));
                    ChangeType AddedModelElementChangeType = Element.GetChangeTypeElementAddition();

                    UpdaterRegistry.AddTrigger(_uid, args.Document, ModelElementFilter, AddedModelElementChangeType);
                    Log.Debug($"{this.GetUpdaterName()} trigger registered");
                }
                else {
                    Log.Debug($"Document is not FamilyDocument");
                }
            }
            catch (MissingBindingException e) {
                Log.Error($"!MissingBindingException: {this.GetUpdaterName()} Disabled");
                UpdaterRegistry.DisableUpdater(_uid);
            }
        }
    }
}
