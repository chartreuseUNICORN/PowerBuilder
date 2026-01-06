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
using System.Xml.Linq;

namespace PowerBuilder.IUpdaters {
    /// <summary>
    /// Updater to manage and conform association between Mechanical Equipment families and family instances used to represent controls systems
    /// </summary>
    /// TODO: Configure/Register updaters
    internal class ControlSystemUpdater : DocumentScopeUpdater, IUpdater {

        protected override string _name => "Control System Updater";
        protected override string _description => "Mantain association between equipment and sensors in Control circuits";
        public override bool LoadOnStartup => true;
        static private ForgeTypeId _KeyParameterTypeId;
        public ControlSystemUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("8E28B1E7-1626-4978-82B2-7EB46084C9E8"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();
            Log.Debug(data.ToString());

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                Log.Debug($"ModifiedElement trigger {ChangedElement.ToString()}");
                FamilyInstance CurrentFamilyInstance = doc.GetElement(ChangedElement) as FamilyInstance;

                //this is a lazy fix not using succinct LINQ expression
                HashSet<ElectricalSystem> ControlSystems = new HashSet<ElectricalSystem>();
                foreach (ElectricalSystem SomeSystem in CurrentFamilyInstance.MEPModel.GetElectricalSystems()) {
                    if (SomeSystem.SystemType == ElectricalSystemType.Controls)
                        ControlSystems.Add(SomeSystem);
                }
                
                if (ControlSystems != null) {
                    foreach (ElectricalSystem CurrentSystem in ControlSystems) {
                        UpdateSensorData(CurrentSystem as MEPSystem);
                        Log.Debug($"{CurrentSystem}\t{CurrentSystem.GetType()}\t{CurrentSystem.Id}");
                    }
                }
            }
            // how does this need to be structured with multiple triggers.  this is probably supposed to be like two functions and Map function on the different
            // UpdaterData
            foreach (ElementId ChangedElement in data.GetAddedElementIds()) {
                Log.Debug($"AddedElement trigger {ChangedElement.ToString()}");

                Element CurrentSystem = doc.GetElement(ChangedElement);
                if (CurrentSystem is MEPSystem) {
                    UpdateSensorData(CurrentSystem as MEPSystem);
                    Log.Debug($"{CurrentSystem}\t{CurrentSystem.GetType()}\t{CurrentSystem.Id}");

                }
            }
        }
        internal void UpdateSensorData (MEPSystem TargetSystem) {
            if (TargetSystem.Elements.Size >= 2) {
                Element ControlledElement = null, ControllerElement = null;
                ElectricalSystem TargetElectricalSystem = TargetSystem as ElectricalSystem;
                // TODO: try and refine this some. this is hackey and expecting 2 elements in the set
                foreach (Element e in TargetSystem.Elements) {
                    if (e.Category.BuiltInCategory == BuiltInCategory.OST_MechanicalControlDevices) {
                        ControllerElement = e;
                        Log.Debug($"MechanicalControlDevice:\t{e.Id}");
                    }
                    if (e.Category.BuiltInCategory == BuiltInCategory.OST_MechanicalEquipment) {
                        ControlledElement = e;
                        Log.Debug($"MechanicalEquipment:\t{e.Id}");
                    }
                }
                if (ControllerElement != null && ControlledElement != null) {
                    string ControlledElementName = ControlledElement.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsValueString();
                    ControllerElement.GetParameter(_KeyParameterTypeId).Set(ControlledElementName);
                    TargetElectricalSystem.LoadName = ControlledElementName;
                }
            }
            //does this need to be handled as an exception?
            
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            
            Log.Debug($"{args.Document.Title} opened");
            try {
                BuiltInParameter TriggerParameter = BuiltInParameter.ALL_MODEL_MARK;
                _KeyParameterTypeId = ParameterUtils.GetParameterTypeId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                ElementCategoryFilter ValidateElementFilter = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);
                ElementClassFilter SecondaryElementFilter = new ElementClassFilter(typeof(ElectricalSystem));
                UpdaterRegistry.AddTrigger(_uid, args.Document, ValidateElementFilter, Element.GetChangeTypeParameter(new ElementId(TriggerParameter)));
                UpdaterRegistry.AddTrigger(_uid, args.Document, SecondaryElementFilter, Element.GetChangeTypeElementAddition());
            }
            catch (Exception ex) {
                Log.Debug($"{this.GetType()} Trigger Failed: {ex.Message}");
            }
        }
    }
}
