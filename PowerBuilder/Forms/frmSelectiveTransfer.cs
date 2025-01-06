using Autodesk.Revit.DB;
using PowerBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerBuilderUI
{
    public partial class frmSelectiveTransfer : System.Windows.Forms.Form
    {
        private PBDialogResult _PBDialogResult;
        private DocumentSet docs;
        public frmSelectiveTransfer()
        {
            InitializeComponent();
        }
        public void AddItemsToCBox(IEnumerable<object> items)
        {
            docs = items as DocumentSet;
            cbSelection1.Items.AddRange(items.Cast<Document>().Select(t => t.Title) as object[]);

            //add initial items to TreeView
        }
        public void AddItemsToTreeView(Document  docCurrent)
        {
            // refresh tree view with elements in docCurrent not in docTarget
            // i think this is why Target was the active document, pull other things into it..
            throw new NotImplementedException("implement this connection");
        }
        public PBDialogResult ShowDialogWithResult()
        {
            this.ShowDialog();
            return _PBDialogResult;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            _PBDialogResult = new PBDialogResult
            {
                IsAccepted = true,
            };
            _PBDialogResult.AddSelectionResult(cbSelection1.SelectedIndex);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _PBDialogResult = new PBDialogResult
            {
                IsAccepted = false,
            };
            this.Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void cbSelection1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
