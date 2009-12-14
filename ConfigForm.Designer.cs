using System;
using System.Drawing;

namespace DepCharter
{
    partial class ConfigForm
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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
          this.projectsBox = new System.Windows.Forms.ListBox();
          this.HelpText = new System.Windows.Forms.ToolTip(this.components);
          this.btGenerate = new System.Windows.Forms.Button();
          this.cbReduce = new System.Windows.Forms.CheckBox();
          this.cbTrueType = new System.Windows.Forms.CheckBox();
          this.tbFontsize = new System.Windows.Forms.TextBox();
          this.cbFontsize = new System.Windows.Forms.CheckBox();
          this.cbAspect = new System.Windows.Forms.CheckBox();
          this.tbAspect = new System.Windows.Forms.TextBox();
          this.SuspendLayout();
          // 
          // projectsBox
          // 
          this.projectsBox.FormattingEnabled = true;
          this.projectsBox.Location = new System.Drawing.Point(12, 12);
          this.projectsBox.Name = "projectsBox";
          this.projectsBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
          this.projectsBox.Size = new System.Drawing.Size(174, 186);
          this.projectsBox.Sorted = true;
          this.projectsBox.TabIndex = 5;
          // 
          // btGenerate
          // 
          this.btGenerate.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.btGenerate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
          this.btGenerate.Location = new System.Drawing.Point(318, 160);
          this.btGenerate.Name = "btGenerate";
          this.btGenerate.Size = new System.Drawing.Size(160, 38);
          this.btGenerate.TabIndex = 6;
          this.btGenerate.Text = "Generate graph";
          this.btGenerate.UseVisualStyleBackColor = true;
          // 
          // cbReduce
          // 
          this.cbReduce.AutoSize = true;
          this.cbReduce.Location = new System.Drawing.Point(217, 12);
          this.cbReduce.Name = "cbReduce";
          this.cbReduce.Size = new System.Drawing.Size(59, 17);
          this.cbReduce.TabIndex = 7;
          this.cbReduce.Text = "reduce";
          this.cbReduce.UseVisualStyleBackColor = true;
          // 
          // cbTrueType
          // 
          this.cbTrueType.AutoSize = true;
          this.cbTrueType.Location = new System.Drawing.Point(217, 35);
          this.cbTrueType.Name = "cbTrueType";
          this.cbTrueType.Size = new System.Drawing.Size(105, 17);
          this.cbTrueType.TabIndex = 8;
          this.cbTrueType.Text = "use truetype font";
          this.cbTrueType.UseVisualStyleBackColor = true;
          // 
          // tbFontsize
          // 
          this.tbFontsize.Enabled = false;
          this.tbFontsize.Location = new System.Drawing.Point(318, 58);
          this.tbFontsize.Name = "tbFontsize";
          this.tbFontsize.Size = new System.Drawing.Size(36, 20);
          this.tbFontsize.TabIndex = 10;
          // 
          // cbFontsize
          // 
          this.cbFontsize.AutoSize = true;
          this.cbFontsize.Location = new System.Drawing.Point(217, 58);
          this.cbFontsize.Name = "cbFontsize";
          this.cbFontsize.Size = new System.Drawing.Size(82, 17);
          this.cbFontsize.TabIndex = 9;
          this.cbFontsize.Text = "use fontsize";
          this.cbFontsize.UseVisualStyleBackColor = true;
          this.cbFontsize.CheckedChanged += new System.EventHandler(this.cbFontsize_CheckedChanged);
          // 
          // cbAspect
          // 
          this.cbAspect.AutoSize = true;
          this.cbAspect.Checked = true;
          this.cbAspect.CheckState = System.Windows.Forms.CheckState.Checked;
          this.cbAspect.Location = new System.Drawing.Point(217, 84);
          this.cbAspect.Name = "cbAspect";
          this.cbAspect.Size = new System.Drawing.Size(95, 17);
          this.cbAspect.TabIndex = 11;
          this.cbAspect.Text = "Fit aspect ratio";
          this.cbAspect.UseVisualStyleBackColor = true;
          this.cbAspect.CheckedChanged += new System.EventHandler(this.cbAspect_CheckedChanged);
          // 
          // tbAspect
          // 
          this.tbAspect.Location = new System.Drawing.Point(318, 84);
          this.tbAspect.Name = "tbAspect";
          this.tbAspect.Size = new System.Drawing.Size(36, 20);
          this.tbAspect.TabIndex = 12;
          this.tbAspect.Text = "0.7";
          // 
          // ConfigForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.BackColor = System.Drawing.Color.Lavender;
          this.ClientSize = new System.Drawing.Size(509, 213);
          this.Controls.Add(this.tbAspect);
          this.Controls.Add(this.cbAspect);
          this.Controls.Add(this.tbFontsize);
          this.Controls.Add(this.cbFontsize);
          this.Controls.Add(this.cbTrueType);
          this.Controls.Add(this.cbReduce);
          this.Controls.Add(this.btGenerate);
          this.Controls.Add(this.projectsBox);
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MinimizeBox = false;
          this.MinimumSize = new System.Drawing.Size(420, 240);
          this.Name = "ConfigForm";
          this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "DepCharter v1.2 by Jan Wilmans [ Graphical configuration ]";
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip HelpText;
        private System.Windows.Forms.Button btGenerate;
        public System.Windows.Forms.ListBox projectsBox;
        public System.Windows.Forms.CheckBox cbReduce;
        public System.Windows.Forms.CheckBox cbTrueType;
        public System.Windows.Forms.TextBox tbFontsize;
        public System.Windows.Forms.CheckBox cbFontsize;
        public System.Windows.Forms.CheckBox cbAspect;
        public System.Windows.Forms.TextBox tbAspect;
    }
}