using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    public class CategoryUtils {
    
    public CategorySet GetProjectParameterCategories (Document doc, string ParameterName) {

            BindingMap ProjectParametersMap = doc.ParameterBindings;
            //How does this want to address parameters with the same names..
            foreach (KeyValuePair<InternalDefinition, ElementBinding> kvp in ProjectParametersMap) {
                if (kvp.Key.Name == ParameterName) {
                    return kvp.Value.Categories;
                }
            }
            throw new KeyNotFoundException($"{ParameterName} not a valid Key");
        }

    public CategorySet GetProjectParameterCategories (Document doc, Definition Parameter) {
            BindingMap ProjectParametersMap = doc.ParameterBindings;

            if (ProjectParametersMap.Contains(Parameter)) {
                ElementBinding ParameterBinding = ProjectParametersMap.get_Item(Parameter) as ElementBinding;
                return ParameterBinding.Categories;
            }
            else {
                throw new KeyNotFoundException($"{Parameter.ToString()} not a valid Key");
            }
        }
    }
}
