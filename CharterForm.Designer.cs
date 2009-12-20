namespace DepCharter
{
  partial class CharterForm
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
          this.components = new System.ComponentModel.Container();
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharterForm));
          ImageViewport imageViewport1 = new ImageViewport();
          this.projectsBox = new System.Windows.Forms.ListBox();
          this.HelpText = new System.Windows.Forms.ToolTip(this.components);
          this.btGenerate = new System.Windows.Forms.Button();
          this.cbTrueType = new System.Windows.Forms.CheckBox();
          this.tbFontsize = new System.Windows.Forms.TextBox();
          this.cbFontsize = new System.Windows.Forms.CheckBox();
          this.cbAspect = new System.Windows.Forms.CheckBox();
          this.tbAspect = new System.Windows.Forms.TextBox();
          this.mainSplitpanel = new System.Windows.Forms.SplitContainer();
          this.topleftPanel = new System.Windows.Forms.Panel();
          this.solutionTree = new System.Windows.Forms.TreeView();
          this.bottomLeftPanel = new System.Windows.Forms.Panel();
          this.cbReduce = new System.Windows.Forms.CheckBox();
          this.nwImageViewer1 = new depcharter.NWImageViewer();
          this.statusStrip = new System.Windows.Forms.StatusStrip();
          this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
          this.mainSplitpanel.Panel1.SuspendLayout();
          this.mainSplitpanel.Panel2.SuspendLayout();
          this.mainSplitpanel.SuspendLayout();
          this.topleftPanel.SuspendLayout();
          this.bottomLeftPanel.SuspendLayout();
          this.statusStrip.SuspendLayout();
          this.SuspendLayout();
          // 
          // projectsBox
          // 
          this.projectsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.projectsBox.FormattingEnabled = true;
          this.projectsBox.Location = new System.Drawing.Point(14, 11);
          this.projectsBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 150);
          this.projectsBox.Name = "projectsBox";
          this.projectsBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
          this.projectsBox.Size = new System.Drawing.Size(154, 121);
          this.projectsBox.Sorted = true;
          this.projectsBox.TabIndex = 5;
          // 
          // btGenerate
          // 
          this.btGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.btGenerate.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.btGenerate.Image = global::depcharter.Properties.Resources.chart_organisation;
          this.btGenerate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
          this.btGenerate.Location = new System.Drawing.Point(57, 234);
          this.btGenerate.Name = "btGenerate";
          this.btGenerate.Size = new System.Drawing.Size(160, 34);
          this.btGenerate.TabIndex = 6;
          this.btGenerate.Text = "Generate graph";
          this.btGenerate.UseVisualStyleBackColor = true;
          this.btGenerate.Click += new System.EventHandler(this.btGenerate_Click);
          // 
          // cbTrueType
          // 
          this.cbTrueType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.cbTrueType.AutoSize = true;
          this.cbTrueType.Location = new System.Drawing.Point(19, 162);
          this.cbTrueType.Name = "cbTrueType";
          this.cbTrueType.Size = new System.Drawing.Size(105, 17);
          this.cbTrueType.TabIndex = 8;
          this.cbTrueType.Text = "use truetype font";
          this.cbTrueType.UseVisualStyleBackColor = true;
          // 
          // tbFontsize
          // 
          this.tbFontsize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.tbFontsize.Enabled = false;
          this.tbFontsize.Location = new System.Drawing.Point(120, 185);
          this.tbFontsize.Name = "tbFontsize";
          this.tbFontsize.Size = new System.Drawing.Size(36, 20);
          this.tbFontsize.TabIndex = 10;
          // 
          // cbFontsize
          // 
          this.cbFontsize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.cbFontsize.AutoSize = true;
          this.cbFontsize.Location = new System.Drawing.Point(19, 185);
          this.cbFontsize.Name = "cbFontsize";
          this.cbFontsize.Size = new System.Drawing.Size(82, 17);
          this.cbFontsize.TabIndex = 9;
          this.cbFontsize.Text = "use fontsize";
          this.cbFontsize.UseVisualStyleBackColor = true;
          this.cbFontsize.CheckedChanged += new System.EventHandler(this.cbFontsize_CheckedChanged);
          // 
          // cbAspect
          // 
          this.cbAspect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.cbAspect.AutoSize = true;
          this.cbAspect.Checked = true;
          this.cbAspect.CheckState = System.Windows.Forms.CheckState.Checked;
          this.cbAspect.Location = new System.Drawing.Point(19, 211);
          this.cbAspect.Name = "cbAspect";
          this.cbAspect.Size = new System.Drawing.Size(95, 17);
          this.cbAspect.TabIndex = 11;
          this.cbAspect.Text = "Fit aspect ratio";
          this.cbAspect.UseVisualStyleBackColor = true;
          this.cbAspect.CheckedChanged += new System.EventHandler(this.cbAspect_CheckedChanged);
          // 
          // tbAspect
          // 
          this.tbAspect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.tbAspect.Location = new System.Drawing.Point(120, 211);
          this.tbAspect.Name = "tbAspect";
          this.tbAspect.Size = new System.Drawing.Size(36, 20);
          this.tbAspect.TabIndex = 12;
          this.tbAspect.Text = "0.7";
          // 
          // mainSplitpanel
          // 
          this.mainSplitpanel.Dock = System.Windows.Forms.DockStyle.Fill;
          this.mainSplitpanel.Location = new System.Drawing.Point(0, 0);
          this.mainSplitpanel.Name = "mainSplitpanel";
          // 
          // mainSplitpanel.Panel1
          // 
          this.mainSplitpanel.Panel1.Controls.Add(this.topleftPanel);
          this.mainSplitpanel.Panel1.Controls.Add(this.bottomLeftPanel);
          this.mainSplitpanel.Panel1MinSize = 250;
          // 
          // mainSplitpanel.Panel2
          // 
          this.mainSplitpanel.Panel2.AutoScroll = true;
          this.mainSplitpanel.Panel2.Controls.Add(this.nwImageViewer1);
          this.mainSplitpanel.Panel2.Controls.Add(this.statusStrip);
          this.mainSplitpanel.Size = new System.Drawing.Size(992, 573);
          this.mainSplitpanel.SplitterDistance = 263;
          this.mainSplitpanel.TabIndex = 15;
          // 
          // topleftPanel
          // 
          this.topleftPanel.Controls.Add(this.solutionTree);
          this.topleftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
          this.topleftPanel.Location = new System.Drawing.Point(0, 0);
          this.topleftPanel.Name = "topleftPanel";
          this.topleftPanel.Size = new System.Drawing.Size(263, 289);
          this.topleftPanel.TabIndex = 16;
          // 
          // solutionTree
          // 
          this.solutionTree.Dock = System.Windows.Forms.DockStyle.Fill;
          this.solutionTree.Location = new System.Drawing.Point(0, 0);
          this.solutionTree.Name = "solutionTree";
          this.solutionTree.Size = new System.Drawing.Size(263, 289);
          this.solutionTree.TabIndex = 14;
          // 
          // bottomLeftPanel
          // 
          this.bottomLeftPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
          this.bottomLeftPanel.Controls.Add(this.btGenerate);
          this.bottomLeftPanel.Controls.Add(this.cbTrueType);
          this.bottomLeftPanel.Controls.Add(this.tbFontsize);
          this.bottomLeftPanel.Controls.Add(this.projectsBox);
          this.bottomLeftPanel.Controls.Add(this.cbFontsize);
          this.bottomLeftPanel.Controls.Add(this.cbAspect);
          this.bottomLeftPanel.Controls.Add(this.cbReduce);
          this.bottomLeftPanel.Controls.Add(this.tbAspect);
          this.bottomLeftPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
          this.bottomLeftPanel.Location = new System.Drawing.Point(0, 289);
          this.bottomLeftPanel.Name = "bottomLeftPanel";
          this.bottomLeftPanel.Size = new System.Drawing.Size(263, 284);
          this.bottomLeftPanel.TabIndex = 15;
          // 
          // cbReduce
          // 
          this.cbReduce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.cbReduce.AutoSize = true;
          this.cbReduce.Location = new System.Drawing.Point(19, 139);
          this.cbReduce.Name = "cbReduce";
          this.cbReduce.Size = new System.Drawing.Size(59, 17);
          this.cbReduce.TabIndex = 7;
          this.cbReduce.Text = "reduce";
          this.cbReduce.UseVisualStyleBackColor = true;
          // 
          // nwImageViewer1
          // 
          this.nwImageViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.nwImageViewer1.Image = ((System.Drawing.Image)(resources.GetObject("nwImageViewer1.Image")));
          imageViewport1.DrawingAreaSize = new System.Drawing.Size(725, 551);
          imageViewport1.ImageSize = new System.Drawing.Size(4023, 2267);
          imageViewport1.Location = new System.Drawing.Point(0, 0);
          imageViewport1.Zoom = 1F;
          this.nwImageViewer1.ImageViewport = imageViewport1;
          this.nwImageViewer1.Location = new System.Drawing.Point(0, 0);
          this.nwImageViewer1.Name = "nwImageViewer1";
          this.nwImageViewer1.Size = new System.Drawing.Size(725, 551);
          this.nwImageViewer1.TabIndex = 15;
          // 
          // statusStrip
          // 
          this.statusStrip.BackColor = System.Drawing.Color.LightSkyBlue;
          this.statusStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
          this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
          this.statusStrip.Location = new System.Drawing.Point(0, 551);
          this.statusStrip.Name = "statusStrip";
          this.statusStrip.Size = new System.Drawing.Size(725, 22);
          this.statusStrip.TabIndex = 14;
          this.statusStrip.Text = "statusStrip1";
          // 
          // statusLabel
          // 
          this.statusLabel.Name = "statusLabel";
          this.statusLabel.Size = new System.Drawing.Size(52, 17);
          this.statusLabel.Text = "Zoom 1:1";
          // 
          // CharterForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackColor = System.Drawing.Color.LightSkyBlue;
          this.ClientSize = new System.Drawing.Size(992, 573);
          this.Controls.Add(this.mainSplitpanel);
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MinimizeBox = false;
          this.MinimumSize = new System.Drawing.Size(420, 240);
          this.Name = "CharterForm";
          this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "DepCharter v1.2 by Jan Wilmans [ Graphical configuration ]";
          this.mainSplitpanel.Panel1.ResumeLayout(false);
          this.mainSplitpanel.Panel2.ResumeLayout(false);
          this.mainSplitpanel.Panel2.PerformLayout();
          this.mainSplitpanel.ResumeLayout(false);
          this.topleftPanel.ResumeLayout(false);
          this.bottomLeftPanel.ResumeLayout(false);
          this.bottomLeftPanel.PerformLayout();
          this.statusStrip.ResumeLayout(false);
          this.statusStrip.PerformLayout();
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip HelpText;
        private System.Windows.Forms.Button btGenerate;
        public System.Windows.Forms.ListBox projectsBox;
        public System.Windows.Forms.CheckBox cbTrueType;
        public System.Windows.Forms.TextBox tbFontsize;
        public System.Windows.Forms.CheckBox cbFontsize;
        public System.Windows.Forms.CheckBox cbAspect;
        public System.Windows.Forms.TextBox tbAspect;
        private System.Windows.Forms.SplitContainer mainSplitpanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        public System.Windows.Forms.CheckBox cbReduce;
        private System.Windows.Forms.Panel bottomLeftPanel;
        public System.Windows.Forms.TreeView solutionTree;
        private System.Windows.Forms.Panel topleftPanel;
        private depcharter.NWImageViewer nwImageViewer1;
        public System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}