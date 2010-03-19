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
  public partial class NWImageViewerBase : System.Windows.Forms.UserControl
  {
    /*
    // C# 3.0 syntax
    public Image Image { get; set; }
    public TextureBrush Brush { get; set; }
    public ImageViewport ImageViewport { get; set; }
    */
    public Image Image { 
      get { return image; } 
      set { image = value; }      
    }

    public TextureBrush Brush {
      get { return textureBrush; }
      set { textureBrush = value; }
    }

    public ImageViewport ImageViewport
    {
      get { return imageViewport; }
      set { imageViewport = value; }
    }

    Image image;
    TextureBrush textureBrush;
    ImageViewport imageViewport;

  }

  public partial class NWImageViewer : NWImageViewerBase
  {
    public NWImageViewer()
    {
      base.SetStyle(ControlStyles.DoubleBuffer, true);
      base.SetStyle(ControlStyles.UserPaint, true);
      base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      base.ResumeLayout(false);

      ImageViewport = new ImageViewport();
      ImageViewport.DrawingAreaSize = this.Size;

      LoadImage("large.png");

      this.MouseDown += new MouseEventHandler(this.MouseDown_event);
      this.MouseUp += new MouseEventHandler(this.event_MouseUp);
      this.MouseWheel += new MouseEventHandler(this.MouseWheel_event);
      this.MouseLeave += new EventHandler(this.event_MouseLeave);
      this.MouseMove += new MouseEventHandler(this.event_MouseMove);

      //FontDialog flg = new FontDialog(); flg.ShowDialog(); tb.FontName = flg.Font;
    }

    public void LoadImage(string filename)
    {
      FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
      this.Image = Image.FromStream(fs);
      this.Brush = new TextureBrush(Image);     
      fs.Close();   // close the file after accessing the image (load_on_access)

      // update viewport input
      ImageViewport.ImageSize = this.Image.Size;
    }

    private Point screenPoint(int x, int y)
    {
      // return a screen-relative point
      return new Point(this.Left + x, this.Top + y);
    }

    public void MouseDown_event(object sender, MouseEventArgs e)
    {
      mouseDownLocation = screenPoint(e.X, e.Y);
      lastTranslation = ImageViewport.Location;
      //Console.WriteLine("drag on, down x/y: " + mouseDownLocation.X + ", " + mouseDownLocation.Y);
      dragging = true;
    }

    public void event_MouseUp(object sender, MouseEventArgs e)
    {
      dragging = false;
      //Console.WriteLine("drag off");
    }

    public void event_MouseLeave(object sender, EventArgs e)
    {
      // ignore this event
    }

    public void event_MouseMove(object sender, MouseEventArgs e)
    {
      if (dragging)
      {
        Point activeTranslation = new Point();
        currentLocation = screenPoint(e.X, e.Y);
        activeTranslation.X = lastTranslation.X + mouseDownLocation.X - currentLocation.X;
        activeTranslation.Y = lastTranslation.Y + mouseDownLocation.Y - currentLocation.Y;
        ImageViewport.Location = activeTranslation;
    
        //Console.WriteLine("down x/y: " + activeTranslation.X + ", " + activeTranslation.Y);
        this.Invalidate();
      }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      ImageViewport.DrawingAreaSize = this.Size;
      this.Invalidate();
    }

    public void MouseWheel_event(object sender, MouseEventArgs e)
    {
      int delta = e.Delta;
      if (delta == 0) return;

      if (delta > 0)
      {
        ImageViewport.ZoomOut(e.Location);
      }
      else
      {
        ImageViewport.ZoomIn(e.Location);
      }

      this.Invalidate();
      //Console.WriteLine("wheel delta: {0}, zoom: {1}", delta, zoom);
    }

    /*
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      this.Location = ImageViewPort.Location;
      base.OnPaint(paintEventArgs);
    }
    */

    /*
    // very smooth, but brushsize is limited?
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      Matrix mx = new Matrix(); // create an identity matrix
      mx.Translate(ImageViewPort.Location.X, ImageViewPort.Location.Y, MatrixOrder.Append);
      mx.Scale(zoom, zoom); // zoom image

      Graphics graphics = paintEventArgs.Graphics;
      graphics.Transform = mx;
      Rectangle destRect = new Rectangle(0, 0, this.Width, this.Height);
      graphics.FillRectangle(Brush, destRect);
    }
    */

    Rectangle calculateSourceRect()
    {
      Rectangle srcRect = new Rectangle(ImageViewport.Location.X, ImageViewport.Location.Y, ImageViewport.Size.Width, ImageViewport.Size.Height);
      return srcRect;
    }
   
    // works well, just not very fast
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {   
      Graphics graphics = paintEventArgs.Graphics;
      graphics.InterpolationMode = InterpolationMode.Low;
      Rectangle destRect = new Rectangle(0, 0, this.Width, this.Height);
      graphics.DrawImage(Image, destRect, calculateSourceRect(), GraphicsUnit.Pixel);
    }
   
    /*
    // alternive method, does not seem faster
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      Matrix mx = new Matrix(); // create an identity matrix
      mx.Scale(zoom, zoom); // zoom image

      Graphics graphics = paintEventArgs.Graphics;
      graphics.InterpolationMode = InterpolationMode.Low;
      graphics.Transform = mx;
      graphics.DrawImage(Image, activeTranslation);
    }
    */

    bool dragging;
    Point mouseDownLocation;
    Point currentLocation;
    Point lastTranslation;    
  }

}
