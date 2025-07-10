#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Extensions;
using PowerBuilder.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdSwapUnitDefinitions : IPowerCommand {
        public string DisplayName { get; } = "Swap Unit Specifications";
        public string ShortDesc { get; } = "Write current Unit Specification and load cached Unit Specification";
        public bool RibbonIncludeFlag { get; } = true;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Log.Debug($"{this.GetType()}");

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            string UnitsXmlName = "UNITS";
            string UnitsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PowerBuilder\\" + UnitsXmlName + ".xml";
            bool ExportControl = false;
            Units docUnits = doc.GetUnits();
            XmlDocument UnitsXml = new XmlDocument();

            //jank fix from https://stackoverflow.com/questions/17795167/xml-loaddata-data-at-the-root-level-is-invalid-line-1-position-1
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

            Log.Debug("TEST-SERIALIZE-XML");

            if (File.Exists(UnitsFile)) {
                
                UnitsXml.Load(UnitsFile);
                Log.Debug("Existing UnitsXML loaded");
                ExportControl = true;
            }
            else {
                Log.Debug($"No units file found.");
                //TODO improve this UX. this should launch a file dialog as follows
                //  OK to create a swap file from active document
                TaskDialog MissingFileDialog = new TaskDialog("Swap Units Specification");
                MissingFileDialog.MainContent = "Swap file not found. Click OK to create swap file from Active Document";
                MissingFileDialog.Show();
                ExportControl = MissingFileDialog.DefaultButton == TaskDialogResult.Ok;
            }
            /* 
            if (ExportControl) {
                docUnits.ExportToXml(UnitsFile);
            }
            */
            if (UnitsXml != new XmlDocument()) {
                using (Transaction T = new Transaction(doc, "load-unit-specifications")) {
                    T.Start();
                    doc.SetUnits(docUnits.ImportFromXml(UnitsXml));
                    T.Commit();
                }
            }

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }

    }
}
