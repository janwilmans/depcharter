using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

public class ImageViewport
{
  public ImageViewport()
  {
    Initialize();
  }

  private void Initialize()
  {
    location = new Point(0, 0);
    zoom = 1.0F;
  }

  // location of the visible area relative to the real image
  public Point Location
  {
    get
    {
      return location;
    }
    set
    {
      location = value;
    }
  }

  // real size of the control's drawing area
  public Size DrawingAreaSize
  {
    get
    {
      return drawingAreaSize;
    }
    set
    {
      drawingAreaSize = value;
      Initialize();
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
      Initialize();
    }
  }

  public float Zoom
  {
    get
    {
      return zoom;
    }
    set
    {
      // manually setting Zoom resets all other parameters
      changeZoom(value, false, new Point(0, 0));
    }
  }

  // focusPoint is relative to this.Location
  private bool changeZoom(float newZoom, bool zoomToFocusPoint, Point focusPoint)
  {
    bool result = true;
    float imageWidth = (float)ImageSize.Width;

    // 1:1 zoomlock
    if (newZoom < (1 + zoomStep) && newZoom > (1 - zoomStep))
    {
      newZoom = 1.0F;
    }

    if ((drawingAreaSize.Width / newZoom) > imageWidth)
    {
      // refuse to zoom, calculate minimal zoom
      newZoom = drawingAreaSize.Width / imageWidth;
      result = false;
    }

    Point portMiddle;
    if (zoomToFocusPoint)
    {
      // zoom to focusPoint
      portMiddle = new Point(location.X + focusPoint.X, location.Y + focusPoint.Y);
    }
    else
    {
      //zoom to middle to ImageViewport
      portMiddle = new Point(location.X + (Size.Width / 2), location.Y + (Size.Height / 2));
    }

    zoom = newZoom;
    location.X = portMiddle.X - (Size.Width / 2);
    location.Y = portMiddle.Y - (Size.Height / 2);
    applyLimits();
    return result;
  }

  private void applyLimits()
  {
    if (location.X < 0) location.X = 0;
    if (location.Y < 0) location.Y = 0;

    if (location.X > MaxLocX) location.X = MaxLocX;
    if (location.Y > MaxLocY) location.Y = MaxLocY;
  }

  float zoomStep = 0.1F;
  public bool ZoomIn(Point focusPoint)
  {
    float newZoom = zoom + zoomStep;
    if (newZoom > 3.0) newZoom = 3.0F;
    return changeZoom(newZoom, false, focusPoint);
  }

  public bool ZoomOut(Point focusPoint)
  {
    float newZoom = zoom - zoomStep;
    return changeZoom(newZoom, false, focusPoint);
  }

  public int MaxLocX
  {
    get
    {
      return imageSize.Width - Size.Width;
    }
  }

  public int MaxLocY
  {
    get
    {
      return imageSize.Height - Size.Height;
    }
  }

  // zoom = 2.0 means 2x zoom (everything look twice as big as it is), so virtualSize is half the real size
  // th
  public Size Size
  {
    get
    {
      return new Size((int)(drawingAreaSize.Width / zoom), (int)(drawingAreaSize.Height / zoom));
    }
  }

  private float zoom;
  private Point location;
  private Size imageSize;
  private Size drawingAreaSize;
}
