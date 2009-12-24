using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DepCharter
{
    public partial class CharterForm : Form
    {
      public CharterForm()
        {
          InitializeComponent();
          nwImageViewer1.Invalidated += new InvalidateEventHandler(updateStatus);
          ResumeLayout(false);
        }

        private void updateStatus(object sender, EventArgs e)
        {
          statusLabel.Text = "Zoom " + String.Format("{0}%", (int)(nwImageViewer1.ImageViewport.Zoom * 100));
        }

        private void cbFontsize_CheckedChanged(object sender, EventArgs e)
        {
          tbFontsize.Enabled = cbFontsize.Checked;
        }

        private void cbAspect_CheckedChanged(object sender, EventArgs e)
        {
          tbAspect.Enabled = cbAspect.Checked;
        }

        private void btGenerate_Click(object sender, EventArgs e)
        {
          // generate graph using dot here
        }

        string folderName = "";
        private void browseButton_Click(object sender, EventArgs e)
        {
          FolderBrowserDialog folderDialog = new FolderBrowserDialog();
          if (folderDialog.ShowDialog() == DialogResult.OK)
          {
            folderName = folderDialog.SelectedPath;
            Thread thread = new Thread(new ThreadStart(this.FillTree));
            thread.Start();
          }
        }

        private void FillTree()
        {
          Program.PopulateSolutionTree(this, folderName, "*.sln");
        }
    }
}
