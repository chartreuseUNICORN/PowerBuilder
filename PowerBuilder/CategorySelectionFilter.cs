using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.SelectionHelpers {
    public class CategorySelectionFilter : ISelectionFilter {
        private BuiltInCategory _cat;
        public CategorySelectionFilter(BuiltInCategory cat) {
            _cat = cat;
        }
        public bool AllowElement(Element elem) {
            if (elem.Category != null) {
                return elem.Category.BuiltInCategory == _cat;
            }
            else {
                return false;
            }
        }
        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
