using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Nice3point.Revit.Extensions;
using PowerBuilder.Extensions;
using PowerBuilder.Interfaces;
using PowerBuilder.Services;

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdUpdateViewTemplatesByViewLayers : IPowerCommand {
        public string DisplayName { get; } = "Update View Templates by Layer";
        public string ShortDesc { get; } = "Sets view accessible view properties according to Templates listed in the parameter \"TemplateLayers\" to each view template." +
            "Graphic Overrides from all Layered Templates are combined." +
            "\nUnmodifiable options:\n" +
            "\tShadows\n\tLighting\n\tPhotographic Exposure\n\n" +
            "Control these independently in the View Template's settings. They are unchanged by this procedure.";
        public bool RibbonIncludeFlag { get; } = true;
        
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements) {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            //TODO
            //  get control parameter "ViewTemplateLayers" from project parameters if it targets Category:Views
            //  may be useful to package this into Utils.ViewsUtils or something like this
            //  do you ever stash this in the Command attributes
            Definition ControlParam = doc.ActiveView.LookupParameter("ViewTemplateLayers").Definition; //update this with a better procedure

            if (ControlParam != null) {
                List<Autodesk.Revit.DB.View> ViewTemplates = new FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.View))
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(vp => vp.IsTemplate).
                    ToList<Autodesk.Revit.DB.View>();

                ViewTemplateViewLayerUpdateManager VTLUM = new ViewTemplateViewLayerUpdateManager(doc, ControlParam);

                using (Transaction T = new Transaction(doc)) {
                    if (T.Start("update-view-templates") == TransactionStatus.Started) {

                        VTLUM.UpdateViewTemplates();
                        T.Commit();
                    }
                    else {
                        T.RollBack();
                    }
                }
            }
            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("method not used");
        }
    }
}
