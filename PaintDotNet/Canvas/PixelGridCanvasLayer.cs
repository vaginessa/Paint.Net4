namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Windows;

    internal sealed class PixelGridCanvasLayer : CanvasLayer
    {
        private static readonly DependencyProperty IsPixelGridEnabledChangedEvent = DependencyProperty.RegisterAttached("IsPixelGridEnabledChanged", typeof(ValueChangedEventHandler<bool>), typeof(PixelGridCanvasLayer), new PropertyMetadata(null));
        public static readonly DependencyProperty IsPixelGridEnabledProperty = DependencyProperty.RegisterAttached("IsPixelGridEnabled", typeof(bool), typeof(PixelGridCanvasLayer), new PropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(PixelGridCanvasLayer.OnIsPixelGridEnabledPropertyChanged)));
        private static readonly IBitmap stippleBitmap;
        private BitmapBrush stippleBitmapBrush = new BitmapBrush(stippleDeviceBitmap);
        private static readonly DeviceBitmap stippleDeviceBitmap;
        private static readonly Surface stippleSurface = new Surface(0x10, 0x10);

        static PixelGridCanvasLayer()
        {
            ColorBgra white = ColorBgra.White;
            ColorBgra black = ColorBgra.Black;
            for (int i = 0; i < stippleSurface.Height; i++)
            {
                for (int j = 0; j < stippleSurface.Width; j++)
                {
                    stippleSurface[j, i] = (((j + i) & 1) == 0) ? white : black;
                }
            }
            stippleBitmap = stippleSurface.CreateAliasedImagingBitmap(PixelFormats.Pbgra32);
            stippleDeviceBitmap = new DeviceBitmap(stippleBitmap);
            stippleDeviceBitmap.Freeze();
        }

        public PixelGridCanvasLayer()
        {
            this.stippleBitmapBrush.InterpolationMode = BitmapInterpolationMode.NearestNeighbor;
        }

        public static void AddIsPixelGridEnabledChangedHandler(CanvasView canvasView, ValueChangedEventHandler<bool> handler)
        {
            ValueChangedEventHandler<bool> a = (ValueChangedEventHandler<bool>) canvasView.GetValue(IsPixelGridEnabledChangedEvent);
            ValueChangedEventHandler<bool> handler3 = (ValueChangedEventHandler<bool>) Delegate.Combine(a, handler);
            canvasView.SetValue(IsPixelGridEnabledChangedEvent, handler3);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stippleBitmapBrush.DeviceBitmap = null;
            }
            base.Dispose(disposing);
        }

        public static bool GetIsPixelGridEnabled(CanvasView canvasView) => 
            ((bool) canvasView.GetValue(IsPixelGridEnabledProperty));

        private static void OnIsPixelGridEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CanvasView canvasView = (CanvasView) target;
            canvasView.Invalidate(canvasView.GetCanvasBounds());
            RaiseIsPixelGridEnabledChanged(canvasView, e);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            if ((canvasView.ScaleRatio >= 2.0) && GetIsPixelGridEnabled(canvasView))
            {
                SizeDouble canvasSize = base.Owner.CanvasSize;
                RectDouble b = new RectDouble(PointDouble.Zero, canvasSize);
                RectInt32 rect = RectDouble.Intersect(clipRect, b).Int32Bound;
                Matrix3x2Double transform = dc.Transform;
                double num6 = 1.0 / canvasView.CanvasHairWidth;
                RectDouble num7 = transform.Transform(rect);
                double num9 = DoubleUtil.Clamp((canvasView.ScaleRatio - 2.0) / 4.0, 0.0, 1.0) / 2.0;
                this.stippleBitmapBrush.Opacity = num9;
                using (dc.UseAntialiasMode(AntialiasMode.Aliased))
                {
                    using (dc.UseTransform(Matrix3x2Float.Identity))
                    {
                        using (dc.UseAxisAlignedClip((RectFloat) num7, AntialiasMode.Aliased))
                        {
                            for (int i = -(rect.Width & 1); i <= rect.Width; i++)
                            {
                                dc.DrawLine(num7.X + (i * num6), num7.Y, num7.X + (i * num6), num7.Y + num7.Height, this.stippleBitmapBrush, 1.0);
                            }
                            for (int j = -(rect.Height & 1); j <= rect.Height; j++)
                            {
                                dc.DrawLine(num7.X, num7.Y + (j * num6), num7.X + num7.Width, num7.Y + (j * num6), this.stippleBitmapBrush, 1.0);
                            }
                        }
                    }
                }
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        private static void RaiseIsPixelGridEnabledChanged(CanvasView canvasView, DependencyPropertyChangedEventArgs e)
        {
            ((ValueChangedEventHandler<bool>) canvasView.GetValue(IsPixelGridEnabledChangedEvent)).Raise<bool>(canvasView, e);
        }

        public static void RemoveIsPixelGridEnabledChangedHandler(CanvasView canvasView, ValueChangedEventHandler<bool> handler)
        {
            ValueChangedEventHandler<bool> source = (ValueChangedEventHandler<bool>) canvasView.GetValue(IsPixelGridEnabledChangedEvent);
            ValueChangedEventHandler<bool> handler3 = (ValueChangedEventHandler<bool>) Delegate.Remove(source, handler);
            canvasView.SetValue(IsPixelGridEnabledChangedEvent, handler3);
        }

        public static void SetIsPixelGridEnabled(CanvasView canvasView, bool value)
        {
            canvasView.SetValue(IsPixelGridEnabledProperty, BooleanUtil.GetBoxed(value));
        }
    }
}

