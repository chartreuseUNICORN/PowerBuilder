using Autodesk.Revit.UI;
using PowerBuilder.Services;
using QuickGraph;
using QuickGraph.Algorithms.TopologicalSort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Handlers {
    /// <summary>
    /// Cached Task processor for updating View Template properties and graphic overrides by evaluating the set of view layers.
    /// </summary>
    
    
    public class ViewTemplateViewLayerUpdateService {
        private Document _doc;
        private Definition _ControlParameter;
        private AdjacencyGraph<ElementId, Edge<ElementId>> ViewTemplateGraph; //need to find Graph library
        private Dictionary<string, ElementId> _ViewTemplateMap;
        private IEdge<ElementId> _ElementRelation;

        public ViewTemplateViewLayerUpdateService (Document doc, Definition parameter) {
            /*TODO
             *  horizontally produce Graphic Overrides 
             */
            _doc = doc;
            _ControlParameter = parameter;

            List<Autodesk.Revit.DB.View> ViewTemplates = new FilteredElementCollector(_doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(vp => vp.IsTemplate)
                .ToList<Autodesk.Revit.DB.View>();

            _ViewTemplateMap = ViewTemplates.ToDictionary(vt => vt.Name, vt => vt.Id);
            ViewTemplateGraph = ElementRelations(ViewTemplates);
            
        }

        /// <summary>
        /// Return true if set of related View Templates do not form a circular reference
        /// </summary>
        /// <returns></returns>
        private AdjacencyGraph<ElementId,Edge<ElementId>> ElementRelations (List<Autodesk.Revit.DB.View> ViewTemplates) {

            
            AdjacencyGraph<ElementId, Edge<ElementId>> ViewTemplateGraph = new AdjacencyGraph<ElementId, Edge<ElementId>>(); 
            
           foreach (Autodesk.Revit.DB.View vt in ViewTemplates) {

                HashSet<ElementId> DependeeViews = GetViewLayers(vt);
                foreach(ElementId dv in DependeeViews) {
                    Edge<ElementId> E_i = new Edge<ElementId>(vt.Id, dv);

                    ViewTemplateGraph.AddEdge(E_i);
                }
                
            }
            
            return ViewTemplateGraph;
        }

        private HashSet<ElementId> GetViewLayers (Autodesk.Revit.DB.View ViewTemplate) {

            List<string> LayerSequenceNames = ViewTemplate.get_Parameter(_ControlParameter)
                .AsString()
                .Split('\n')
                .Select(x => x.Trim())
                .ToList();

            return new HashSet<ElementId>(LayerSequenceNames.Select(vtn => _ViewTemplateMap[vtn]));
        }

        /// <summary>
        /// Apply OverrideGraphisSettings from the Override Caches
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void UpdateViewTemplate() {

            try {
                TopologicalSortAlgorithm<ElementId, Edge<ElementId>> Algorithm = new TopologicalSortAlgorithm<ElementId, Edge<ElementId>>(ViewTemplateGraph);
                Algorithm.Compute();
                List<ElementId> TemplateUpdateSequence = Algorithm.SortedVertices.ToList<ElementId>();

                foreach (ElementId vtid in TemplateUpdateSequence) {
                    Autodesk.Revit.DB.View vt = _doc.GetElement(vtid) as Autodesk.Revit.DB.View;
                    if (GetViewLayers(vt).Count > 0) {
                        ViewTemplateViewLayerUpdater VTVLU = new ViewTemplateViewLayerUpdater(vt);
                        
                    }
                }
            }
            catch (NonAcyclicGraphException nage) {
                TaskDialog.Show("Circular Dependencies Detected", nage.Message);
            }
        }

    }
}
