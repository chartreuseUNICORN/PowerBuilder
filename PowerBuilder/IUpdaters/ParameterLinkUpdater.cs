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
        
        private static List<DefinitionStub[]> _linkedParameterPairs = new List<DefinitionStub[]>();
        private static Dictionary<ChangeType, (int index, int orderFlag)> _registeredChangeType = new Dictionary<ChangeType, (int index, int orderFlag)>();
        
        /// <summary>
        /// Updater that enforces value equality between pairs of registered parameters. Only BuiltInParameters and Shared Parameters may be selected.
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

                ChangeType thisChangeType = _registeredChangeType
                    .Keys
                    .FirstOrDefault(x => data.IsChangeTriggered(eid, x));
                int dsIndex = _registeredChangeType[thisChangeType].index;
                int orderFlag = _registeredChangeType[thisChangeType].orderFlag;
                DefinitionStub[] defStubs = _linkedParameterPairs[dsIndex];

                DefinitionStub Source = defStubs[orderFlag];
                DefinitionStub Sink = defStubs[1-orderFlag];
                try {
                    Parameter SinkParameter = Sink.GetElementParameter(e);
                    Parameter SourceParameter = Source.GetElementParameter(e);

                    SinkParameter.Match(SourceParameter);
                }
                catch (Exception ex) {
                    Debug.WriteLine($"{ex.GetType()}|{ex.Message}");
                }

                Debug.WriteLine($"ParameterLinkUpdater Trigger on {eid}: {Sink.GetElementParameter(e).Definition.Name} -linkedTo- {Source.GetElementParameter(e).Definition.Name}");
                //SinkParameter.Match(SourceParameter);
                
            }
        }

        public override void updater_OnDocumentOpened(object sender, DocumentOpenedEventArgs args) {

            // TODO: add a function to add DefinitionStub pairs from a file attached to the document
            
            ElementClassFilter TargetElementFilter = new ElementClassFilter(typeof(FamilyInstance)); //temporary - is this part of the registration data
            if (!args.Document.IsFamilyDocument) {
                for (int i=0; i<_linkedParameterPairs.Count; i++) {
                    try {
                        DefinitionStub[] defStubPair = _linkedParameterPairs[i];
                        Log.Debug($"try trigger {defStubPair[0].GetParameterId(args.Document)} <=> {defStubPair[1].GetParameterId(args.Document)}");
                        for (int j = 0; j < defStubPair.Length; j++) {
                            ChangeType changeType = Element.GetChangeTypeParameter(defStubPair[j].GetParameterId(args.Document));
                            UpdaterRegistry.AddTrigger(_uid, args.Document, TargetElementFilter, changeType);
                            _registeredChangeType.Add(changeType, (i, j));
                        }
                    }
                    catch (Exception e) {
                        Log.Error($"{this.GetUpdaterName()}: {e.Message}");
                    }
                }
            }
        }

        /*internal Dictionary<string,List<string>> GetParameterMapFromPath (string filepath) {
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
        }*/

        internal void AddDefinitionStubPair(DefinitionStub A, DefinitionStub B) {
            _linkedParameterPairs.Add([A, B]);
        }
    }
}
