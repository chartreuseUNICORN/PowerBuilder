using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using PowerBuilder.Extensions;

namespace PowerBuilder.SelectionFilter
{
    public class ClassSelectionFilter : ISelectionFilter
    {
        private List<Type> _classes;
        public ClassSelectionFilter(Type ThisClass)
        {
            _classes = new List<Type>() { ThisClass };
        }
        public ClassSelectionFilter(List<Type> TheseClasses)
        {
            _classes = new List<Type>(TheseClasses);
        }
        public bool AllowElement(Element elem)
        {
            //apparently, there is ElementClassFilter which does something similar.  is it worth using that internal function instead?
            return _classes.Where(t => elem.IsSameOrSubclass(t)).Any();
            
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

    }
}
