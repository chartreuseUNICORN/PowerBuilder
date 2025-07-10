using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Extensions {
    public static class ParameterExtensions {
        /// <summary>
        /// Extension method to set current Parameter value to input Parameter value
        /// </summary>
        /// <param name="param"></param>
        /// <param name="that">Parameter to use for setting current value</param>
        /// <returns></returns>
        /// <exception cref="Exception">Exception when StorageType is None</exception>
        /// <exception cref="ArgumentOutOfRangeException">Default Exception if parameter does not have a storage type</exception>
        public static Parameter Match(this Parameter param, Parameter that) {
            switch (param.StorageType){
                case StorageType.Double:
                    param.Set(that.AsDouble());
                    break;
                case StorageType.Integer:
                    param.Set(that.AsInteger());
                    break;
                case StorageType.String:
                    param.Set(that.AsString());
                    break;
                case StorageType.ElementId:
                    param.Set(that.AsElementId());
                    break;
                case StorageType.None:
                    //TODO: does this just break? is an exception necessary? StorageType.None implies the value cannot change?
                    throw new Exception("cannot set StorageType.None");
                default:
                    throw new ArgumentOutOfRangeException("invalid storage type");
                    
            }
            return param;
        }
    }
}
