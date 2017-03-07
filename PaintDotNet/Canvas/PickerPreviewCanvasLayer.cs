namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class PickerPreviewCanvasLayer : CanvasLayer
    {
        private int brushAlpha = 0xff;
        private PointDouble brushLocation;
        private double brushSize;
        private static readonly CalculateInvalidRectCallback calculateInvalidRect = new CalculateInvalidRectCallback(PickerPreviewCanvasLayer.CalculateInvalidRect);

        private static RectDouble CalculateInvalidRect(CanvasView canvasView, RectDouble canvasRect)
        {
            double canvasHairWidth = canvasView.CanvasHairWidth;
            return RectDouble.Inflate(canvasRect, canvasHairWidth * 2.0, canvasHairWidth * 2.0);
        }

        private void InvalidateHandle()
        {
            base.Invalidate(calculateInvalidRect, this.BrushRect);
        }

        protected override void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            this.InvalidateHandle();
            base.OnIsVisibleChanged(oldValue, newValue);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            if ((this.brushSize > 0.0) && (this.brushAlpha > 0))
            {
                double opacity = ((double) this.brushAlpha) / 255.0;
                Brush brush = SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.White, opacity);
                Brush brush2 = SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.Black, opacity);
                double canvasHairWidth = canvasView.CanvasHairWidth;
                RectDouble brushRect = this.BrushRect;
                brushRect.Inflate(-canvasHairWidth, -canvasHairWidth);
                if (brushRect.HasPositiveArea)
                {
                    RectFloat num4 = (RectFloat) brushRect;
                    using (dc.UseAntialiasMode(AntialiasMode.Aliased))
                    {
                        dc.DrawRectangle(brushRect, brush2, canvasHairWidth * 3.0);
                        dc.DrawRectangle(brushRect, brush, canvasHairWidth);
                    }
                }
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        public int BrushAlpha
        {
            get => 
                this.brushAlpha;
            set
            {
                base.VerifyAccess();
                if (value != this.brushAlpha)
                {
                    this.InvalidateHandle();
                    this.brushAlpha = value;
                    this.InvalidateHandle();
                }
            }
        }

        public PointDouble BrushLocation
        {
            get => 
                this.brushLocation;
            set
            {
                base.VerifyAccess();
                if (value != this.brushLocation)
                {
                    this.InvalidateHandle();
                    PointDouble num = new PointDouble(((int) value.X) + 0.5, ((int) value.Y) + 0.5);
                    if (value.X < 0.0)
                    {
                        num.X--;
                    }
                    if (value.Y < 0.0)
                    {
                        num.Y--;
                    }
                    this.brushLocation = num;
                    this.InvalidateHandle();
                }
            }
        }

        public RectDouble BrushRect =>
            RectDouble.FromCenter(this.brushLocation, this.brushSize);

        public double BrushSize
        {
            get => 
                this.brushSize;
            set
            {
                base.VerifyAccess();
                if (value != this.brushSize)
                {
                    this.InvalidateHandle();
                    this.brushSize = value;
                    this.InvalidateHandle();
                }
            }
        }
    }
}

