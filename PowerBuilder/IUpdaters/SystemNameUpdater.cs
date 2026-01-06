using Autodesk.Revit.DB.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Autodesk.Revit.DB.Electrical;

namespace PowerBuilder.IUpdaters {
    internal class SystemNameUpdater : DocumentScopeUpdater {
        protected override string _name => "System Name Updater";
        protected override string _description => "Set System Name when base equipment is assigned";
        public override bool LoadOnStartup => true;
        private ForgeTypeId _KeyParameterTypeId;
        
        /// <summary>
        /// Updater to set system names when base equipment is assigned
        /// </summary>
        /// <param name="id"></param>
        public SystemNameUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("3A5573A3-B5AA-4F32-BFEB-1AC801D23EDD"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                MEPSystem CurrentSystem = doc.GetElement(ChangedElement) as MEPSystem;
                Element BaseEquipment = CurrentSystem.BaseEquipment as Element;

                //TODO: this needs a better way to run if the base equipment is assigned, but the Mark value is changed.
                //TODO: also can adjust this to accept a variable EquipmentIdParameter
                if (BaseEquipment != null && CurrentSystem is not ElectricalSystem) {
                    
                    Parameter Name = CurrentSystem.GetParameter(_KeyParameterTypeId);
                    ElementType CurrentSystemType = doc.GetElement(CurrentSystem.GetTypeId()) as ElementType;
                    //there is also this edge case where an apparatus may have multiple connectors and be the base equipment for multiple systems
                    //consider older multi-zone equipments where you might name AHU-1_SA for multiple SA systems.
                    List<string> NameParts = new List<string>() { 
                        CurrentSystemType.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM).AsValueString(),
                        BaseEquipment.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsValueString()
                    };
                    string ExpectedSystemName = String.Join("_", NameParts);
                    
                    if (Name.AsString() != ExpectedSystemName) {
                        Name.Set(ExpectedSystemName);
                    }
                }
            }
            Log.Debug($"{this.GetType().Name} COMPLETE: {data.GetModifiedElementIds().Count} items changed");
        }
        
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {

            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");
            try {
                BuiltInParameter KeyParameter = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;
                _KeyParameterTypeId = ParameterUtils.GetParameterTypeId(KeyParameter);

                ElementClassFilter ValidateElementFilter = new ElementClassFilter(typeof(MEPSystem));
                UpdaterRegistry.AddTrigger(_uid, args.Document, ValidateElementFilter, Element.GetChangeTypeAny());
            }
            catch (Exception ex) {
                Log.Error($"{this.GetUpdaterName()}: {ex.Message}");
            }
        }
    }
}
