using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    public class SpaceCalculationService {
        private Document _doc;
        private string _loadFilePath;
        private Dictionary<ElementId, List<FamilyInstance>> _AirTerminalCache;
        public SpaceCalculationService(Document doc) {

            _doc = doc;
            Parameter LoadFileURL = _doc.ProjectInformation.LookupParameter("HVACLoadFile");

            Dictionary<ElementId, List<FamilyInstance>> init_cache = new Dictionary<ElementId, List<FamilyInstance>>();
            
            IEnumerable<IGrouping<ElementId, FamilyInstance>> query = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_DuctTerminal)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .GroupBy(x => x.Space.Id);
            foreach (IGrouping<ElementId,FamilyInstance> result in query) {
                init_cache[result.Key] = result.ToList();
            }
            _AirTerminalCache = init_cache;
            
            if (LoadFileURL == null) _loadFilePath = LoadFileURL.AsValueString();
        }
        //do you ever memoize Air Terminal ElementIds? is there an efficient way of updating this on run.
        //i don't think this actually saves any time. unless it's actually done Functionally and the whole set is updated at once.
        public bool RefreshAirflowDensity (Autodesk.Revit.DB.Mechanical.Space Space) {
            Parameter AirflowDensity = Space.LookupParameter("AirflowDensity");
            if (AirflowDensity != null) {
                Parameter AreaParameter = Space.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter ActualSupplyAirflowParameter = Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM);
                double AirflowDensityValue = ActualSupplyAirflowParameter.AsDouble() / AreaParameter.AsDouble();
                AirflowDensity.Set(AirflowDensityValue);
                return true;
            }
            else {
                return false;
            }
        }
        private double LookupLoadDataByTimeSpaceColumn (string SpaceLoadHour, string SpaceId, int col) {

            throw new NotImplementedException("Method: LookupLoadDataByTimeSpaceColumn");
        }
        public void RefreshSpecifiedHeatingAndCoolingLoad() {
            //TODO: decide expected formatting for this.  It may actually make more sense to just let this be an integer hour, or MM.DD.HH
            //  a necessary expansion will be trying to interpret how this needs to work for multiple 
            throw new NotImplementedException("Method: RefreshSpecifiedHeatingAndCoolingLoad not implemented");
            
            Dictionary<BuiltInParameter, int> BipCsvLookupMap = new Dictionary<BuiltInParameter, int>(){
                { BuiltInParameter.ROOM_DESIGN_COOLING_LOAD_PARAM, 1 },
                { BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM, 2 },
                { BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM, 3 },
                }; // TODO: this wants to be more dynamic

            string DateString = _doc.ProjectInformation.LookupParameter("LoadResultTime").AsValueString();
            IList<Element> Spaces = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_MEPSpaces).ToElements();
            foreach (Element Space in Spaces) {
                foreach (KeyValuePair<BuiltInParameter, int> kvp in BipCsvLookupMap) {
                    Space.get_Parameter(kvp.Key).Set(LookupLoadDataByTimeSpaceColumn(DateString, Space.Name, kvp.Value));
                }
            }
        }
        public void RefreshPressureBalance (Autodesk.Revit.DB.Mechanical.Space Space) {
            Parameter SpacePressureBalance = Space.LookupParameter("SpacePressureBalance");
            if (SpacePressureBalance != null) {
                SpacePressureBalance.Set(
                    Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM).AsDouble()-
                    (Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_RETURN_AIRFLOW_PARAM).AsDouble()+
                    Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_EXHAUST_AIRFLOW_PARAM).AsDouble())
                    );
            }
        }
        public void SyncSpecifiedAirflowToActual (Autodesk.Revit.DB.Mechanical.Space Space) {
            
            List<FamilyInstance> AirTerminals = _AirTerminalCache[Space.Id];
            SetRoundedAirflowToElements(Space, AirTerminals);
        }
        //does this take a unit type as an argument
        private void SetRoundedAirflowToElements (Autodesk.Revit.DB.Mechanical.Space Space, IEnumerable<Element> AirTerminals) {
            Parameter SpecifiedAirflow = Space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM);
            ForgeTypeId AirflowUnit = SpecifiedAirflow.GetUnitTypeId();
            double SpecifiedAirflowValue = SpecifiedAirflow.AsDouble().ToUnit(AirflowUnit);
            
            int AirTerminalQuantity = AirTerminals.Count();

            //this does rely on a common airflow value
            //COULD make this search the connectors to find the driving parameter
            //TODO: update this to round to the nearest 5 in the current display units
            double RoundedAirflow = Math.Ceiling(SpecifiedAirflowValue / 5.0)*5;
            double NewAirflow = RoundedAirflow / (double)AirTerminalQuantity;
            double NewAirflowCeil = Math.Ceiling(NewAirflow/5.0)*5;
            double NewAirflowFloor = NewAirflowCeil - 5.0;
            //int QtyCeil = ((AirTerminalQuantity - (RoundedAirflow / NewAirflowFloor)) / (1 - (NewAirflowCeil / NewAirflowFloor)));
            int QtyFloor = (int)RoundedAirflow % AirTerminalQuantity;
            int QtyCeil = AirTerminalQuantity - QtyFloor;

            List<double> NewAirflowValues = new List<double>(Enumerable.Repeat(NewAirflowCeil, QtyCeil));
            Element[] AirTerminalArray = AirTerminals.ToArray();
            NewAirflowValues.AddRange(Enumerable.Repeat(NewAirflowFloor, QtyFloor));
            
            for (int i  =0; i < AirTerminals.Count(); i++) {
                Element AT = AirTerminalArray[i];
                Parameter FlowParam = AT.LookupParameter("Flow"); //TODO: modify this identify the parameter connected to the duct connector(?)
                if (FlowParam != null) {
                    
                    FlowParam.Set(UnitUtils.ConvertToInternalUnits(NewAirflowValues[i], AirflowUnit));
                }
            }
        }
    }
}
