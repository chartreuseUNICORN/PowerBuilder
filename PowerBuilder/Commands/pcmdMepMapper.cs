#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Interfaces;
using PowerBuilder.SelectionFilter;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdMepMapper : IPowerCommand {
        public string DisplayName { get; } = "MEP Map";
        public string ShortDesc { get; } = "Produce a system graphic for based on the selection.";
        public bool RibbonIncludeFlag { get; } = false;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            uidoc.Selection.PickObject(ObjectType.Element, new ClassSelectionFilter(typeof(MEPSystem)));

            return Result.Succeeded;
        }
        public PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PowerDialogResult res = new PowerDialogResult();
            Selection sel = uidoc.Selection;
            if (sel == null) {
                //get selection from model selection
                sel.PickObject(ObjectType.Element);
            }
            return res;
        }
        internal AdjacencyGraph<ElementId, Edge<ElementId>> MEPSystemToGraph (MEPSystem CurrentSystem) {
            Element Root = CurrentSystem.BaseEquipment;
            Document doc = CurrentSystem.Document;
            Connector RootConnector = CurrentSystem.BaseEquipmentConnector;
            ConnectorManager Conman = CurrentSystem.ConnectorManager;
            Stack<Connector> TraversalStack = new Stack<Connector>();
            TraversalStack.Push(RootConnector);
            AdjacencyGraph<ElementId, Edge<ElementId>> G = new AdjacencyGraph<ElementId, Edge<ElementId>>();
            
            while (TraversalStack.Count > 0) {
                Connector CurrentNode = TraversalStack.Pop();
                G.AddVertex(CurrentNode.Owner.Id);
                List<Connector> Neighbors = GetAdjacentElements(CurrentNode, CurrentSystem).ToList();
                foreach (Connector con in Neighbors) {
                    Edge<ElementId> NewConnection = (con.Direction == FlowDirectionType.In) ? 
                        new Edge<ElementId>(CurrentNode.Owner.Id, con.Owner.Id) : 
                        new Edge<ElementId>(con.Owner.Id, CurrentNode.Owner.Id);
                    G.AddEdge(NewConnection);
                    TraversalStack.Push(con);
                }
            }
            return G;
        }
        
        internal ICollection<Connector> GetAdjacentElements (Connector c, MEPSystem TargetSystem) {
            List<Connector> Connections = new List<Connector>();
            foreach (Connector con in c.AllRefs) {
                if (con.ConnectorType != ConnectorType.Logical && con.MEPSystem.Equals(TargetSystem)) Connections.Add(con);
            }
            return Connections;
        }
    }
}
