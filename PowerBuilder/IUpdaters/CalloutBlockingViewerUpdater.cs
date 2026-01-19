using Autodesk.Revit.DB.Events;
using PowerBuilder.Infrastructure;
using PowerBuilder.Services;
using PowerBuilder.Utils;
using Serilog;
using System.Diagnostics;

namespace PowerBuilder.IUpdaters {
    internal class CalloutBlockingViewerUpdater : DocumentScopeUpdater {

        protected override string _name => "Callout Blocking Updater (Viewer)";
        protected override string _description => "Dynamic management of element visibility in relation to callout changes";
        public override bool LoadOnStartup => true;

        public CalloutBlockingViewerUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("4F67EC49-5D7A-4A18-97C6-52368F393278"));
        }
        public override void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();
            CalloutBlockingManager cbm = new CalloutBlockingManager(doc, doc.ActiveView, false);
            

            if (cbm.isEnabled) {
                //List<BuiltInCategory> modelElementCats = CategoryUtils.GetCategoriesByType(doc, CategoryType.Model).ToList();
                List<BuiltInCategory> modelElementCats = CategoryUtils.GetMepModeElements();
                ElementFilter modelElementFilter = new ElementMulticategoryFilter(modelElementCats);

                ElementFilter isInsideViewerFilter = cbm.GetInsideViewerFilter();

                foreach (ElementId eid in data.GetAddedElementIds()) {
                    ICollection<ElementId> managedCalloutElements = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .WhereElementIsNotElementType()
                    .WherePasses(modelElementFilter)
                    .WherePasses(isInsideViewerFilter)
                    .ToElementIds();

                    cbm.AssignElementsVisibility(managedCalloutElements);
                }
                foreach (ElementId eid in data.GetDeletedElementIds()) {
                    ICollection<ElementId> managedCalloutElements = cbm.GetManagedElements();

                    cbm.AssignElementsVisibility(managedCalloutElements);
                }
            }
            
            Debug.WriteLine($"IUpdater COMPLETE: {data.GetModifiedElementIds().Count} items changed");
        }
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{this.GetUpdaterName()} | document opened @ {args.Document.Title}");

            if (!args.Document.IsFamilyDocument) {
                Log.Debug($"initialized CalloutBlockingUpdater-Viewer");

                ElementCategoryFilter ViewerFilter = new ElementCategoryFilter(BuiltInCategory.OST_Viewers);
                UpdaterRegistry.AddTrigger(_uid, args.Document, ViewerFilter, Element.GetChangeTypeGeometry());
                UpdaterRegistry.AddTrigger(_uid, args.Document, ViewerFilter, Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(_uid, args.Document, ViewerFilter, Element.GetChangeTypeElementDeletion());

                Log.Debug($"{this.GetUpdaterName()} trigger registered");
            }
            else {
                Log.Debug($"Document is FamilyDocument");
            }
        }
    }
}
