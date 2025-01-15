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
            return elem.Category.BuiltInCategory == _cat;
        }
        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
