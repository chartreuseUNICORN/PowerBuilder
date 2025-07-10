using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace PowerBuilder.Objects {
    /*
     TODO: figure out what this actually is
        I think this is something different than just a named reference for LLM to interpret.  KVP for the fixed/allowable/known categories
        elements can be clasified into.
     */
    public class SpecCulture {
        private Dictionary<string, string> _spec;
        private string _specName;
        /// <summary>
        /// Initialize a new SpecCulture definition from a dictionary
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public string Name {  get { return _specName; } }
        public string this[string k] {
            get => _spec[k];
        }
        public SpecCulture(string name, Dictionary<string,string> keyValuePairs) {
            _specName = name;
            _spec = keyValuePairs;
        }
        /// <summary>
        /// Initialize a new SpecCulture definition from the CSV file at the specified path
        /// </summary>
        /// <param name="path">path of the specified </param>
        public SpecCulture(string name, string path) {
            _specName = name;

            //the spec definition files need to be valid.  i guess the warning message should identify which
            // key is duplicate
            _spec =
                File.ReadLines(path)
                    .Select(line => line.Split(','))
                    .ToDictionary(gr => gr[0],
                                  gr => gr[1]);
            
        }
        public string ToJson() {
            string json = JsonSerializer.Serialize(_spec);
            return json;
        }
        
    }
}
