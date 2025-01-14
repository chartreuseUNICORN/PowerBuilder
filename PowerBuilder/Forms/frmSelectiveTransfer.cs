using Autodesk.Revit.DB;
using PowerBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerBuilderUI.Forms
{
    public partial class frmSelectiveTransfer : System.Windows.Forms.Form
    {
        private PBDialogResult _PBDialogResult;
        private List<Document> docs;
        /* here's the original list from Transfer Project Standards
         * ["Analytical Link Types",
            "Arrowhead Styles",
            #"Bending Detail",
            "Callout Tags",
            "Ceiling Types",
            "Color Fill Schemes",
            "Construction Types",
            "Curtain System Types",
            "Curtain Wall Types",
            "Cut Mark Types",
            "Dimension Styles",
            "Elevation Tag Types",
            "Fill Patterns",
            "Filled Region Types",
            "Filters",
            "Floor Types",
            "Foundation Slab Types",
            "Grid Types",
            "Handrail Types",
            "Level Types",
            "Line Patterns",
            "Line Styles",
            #"Line Weights",
            #"Load Types",
            "Materials",
            "Model Text Types",
            #"Object Styles",
            "Phase Settings",
            "Project Parameters",
            "Railing Types",
            "Ramp Types",
            "Repeating Detail Types",
            "Reveal Types",
            "Revision Numbering Sequences",
            "Roof Types",
            "Section Tag Types",
            "Sloped Glazing Types",
            "Stair Path Types",
            "Stair Types",
            "Text Types",
            "Top Rail Types",
            "Toposolid Types",
            "View Reference Types",
            "View Templates",
            "Viewport Types",
            "Wall sweep Types",
            "Wall Types",
            ]
            implement the specific collectors for these after we get the simple sample working
            QUESTION: is there ever a world where we store some of these collectors in its own query?
         */
        private List<(string, Func<FilteredElementCollector, FilteredElementCollector>)> queries = new List<(string, Func<FilteredElementCollector, FilteredElementCollector>)>(){

            ( "Callout Tags", fec => fec.OfCategory(BuiltInCategory.OST_CalloutHeads) ),
            ( "Ceiling Types", fec => fec.OfClass(typeof(CeilingType)) ),
            ( "Color Fill Schemes", fec => fec.OfClass(typeof(ColorFillScheme)) ),
            ( "Construction Types", fec => fec.OfCategory(BuiltInCategory.OST_EAConstructions) ),
            ( "Curtain System Types", fec => fec.OfClass(typeof(CurtainSystemType)) ),
            ( "Curtain Wall Types", fec => fec.OfClass(typeof(WallType)).Cast<WallType>().Where(wt => wt.Kind == WallKind.Curtain) as FilteredElementCollector ),
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
            ( "View Templates", fec => fec.OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>(v => v.IsTemplate) as FilteredElementCollector),
            ( "Viewport Types", fec => fec.OfClass(typeof(ElementType)).Cast<ElementType>().Where(et => et.FamilyName == "Viewport") as FilteredElementCollector),
            ( "Wall Types", fec => fec.OfClass(typeof(WallType)))
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
            
            foreach ((string queryName, Func<FilteredElementCollector,FilteredElementCollector> query) in queries)
            {
                TreeNode tnCurrentQuery = new TreeNode(queryName);
                FilteredElementCollector fec = new FilteredElementCollector(docCurrent);
                query(fec);
                //TODO: simplify to one-liner
                //should this also filter against what exists in the current document? technically no, because you may want to override.
                
                foreach (Element e in fec) {
                    TreeNode child = new TreeNode(e.Name);
                    child.Tag = e.Id;
                    tnCurrentQuery.Nodes.Add(child);
                }
                root.Nodes.Add(tnCurrentQuery);
            }
            tvElementTypeTree.Nodes.Add(root);
            tvElementTypeTree.EndUpdate();
        }
        
        public PBDialogResult ShowDialogWithResult()
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
            _PBDialogResult = new PBDialogResult
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
            _PBDialogResult = new PBDialogResult
            {
                IsAccepted = false,
            };
            this.Close();
        }
    }
}
