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
    public partial class frmSelectiveTransfer : Form
    {
        private PBDialogResult _PBDialogResult;
        public frmSelectiveTransfer()
        {
            InitializeComponent();
        }
        public void AddItemsToCBox(object[] items)
        {
            cbSelection1.Items.AddRange(items);
        }
        public void AddItemsToTreeView(object[] items)
        {
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
    }
}
