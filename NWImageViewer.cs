using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace depcharter
{
  public partial class NWImageViewerBase : System.Windows.Forms.PictureBox
  {

  }

  public partial class NWImageViewerBase2 : System.Windows.Forms.UserControl
  {
    public Image Image { get; set; }
    public TextureBrush Brush { get; set; }
  }

  public partial class NWImageViewer : NWImageViewerBase2
  {
    public NWImageViewer()
    {
      zoom = 1.0F;
//      InitializeComponent();

      base.SetStyle(ControlStyles.DoubleBuffer, true);
      base.SetStyle(ControlStyles.UserPaint, true);
      base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.Dock = DockStyle.Fill;
      base.ResumeLayout(false);

      FileStream fs = new FileStream("large.png", FileMode.Open, FileAccess.Read, FileShare.Read);
      this.Image = Image.FromStream(fs);
      this.Brush = new TextureBrush(Image);     
      fs.Close();   // close the file after accessing the image (load_on_access)

      this.MouseDown += new MouseEventHandler(this.MouseDown_event);
      this.MouseUp += new MouseEventHandler(this.MouseUp_event);
      this.MouseWheel += new MouseEventHandler(this.MouseWheel_event);
      this.MouseLeave += new EventHandler(this.MouseLeave_event);
      this.MouseMove += new MouseEventHandler(this.MouseMove_event);
      
      //FontDialog flg = new FontDialog(); flg.ShowDialog(); tb.FontName = flg.Font;
    }

    private Point screenPoint(Point point)
    {
      // return a screen-relative point
      return new Point(this.Left + point.X, this.Top + point.Y);
    }

    private Point screenPoint(int x, int y)
    {
      // return a screen-relative point
      return new Point(this.Left + x, this.Top + y);
    }

    public void MouseDown_event(object sender, MouseEventArgs e)
    {
      mouseDownLocation = screenPoint(e.X, e.Y);
      Console.WriteLine("down x/y: " + mouseDownLocation.X + ", " + mouseDownLocation.Y);
      dragging = true;
      Console.WriteLine("drag on");

    }

    public void MouseUp_event(object sender, MouseEventArgs e)
    {
      dragging = false;
      lastTranslation = activeTranslation;
      Console.WriteLine("drag off");
    }

    public void MouseLeave_event(object sender, EventArgs e)
    {

    }

    public void MouseMove_event(object sender, MouseEventArgs e)
    {
      if (dragging)
      {
        currentLocation = screenPoint(e.X, e.Y);
        activeTranslation.X = lastTranslation.X + mouseDownLocation.X - currentLocation.X;
        activeTranslation.Y = lastTranslation.Y + mouseDownLocation.Y - currentLocation.Y;
        //Console.WriteLine("down x/y: " + activeTranslation.X + ", " + activeTranslation.Y);
        this.Invalidate();
      }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      this.Invalidate();
    }

    public void MouseWheel_event(object sender, MouseEventArgs e)
    {
      int delta = e.Delta;
      float step = 0.05F;
      if (delta == 0) return;

      if (delta > 0)
      {
        zoom -= step;
      }
      else
      {
        zoom += step;
      }

      if (zoom < 0.12) zoom = 0.1F;
      if (zoom > 10.0) zoom = 10.0F;   //todo: max full image
      this.Invalidate();
      Console.WriteLine("wheel delta: {0}, zoom: {1}", delta, zoom);
    }

    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      //this.Location = activeTranslation;
      //base.OnPaint(paintEventArgs);
      
      Matrix mx = new Matrix(); // create an identity matrix
      mx.Scale(zoom, zoom); // zoom image
      //mx.Translate(viewRectWidth / 2.0f, viewRectHeight / 2.0f, MatrixOrder.Append); // move image to view window center

      Graphics graphics = paintEventArgs.Graphics;
      //graphics.InterpolationMode = InterpolationMode.Low;
      //graphics.SmoothingMode = SmoothingMode.AntiAlias;
      graphics.Transform = mx;

      Rectangle srcRect = new Rectangle(activeTranslation.X, activeTranslation.Y, (int)(zoom*this.Width), (int)(zoom*this.Height));
      Rectangle destRect = new Rectangle(0, 0, this.Width, this.Height);

      //graphics.DrawImage(Image, destRect, srcRect, GraphicsUnit.Pixel);
      graphics.FillRectangle(Brush, destRect);
    }

    bool dragging;
    float zoom;
    Point mouseDownLocation;
    Point currentLocation;
    Point lastTranslation;
    Point activeTranslation;
    
  }

}
