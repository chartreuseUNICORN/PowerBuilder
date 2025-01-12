using System.Collections.Generic;

namespace PowerBuilderUI
{
    partial class frmSelectiveTransfer
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Element Type of A");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Category A", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Category B");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("SelectedDocument", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode3});
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.cbSelectDocument = new System.Windows.Forms.ComboBox();
            this.tvElementTypeTree = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(227, 192);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Location = new System.Drawing.Point(146, 191);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 1;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // cbSelectDocument
            // 
            this.cbSelectDocument.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSelectDocument.FormattingEnabled = true;
            this.cbSelectDocument.Location = new System.Drawing.Point(13, 13);
            this.cbSelectDocument.Name = "cbSelectDocument";
            this.cbSelectDocument.Size = new System.Drawing.Size(289, 28);
            this.cbSelectDocument.TabIndex = 2;
            this.cbSelectDocument.Text = "Select Target Document";
            this.cbSelectDocument.SelectedIndexChanged += new System.EventHandler(this.cbSelectDocument_SelectedIndexChanged);
            // 
            // tvElementTypeTree
            // 
            this.tvElementTypeTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvElementTypeTree.BackColor = System.Drawing.Color.White;
            this.tvElementTypeTree.CheckBoxes = true;
            this.tvElementTypeTree.HideSelection = false;
            this.tvElementTypeTree.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tvElementTypeTree.Location = new System.Drawing.Point(13, 48);
            this.tvElementTypeTree.Name = "tvElementTypeTree";
            treeNode1.Name = "nA1";
            treeNode1.Text = "Element Type of A";
            treeNode2.Name = "nCategoryA";
            treeNode2.Text = "Category A";
            treeNode3.Name = "nCategoryB";
            treeNode3.Text = "Category B";
            treeNode4.Name = "nTargetDocument";
            treeNode4.Text = "SelectedDocument";
            this.tvElementTypeTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4});
            this.tvElementTypeTree.Size = new System.Drawing.Size(289, 138);
            this.tvElementTypeTree.TabIndex = 3;
            // 
            // frmSelectiveTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 227);
            this.Controls.Add(this.tvElementTypeTree);
            this.Controls.Add(this.cbSelectDocument);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.btnCancel);
            this.Name = "frmSelectiveTransfer";
            this.Text = "Selective Transfer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.ComboBox cbSelectDocument;
        private System.Windows.Forms.TreeView tvElementTypeTree;
    }
}