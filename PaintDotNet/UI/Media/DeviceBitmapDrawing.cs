namespace PaintDotNet.UI.Media
{
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal sealed class DeviceBitmapDrawing : Drawing
    {
        public static readonly DependencyProperty DeviceBitmapProperty = DependencyProperty.Register("DeviceBitmap", typeof(PaintDotNet.UI.Media.DeviceBitmap), typeof(DeviceBitmapDrawing), new PropertyMetadata(null));

        public DeviceBitmapDrawing()
        {
        }

        public DeviceBitmapDrawing(PaintDotNet.UI.Media.DeviceBitmap deviceBitmap)
        {
            this.DeviceBitmap = deviceBitmap;
        }

        protected override Geometry CreateClip(bool isStroked)
        {
            PaintDotNet.UI.Media.DeviceBitmap deviceBitmap = this.DeviceBitmap;
            if (deviceBitmap == null)
            {
                return new RectangleGeometry(RectDouble.Zero);
            }
            return new RectangleGeometry(new RectDouble(PointDouble.Zero, deviceBitmap.Size));
        }

        protected override Freezable CreateInstanceCore() => 
            new DeviceBitmapDrawing();

        protected override void OnRender(IDrawingContext dc)
        {
            PaintDotNet.UI.Media.DeviceBitmap deviceBitmap = this.DeviceBitmap;
            if (deviceBitmap != null)
            {
                SizeDouble size = deviceBitmap.Size;
                RectDouble num2 = new RectDouble(PointDouble.Zero, size);
                dc.DrawBitmap(deviceBitmap, new RectDouble?(num2), 1.0, BitmapInterpolationMode.Linear, new RectDouble?(num2));
            }
        }

        public PaintDotNet.UI.Media.DeviceBitmap DeviceBitmap
        {
            get => 
                ((PaintDotNet.UI.Media.DeviceBitmap) base.GetValue(DeviceBitmapProperty));
            set
            {
                base.SetValue(DeviceBitmapProperty, value);
            }
        }
    }
}

