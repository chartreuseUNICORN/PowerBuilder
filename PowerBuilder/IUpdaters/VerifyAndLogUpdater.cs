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
    internal class VerifyAndLogUpdater : IUpdater {

        //this can probably implement a parent abstract class "ParameterUpdater"
        //but let's bulid this toy example first.  yeah, like this could be BoolParameterUpdater: ParameterUpdater
        static private UpdaterId _uid;
        static private AddInId _appId;
        static private ChangePriority _changePriority;
        static private Definition _KeyParameter;
        public bool LoadOnStartup { get; set; } = true; //this should be required as part of IPowerUpdater (or base class?)
        public VerifyAndLogUpdater (AddInId id) {
            
            _appId = id;
            _uid = new UpdaterId(_appId, new Guid("4D7EC7FB-A211-44B9-8F0B-5BA675475F81"));
        }
        public void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                Element e = doc.GetElement(ChangedElement);
                bool checkState = e.get_Parameter(_KeyParameter).AsBool();
                Debug.WriteLine($"{e.Id} Pinned: {e.Pinned} | isVerified:{checkState}");
                if (e.Pinned != checkState) {
                    e.Pinned = checkState;
                }
            }
            Debug.WriteLine($"IUpdater COMPLETE: {data.GetModifiedElementIds().Count} items changed");
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
        public void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
            Log.Debug($"{args.Document.Title} opened: ADD TRIGGER VerifyAndLogUpdater");
            SharedParameterElement KeyParameterElement = new FilteredElementCollector(args.Document)
                    .OfClass(typeof(SharedParameterElement))
                    .Cast<SharedParameterElement>()
                    .Where(x => x.GuidValue == new Guid("01db708d-9a82-404a-a4fd-ac6987d06897"))
                    .First();
            if (KeyParameterElement != null) {
                _KeyParameter = KeyParameterElement.GetDefinition();

                ElementClassFilter ValidateElementFilter = new ElementClassFilter(typeof(FamilyInstance));
                UpdaterRegistry.AddTrigger(_uid, args.Document, ValidateElementFilter, Element.GetChangeTypeParameter(KeyParameterElement.Id));

                Log.Debug($"VerifyAndLogUpdater:\tkey parameter: {KeyParameterElement.Name} found => TRIGGER ADDED");
            }
        }
        
        public void updater_OnDocumentClosing (object sender, DocumentClosingEventArgs args) {
            UpdaterRegistry.RemoveDocumentTriggers(_uid, args.Document);

            Debug.WriteLine($"-TRIGGER REMOVED: {args.Document.Title}");
        }
    }
}
