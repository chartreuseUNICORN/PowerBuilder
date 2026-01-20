using Autodesk.Revit.DB.Events;
using PowerBuilder.Services;
using Serilog;
using System.Diagnostics;

namespace PowerBuilder.IUpdaters {
    internal class CalloutBlockingElementUpdater : DocumentScopeUpdater {

        protected override string _name => "Callout Blocking Updater (Element)";
        protected override string _description => "Dynamic management of element visibility relative to callouts";
        public override bool LoadOnStartup => true;

        public CalloutBlockingElementUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("6550FACB-6B58-4BD2-80B4-D5DC405939FB"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();
            CalloutBlockingManager cbm = new CalloutBlockingManager(doc, doc.ActiveView, false);

            if (cbm.isEnabled) {
                
                List<ElementId> managedElementIds = data.GetAddedElementIds().ToList();
                managedElementIds.AddRange(data.GetModifiedElementIds());

                if (managedElementIds.Count != 0) {
                    if (managedElementIds.Count > 1)
                        cbm.AssignElementsVisibility(managedElementIds);
                    else
                        cbm.AssignElementVisibility(managedElementIds.First());
                }
            }
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");

            if (!args.Document.IsFamilyDocument) {
                Log.Debug($"initialized CalloutBlockingUpdater-Element");

                List<BuiltInCategory> TestCategoryTargets = new List<BuiltInCategory> {BuiltInCategory.OST_DuctTerminal,
                    BuiltInCategory.OST_MechanicalEquipment,
                    BuiltInCategory.OST_ElectricalFixtures,
                    BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_DuctAccessory,
                    };
                ElementMulticategoryFilter ModelElementFilter = new ElementMulticategoryFilter(TestCategoryTargets);
                //ElementMulticategoryFilter ModelElementFilter = new ElementMulticategoryFilter(CategoryUtils.GetCategoriesByType(args.Document, CategoryType.Model));


                UpdaterRegistry.AddTrigger(_uid, args.Document, ModelElementFilter, Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(_uid, args.Document, ModelElementFilter, Element.GetChangeTypeGeometry());

                Log.Debug($"{this.GetUpdaterName()} trigger registered");
            }
            else {
                Log.Debug($"Document is FamilyDocument");
            }
        }
    }
}
