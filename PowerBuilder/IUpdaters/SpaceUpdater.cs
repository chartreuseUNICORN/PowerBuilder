using Autodesk.Revit.DB.Events;
using PowerBuilder.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.IUpdaters {
    public class SpaceUpdater : DocumentScopeUpdater, IUpdater {

        static private ChangePriority _ChangePriority;
        public SpaceUpdater(AddInId id) {
            _appId = id;
            _uid = new UpdaterId(_appId, new Guid("86390EF0-9246-4CFD-B5B5-E59F0ECA89D4"));
        }
        public void Execute(UpdaterData data) {
            Document doc = data.GetDocument();
            try {
                SpaceCalculationService SCS = new SpaceCalculationService(doc);

                foreach (ElementId eid in data.GetModifiedElementIds()) {
                    Autodesk.Revit.DB.Mechanical.Space e = doc.GetElement(eid) as Autodesk.Revit.DB.Mechanical.Space;
                    
                    SCS.RefreshAirflowDensity(e);
                    SCS.RefreshPressureBalance(e);
                    SCS.SyncSpecifiedAirflowToActual(e);
                }
            }
            catch (Exception e) {
                Debug.WriteLine(e.ToString());
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
            return "no additional information";
        }

        public override void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args) {
            
            try {
                if (!args.Document.IsFamilyDocument) {
                    ElementClassFilter SpaceElementFilter = new ElementClassFilter(typeof(Autodesk.Revit.DB.SpatialElement));
                    ChangeType MultiChangeType = ChangeType.ConcatenateChangeTypes(
                        Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM)),
                        Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM)));
                        
                    UpdaterRegistry.AddTrigger(_uid, args.Document, SpaceElementFilter, MultiChangeType);
                }
            }
            catch (Exception e) {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
