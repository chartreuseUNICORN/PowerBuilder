using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PowerBuilder;

namespace PowerBuilderUI.Forms
{
    public partial class frmSetArrowheadTypes : System.Windows.Forms.Form
    {
        private PBDialogResult _PBDialogResult;
        public frmSetArrowheadTypes()
        {
            InitializeComponent();
        }

        public void AddArrowTypes(object[] ArrowTypes) {
            cbSelectArrowhead.Items.AddRange(ArrowTypes);
        }
        public void AddTargets(object[] Targets) {
            clbSelectTargets.Items.AddRange(Targets);
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
            _PBDialogResult.AddSelectionResult(cbSelectArrowhead.SelectedIndex);
            _PBDialogResult.AddSelectionResult(clbSelectTargets.CheckedIndices);
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
