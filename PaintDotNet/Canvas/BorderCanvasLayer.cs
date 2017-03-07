namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.VisualStyling;
    using System;

    internal sealed class BorderCanvasLayer : CanvasLayer
    {
        private DropShadowRenderer dropShadowRenderer = new DropShadowRenderer();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<DropShadowRenderer>(ref this.dropShadowRenderer);
            }
            base.Dispose(disposing);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            Matrix3x2Float transform = dc.Transform;
            SizeDouble canvasSize = canvasView.CanvasSize;
            RectFloat rect = new RectFloat(0f, 0f, (float) canvasSize.Width, (float) canvasSize.Height);
            RectFloat num4 = transform.Transform(rect);
            int recommendedExtent = this.dropShadowRenderer.GetRecommendedExtent(num4.Int32Bound.Size);
            using (dc.UseTransform(Matrix3x2Float.Identity))
            {
                this.dropShadowRenderer.RenderOutside(dc, new ColorRgba128Float(0f, 0f, 0f, 0.5f), num4, recommendedExtent);
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        protected override void OnViewRegistered(CanvasView canvasView)
        {
            SizeDouble canvasExtentPadding = canvasView.CanvasExtentPadding;
            canvasView.CanvasExtentPadding = new SizeDouble(canvasExtentPadding.Width + 8.0, canvasExtentPadding.Height + 8.0);
            base.OnViewRegistered(canvasView);
        }

        protected override void OnViewUnregistered(CanvasView canvasView)
        {
            SizeDouble canvasExtentPadding = canvasView.CanvasExtentPadding;
            canvasView.CanvasExtentPadding = new SizeDouble(canvasExtentPadding.Width - 8.0, canvasExtentPadding.Height - 8.0);
            base.OnViewUnregistered(canvasView);
        }
    }
}

