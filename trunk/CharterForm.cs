using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DepCharter
{
    public partial class CharterForm : Form
    {
      public CharterForm()
        {
            InitializeComponent();
            this.ResumeLayout(false);
        }

        private void cbFontsize_CheckedChanged(object sender, EventArgs e)
        {
          tbFontsize.Enabled = cbFontsize.Checked;
        }

        private void cbAspect_CheckedChanged(object sender, EventArgs e)
        {
          tbAspect.Enabled = cbAspect.Checked;
        }

        private void fitImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
          //pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void fullImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
          //pictureBox.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void imageViewer_Click(object sender, EventArgs e)
        {

        }
    }
}
