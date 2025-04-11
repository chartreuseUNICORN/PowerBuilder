#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdDisableScheduleUnits : IPowerCommand {
        public string DisplayName { get; } = "Disable Schedule Units";
        public string ShortDesc { get; } = "Set all numeric Schedule Fields to use the ForgeTypeId <empty>";
        public bool RibbonIncludeFlag { get; } = true;
        private Document _doc;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            _doc = doc;

            PowerDialogResult res = GetInput(uiapp);
            Autodesk.Revit.DB.View ThisActiveView = res.SelectionResults[0] as Autodesk.Revit.DB.View;
            if ( ThisActiveView.ViewType == ViewType.Schedule) {
                using (Transaction Tx = new Transaction(doc)) {

                    Tx.Start("disable-schedule-units");
                    try {
                        DisableScheduleUnits(ThisActiveView as ViewSchedule);
                        Tx.Commit();
                    }
                    catch {
                        Tx.RollBack();
                    }
                    
                }
            }

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            PowerDialogResult res = new PowerDialogResult();

            UIDocument uidoc = uiapp.ActiveUIDocument;
            res.AddSelectionResult(uidoc.ActiveView);
            return res;
        }
        public void DisableScheduleUnits(Autodesk.Revit.DB.ViewSchedule ScheduleView) {
            ScheduleDefinition SchDef = ScheduleView.Definition;
            DisableUnitsByScheduleDefinition(SchDef);

            if (SchDef.EmbeddedDefinition != null) {
                DisableUnitsByScheduleDefinition(SchDef.EmbeddedDefinition);
            }
        }
        public void DisableUnitsByScheduleDefinition (ScheduleDefinition SchDef) {

            ForgeTypeId EmptySymbolTypeId = new ForgeTypeId("");

            for (int i = 0; i < SchDef.GetFieldCount() - 1; i++) {
                
                ScheduleField CurrentScheduleField = SchDef.GetField(i);
                FormatOptions csfOptions = CurrentScheduleField.GetFormatOptions();
                ForgeTypeId csfSpecTypeId = CurrentScheduleField.GetSpecTypeId();
                
                if (CurrentScheduleField.GetSpecTypeId().TypeId != "") {

                    if (csfOptions.UseDefault) {
                        
                        Units docUnits = _doc.GetUnits();
                        csfOptions = docUnits.GetFormatOptions(csfSpecTypeId);
                        csfOptions.UseDefault = false;
                    }
                    ForgeTypeId csfUnitTypeId = csfOptions.GetUnitTypeId();

                    bool check = UnitUtils.IsValidUnit(csfSpecTypeId, EmptySymbolTypeId);
                    csfOptions.SetSymbolTypeId(EmptySymbolTypeId);
                    
                    CurrentScheduleField.SetFormatOptions(csfOptions);
                }
                
            }
        }
    }
}
