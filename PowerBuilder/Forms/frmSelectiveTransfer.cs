using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using PowerBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerBuilderUI.Forms
{
    public partial class frmSelectiveTransfer : System.Windows.Forms.Form
    {
        private PowerDialogResult _PBDialogResult;
        private List<Document> docs;
        /* remaining list of 'things' to include
         *  "Analytical Link Types",
            #"Bending Detail",
            "Cut Mark Types",
            "Foundation Slab Types",
            "Handrail Types",
            "Level Types",
            #"Line Weights",
            #"Load Types",
            "Model Text Types",
            #"Object Styles",
            "Phase Settings",
            "Railing Types",
            "Ramp Types",
            "Repeating Detail Types",
            "Reveal Types",
            "Revision Numbering Sequences",
            "Sloped Glazing Types",
            "Stair Path Types",
            "Stair Types",
            "Top Rail Types",
            "Toposolid Types",
            "View Reference Types",
            "Wall sweep Types",
            
            implement the specific collectors for these after we get the simple sample working
            QUESTION: is there ever a world where we store some of these collectors in its own query?
         */
        //maybe the switch expression does this just the same
        private List<(string queryName, Func<FilteredElementCollector, FilteredElementCollector> method)> queries = new List<(string, Func<FilteredElementCollector, FilteredElementCollector>)>(){

            ( "Callout Tags", fec => fec.OfCategory(BuiltInCategory.OST_CalloutHeads) ),
            ( "Ceiling Types", fec => fec.OfClass(typeof(CeilingType)) ),
            ( "Color Fill Schemes", fec => fec.OfClass(typeof(ColorFillScheme)) ),
            ( "Construction Types", fec => fec.OfCategory(BuiltInCategory.OST_EAConstructions) ),
            ( "Curtain System Types", fec => fec.OfClass(typeof(CurtainSystemType)) ),
            ( "Curtain Wall Types", fec => fec.OfClass(typeof(WallType)).Cast<WallType>().Where(wt => wt.Kind == WallKind.Curtain) as FilteredElementCollector),
            ( "Dimension Styles", fec => fec.OfClass(typeof(DimensionType)) ),
            ( "Elevation Tag Types", fec => fec.OfClass(typeof(ElementType)).Cast<ElementType>().Where(et => et.FamilyName == "Elevation Tag") as FilteredElementCollector ),
            ( "Fill Patterns", fec => fec.OfClass(typeof(FillPatternElement))),
            ( "Filled Region Types", fec => fec.OfClass(typeof(FilledRegionType))),
            ( "Filters", fec => fec.OfClass(typeof(FilterElement))),
            ( "Floor Types", fec => fec.OfClass(typeof(FloorType)) ),
            ( "Grid Types", fec => fec.OfClass(typeof(GridType))),
            ( "Line Patterns", fec => fec.OfClass(typeof(LinePatternElement))),
            ( "Line Styles", fec => fec.OfClass(typeof(GraphicsStyle))),
            ( "Materials", fec => fec.OfClass(typeof(Material))),
            ( "Project Parameters", fec => fec.OfClass(typeof(ParameterElement))),
            ( "Roof Types", fec => fec.OfClass(typeof(RoofType))),
            ( "Section Tag Types", fec => fec.OfCategory(BuiltInCategory.OST_SectionHeads)),
            ( "Text Types", fec => fec.OfClass(typeof(TextNoteType))),
            ( "View Templates", fec => fec.OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where(v => v.IsTemplate) as FilteredElementCollector),
            ( "Viewport Types", fec => fec.OfClass(typeof(ElementType)).Cast<ElementType>().Where(et => et.FamilyName == "Viewport") as FilteredElementCollector),
            ( "Wall Types", fec => fec.OfClass(typeof(WallType))),
            ( "Arrowhead Types", fec => fec.OfClass(typeof(ElementType)).Cast<ElementType>().Where(et => et.FamilyName == "Arrowhead") as FilteredElementCollector),
            ( "Pipe Types", fec => fec.OfClass(typeof(PipeType)).WhereElementIsElementType()),
            ( "Piping System Types", fec => fec.OfClass(typeof(PipingSystemType)).WhereElementIsElementType())
        };
        public frmSelectiveTransfer()
        {
            InitializeComponent();
        }
        public void AddItemsToCBox(IEnumerable<object> items)
        {
            docs = items.Cast<Document>().ToList<Document>();
            string[] titles = docs.Select(t => t.Title).ToArray();
            cbSelectDocument.Items.AddRange(titles);
            cbSelectDocument.Text = docs.First().Title;
            AddItemsToTreeView(docs.First());
            //add initial items to TreeView
        }
        public void AddItemsToTreeView(Document  docCurrent)
        {
            // originally built this to mirror the built-in transfer project standards command
            // maybe we open the list of targets to .. Family Types (Annotation, Model Category)
            
            tvElementTypeTree.BeginUpdate();
            tvElementTypeTree.Nodes.Clear();
            TreeNode root = new TreeNode(docCurrent.Title);
            
            foreach ((string queryName, Func<FilteredElementCollector,FilteredElementCollector> query) in queries.OrderBy(x => x.queryName))
            {
                TreeNode tnCurrentQuery = new TreeNode(queryName);
                //FilteredElementCollector fec = new FilteredElementCollector(docCurrent);
                //query(fec);
                //TODO: this is mess, but it works.  i think the original approach was good, just not working.
                List<(string displayName, ElementId eid)> fec = HelperGetElementsByQuery(queryName, docCurrent).ToList< (string displayName, ElementId eid)>();
                
                //TODO: simplify to one-liner
                foreach ((string displayName, ElementId eid) e in fec) {
                    //TODO: set text color if element exists in the active document
                    TreeNode child = new TreeNode(e.displayName);
                    child.Tag = e.eid;
                    tnCurrentQuery.Nodes.Add(child);
                }
                root.Nodes.Add(tnCurrentQuery);
            }
            tvElementTypeTree.Nodes.Add(root);
            tvElementTypeTree.EndUpdate();
        }
        private IEnumerable<(string displayName, ElementId eid)> HelperGetElementsByQuery(string query, Document doc) => query switch
        {
            "Arrowhead Types" => new FilteredElementCollector(doc).OfClass(typeof(ElementType)).WhereElementIsElementType()
                .Cast<ElementType>()
                .Where(et => et.FamilyName == "Arrowhead")
                .Select(x => (x.Name, x.Id)),
            "Callout Tags" => new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_CalloutHeads)
                .WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)) ,
            "Ceiling Types"=> new FilteredElementCollector(doc).OfClass(typeof(CeilingType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Color Fill Schemes"=> new FilteredElementCollector(doc).OfClass(typeof(ColorFillScheme))
                .Select(x => (x.Name, x.Id)),
            "Construction Types"=> new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_EAConstructions)
                .Select(x => (x.Name, x.Id)),
            "Curtain System Types"=> new FilteredElementCollector(doc).OfClass(typeof(CurtainSystemType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Curtain Wall Types"=> new FilteredElementCollector(doc).OfClass(typeof(WallType)).WhereElementIsElementType()
                .Cast<WallType>()
                .Where(wt => wt.Kind == WallKind.Curtain)
                .Select(x => (x.Name, x.Id)),
            "Dimension Styles"=> new FilteredElementCollector(doc).OfClass(typeof(DimensionType))
                .Select(x => (x.Name, x.Id)),
            "Elevation Tag Types"=> new FilteredElementCollector(doc).OfClass(typeof(ElementType)).WhereElementIsElementType()
                .Cast<ElementType>()
                .Where(et => et.FamilyName == "Elevation Tag")
                .Select(x => (x.Name, x.Id)),
            "Fill Patterns"=> new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement))
                .Select(x => (x.Name, x.Id)),
            "Filled Region Types"=> new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Filters"=> new FilteredElementCollector(doc).OfClass(typeof(FilterElement))
                .Select(x => (x.Name, x.Id)),
            "Floor Types"=> new FilteredElementCollector(doc).OfClass(typeof(FloorType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Grid Types"=> new FilteredElementCollector(doc).OfClass(typeof(GridType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Line Patterns"=> new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement))
                .Select(x => (x.Name, x.Id)),
            "Line Styles"=> new FilteredElementCollector(doc).OfClass(typeof(GraphicsStyle))
                .Select(x => (x.Name, x.Id)),
            "Materials"=> new FilteredElementCollector(doc).OfClass(typeof(Material))
                .Select(x => (x.Name, x.Id)),
            "Project Parameters"=> new FilteredElementCollector(doc).OfClass(typeof(ParameterElement))
                .Select(x => (x.Name, x.Id)),
            "Roof Types"=> new FilteredElementCollector(doc).OfClass(typeof(RoofType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Section Tag Types"=> new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_SectionHeads).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Text Types"=> new FilteredElementCollector(doc).OfClass(typeof(TextNoteType))
                .Select(x => (x.Name, x.Id)),
            "View Templates"=> new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(v => v.IsTemplate)
                .Select(x => (x.Name, x.Id)),
            "Viewport Types"=> new FilteredElementCollector(doc).OfClass(typeof(ElementType)).WhereElementIsElementType()
                .Cast<ElementType>()
                .Where(et => et.FamilyName == "Viewport")
                .Select(x => (x.Name, x.Id)),
            "Wall Types"=> new FilteredElementCollector(doc).OfClass(typeof(WallType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Pipe Types" => new FilteredElementCollector(doc).OfClass(typeof(PipeType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            "Piping System Types" => new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType)).WhereElementIsElementType()
                .Select(x => (x.Name, x.Id)),
            _ => throw new KeyNotFoundException($"{query} is not a valid key"),
        };
        public PowerDialogResult ShowDialogWithResult()
        {
            this.ShowDialog();
            return _PBDialogResult;
        }
        private void cbSelectDocument_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tvElementTypeTree.TopNode != null) { tvElementTypeTree.TopNode.Text = cbSelectDocument.SelectedText; }
            int selected = cbSelectDocument.SelectedIndex;
            AddItemsToTreeView(docs[selected]);
        }
        private void btnAccept_Click(object sender, EventArgs e)
        {
            _PBDialogResult = new PowerDialogResult
            {
                IsAccepted = true,
            };
            _PBDialogResult.AddSelectionResult(docs[cbSelectDocument.SelectedIndex]);
            List<ElementId> SelectedElements = new List<ElementId>();
            //replace this with a tidy recursive implementation
            //i think there's a whole more efficient way to get the selected results from the treeview
            foreach (TreeNode cur in tvElementTypeTree.Nodes)
            {
                foreach (TreeNode l1 in cur.Nodes)
                {
                    foreach (TreeNode l2 in l1.Nodes)
                    {
                        if (l2.Checked) { SelectedElements.Add((ElementId)l2.Tag); }
                    }
                }
            }
            _PBDialogResult.AddSelectionResult(SelectedElements);
            this.Close();
        }
        //TODO: add hierarchical selection.
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _PBDialogResult = new PowerDialogResult
            {
                IsAccepted = false,
            };
            this.Close();
        }
    }
}
