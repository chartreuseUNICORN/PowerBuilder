namespace PowerBuilder.Forms
{
    partial class frmSetArrowheadTypes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbSelectArrowhead = new System.Windows.Forms.ComboBox();
            this.clbSelectTargets = new System.Windows.Forms.CheckedListBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblSelectTargets = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbSelectArrowhead
            // 
            this.cbSelectArrowhead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSelectArrowhead.FormattingEnabled = true;
            this.cbSelectArrowhead.Location = new System.Drawing.Point(18, 18);
            this.cbSelectArrowhead.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSelectArrowhead.Name = "cbSelectArrowhead";
            this.cbSelectArrowhead.Size = new System.Drawing.Size(218, 28);
            this.cbSelectArrowhead.TabIndex = 0;
            // 
            // clbSelectTargets
            // 
            this.clbSelectTargets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbSelectTargets.CheckOnClick = true;
            this.clbSelectTargets.FormattingEnabled = true;
            this.clbSelectTargets.Location = new System.Drawing.Point(18, 102);
            this.clbSelectTargets.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.clbSelectTargets.Name = "clbSelectTargets";
            this.clbSelectTargets.Size = new System.Drawing.Size(218, 257);
            this.clbSelectTargets.TabIndex = 1;
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Location = new System.Drawing.Point(66, 423);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(112, 35);
            this.btnAccept.TabIndex = 2;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(186, 423);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblSelectTargets
            // 
            this.lblSelectTargets.AutoSize = true;
            this.lblSelectTargets.Location = new System.Drawing.Point(18, 62);
            this.lblSelectTargets.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSelectTargets.Name = "lblSelectTargets";
            this.lblSelectTargets.Size = new System.Drawing.Size(112, 20);
            this.lblSelectTargets.TabIndex = 4;
            this.lblSelectTargets.Text = "Select Targets";
            // 
            // frmSetArrowheadTypes
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(311, 472);
            this.Controls.Add(this.lblSelectTargets);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.clbSelectTargets);
            this.Controls.Add(this.cbSelectArrowhead);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmSetArrowheadTypes";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbSelectArrowhead;
        private System.Windows.Forms.CheckedListBox clbSelectTargets;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblSelectTargets;
    }
}