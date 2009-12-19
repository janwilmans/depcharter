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
  public class ImageViewport
  {
    public Point Location { 
      get
      {
        if (dirty) recalculate();
        return location;
      }
      set
      {
        location = value;
        dirty = true;
      }
    }

    // real size of the control's drawing area
    public Size Size
    {
      get
      {
        return size;
      }
      set
      {
        size = value;
        dirty = true;
      }
    }

    // real size of the image
    public Size ImageSize
    {
      get
      {
        return imageSize;
      }
      set
      {
        imageSize = value;
        dirty = true;
      }
    }

    public float Zoom { 
      get
      {
        return zoom;
      }
      set
      {
        zoom = value;
        dirty = true;
      }
    }

    public int MaxLocX
    {
      get
      {
        return (int) (imageSize.Width - VirtualSize.Width);
      }
    }

    public int MaxLocY
    {
      get
      {
        return (int) (imageSize.Height - VirtualSize.Height);
      }
    }

    public Size VirtualSize
    {
      get
      {
        return new Size((int)(size.Width*zoom), (int)(size.Height*zoom));
      }
    }

    // input: location
    // ouput: location
    private void recalculate()
    {
      if (location.X < 0) location.X = 0;
      if (location.Y < 0) location.Y = 0;

      if (location.X > MaxLocX) location.X = MaxLocX;
      if (location.Y > MaxLocY) location.Y = MaxLocY;

      Console.WriteLine("recalculate()");
      dirty = false;
    }

    private float zoom;
    private Point location;
    private Size imageSize;
    private Size size;
    private bool dirty;
  }

  public partial class NWImageViewerBase : System.Windows.Forms.UserControl
  {
    public Image Image { get; set; }
    public TextureBrush Brush { get; set; }
    public ImageViewport ImageViewport { get; set; }
  }

  public partial class NWImageViewer : NWImageViewerBase
  {
    public NWImageViewer()
    {
      zoom = 1.0F;
      base.SetStyle(ControlStyles.DoubleBuffer, true);
      base.SetStyle(ControlStyles.UserPaint, true);
      base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
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

      ImageViewport = new ImageViewport();
      ImageViewport.Size = this.Size;

      
      //FontDialog flg = new FontDialog(); flg.ShowDialog(); tb.FontName = flg.Font;
    }

    private Point screenPoint(int x, int y)
    {
      // return a screen-relative point
      return new Point(this.Left + x, this.Top + y);
    }

    public void MouseDown_event(object sender, MouseEventArgs e)
    {
      mouseDownLocation = screenPoint(e.X, e.Y);
      //Console.WriteLine("drag on, down x/y: " + mouseDownLocation.X + ", " + mouseDownLocation.Y);
      dragging = true;
    }

    public void MouseUp_event(object sender, MouseEventArgs e)
    {
      dragging = false;
      lastTranslation = activeTranslation;
      //Console.WriteLine("drag off");
    }

    public void MouseLeave_event(object sender, EventArgs e)
    {
      // ignore this event
    }

    public void MouseMove_event(object sender, MouseEventArgs e)
    {
      if (dragging)
      {
        currentLocation = screenPoint(e.X, e.Y);
        activeTranslation.X = lastTranslation.X + mouseDownLocation.X - currentLocation.X;
        activeTranslation.Y = lastTranslation.Y + mouseDownLocation.Y - currentLocation.Y;

        ImageViewport.Location = activeTranslation;
    
        Console.WriteLine("down x/y: " + activeTranslation.X + ", " + activeTranslation.Y);
        this.Invalidate();
      }
    }

    void applyLimits(ref Point translation)
    {
      if (translation.X < 0) translation.X = 0;
      if (translation.Y < 0) translation.Y = 0;

      Console.WriteLine("zoom: " + zoom);

      int virtWidth = (int)(Image.Width * zoom);
      int virtHeight = (int)(Image.Height * zoom);

      int maxX = virtWidth - this.Width;
      int maxY = virtHeight - this.Height;
      Console.WriteLine("max x/y: " + maxX + ", " + maxY);

      if (translation.X > maxX) translation.X = maxX;
      if (translation.Y > maxY) translation.Y = maxY;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      ImageViewport.Size = this.Size;
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

      if (zoom < 0.07) zoom = 0.05F;
      if (zoom > 2.0) zoom = 2.0F;

      applyLimits(ref activeTranslation);
      this.Invalidate();
      //Console.WriteLine("wheel delta: {0}, zoom: {1}", delta, zoom);
    }

    /*
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      this.Location = activeTranslation;
      base.OnPaint(paintEventArgs);
    }
    */

    /*
    // very smooth, but brushsize is limited?
    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
      Matrix mx = new Matrix(); // create an identity matrix
      mx.Translate(activeTranslation.X, activeTranslation.Y, MatrixOrder.Append);
      mx.Scale(zoom, zoom); // zoom image

      Graphics graphics = paintEventArgs.Graphics;
      graphics.Transform = mx;
      Rectangle destRect = new Rectangle(0, 0, this.Width, this.Height);
      graphics.FillRectangle(Brush, destRect);
    }
    */

    Rectangle calculateSourceRect()
    {
      Rectangle srcRect = new Rectangle(activeTranslation.X, activeTranslation.Y, (int)(this.Width / zoom), (int)(this.Height / zoom));
      return srcRect;
    }
   
    // works well, but not very fast
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
    float zoom;
    Point mouseDownLocation;
    Point currentLocation;
    Point lastTranslation;
    Point activeTranslation;
    
  }

}
