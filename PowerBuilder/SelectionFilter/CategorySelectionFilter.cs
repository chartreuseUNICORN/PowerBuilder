﻿using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.SelectionFilter
{
    public class CategorySelectionFilter : ISelectionFilter
    {
        private List<BuiltInCategory> _cats;
        public CategorySelectionFilter(BuiltInCategory cat)
        {
            _cats = new List<BuiltInCategory>() { cat };
        }
        public CategorySelectionFilter(List<BuiltInCategory> cats)
        {
            _cats = new List<BuiltInCategory>(cats);
        }
        public bool AllowElement(Element elem)
        {
            if (elem.Category != null)
            {
                return _cats.Contains<BuiltInCategory>(elem.Category.BuiltInCategory);
            }
            else
            {
                return false;
            }
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
