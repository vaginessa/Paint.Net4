namespace PaintDotNet.Tools.Media
{
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows;

    internal abstract class GdiPlusDrawing : Drawing
    {
        private DeviceBitmap deviceBitmap = new DeviceBitmap();
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(SizeInt32), typeof(GdiPlusDrawing), new PropertyMetadata(SizeInt32.Zero));

        private IBitmap CreateBitmap()
        {
            SizeInt32 size = this.Size;
            IBitmap<ColorPbgra32> bitmap = BitmapAllocator.Pbgra32.Allocate(size, AllocationOptions.Default);
            using (IBitmapLock<ColorPbgra32> @lock = bitmap.Lock<ColorPbgra32>(BitmapLockOptions.Write))
            {
                using (System.Drawing.Bitmap bitmap2 = new System.Drawing.Bitmap(size.Width, size.Height, @lock.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, @lock.Scan0))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap2))
                    {
                        this.Draw(graphics);
                    }
                }
            }
            return bitmap;
        }

        protected override Geometry CreateClip(bool isStroked)
        {
            SizeInt32 size = this.Size;
            return new RectangleGeometry(new RectDouble(PointDouble.Zero, size));
        }

        internal void Draw(Graphics g)
        {
            this.OnDraw(g);
        }

        protected override void OnChanged()
        {
            this.deviceBitmap.BitmapSource = null;
            base.OnChanged();
        }

        protected abstract void OnDraw(Graphics g);
        protected sealed override void OnRender(IDrawingContext dc)
        {
            SizeInt32 size = this.Size;
            if (size.HasPositiveArea)
            {
                if (this.deviceBitmap.BitmapSource == null)
                {
                    this.deviceBitmap.BitmapSource = this.CreateBitmap();
                }
                if (this.deviceBitmap.BitmapSource != null)
                {
                    RectDouble num2 = new RectDouble(0.0, 0.0, (double) size.Width, (double) size.Height);
                    dc.DrawBitmap(this.deviceBitmap, new RectDouble?(num2), 1.0, BitmapInterpolationMode.Linear, new RectDouble?(num2));
                }
            }
        }

        public SizeInt32 Size
        {
            get => 
                ((SizeInt32) base.GetValue(SizeProperty));
            set
            {
                base.SetValue(SizeProperty, value);
            }
        }
    }
}

