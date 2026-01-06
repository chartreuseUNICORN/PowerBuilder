using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PowerBuilder.Infrastructure {
    /// <summary>
    /// This is sort of a placeholder class to provide a way to access Definition identity parameters for generic usage.  
    /// </summary>
    public class DefinitionStub {
        
        private BuiltInParameter? _builtInParameter = null;
        private Guid? _guid;
        public DefinitionStub(BuiltInParameter bip) {
            _builtInParameter = bip;
        }
        
        public DefinitionStub(Guid guid) {
            
            _guid = guid;
        }

        public Parameter GetElementParameter (Element e) {
            Parameter p;
            if (_guid == null) {
                p = e.get_Parameter(_builtInParameter.Value);
            }
            else {
                p = e.get_Parameter(_guid.Value);
            }
            return p;
        }
        public ElementId GetParameterId (Document doc) {
            ElementId parameterId;
            if (_guid == null) {
                parameterId = new ElementId(_builtInParameter.Value);
            }
            else {
                SharedParameterElement sp = SharedParameterElement.Lookup(doc, _guid.Value);
                parameterId = sp.GetDefinition().Id;
            }
            return parameterId;
        }
        // ok, so i don't think this works as a 'portable' version of Definitions. I guess it sort of reveals why SharedParameters work the way they do.
        // Parameter ElementIds are locally generated, except for BuiltInParameters. SharedParameterElements can be anchored via their guid so you can 
        // retrieve the reference in multiple projects.  the whole parameterlinker may just have to be relegated to Bip and SharedParameters, just like
        // schedules and tags.. which is probably fine.
    }
}
