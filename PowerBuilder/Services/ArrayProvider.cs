using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Services {
    public class ElementArrayProvider {
        /*
         * what do we have to account for:
         *  spacing patterns
         *      - center-to-center
         *      - autofit x,y
         *      - fixed width
         *      
         *  let's just start with single column array.
         *  i think typically a label gets assigned
         *      - Tags must occur in a Legend, need text identifier
         *      - TextNoteTypes need a Text label (in that type)
         *      - Model Elements should get tagged Family:Type
         *      - System Elements need different placements
         *      
         *  Other modal behaviors
         *      - Array by Types
         *      - Array by category
         *          - this is changes the carrige return criteria
         *              - columns by Family
         *      - Array by class(?)
         */
        private ElementId[] _parents;
        private Document _current;
        private XYZ _startpoint;
        private Category _category;
        private Func<ElementId, ICollection<ElementId>> GetChildren;
        private Func<ElementId, BoundingBoxXYZ> GetChildBoundingBox;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Collection"></param>
        /// <param name="doc"></param>
        /// <param name="StartPoint"></param>
        public ElementArrayProvider(IEnumerable<ElementId> Collection, Document doc, XYZ StartPoint) {
            /*
             * does using a constructor here make sense? i want to be able to access doc freely throughout the calculation so we can pass things as 
             * a list of ElementIds and not elements
             * 
             */
            _parents= Collection.ToArray();
            _current = doc;
            _startpoint = StartPoint;
        }

        /*
         * what does this really need to take care of
         *      - Annotations
         *      - FamilyInstances
         *      - System Families
         *          - Line based (wall, duct, pipe)
         *          - SketchBased (floor, ceiling, roof)
         *      - Multi Column
         *          - ByCategory (system)
         *              - these are technically all multi-column, just with one (system) family
         *                  **ok, this is what isn't true.  this creates a horizontal result.  but maybe you let that be the pattern.  new row for every Type
         *              - Lines, Walls, Duct, Pipe, Roof, Ceiling, Floor
         *          so it can always be 'multi-column, just with one family'
         *          - may need a selection mode for doing a singluar Family
         */
        public ElementArrayProvider (Category target, Document doc, XYZ StartPoint) {

            FilteredElementCollector fec = new FilteredElementCollector(doc).OfCategory(target.BuiltInCategory);
            FilteredElementCollector Targets = null;
            List<Func<FilteredElementCollector, List<ElementId>>> CollectorMethods = new List<Func<FilteredElementCollector, List<ElementId>>>() {
                collector => collector.OfClass(typeof(Family)).ToElementIds().ToList(),
                collector => collector.WhereElementIsElementType().ToElementIds().ToList(),

            };
/*
             * the cases
             *  FamilyInstance based
             *  Annotation
             *  System family based
             *  TextNotes
             *  Lines
             *  Filled Regions
             *  Dimensions also special
             */
            
            
            _parents = Targets.ToElementIds().ToArray();
            _category = target;
        }
        /// <summary>
        /// Generate an Array of Elements in Revit depending on 
        /// </summary>
        public void GenerateElementArray() {

            List<List<XYZ>> StartPoints = GenerateArrayByParentsAndChildren(_startpoint, 0.5);
            if (_category.BuiltInCategory == BuiltInCategory.OST_DuctCurves) {

            }
        }
        /// <summary>
        /// Generate a List of List of points representing the minimum point for Array cells where each row corresponds to the children of a specific parent element type
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="PaddingSize"></param>
        /// <returns></returns>
        internal List<List<XYZ>> GenerateArrayByParentsAndChildren( XYZ StartPoint, double PaddingSize = 0.0) {

            // This needs to be re-thought a little bit to do the actual Legend Builder because that wants strict column limits/wrapping
            // i think this at least gets separated into the type handler and the tiler
            List<XYZ> Column = new List<XYZ>();
            List<List<XYZ>> Array = new List<List<XYZ>>();
            XYZ Prev = StartPoint;
            XYZ Next, Move;
            double BinX, BinY, MaxX = 0, MaxY = 0;

            foreach (ElementId CurrentParent in _parents){
                
                List<ElementId> CurrentChildren = GetChildren(CurrentParent).ToList();
                foreach (ElementId CurrentChild in CurrentChildren){

                    BoundingBoxXYZ CurrentBbox = GetChildBoundingBox(CurrentChild);

                    BinX = CurrentBbox.Max.X - CurrentBbox.Min.X;
                    BinY = CurrentBbox.Max.Y - CurrentBbox.Min.Y;

                    if (BinY > MaxY) { BinY = MaxY; }

                    Move = new XYZ(BinX, 0.0, 0.0);
                    
                    Next = Prev + Move;
                    Column.Add(Next);
                }
                Prev = Column[0];
                Move = new XYZ(0.0, MaxY + PaddingSize, 0.0);
                Array.Add(Column);
                Column = new List<XYZ>();
                MaxY = 0.0;
                BinX = 0.0;
            }
            //Do I care about [[columns]..] or can we flatten this to just be a collection of points
            //collection of points makes the consumption easier
            return Array;
        }

        internal void CreateFamilyInstances (IEnumerable<XYZ> Points) {
            throw new NotImplementedException("CreateFamilyInstanceByPoints not Implemented");
        }
        internal void CreateElementByPoints (IEnumerable <XYZ> Points) { 
            throw new NotImplementedException("CreateElementsByPoints not implemented"); 
        }
        internal IEnumerable<ElementId> GetChildrenFamilyInstance(ElementId elementId) {
            Family ThisFamily = _current.GetElement(elementId) as Family;
            return ThisFamily.GetFamilySymbolIds();
        }
        internal IEnumerable<ElementId> GetChildrenAsType (ElementId elementId) {
            return [elementId];
        }
        
    }
}
