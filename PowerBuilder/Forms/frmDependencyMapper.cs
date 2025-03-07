using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace PowerBuilder.Forms {
    public partial class frmDependencyMapper : System.Windows.Forms.Form {
        public frmDependencyMapper() {
            InitializeComponent();
        }

        public void AddItemsToTreeView(Document doc, ElementId tar, Dictionary<ElementId,List<ElementId>> dependencyMap) {
            Element thisElement = doc.GetElement(tar);
            TreeNode root = new TreeNode();
            root.Text = thisElement.Name;
            //if this ends up not being static, it needs to be recursive
            foreach (ElementId branch in dependencyMap.Keys) {
                TreeNode source = new TreeNode();
                Element branchElement = doc.GetElement(branch);
                /* how should the graphic representation of this be.  think about it.
                 ownership by:
                    Type
                    View
                        Legend
                    Group (?)
                    subcomponent
                    Schedule (Text)
                    Sketch (line style)
                 * */
                source.Text = $"[{branchElement.Id}] {doc.GetElement(branch).Name}";
                foreach(ElementId leaf in dependencyMap[branch]) {
                    
                    TreeNode tnLeaf = new TreeNode();
                    tnLeaf.Text = doc.GetElement(leaf).Name;
                    source.Nodes.Add(tnLeaf);
                }
                root.Nodes.Add(source);
            }
            tvDependencyMap.Nodes.Add(root);
        }
        private void btnAccept_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
