using Autodesk.Revit.UI;
using QuickGraph;
using QuickGraph.Algorithms.TopologicalSort;
using RevitTaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace PowerBuilder.Services {
    /// <summary>
    /// Cached Task processor for updating View Template properties and graphic overrides by evaluating the set of view layers.
    /// </summary>
    
    
    public class ViewTemplateViewLayerUpdateManager {
        private Document _doc;
        private Definition _ControlParameter;
        private AdjacencyGraph<ElementId, Edge<ElementId>> ViewTemplateGraph; //need to find Graph library
        private Dictionary<string, ElementId> _ViewTemplateMap;

        public ViewTemplateViewLayerUpdateManager (Document doc, Definition parameter) {
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
        /// <returns>AdjacencyGraph of the ElementId relationships</returns>
        private AdjacencyGraph<ElementId,Edge<ElementId>> ElementRelations (List<Autodesk.Revit.DB.View> ViewTemplates) {

            AdjacencyGraph<ElementId, Edge<ElementId>> ViewTemplateGraph = new AdjacencyGraph<ElementId, Edge<ElementId>>(); 
            
            foreach (Autodesk.Revit.DB.View vt in ViewTemplates) {

                ViewTemplateGraph.AddVertex(vt.Id);

                if (vt.get_Parameter(_ControlParameter).HasValue) {
                    List<ElementId> DependeeViews = GetViewLayers(vt);
                    
                    foreach (ElementId dv in DependeeViews) {

                        Edge<ElementId> E_i = new Edge<ElementId>(vt.Id, dv);
                        ViewTemplateGraph.AddEdge(E_i);
                    }
                } 
            }
            
            return ViewTemplateGraph;
        }

        private List<ElementId> GetViewLayers (Autodesk.Revit.DB.View ViewTemplate) {

            Parameter LayerSequence = ViewTemplate.get_Parameter(_ControlParameter);
            List<string> LayerSequenceNames;
            List<ElementId> LayerSequenceIds;

            if (LayerSequence.HasValue) {
                LayerSequenceNames = LayerSequence
                .AsString()
                .Split('\n')
                .Select(x => x.Trim())
                .ToList();

                LayerSequenceIds = LayerSequenceNames.Select(vtn => _ViewTemplateMap[vtn]).ToList();
            }
            else LayerSequenceIds = new List<ElementId>();

            return LayerSequenceIds;
        }

        /// <summary>
        /// Call Updater on the sequence of topologically sorted View Templates
        /// </summary>
        /// <returns></returns>
        public void UpdateViewTemplates() {
            try {
                TopologicalSortAlgorithm<ElementId, Edge<ElementId>> Algorithm = new TopologicalSortAlgorithm<ElementId, Edge<ElementId>>(ViewTemplateGraph);
                Algorithm.Compute();


                foreach (ElementId vtid in Algorithm.SortedVertices) {

                    //TODO: add some check for self edges (omit? notify? exception?)
                    List<ElementId> dependees = ViewTemplateGraph.OutEdges(vtid).Select(x => x.Target).ToList();

                    Autodesk.Revit.DB.View vt = _doc.GetElement(vtid) as Autodesk.Revit.DB.View;
                    if (GetViewLayers(vt).Count > 0) {
                        ViewTemplateViewLayerUpdater VTVLU = new ViewTemplateViewLayerUpdater(vt, GetViewLayers(vt).Select(x => _doc.GetElement(x) as Autodesk.Revit.DB.View));
                        VTVLU.Update();
                    }
                }
            }
            catch (NonAcyclicGraphException Ex) {
                RevitTaskDialog.Show("Circular Dependencies Detected", Ex.Message);
            }

        }
    }
}
