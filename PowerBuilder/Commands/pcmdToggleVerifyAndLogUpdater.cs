using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using PowerBuilder.Interfaces;
using PowerBuilder.Extensions;
using Autodesk.Revit.Attributes;

namespace PowerBuilder.Commands {
    [Transaction(TransactionMode.Manual)]
    public class pcmdToggleVerifyAndLogUpdater : IPowerCommand {

        public string DisplayName { get; } = "Toggle VerifyAndLog Updater";
        public string ShortDesc { get; } = $"Toggle VerifyAndLog utility state.";
        public bool RibbonIncludeFlag { get; } = true;

        private Guid _TargetUIpdaterId = new  Guid( "4D7EC7FB-A211-44B9-8F0B-5BA675475F81");
        private bool _IsInitialRun = false;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //change this to ElementCategoryFilter.  use CategoryUtils.GetProjectParameterCategories method for BIC targets
            ElementClassFilter ValidateElementFilter = new ElementClassFilter(typeof(FamilyInstance));
            
            ElementId ValidationParameterId = new FilteredElementCollector(doc)
                    .OfClass(typeof(SharedParameterElement))
                    .Cast<SharedParameterElement>()
                    .Where(x => x.GuidValue == new Guid("01db708d-9a82-404a-a4fd-ac6987d06897"))
                    .First().Id;

            UpdaterId TargetUpdater = new UpdaterId(app.ActiveAddInId, _TargetUIpdaterId);

            if (UpdaterRegistry.IsUpdaterEnabled(TargetUpdater)) {
                UpdaterRegistry.RemoveDocumentTriggers(TargetUpdater, doc);
                UpdaterRegistry.DisableUpdater(TargetUpdater);
                Debug.WriteLine("V&L Updater DISABLED");
            }
            else {
                UpdaterRegistry.AddTrigger(TargetUpdater, ValidateElementFilter, Element.GetChangeTypeParameter(ValidationParameterId));
                UpdaterRegistry.EnableUpdater(TargetUpdater);
                Debug.WriteLine("V&L Updater ENABLED");
            }
            
            

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }

        
    }
}
