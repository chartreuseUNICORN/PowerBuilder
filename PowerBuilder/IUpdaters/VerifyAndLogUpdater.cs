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
    internal class VerifyAndLogUpdater : DocumentScopeUpdater {

        protected override string _name => "Verification Logger";
        protected override string _description => "Update pin status and log entry on verification";
        public override bool LoadOnStartup => false;
        
        static private Definition _KeyParameter;
        
        public VerifyAndLogUpdater (AddInId id) {
            
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("4D7EC7FB-A211-44B9-8F0B-5BA675475F81"));
        }
        public override void Execute (UpdaterData data) {
            
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
        public override void updater_OnDocumentOpened (object sender, DocumentOpenedEventArgs args) {
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
    }
}
