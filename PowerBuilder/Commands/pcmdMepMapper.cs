#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Msagl.Drawing;
using PowerBuilder.Infrastructure;
using PowerBuilder.Interfaces;
using PowerBuilder.SelectionFilter;
using PowerBuilder.Utils;
using QuickGraph;
using QuickGraph.Graphviz;
using QuikGraph.MSAGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace PowerBuilder.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class pcmdMepMapper : CmdBase{
        public override string DisplayName { get; } = "MEP Map";
        public override string ShortDesc { get; } = "Produce a system graphic for based on the selection.";
        public override bool RibbonIncludeFlag { get; set; } = true;
        public override Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            PowerDialogResult res = GetInput(uiapp);
            MEPSystem Target = doc.GetElement(res.SelectionResults[0] as ElementId) as MEPSystem;
            AdjacencyGraph<ElementId, Edge<ElementId>> SystemGraph = MEPSystemToAdjacencyGraph(Target);

            TaskDialog.Show("MEPmapper", "System graph generated");
            
            

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PowerDialogResult res = new PowerDialogResult();
            Selection sel = uidoc.Selection;
            if (sel == null) {
                
                Reference reference = sel.PickObject(ObjectType.Element, new ClassSelectionFilter(typeof(MEPSystem)));
                res.AddSelectionResult(reference.ElementId);
            }
            else {
                res.AddSelectionResult(sel.GetElementIds().First());
            }
                
            return res;
        }
        
        internal ICollection<Connector> GetOutConnectors (Connector c, MEPSystem TargetSystem) {
            List<Connector> OutCons = new List<Connector>();
            ConnectorManager NodeConMan = null;
            Element CurrentOwner = c.Owner;
            Element EntryNodeOwner = c.Owner;
            int EntryNodeId = c.Id;
            
            Debug.WriteLine($"\tGet OutConnectors\tOwner: {CurrentOwner.Id} on Connector:{EntryNodeId}");

            NodeConMan = GetAdjacentConManByConnector(c);

            if (NodeConMan != null) {
                
                Debug.WriteLine($"\tConMan for {NodeConMan.Owner.Id}:\tConnectorCount:{NodeConMan.Connectors.Size}");
                foreach (Connector con in NodeConMan.Connectors) {
                    //Debug.WriteLine($"Node Index: {con.Id}");
                    if (con.ConnectorType != ConnectorType.Logical && con.MEPSystem.Id == TargetSystem.Id && !con.IsConnectedTo(c)) {
                        
                        //Debug.WriteLine($"\tPairedConnectorInfo: Owner{con.Owner.Id}\tConnectorId:{con.Id}");
                        OutCons.Add(con);
                    }
                    /*else if (con.IsConnectedTo(c)) {
                        Debug.WriteLine($"\tACCESS CONNECTOR\tPreviousOwner{c.Owner.Id}");
                    }
                    else {
                        Debug.WriteLine("\tLogicalConnector");
                    }*/
                }
            }
            else throw new ArgumentException("No Physical Connectors Found");

            return OutCons;
        }

        internal ConnectorManager GetAdjacentConManByConnector (Connector c0, bool TraversePhysical= true) {

            ConnectorManager NextConnectorManager = null;
            foreach (Connector ci in c0.AllRefs) {
                if (ci.Owner.Id != c0.Owner.Id && (TraversePhysical == (ci.ConnectorType != ConnectorType.Logical))) {
                    NextConnectorManager = ci.ConnectorManager;
                    break;
                }
            }
            return NextConnectorManager;
        }
        internal Edge<ElementId> NewEdgeByConnector (Connector c0, bool TraversePhysical = true) {
            Edge<ElementId> edge;
            Connector c1 = null;
            foreach (Connector c in c0.AllRefs) {
                
                if (c.Owner.Id != c0.Owner.Id && TraversePhysical == (c.ConnectorType != ConnectorType.Logical)) {
                    c1 = c;
                }
            }
            if (c1 != null) {
                edge = new Edge<ElementId>(c0.Owner.Id, c1.Owner.Id);
            }
            else {
                throw new ArgumentNullException("no connector found");
            }
            Debug.WriteLine($"NEW EDGE:\t{edge.Source} -> {edge.Target}");
            return edge;
        }
        internal AdjacencyGraph<ElementId, Edge<ElementId>> MEPSystemToAdjacencyGraph (MEPSystem system) {

            Connector CurrentNode;
            ElementId CurrentVertex;
            HashSet<ElementId> Visited = new HashSet<ElementId>();
            AdjacencyGraph<ElementId, Edge<ElementId>> G = new AdjacencyGraph<ElementId, Edge<ElementId>>();
            Stack<Connector> SearchStack = new Stack<Connector>();
            Connector RootConnector = system.BaseEquipmentConnector;

            SearchStack.Push(RootConnector);

            int count = 0; //debugging counter

            while (SearchStack.Count > 0) {
                CurrentNode = SearchStack.Pop();
                CurrentVertex = CurrentNode.Owner.Id;
                Debug.WriteLine($"{CurrentNode.Owner.Id}_{CurrentNode.Id}\t|\t{String.Join(",",SearchStack.Select(x => $"{x.Owner.Id}_{x.Id}").ToArray() as object[])}");
                Edge<ElementId> CurrentEdge = NewEdgeByConnector(CurrentNode);
                G.AddVerticesAndEdge(CurrentEdge);
                    
                if (!Visited.Contains(CurrentEdge.Target)) {
                    
                    List<Connector> OutCons = GetOutConnectors(CurrentNode, system).ToList();
                    foreach (Connector NewCon in OutCons) {
                        SearchStack.Push(NewCon);
                    }
                }
                Debug.WriteLine($"<>{String.Join(",", SearchStack.Select(x => $"{x.Owner.Id}_{x.Id}").ToArray() as object[])}\n");
                //remove
                if (count > 50) {
                    throw new Exception("Undefined Loop Condition");
                }
                count++;
            }
            return G;
        }
    }
}
