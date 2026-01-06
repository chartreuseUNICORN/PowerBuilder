#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using PowerBuilder.Infrastructure;
using Serilog;
using RevitTaskDialog = Autodesk.Revit.UI.TaskDialog;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdHelloWorld : CmdBase{
        public override string DisplayName { get; } = "Hello World!";
        public override string ShortDesc { get; } = "A standard Hello, World command in Revit";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Log.Information("IExternalCommand: {Name}", DisplayName);
            RevitTaskDialog.Show("Command1", "Hello World");

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
