#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PowerBuilder.Infrastructure;
using PowerBuilder.SelectionFilter;
using QuickGraph;
using System.Diagnostics;
using System.IO;
using System.Text;
using RevitTaskDialog = Autodesk.Revit.UI.TaskDialog;

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

            // Old version - generates full graph with all elements
            AdjacencyGraph<ElementId, Edge<ElementId>> SystemGraph = MEPSystemToAdjacencyGraph(Target);

            // Generate compact graph with only significant elements (equipment, devices, branches)
            //AdjacencyGraph<ElementId, Edge<ElementId>> CompactGraph = MEPSystemToCompactAdjacencyGraph(Target);

            // Generate DOT file content from compact graph
            string dotContent = AdjacencyGraphToViz(SystemGraph, doc, Target);

            // Generate filename: [projectTitle].[systemName].dot
            string projectTitle = string.IsNullOrEmpty(doc.Title) ? Path.GetFileNameWithoutExtension(doc.PathName) : doc.Title;
            projectTitle = string.IsNullOrEmpty(projectTitle) ? "Untitled" : projectTitle;
            string systemName = Target.Name;

            // Sanitize filename
            string safeProjectTitle = string.Join("_", projectTitle.Split(Path.GetInvalidFileNameChars()));
            string safeSystemName = string.Join("_", systemName.Split(Path.GetInvalidFileNameChars()));
            string filename = $"{safeProjectTitle}.{safeSystemName}.dot";

            // Write to %appdata%/PowerBuilder folder
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerBuilder");
            Directory.CreateDirectory(outputDir);  // Create directory if it doesn't exist
            string outputPath = Path.Combine(outputDir, filename);
            File.WriteAllText(outputPath, dotContent);

            // Try to generate PNG using GraphViz if installed
            string pngPath = Path.ChangeExtension(outputPath, ".png");
            bool pngGenerated = TryGeneratePngWithGraphViz(outputPath, pngPath);

            string displayMessage = pngGenerated
                ? $"System graph generated and saved to:\n{outputPath}\n\nPNG visualization created:\n{pngPath}"
                : $"System graph generated and saved to:\n{outputPath}\n\n(GraphViz not found - install GraphViz to auto-generate PNG)";

            RevitTaskDialog.Show("MEP Mapper", displayMessage);

            return Result.Succeeded;
        }
        public override PowerDialogResult GetInput(UIApplication uiapp) {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            PowerDialogResult res = new PowerDialogResult();
            Selection sel = uidoc.Selection;

            // Check if there are any pre-selected elements
            if (sel.GetElementIds().Count == 0) {
                // No pre-selection, prompt user to pick an element
                Reference reference = sel.PickObject(ObjectType.Element, new ClassSelectionFilter(typeof(MEPSystem)));
                res.AddSelectionResult(reference.ElementId);
            }
            else {
                // Use first pre-selected element
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
                    MEPSystem meps = con.MEPSystem;
                    if (meps != null) {
                        if (con.ConnectorType != ConnectorType.Logical && con.MEPSystem.Id == TargetSystem.Id && !con.IsConnectedTo(c)) {

                            //Debug.WriteLine($"\tPairedConnectorInfo: Owner{con.Owner.Id}\tConnectorId:{con.Id}");
                            OutCons.Add(con);
                        }
                    }
                    else {
                        Debug.WriteLine($"connector {con.Owner.Id}_{con.Id} not part of mep system: {con.Domain}");
                        // TODO: these traversal snippets need to be domain sensitive.  we find here a situation trying to navigate a vav box where electrical connectors show up
                        //    if this ever affects electrical circuits, it needs to know what domain to look for as it's traversing.
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
                    Visited.Add(CurrentEdge.Target);

                    List<Connector> OutCons = GetOutConnectors(CurrentNode, system).ToList();
                    foreach (Connector NewCon in OutCons) {
                        SearchStack.Push(NewCon);
                    }
                }
                Debug.WriteLine($"<>{String.Join(",", SearchStack.Select(x => $"{x.Owner.Id}_{x.Id}").ToArray() as object[])}\n");
                
                count++;
            }
            return G;
        }

        internal AdjacencyGraph<ElementId, Edge<ElementId>> MEPSystemToCompactAdjacencyGraph(MEPSystem system)
        {
            AdjacencyGraph<ElementId, Edge<ElementId>> G = new AdjacencyGraph<ElementId, Edge<ElementId>>();
            HashSet<ElementId> visitedSignificant = new HashSet<ElementId>();
            HashSet<ElementId> visitedInsignificant = new HashSet<ElementId>();
            Stack<Connector> searchStack = new Stack<Connector>();

            Connector rootConnector = system.BaseEquipmentConnector;
            searchStack.Push(rootConnector);

            int count = 0;

            while (searchStack.Count > 0)
            {
                Connector currentConnector = searchStack.Pop();
                ElementId sourceVertex = currentConnector.Owner.Id;

                Debug.WriteLine($"Processing connector from: {sourceVertex}_{currentConnector.Id}");

                // Find all next significant elements reachable through this connector
                List<(ElementId targetId, Connector targetConnector)> nextSignificantElements =
                    TraverseToNextSignificantElements(currentConnector, system, visitedInsignificant);

                foreach (var (targetId, targetConnector) in nextSignificantElements)
                {
                    // Create edge from source to target significant element
                    Edge<ElementId> edge = new Edge<ElementId>(sourceVertex, targetId);
                    G.AddVerticesAndEdge(edge);
                    Debug.WriteLine($"COMPACT EDGE:\t{edge.Source} -> {edge.Target}");

                    // If target significant element not visited, explore from it
                    if (!visitedSignificant.Contains(targetId))
                    {
                        visitedSignificant.Add(targetId);

                        // Get all "out connectors" on the target significant element
                        // targetConnector is the connector we arrived at, GetOutConnectors gets the others
                        List<Connector> outConnectors = GetOutConnectors(targetConnector, system).ToList();
                        foreach (Connector con in outConnectors)
                        {
                            searchStack.Push(con);
                        }
                    }
                }

                if (count > 100)
                {
                    throw new Exception("Undefined Loop Condition");
                }
                count++;
            }

            return G;
        }

        internal List<(ElementId, Connector)> TraverseToNextSignificantElements(
            Connector startConnector,
            MEPSystem targetSystem,
            HashSet<ElementId> visitedInsignificant)
        {
            // startConnector is on element A (may or may not be significant)
            // We traverse through it to find the next significant element(s)
            // Returns list of (ElementId, Connector) where Connector is the entry point on the significant element

            List<(ElementId, Connector)> significantElements = new List<(ElementId, Connector)>();
            Queue<Connector> traversalQueue = new Queue<Connector>();

            // Get the adjacent element through startConnector
            ConnectorManager adjacentConMan = GetAdjacentConManByConnector(startConnector);
            if (adjacentConMan == null)
            {
                Debug.WriteLine($"\tDead end at connector {startConnector.Owner.Id}_{startConnector.Id}");
                return significantElements;
            }

            Element adjacentElement = adjacentConMan.Owner;

            // Check if immediately adjacent element is significant
            if (IsSignificantElement(adjacentElement, adjacentConMan))
            {
                // Found significant element immediately - find the connector that connects to startConnector
                foreach (Connector con in adjacentConMan.Connectors)
                {
                    if (con.IsConnectedTo(startConnector) &&
                        con.ConnectorType != ConnectorType.Logical &&
                        con.MEPSystem != null &&
                        con.MEPSystem.Id == targetSystem.Id)
                    {
                        Debug.WriteLine($"\tFound adjacent significant element: {adjacentElement.Id}");
                        significantElements.Add((adjacentElement.Id, con));
                        return significantElements;
                    }
                }
            }

            // Adjacent element is insignificant - traverse through it
            if (visitedInsignificant.Contains(adjacentElement.Id))
            {
                Debug.WriteLine($"\tAlready visited insignificant element: {adjacentElement.Id}");
                return significantElements;
            }

            visitedInsignificant.Add(adjacentElement.Id);
            Debug.WriteLine($"\tTraversing through insignificant element: {adjacentElement.Id}");

            // Get all "out connectors" from the adjacent insignificant element
            List<Connector> outConnectors = GetOutConnectors(startConnector, targetSystem).ToList();
            foreach (Connector con in outConnectors)
            {
                traversalQueue.Enqueue(con);
            }

            // BFS through insignificant elements
            while (traversalQueue.Count > 0)
            {
                Connector currentConnector = traversalQueue.Dequeue();

                // Get the next adjacent element
                ConnectorManager nextConMan = GetAdjacentConManByConnector(currentConnector);
                if (nextConMan == null) continue;

                Element nextElement = nextConMan.Owner;

                // Check if this element is significant
                if (IsSignificantElement(nextElement, nextConMan))
                {
                    // Found a significant element - find the connector that connects to currentConnector
                    foreach (Connector con in nextConMan.Connectors)
                    {
                        if (con.IsConnectedTo(currentConnector) &&
                            con.ConnectorType != ConnectorType.Logical &&
                            con.MEPSystem != null &&
                            con.MEPSystem.Id == targetSystem.Id)
                        {
                            Debug.WriteLine($"\tFound significant element after traversal: {nextElement.Id}");
                            significantElements.Add((nextElement.Id, con));
                            break;
                        }
                    }
                }
                else
                {
                    // Still insignificant - continue traversing
                    if (visitedInsignificant.Contains(nextElement.Id))
                    {
                        Debug.WriteLine($"\tSkipping already visited insignificant: {nextElement.Id}");
                        continue;
                    }

                    visitedInsignificant.Add(nextElement.Id);
                    Debug.WriteLine($"\tContinuing through insignificant element: {nextElement.Id}");

                    // Get out connectors and continue
                    List<Connector> nextOutConnectors = GetOutConnectors(currentConnector, targetSystem).ToList();
                    foreach (Connector con in nextOutConnectors)
                    {
                        traversalQueue.Enqueue(con);
                    }
                }
            }

            return significantElements;
        }

        internal bool IsSignificantElement(Element elem, ConnectorManager conMan)
        {
            if (elem?.Category == null)
                return true; // Keep if no category

            BuiltInCategory cat = (BuiltInCategory)elem.Category.Id.IntegerValue;

            // Not a duct or fitting = significant (equipment, devices, terminals)
            if (cat != BuiltInCategory.OST_DuctCurves && cat != BuiltInCategory.OST_DuctFitting)
                return true;

            // Duct/fitting with >2 physical connectors = branch point (tee) = significant
            int physicalConnectorCount = conMan.Connectors.Cast<Connector>()
                .Count(c => c.ConnectorType != ConnectorType.Logical && c.MEPSystem != null);

            return physicalConnectorCount > 2;
        }

        internal bool TryGeneratePngWithGraphViz(string dotFilePath, string pngFilePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "dot",
                    Arguments = $"-Tpng \"{dotFilePath}\" -o \"{pngFilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0 && File.Exists(pngFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GraphViz not found or error generating PNG: {ex.Message}");
                return false;
            }
        }

        internal string AdjacencyGraphToViz(AdjacencyGraph<ElementId, Edge<ElementId>> graph, Document doc, MEPSystem system)
        {
            StringBuilder sb = new StringBuilder();

            // Header - directed graph with system name
            string graphName = system.Name.Replace("\"", "\\\"");
            sb.AppendLine($"digraph \"{graphName}\" {{");
            sb.AppendLine("    // Graph attributes");
            sb.AppendLine("    rankdir=LR;  // Left-to-right layout for MEP flow direction");
            sb.AppendLine("    node [shape=box, style=filled];");
            sb.AppendLine();

            // Add nodes with descriptive labels
            sb.AppendLine("    // Nodes");
            foreach (var vertex in graph.Vertices)
            {
                Element elem = doc.GetElement(vertex);
                string elemName = elem.Name ?? "Unnamed";
                string category = elem.Category?.Name ?? "Unknown";

                // Create multi-line label with element info
                string label = $"{elemName}\\n{category}\\nID: {vertex.IntegerValue}";
                label = label.Replace("\"", "\\\"");  // Escape quotes

                // Apply different styling for ducts and duct fittings
                string nodeStyle = "";
                if (elem?.Category != null)
                {
                    BuiltInCategory cat = (BuiltInCategory)elem.Category.Id.IntegerValue;
                    if (cat == BuiltInCategory.OST_DuctCurves || cat == BuiltInCategory.OST_DuctFitting)
                    {
                        // White fill with 60% grey outline for ducts and fittings
                        nodeStyle = ", fillcolor=white, color=\"#999999\"";
                    }
                    else
                    {
                        // Light blue fill for other elements
                        nodeStyle = ", fillcolor=lightblue";
                    }
                }
                else
                {
                    nodeStyle = ", fillcolor=lightblue";
                }

                sb.AppendLine($"    \"{vertex.IntegerValue}\" [label=\"{label}\"{nodeStyle}];");
            }

            sb.AppendLine();

            // Add edges showing flow direction
            sb.AppendLine("    // Edges (flow direction)");
            foreach (var edge in graph.Edges)
            {
                sb.AppendLine($"    \"{edge.Source.IntegerValue}\" -> \"{edge.Target.IntegerValue}\";");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
