using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nice3point.Revit.Extensions;
using Autodesk.Revit.DB;
using PowerBuilder.Exceptions;
using Serilog;

namespace PowerBuilder.Services {
    public class SpaceCalculationService
    {
        private Document _doc;
        private string _loadFilePath;
        private Dictionary<Guid, List<BuiltInCategory>> _requiredParameterBindings = new Dictionary<Guid, List<BuiltInCategory>> {
            {new Guid ("80ad4f1a-e59d-4765-bb11-e6e9169d5980"), new List<BuiltInCategory> {BuiltInCategory.OST_MEPSpaces} },
            {new Guid ("254b3d4f-3a06-403a-ada8-7dd565bb8d8f"), new List<BuiltInCategory> {BuiltInCategory.OST_MEPSpaces} },
            {new Guid ("5475d3ff-7866-4d03-9474-db0253a6d341"), new List<BuiltInCategory> {BuiltInCategory.OST_ProjectInformation } },
        };
        private Dictionary<ElementId, List<FamilyInstance>> _AirTerminalCache;
        public SpaceCalculationService(Document doc)
        {
            _doc = doc;

            // so what is this actually doing.  if the dependency validation fails, how do you disable 
            // related services and unregister things like IUpdaters
            // i guess this is where we use Exceptions to control the code.
            DependencyChecker depCheck = new DependencyChecker(_doc);
            foreach (KeyValuePair<Guid, List<BuiltInCategory>> binding in _requiredParameterBindings) {
                Log.Debug($"in {this.GetType().Name} checking {binding.Key.ToString()}");
                depCheck.ValidateBinding(binding.Key, binding.Value);
            }
            
            Parameter LoadFileURL = _doc.ProjectInformation.LookupParameter("HVACLoadFile");

            if (LoadFileURL == null) _loadFilePath = LoadFileURL.AsValueString();
        }
        //do you ever memoize Air Terminal ElementIds? is there an efficient way of updating this on run.
        //i don't think this actually saves any time. unless it's actually done Functionally and the whole set is updated at once.
        public Dictionary<Guid, List<BuiltInCategory>> RequiredBindings { get => _requiredParameterBindings; }
        public bool RefreshAirflowDensity(Autodesk.Revit.DB.Mechanical.Space Space)
        {
            Parameter AirflowDensity = Space.LookupParameter("AirflowDensity");
            if (AirflowDensity != null)
            {
                Parameter AreaParameter = Space.get_Parameter(BuiltInParameter.ROOM_AREA);
                Parameter ActualSupplyAirflowParameter = Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM);
                double AirflowDensityValue = ActualSupplyAirflowParameter.AsDouble() / AreaParameter.AsDouble();
                AirflowDensity.Set(AirflowDensityValue);
                return true;
            }
            else
            {
                return false;
            }
        }
        private double LookupLoadDataByTimeSpaceColumn(string SpaceLoadHour, string SpaceId, int col)
        {

            throw new NotImplementedException("Implement Method: LookupLoadDataByTimeSpaceColumn");
        }
        public void RefreshSpecifiedHeatingAndCoolingLoad()
        {
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
            foreach (Element Space in Spaces)
            {
                foreach (KeyValuePair<BuiltInParameter, int> kvp in BipCsvLookupMap)
                {
                    Space.get_Parameter(kvp.Key).Set(LookupLoadDataByTimeSpaceColumn(DateString, Space.Name, kvp.Value));
                }
            }
        }
        public void RefreshPressureBalance(Autodesk.Revit.DB.Mechanical.Space Space)
        {
            Parameter SpacePressureBalance = Space.LookupParameter("AirflowBalance");
            if (SpacePressureBalance != null)
            {
                SpacePressureBalance.Set(
                    Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM).AsDouble() -
                    (Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_RETURN_AIRFLOW_PARAM).AsDouble() +
                    Space.get_Parameter(BuiltInParameter.ROOM_ACTUAL_EXHAUST_AIRFLOW_PARAM).AsDouble())
                    );
            }
        }
        public void CacheAirTerminals() {
            Dictionary<ElementId, List<FamilyInstance>> init_cache = new Dictionary<ElementId, List<FamilyInstance>>();

            IEnumerable<IGrouping<ElementId, FamilyInstance>> query = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_DuctTerminal)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .GroupBy(x => x.Space.Id);
            
            //TODO: this currently is incapable of handling the grouping when Space.Id == null
            foreach (IGrouping<ElementId, FamilyInstance> result in query) {
                init_cache[result.Key] = result.ToList();
            }
            _AirTerminalCache = init_cache;
        }
        public void SyncSpecifiedAirflowToActual(Autodesk.Revit.DB.Mechanical.Space Space)
        {
            //TODO: there is an issue with this calculation not functioning correctly.  miscalculating to result in total airflows 5-15cfm greater
            List<FamilyInstance> AirTerminals = _AirTerminalCache[Space.Id].Where(x => x.LookupParameter("System Classification").AsValueString() == "Supply Air").ToList();
            SetRoundedAirflowToElements(Space, AirTerminals);
        }
        
        private void SetRoundedAirflowToElements(Autodesk.Revit.DB.Mechanical.Space Space, IEnumerable<Element> AirTerminals)
        {
            Parameter SpecifiedAirflow = Space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM);
            ForgeTypeId AirflowUnit = SpecifiedAirflow.GetUnitTypeId();
            double SpecifiedAirflowValue = UnitUtils.ConvertFromInternalUnits(SpecifiedAirflow.AsDouble(), AirflowUnit);

            int AirTerminalQuantity = AirTerminals.Count();

            //this does rely on a common airflow value
            //COULD make this search the connectors to find the driving parameter
            //TODO: update this to round to the nearest 5 in the current display units
            double RoundedAirflow = Math.Ceiling(SpecifiedAirflowValue / 5.0) * 5;
            double NewAirflow = RoundedAirflow / (double)AirTerminalQuantity;
            double NewAirflowCeil = Math.Ceiling(NewAirflow / 5.0) * 5;
            double NewAirflowFloor = NewAirflowCeil - 5.0;
            //int QtyCeil = ((AirTerminalQuantity - (RoundedAirflow / NewAirflowFloor)) / (1 - (NewAirflowCeil / NewAirflowFloor)));
            int QtyFloor = (int)RoundedAirflow % AirTerminalQuantity;
            int QtyCeil = AirTerminalQuantity - QtyFloor;

            List<double> NewAirflowValues = new List<double>(Enumerable.Repeat(NewAirflowCeil, QtyCeil));
            Element[] AirTerminalArray = AirTerminals.ToArray();
            NewAirflowValues.AddRange(Enumerable.Repeat(NewAirflowFloor, QtyFloor));

            for (int i = 0; i < AirTerminals.Count(); i++)
            {
                Element AT = AirTerminalArray[i];
                Parameter FlowParam = AT.LookupParameter("Flow"); //TODO: modify this identify the parameter connected to the duct connector(?)
                if (FlowParam != null)
                {

                    FlowParam.Set(UnitUtils.ConvertToInternalUnits(NewAirflowValues[i], AirflowUnit));
                }
            }
        }
    }
}
