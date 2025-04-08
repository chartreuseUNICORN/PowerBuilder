using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.IUpdaters {
    internal class VerifyAndLogUpdater : IUpdater {

        //this can probably implement a parent abstract class "ParameterUpdater"
        //but let's bulid this toy example first.  yeah, like this could be BoolParameterUpdater: ParameterUpdater
        static private UpdaterId _uid;
        static private AddInId _appId;
        static private ChangePriority _changePriority;
        public bool LoadOnStartup { get; set; } = true; //this should be required as part of IPowerUpdater (or base class?)
        public VerifyAndLogUpdater (AddInId id) {
            _appId = id;
            _uid = new UpdaterId(_appId, new Guid("4D7EC7FB-A211-44B9-8F0B-5BA675475F81"));
        }
        public void Execute (UpdaterData data) {
            
            Document doc = data.GetDocument();

            Definition KeyParameterId = new FilteredElementCollector(doc)
                    .OfClass(typeof(SharedParameterElement))
                    .Cast<SharedParameterElement>()
                    .Where(x => x.GuidValue == new Guid("01db708d-9a82-404a-a4fd-ac6987d06897"))
                    .First().GetDefinition();

            foreach (ElementId ChangedElement in data.GetModifiedElementIds()) {
                Element e = doc.GetElement(ChangedElement);
                
                if (e.Pinned != e.get_Parameter(KeyParameterId).AsBool()) {
                    e.Pinned = e.get_Parameter(KeyParameterId).AsBool();
                }
            }
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
    }
}
