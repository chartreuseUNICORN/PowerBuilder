#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Extensions;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdTEST : CmdBase{
        public override string DisplayName { get; } = "TEST FUNCTION";
        public override string ShortDesc { get; } = "Container command for testing logic";
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
            string UnitsXmlName = "UNITS";
            string UnitsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PowerBuilder\\" + UnitsXmlName + ".xml";

            Log.Debug($"{this.GetType()}");
            
            Log.Debug("TEST-SERIALIZE-XML");
            Units docUnits = doc.GetUnits();
            docUnits.ExportToXml(UnitsFile);
            //IList<ForgeTypeId> modifiableSpecs = Units.GetModifiableSpecs();
            /*
            foreach (ForgeTypeId currentSpec in modifiableSpecs) {
                Log.Debug($"{currentSpec.TypeId}");
                FormatOptions fo = docUnits.GetFormatOptions(currentSpec);
                Log.Debug("FORMAT OPTIONS");
                Log.Debug($"\tSymbolTypeId:\t{fo.GetSymbolTypeId().TypeId}");
                Log.Debug($"\tUnitTypeId:\t{fo.GetUnitTypeId()}");
                Log.Debug($"\tAccuracy:\t{fo.Accuracy}");
                Log.Debug($"\tRoundingMethod:\t{fo.RoundingMethod.ToString()}");
                Log.Debug($"\tSuppressLeadingZeroes:\t{fo.SuppressLeadingZeros}");
                Log.Debug($"\tSUppressSpaces:\t{fo.SuppressSpaces}");
                Log.Debug($"\tSuppressTrailingZeroes:\t{fo.SuppressTrailingZeros}");
                Log.Debug($"\tUseDefault:\t{fo.UseDefault}");
                Log.Debug($"\tUseDigitGrouping:\t{fo.UseDigitGrouping}");
                Log.Debug($"\tUsePlusPrefix:\t{fo.UsePlusPrefix}");
            }*/

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            throw new NotImplementedException("No input collection required");
        }
    }
}
