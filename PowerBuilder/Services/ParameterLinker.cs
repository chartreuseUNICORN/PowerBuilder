using PowerBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    public class ParameterLinker {


        public static void UpdateParameterByLinks(Document doc, Parameter p) {
            // i think there's some modality to this, especially looking up element parameters by Name, and multiple sp with the same name
            string filepath = doc.ProjectInformation.get_Parameter(new Guid("c28bbda7 - 5445 - 408f - a4ce - edbe2793ab97")).AsString();
            
            List<string> SourceNames = GetSourceNamesFromPath(filepath, p);
            List<Parameter> Sources = SourceNames.Select(x => p.Element.LookupParameter(x)).ToList();
            //value checking in the function or in the caller?
            if (p == null) {
                foreach (Parameter pref in Sources) {
                    if (p.StorageType == pref.StorageType) {
                        p.Match(pref);
                        break;
                        /*
                         * there's probably a functional implementation of this that's something like
                         *  pattern match on p.StorageType, then get the first element with a non null value
                         */
                    }
                    else if (p.StorageType == StorageType.String && pref.StorageType != StorageType.None) {
                        p.Set(pref.AsValueString()); //better as value string or just as string?
                    }
                }
            }
        }
        internal static List<string> GetSourceNamesFromPath (string path, Parameter p) {
            
            List<string> sources = new List<string>();
            
            string[] lines = File.ReadAllLines(path);
            foreach(string line in lines) {
                Stack<string> parts = new Stack<string>( line.Split(','));
                string key = parts.Pop();
                if (key.Trim() == p.Definition.Name) {
                    sources = parts.Select(x => x.Trim()).ToList();
                    break;
                }
            }

            return sources;
        }
    }
}
