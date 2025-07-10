using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Utils {
    public static class ViewUtils {
        public static bool ToggleCategoryVisibility (ElementId categoryId, Autodesk.Revit.DB.View activeView) {
            bool setState;
            
            if (activeView.IsCategoryOverridable(categoryId)) {
                if (activeView.GetCategoryHidden(categoryId)) {
                    setState = false;
                }
                else {
                    setState = true;
                }
                activeView.SetCategoryHidden(categoryId, setState);
            }
            
            return true;
        }
    }
}
