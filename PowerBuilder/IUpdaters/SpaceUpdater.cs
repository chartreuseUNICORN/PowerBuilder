using Autodesk.Revit.DB.Events;
using PowerBuilder.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using PowerBuilder.Exceptions;

namespace PowerBuilder.IUpdaters {
    /// <summary>
    /// IUpdater to enforce relationships between spatial data and PowerBuilder parameters
    /// </summary>
    public class SpaceUpdater : DocumentScopeUpdater, IUpdater {

        static private ChangePriority _ChangePriority;
        static private SpaceCalculationService _spaceCalculationService;
        public SpaceUpdater(AddInId id)
        {
            _appId = id;
            _uid = new UpdaterId(_appId, new Guid("86390EF0-9246-4CFD-B5B5-E59F0ECA89D4"));
        }
        public void Execute(UpdaterData data) {
            Document doc = data.GetDocument();
            try {
                SpaceCalculationService SCS = _spaceCalculationService;
                SCS.CacheAirTerminals();

                foreach (ElementId eid in data.GetModifiedElementIds()) {
                    Autodesk.Revit.DB.Mechanical.Space e = doc.GetElement(eid) as Autodesk.Revit.DB.Mechanical.Space;

                    SCS.SyncSpecifiedAirflowToActual(e);
                    SCS.RefreshAirflowDensity(e);
                    SCS.RefreshPressureBalance(e);
                }
            }
            catch (Exception e) {
                Log.Error($"SpaceUpdater: {e.Message}");
            }
        }
        public string GetUpdaterName() {
            return "Space Updater";
        }
        public UpdaterId GetUpdaterId() {
            return _uid;
        }

        public ChangePriority GetChangePriority() {
            return _ChangePriority;
        }
        public string GetAdditionalInformation() {
            return "Manage space-airflow relationships";
        }

        public override void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");
            
            try {
                if (!args.Document.IsFamilyDocument) {
                    Log.Debug($"Document is not FamilyDocument");
                    _spaceCalculationService = new SpaceCalculationService(args.Document);
                    Log.Debug($"initialized SpaceCalculationService");

                    ElementClassFilter SpaceElementFilter = new ElementClassFilter(typeof(Autodesk.Revit.DB.SpatialElement));
                    ChangeType MultiChangeType = ChangeType.ConcatenateChangeTypes(
                        Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM)),
                        Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM)));
                    
                    UpdaterRegistry.AddTrigger(_uid, args.Document, SpaceElementFilter, MultiChangeType);
                    Log.Debug($"{this.GetUpdaterName()} trigger registered");
                }
            }
            catch (MissingBindingException e) {
                Log.Error($"!MissingBindingException: {this.GetUpdaterName()} Disabled");
                UpdaterRegistry.DisableUpdater(_uid);
            }
        }
    }
}
