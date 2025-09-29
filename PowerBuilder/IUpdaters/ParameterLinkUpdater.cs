using Autodesk.Revit.DB.Events;
using PowerBuilder.Extensions;
using PowerBuilder.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PowerBuilder.Infrastructure;
using System.Runtime.InteropServices;

namespace PowerBuilder.IUpdaters {
    
    public class ParameterLinkUpdater : DocumentScopeUpdater {

        protected override string _name => "Parameter Link Updater";
        protected override string _description => "Enforce value equality between two parameters";
        public override bool LoadOnStartup => true;
        
        private Dictionary<ElementId, ElementId> _linkedParameters;
        
        /// <summary>
        /// Updater that enforces value equality between pairs of registered parameters
        /// </summary>
        /// <param name="id">AddInId</param>
        public ParameterLinkUpdater(AddInId id) {
            _addInId = id;
            _uid = new UpdaterId(_addInId, new Guid("43FDE7A2-2B2D-4EDC-B33F-5EE99435044E"));
        }
        public override void Execute(UpdaterData data) {
            Document doc = data.GetDocument();
            
            foreach (ElementId eid in data.GetModifiedElementIds()) {
                Element e = doc.GetElement(eid);
                //KeyValuePair<ElementId,ElementId> LinkedParameterIds = _linkedParameters.Where(p => data.IsChangeTriggered(eid, Element.GetChangeTypeParameter(p.Key)));
                KeyValuePair<ElementId, ElementId>? TriggeredPair = _linkedParameters
                    .Where(lp => data.IsChangeTriggered(eid, Element.GetChangeTypeParameter(lp.Key)))
                    .Cast<KeyValuePair<ElementId, ElementId>?>()
                    .FirstOrDefault();
                ForgeTypeId Source = GetForgeTypeIdFromElementId(doc, TriggeredPair.Value.Key);
                ForgeTypeId Sink = GetForgeTypeIdFromElementId(doc, TriggeredPair.Value.Value);

                Parameter SinkParameter = e.GetParameter(Sink);
                Parameter SourceParameter = e.GetParameter(Source);

                Debug.WriteLine($"PLU Trigger on {eid}: {Sink.ToLabel()} -linkedTo- {Source.ToLabel()}");
                //SinkParameter.Match(SourceParameter);
                
            }
        }

        public override void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args) {

            //i think this has to use one updater, but configure the triggers according to the mapping file.
            Log.Debug($"ParameterLinkUpdater\tOnDocumentOpened:{args.Document.Title}");
            Parameter ParameterLinkerMap = args.Document.ProjectInformation.get_Parameter(new Guid("c28bbda7-5445-408f-a4ce-edbe2793ab97"));
            Log.Debug($"ParameterLinkerMap:\t{ParameterLinkerMap.AsValueString()}");
            if (ParameterLinkerMap != null) {
                Log.Debug("ParameterLinkerMap found");
                string filepath = ParameterLinkerMap.AsValueString();

                
                Dictionary<string, List<string>> WatchedParameterMap = GetParameterMapFromPath(filepath);

                //ElementClassFilter TargetElementFilter = new ElementClassFilter(typeof(Autodesk.Revit.DB.Element));

                foreach (KeyValuePair<string, List<string>> kvp in WatchedParameterMap) {
                    //how is this supposed to work with a strict name map.  with a UI you would necessarily create a configurable map between parameter elements, especially in the case of 
                    //parameterelements with duplicate names.  for MVP, just use the first result
                    ElementId WatchedSinkParameter = GetWatchedParameterIdByName(args.Document, kvp.Key);

                    try {
                        if (!args.Document.IsFamilyDocument && WatchedSinkParameter != null) {
                            foreach (string source in kvp.Value) {
                                ElementId WatchedSourceParameter = GetWatchedParameterIdByName(args.Document, kvp.Value.First());
                                Log.Debug($"try trigger {WatchedSourceParameter.Value} as {source}");
                                //UpdaterRegistry.AddTrigger(_uid, args.Document, TargetElementFilter, Element.GetChangeTypeParameter(WatchedSourceParameter));
                                Log.Debug($"\tNEW TRIGGER:\t{source}=>{kvp.Key}");

                                _linkedParameters.Add(WatchedSourceParameter, WatchedSinkParameter);
                            }
                        }
                    }
                    catch (Exception e) {
                        Log.Error($"{this.GetUpdaterName()}: {e.Message}");
                    }
                }
            }
            else Log.Error($"{this.GetUpdaterName()}\tLinker Map parameter not found");
            UpdaterRegistry.DisableUpdater(_uid);
        }

        internal Dictionary<string,List<string>> GetParameterMapFromPath (string filepath) {
            if (File.Exists(filepath)) {
                Dictionary<string, List<string>> SourceParameterMap = new Dictionary<string, List<string>>();
                string[] lines = File.ReadAllLines(filepath);

                foreach (string line in lines) {
                    List<string> parts = line.Split(',').ToList();
                    string key = parts[0];
                    parts.RemoveAt(0);
                    SourceParameterMap.Add(key, parts);
                }
                Log.Debug("Parameter Map complete");
                return SourceParameterMap;
            }
            else {
                throw new FileLoadException("Invalid File Path");
            }
        }

        internal ElementId GetWatchedParameterIdByName(Document doc, string name) {
            ElementId WatchedParameterId;
            List<Element> ParameterElementCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterElement))
                .ToElements()
                .Where(x => x.Name == name)
                .ToList();
            IList<ForgeTypeId> Bips = ParameterUtils.GetAllBuiltInParameters();
            try { 
                Dictionary<string, ForgeTypeId> BipMap = Bips.ToDictionary(x => LabelUtils.GetLabelForBuiltInParameter(x), x => x);
                
                if (BipMap.ContainsKey(name)) {
                    WatchedParameterId = new ElementId(ParameterUtils.GetBuiltInParameter(BipMap[name]));
                }
                else { 
                    WatchedParameterId = ParameterElementCollector.FirstOrDefault().Id;
                }
                return WatchedParameterId;
            }
            catch (Exception e) {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }
        internal ForgeTypeId GetForgeTypeIdFromElementId (Document doc, ElementId ParameterId) {
            ForgeTypeId ParameterTypeId;
            if (ParameterId.Value > 0) {
                ParameterElement PE = doc.GetElement(ParameterId) as ParameterElement;
                ParameterTypeId = PE.GetDefinition().GetTypeId();
            }
            else {
                BuiltInParameter Bip = (BuiltInParameter)ParameterId.Value;
                ParameterTypeId = ParameterUtils.GetParameterTypeId(Bip);
            }
            return ParameterTypeId;
        }
    }
}
