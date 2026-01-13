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
using Autodesk.Revit.DB.Analysis;
using RvtView = Autodesk.Revit.DB.View;

namespace PowerBuilder.IUpdaters {
    internal class CalloutBlockingUpdater : DocumentScopeUpdater {

        protected override string _name => "Callout Blocking Updater";
        protected override string _description => "Dynamic management of element visibility in relation to callout context";
        public override bool LoadOnStartup => true;
        
        static private Definition _KeyParameter;
        


        public CalloutBlockingUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("6550FACB-6B58-4BD2-80B4-D5DC405939FB"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();
            RvtView activeView = doc.ActiveView;
            
            ElementFilter viewerFilter = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);
            List<ElementId> callouts = activeView.GetDependentElements(viewerFilter).ToList();

            if (callouts.Count > 0) {
                foreach (ElementId ChangedElement in data.GetAddedElementIds()) {
                Element e = doc.GetElement(ChangedElement);
                    foreach (ElementId cvid in callouts) {
                        Element cViewer = doc.GetElement(cvid);
                        BoundingBoxXYZ bbox = cViewer.get_BoundingBox(activeView);
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
                        bool checkWithbbcpf = bbcpf.PassesFilter(cViewer); //TODO: use the built in filter instead of this brute force check
                        bool checkBruteForce = BoundingBoxContains(bbox, checkPoint);
                        if (checkBruteForce) {
                            int calloutScale = cViewer.LookupParameter("View Scale").AsInteger();
                            string message = $"set visibility for scale:{calloutScale}";
                            e.get_Parameter(new Guid("561215b4-9b5c-44bf-a162-c875701384d1")).Set(calloutScale);
                        }
                    }
                }
                
            }
            Debug.WriteLine($"IUpdater COMPLETE: {data.GetModifiedElementIds().Count} items changed");
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");

            List<BuiltInCategory> modelCategories = CategoryUtils.GetCategoriesByType(args.Document, CategoryType.Model).ToList();


            //Dictionary<Guid, List<BuiltInCategory>> _requiredParameterBindings = new Dictionary<Guid, List<BuiltInCategory>> {
            //    {new Guid ("561215b4-9b5c-44bf-a162-c875701384d1"), modelCategories },
            //}; TODO: some model categories in this selection are not bindable. there is this difference between true 'model categories'  CategoryType.Model
            Dictionary<Guid, List<BuiltInCategory>> _requiredParameterBindings = new Dictionary<Guid, List<BuiltInCategory>> {
                {new Guid ("561215b4-9b5c-44bf-a162-c875701384d1"), new List<BuiltInCategory> {BuiltInCategory.OST_DuctTerminal,
                                                                                               BuiltInCategory.OST_MechanicalEquipment,
                                                                                               BuiltInCategory.OST_ElectricalFixtures,
                                                                                               BuiltInCategory.OST_DuctCurves,
                                                                                               BuiltInCategory.OST_DuctFitting,
                                                                                               BuiltInCategory.OST_DuctAccessory,
                                                                                                } },
            }; // this should be sufficient for the demonstration.

            DependencyChecker depCheck = new DependencyChecker(args.Document);
            try{
                foreach (KeyValuePair<Guid, List<BuiltInCategory>> binding in _requiredParameterBindings) {
                    Log.Debug($"in {this.GetType().Name} checking {binding.Key.ToString()}");
                    depCheck.ValidateBinding(binding.Key, binding.Value); // does this ever need to create the view filters as well? i suppose not. it just needs to verify that the required parameters are accessible.
                }

                if (!args.Document.IsFamilyDocument) {
                    Log.Debug($"initialized CalloutBlockingUpdater");

                    ElementMulticategoryFilter ModelElementFilter = new ElementMulticategoryFilter(CategoryUtils.GetCategoriesByType(args.Document, CategoryType.Model));
                    ChangeType AddedModelElementChangeType = Element.GetChangeTypeElementAddition();

                    UpdaterRegistry.AddTrigger(_uid, args.Document, ModelElementFilter, AddedModelElementChangeType);
                    Log.Debug($"{this.GetUpdaterName()} trigger registered");
                }
                else {
                    Log.Debug($"Document is FamilyDocument");
                }
            }
            catch (MissingBindingException e) {
                Log.Error($"!MissingBindingException: {this.GetUpdaterName()} Disabled");
                UpdaterRegistry.DisableUpdater(_uid);
            }
        }
        private bool BoundingBoxContains(BoundingBoxXYZ bbox, XYZ point) {
            
            XYZ maxPoint = bbox.Max;
            XYZ minPoint = bbox.Min;

            bool checkX = point.X <= maxPoint.X && point.X >= minPoint.X;
            bool checkY = point.Y <= maxPoint.Y && point.Y >= minPoint.Y;
            bool checkZ = point.Z <= maxPoint.Z && point.Z >= minPoint.Z;

            return checkX && checkY && checkZ;
        }
    }
}
