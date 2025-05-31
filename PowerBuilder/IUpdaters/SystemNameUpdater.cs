using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace PowerBuilder.IUpdaters {
    internal class SystemNameUpdater : DocumentScopeUpdater, IUpdater {

        //this can probably implement a parent abstract class "ParameterUpdater"
        //but let's bulid this toy example first.  yeah, like this could be BoolParameterUpdater: ParameterUpdater
        static private UpdaterId _uid;
        static private AddInId _appId;
        static private ChangePriority _changePriority;
        static private ForgeTypeId _KeyParameterTypeId;
        public bool LoadOnStartup { get; set; } = true; //this should be required as part of IPowerUpdater (or base class?)
        public SystemNameUpdater (AddInId id) {
            
            _appId = id;
            _uid = new UpdaterId(_appId, new Guid("3A5573A3-B5AA-4F32-BFEB-1AC801D23EDD"));
        }
        public void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                MEPSystem CurrentSystem = doc.GetElement(ChangedElement) as MEPSystem;
                Element BaseEquipment = CurrentSystem.BaseEquipment as Element;

                if (BaseEquipment != null) {
                    
                    Parameter Name = CurrentSystem.GetParameter(_KeyParameterTypeId);
                    ElementType CurrentSystemType = doc.GetElement(CurrentSystem.GetTypeId()) as ElementType;
                    string ExpectedSystemName = BaseEquipment.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsValueString() +
                        CurrentSystemType.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM).AsValueString();
                    
                    if (Name.AsString() != ExpectedSystemName) {
                        
                        Name.Set(ExpectedSystemName);
                    }
                }
            }
            Debug.WriteLine($"{this.GetType().Name} COMPLETE: {data.GetModifiedElementIds().Count} items changed");
        }
        public string GetUpdaterName() {
            return "Element Verification Updater";
        }
        public UpdaterId GetUpdaterId() {
            return _uid;
        }
        public ChangePriority GetChangePriority() {
            return _changePriority;
        }
        public string GetAdditionalInformation() {
            return "no additional information";
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            
            Log.Debug($"{args.Document.Title} opened");
            try {
                BuiltInParameter KeyParameter = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;
                _KeyParameterTypeId = ParameterUtils.GetParameterTypeId(KeyParameter);

                ElementClassFilter ValidateElementFilter = new ElementClassFilter(typeof(MEPSystem));
                UpdaterRegistry.AddTrigger(_uid, args.Document, ValidateElementFilter, Element.GetChangeTypeAny());
            }
            catch (Exception ex) {
                Log.Debug($"Trigger Added");
            }
        }
        
        public void updater_OnDocumentClosing (object sender, DocumentClosingEventArgs args) {
            UpdaterRegistry.RemoveDocumentTriggers(_uid, args.Document);

            Debug.WriteLine($"-TRIGGER REMOVED: {args.Document.Title}");
        }
    }
}
