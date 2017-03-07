namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class BrushPreviewCanvasLayer : CanvasLayer
    {
        private int brushAlpha = 0xff;
        private PointDouble brushLocation;
        private double brushSize;
        private static readonly CalculateInvalidRectCallback calculateInvalidRect = new CalculateInvalidRectCallback(BrushPreviewCanvasLayer.CalculateInvalidRect);
        private EllipseGeometry ellipseGeometryInner = new EllipseGeometry();
        private EllipseGeometry ellipseGeometryMiddle = new EllipseGeometry();
        private EllipseGeometry ellipseGeometryOuter = new EllipseGeometry();
        private StrokedGeometryRealization ellipseRealizationInner;
        private StrokedGeometryRealization ellipseRealizationMiddle;
        private StrokedGeometryRealization ellipseRealizationOuter;

        private static RectDouble CalculateInvalidRect(CanvasView canvasView, RectDouble canvasRect)
        {
            double canvasHairWidth = canvasView.CanvasHairWidth;
            return RectDouble.Inflate(canvasRect, canvasHairWidth, canvasHairWidth);
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
                brushRect.Inflate((double) (-canvasHairWidth * 2.0), (double) (-canvasHairWidth * 2.0));
                if (brushRect.HasPositiveArea)
                {
                    this.ellipseGeometryInner.RadiusX = this.brushSize - (canvasHairWidth * 2.0);
                    this.ellipseGeometryInner.RadiusY = this.brushSize - (canvasHairWidth * 2.0);
                    this.ellipseGeometryMiddle.RadiusX = this.brushSize - canvasHairWidth;
                    this.ellipseGeometryMiddle.RadiusY = this.brushSize - canvasHairWidth;
                    this.ellipseGeometryOuter.RadiusX = this.brushSize;
                    this.ellipseGeometryOuter.RadiusY = this.brushSize;
                    using (dc.UseTranslateTransform(-((VectorFloat) this.brushLocation), MatrixMultiplyOrder.Prepend))
                    {
                        using (CastOrRefHolder<IDrawingContext2> holder = dc.TryCastOrCreateRef<IDrawingContext2>())
                        {
                            if (holder.HasRef)
                            {
                                this.ellipseRealizationInner = this.ellipseRealizationInner ?? new StrokedGeometryRealization(this.ellipseGeometryInner);
                                this.ellipseRealizationMiddle = this.ellipseRealizationMiddle ?? new StrokedGeometryRealization(this.ellipseGeometryMiddle);
                                this.ellipseRealizationOuter = this.ellipseRealizationOuter ?? new StrokedGeometryRealization(this.ellipseGeometryOuter);
                                this.ellipseRealizationInner.Thickness = canvasHairWidth;
                                this.ellipseRealizationMiddle.Thickness = canvasHairWidth;
                                this.ellipseRealizationOuter.Thickness = canvasHairWidth;
                                holder.ObjectRef.DrawGeometryRealization(this.ellipseRealizationInner, brush);
                                holder.ObjectRef.DrawGeometryRealization(this.ellipseRealizationMiddle, brush2);
                                holder.ObjectRef.DrawGeometryRealization(this.ellipseRealizationOuter, brush);
                            }
                            else
                            {
                                dc.DrawGeometry(this.ellipseGeometryInner, brush, canvasHairWidth);
                                dc.DrawGeometry(this.ellipseGeometryMiddle, brush2, canvasHairWidth);
                                dc.DrawGeometry(this.ellipseGeometryOuter, brush, canvasHairWidth);
                            }
                        }
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
                    this.brushLocation = value;
                    this.InvalidateHandle();
                }
            }
        }

        public RectDouble BrushRect =>
            RectDouble.FromCenter(this.brushLocation, (double) (this.brushSize * 2.0));

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

