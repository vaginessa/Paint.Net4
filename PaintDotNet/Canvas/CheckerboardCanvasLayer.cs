namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class CheckerboardCanvasLayer : CanvasLayer
    {
        private static readonly IBitmap checkerboardBitmap;
        private BitmapBrush checkerboardBitmapBrush = new BitmapBrush(checkerboardDeviceBitmap);
        private static readonly DeviceBitmap checkerboardDeviceBitmap;
        private static readonly Surface checkerboardSurface;
        private MatrixTransform checkerboardTx;

        static CheckerboardCanvasLayer()
        {
            ColorBgra white = ColorBgra.White;
            ColorBgra color = ColorBgra.FromUInt32(0xffbfbfbf);
            checkerboardSurface = new Surface(0x10, 0x10);
            checkerboardSurface.Clear(white);
            checkerboardSurface.Clear(new RectInt32(0, 0, 8, 8), color);
            checkerboardSurface.Clear(new RectInt32(8, 8, 8, 8), color);
            checkerboardBitmap = checkerboardSurface.CreateAliasedImagingBitmap(PixelFormats.Bgr32);
            checkerboardDeviceBitmap = new DeviceBitmap(checkerboardBitmap);
            checkerboardDeviceBitmap.IsShareable = true;
            checkerboardDeviceBitmap.Freeze();
        }

        public CheckerboardCanvasLayer()
        {
            this.checkerboardBitmapBrush.InterpolationMode = BitmapInterpolationMode.NearestNeighbor;
            this.checkerboardTx = new MatrixTransform();
            this.checkerboardBitmapBrush.Transform = this.checkerboardTx;
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            PointDouble viewportCanvasOffset = canvasView.ViewportCanvasOffset;
            Matrix3x2Double num2 = dc.Transform.Inverse * Matrix3x2Double.Translation(-viewportCanvasOffset.X, -viewportCanvasOffset.Y);
            this.checkerboardTx.Matrix = num2;
            SizeDouble canvasSize = base.Owner.CanvasSize;
            RectDouble rect = new RectDouble(PointDouble.Zero, canvasSize);
            using (dc.UseAntialiasMode(AntialiasMode.Aliased))
            {
                using (CastOrRefHolder<IDrawingContext1> holder = dc.TryCastOrCreateRef<IDrawingContext1>())
                {
                    if (holder.HasRef)
                    {
                        using (holder.ObjectRef.UsePrimitiveBlend(PrimitiveBlend.Copy))
                        {
                            dc.FillRectangle(rect, this.checkerboardBitmapBrush);
                            goto Label_00CC;
                        }
                    }
                    dc.FillRectangle(rect, this.checkerboardBitmapBrush);
                }
            }
        Label_00CC:
            base.OnRender(dc, clipRect, canvasView);
        }
    }
}

