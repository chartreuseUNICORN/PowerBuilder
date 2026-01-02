using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    public class CategoryUtils {

        public static CategorySet GetProjectParameterCategories(Document doc, string ParameterName) {

            BindingMap ProjectParametersMap = doc.ParameterBindings;
            //How does this want to address parameters with the same names..
            foreach (KeyValuePair<InternalDefinition, ElementBinding> kvp in ProjectParametersMap) {
                if (kvp.Key.Name == ParameterName) {
                    return kvp.Value.Categories;
                }
            }
            throw new KeyNotFoundException($"{ParameterName} not a valid Key");
        }

        public static CategorySet GetProjectParameterCategories(Document doc, Definition Parameter) {
            BindingMap ProjectParametersMap = doc.ParameterBindings;

            if (ProjectParametersMap.Contains(Parameter)) {
                ElementBinding ParameterBinding = ProjectParametersMap.get_Item(Parameter) as ElementBinding;
                return ParameterBinding.Categories;
            }
            else {
                throw new KeyNotFoundException($"{Parameter.ToString()} not a valid Key");
            }
        }

        public static ICollection<BuiltInCategory> GetCategoriesByType(Document doc, CategoryType catType) {
            Settings settings = doc.Settings;
            Categories docCategories = settings.Categories;
            List<BuiltInCategory> selectedCategories = new List<BuiltInCategory>();
            foreach (Category category in docCategories) {
                if (category.CategoryType == catType) {
                    ElementId cid = category.Id;
                    
                    selectedCategories.Add(category.BuiltInCategory);
                }
            }
            return selectedCategories;
        }
    }
}
