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
    public class SpaceUpdater : DocumentScopeUpdater {

        protected override string _name => "Space Updater";
        protected override string _description => "Update changed spaces with custom calculations";
        public override bool LoadOnStartup => true;
        
        static private SpaceCalculationService _spaceCalculationService;
        /// <summary>
        /// Updater to enforce relationships between spatial data and PowerBuilder parameters. Currently generates airflow density, pressure balance, and distributes specified airflow over all Air Terminals in the space.
        /// </summary>
        /// <param name="id">AddInId</param>
        public SpaceUpdater(AddInId id)
        {
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("86390EF0-9246-4CFD-B5B5-E59F0ECA89D4"));
        }
        public override void Execute(UpdaterData data) {
            //TODO: there is an issue where this updater doesn't trigger when air terminals with 0 cfm flow are moved into the space.
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
