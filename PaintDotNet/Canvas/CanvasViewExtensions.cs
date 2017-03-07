namespace PaintDotNet.Canvas
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal static class CanvasViewExtensions
    {
        public static double ConvertCanvasDXToExtentDX(this CanvasView canvasView, double canvasDX) => 
            CanvasCoordinateConversions.ConvertCanvasDXToExtentDX(canvasDX, canvasView.ScaleRatio);

        public static double ConvertCanvasDXToViewportDX(this CanvasView canvasView, double canvasDX) => 
            CanvasCoordinateConversions.ConvertCanvasDXToViewportDX(canvasDX, canvasView.ScaleRatio);

        public static double ConvertCanvasDYToExtentDY(this CanvasView canvasView, double canvasDY) => 
            CanvasCoordinateConversions.ConvertCanvasDYToExtentDY(canvasDY, canvasView.ScaleRatio);

        public static double ConvertCanvasDYToViewportDY(this CanvasView canvasView, double canvasDY) => 
            CanvasCoordinateConversions.ConvertCanvasDYToViewportDY(canvasDY, canvasView.ScaleRatio);

        public static double ConvertCanvasHeightToExtentHeight(this CanvasView canvasView, double canvasHeight) => 
            CanvasCoordinateConversions.ConvertCanvasHeightToExtentHeight(canvasHeight, canvasView.ScaleRatio);

        public static double ConvertCanvasHeightToViewportHeight(this CanvasView canvasView, double canvasHeight) => 
            CanvasCoordinateConversions.ConvertCanvasHeightToViewportHeight(canvasHeight, canvasView.ScaleRatio);

        public static PointDouble ConvertCanvasToExtent(this CanvasView canvasView, PointDouble canvasPt) => 
            CanvasCoordinateConversions.ConvertCanvasToExtent(canvasPt, canvasView.ScaleRatio);

        public static RectDouble ConvertCanvasToExtent(this CanvasView canvasView, RectDouble canvasRect) => 
            CanvasCoordinateConversions.ConvertCanvasToExtent(canvasRect, canvasView.ScaleRatio);

        public static SizeDouble ConvertCanvasToExtent(this CanvasView canvasView, SizeDouble canvasSize) => 
            CanvasCoordinateConversions.ConvertCanvasToExtent(canvasSize, canvasView.ScaleRatio);

        public static VectorDouble ConvertCanvasToExtent(this CanvasView canvasView, VectorDouble canvasVec) => 
            CanvasCoordinateConversions.ConvertCanvasToExtent(canvasVec, canvasView.ScaleRatio);

        public static PointDouble ConvertCanvasToViewport(this CanvasView canvasView, PointDouble canvasPt) => 
            CanvasCoordinateConversions.ConvertCanvasToViewport(canvasPt, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static RectDouble ConvertCanvasToViewport(this CanvasView canvasView, RectDouble canvasRect) => 
            CanvasCoordinateConversions.ConvertCanvasToViewport(canvasRect, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static SizeDouble ConvertCanvasToViewport(this CanvasView canvasView, SizeDouble canvasSize) => 
            CanvasCoordinateConversions.ConvertCanvasToViewport(canvasSize, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static VectorDouble ConvertCanvasToViewport(this CanvasView canvasView, VectorDouble canvasVec) => 
            CanvasCoordinateConversions.ConvertCanvasToViewport(canvasVec, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertCanvasWidthToExtentWidth(this CanvasView canvasView, double canvasWidth) => 
            CanvasCoordinateConversions.ConvertCanvasWidthToExtentWidth(canvasWidth, canvasView.ScaleRatio);

        public static double ConvertCanvasWidthToViewportWidth(this CanvasView canvasView, double canvasWidth) => 
            CanvasCoordinateConversions.ConvertCanvasWidthToViewportWidth(canvasWidth, canvasView.ScaleRatio);

        public static double ConvertCanvasXToExtentX(this CanvasView canvasView, double canvasX) => 
            CanvasCoordinateConversions.ConvertCanvasXToExtentX(canvasX, canvasView.ScaleRatio);

        public static double ConvertCanvasXToViewportX(this CanvasView canvasView, double canvasX) => 
            CanvasCoordinateConversions.ConvertCanvasXToViewportX(canvasX, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertCanvasYToExtentY(this CanvasView canvasView, double canvasY) => 
            CanvasCoordinateConversions.ConvertCanvasYToExtentY(canvasY, canvasView.ScaleRatio);

        public static double ConvertCanvasYToViewportY(this CanvasView canvasView, double canvasY) => 
            CanvasCoordinateConversions.ConvertCanvasYToViewportY(canvasY, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertExtentDXToCanvasDX(this CanvasView canvasView, double extentDX) => 
            CanvasCoordinateConversions.ConvertExtentDXToCanvasDX(extentDX, canvasView.ScaleRatio);

        public static double ConvertExtentDXToViewportDX(this CanvasView canvasView, double extentDX) => 
            CanvasCoordinateConversions.ConvertExtentDXToViewportDX(extentDX);

        public static double ConvertExtentDYToCanvasDY(this CanvasView canvasView, double extentDY) => 
            CanvasCoordinateConversions.ConvertExtentDYToCanvasDY(extentDY, canvasView.ScaleRatio);

        public static double ConvertExtentDYToViewportDY(this CanvasView canvasView, double extentDY) => 
            CanvasCoordinateConversions.ConvertExtentDYToViewportDY(extentDY);

        public static double ConvertExtentHeightToCanvasHeight(this CanvasView canvasView, double extentHeight) => 
            CanvasCoordinateConversions.ConvertExtentHeightToCanvasHeight(extentHeight, canvasView.ScaleRatio);

        public static double ConvertExtentHeightToViewportHeight(this CanvasView canvasView, double extentHeight) => 
            CanvasCoordinateConversions.ConvertExtentHeightToViewportHeight(extentHeight);

        public static PointDouble ConvertExtentToCanvas(this CanvasView canvasView, PointDouble extentPt) => 
            CanvasCoordinateConversions.ConvertExtentToCanvas(extentPt, canvasView.ScaleRatio);

        public static RectDouble ConvertExtentToCanvas(this CanvasView canvasView, RectDouble extentRect) => 
            CanvasCoordinateConversions.ConvertExtentToCanvas(extentRect, canvasView.ScaleRatio);

        public static SizeDouble ConvertExtentToCanvas(this CanvasView canvasView, SizeDouble extentSize) => 
            CanvasCoordinateConversions.ConvertExtentToCanvas(extentSize, canvasView.ScaleRatio);

        public static VectorDouble ConvertExtentToCanvas(this CanvasView canvasView, VectorDouble extentVec) => 
            CanvasCoordinateConversions.ConvertExtentToCanvas(extentVec, canvasView.ScaleRatio);

        public static PointDouble ConvertExtentToViewport(this CanvasView canvasView, PointDouble extentPt) => 
            CanvasCoordinateConversions.ConvertExtentToViewport(extentPt, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static RectDouble ConvertExtentToViewport(this CanvasView canvasView, RectDouble extentRect) => 
            CanvasCoordinateConversions.ConvertExtentToViewport(extentRect, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static SizeDouble ConvertExtentToViewport(this CanvasView canvasView, SizeDouble extentSize) => 
            CanvasCoordinateConversions.ConvertExtentToViewport(extentSize);

        public static VectorDouble ConvertExtentToViewport(this CanvasView canvasView, VectorDouble extentVec) => 
            CanvasCoordinateConversions.ConvertExtentToViewport(extentVec);

        public static double ConvertExtentWidthToCanvasWidth(this CanvasView canvasView, double extentWidth) => 
            CanvasCoordinateConversions.ConvertExtentWidthToCanvasWidth(extentWidth, canvasView.ScaleRatio);

        public static double ConvertExtentWidthToViewportWidth(this CanvasView canvasView, double extentWidth) => 
            CanvasCoordinateConversions.ConvertExtentWidthToViewportWidth(extentWidth);

        public static double ConvertExtentXToCanvasX(this CanvasView canvasView, double extentX) => 
            CanvasCoordinateConversions.ConvertExtentXToCanvasX(extentX, canvasView.ScaleRatio);

        public static double ConvertExtentXToViewportX(this CanvasView canvasView, double extentX) => 
            CanvasCoordinateConversions.ConvertExtentXToViewportX(extentX, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertExtentYToCanvasY(this CanvasView canvasView, double extentY) => 
            CanvasCoordinateConversions.ConvertExtentYToCanvasY(extentY, canvasView.ScaleRatio);

        public static double ConvertExtentYToViewportY(this CanvasView canvasView, double extentY) => 
            CanvasCoordinateConversions.ConvertExtentYToViewportY(extentY, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertViewportDXToCanvasDX(this CanvasView canvasView, double viewportDX) => 
            CanvasCoordinateConversions.ConvertViewportDXToCanvasDX(viewportDX, canvasView.ScaleRatio);

        public static double ConvertViewportDXToExtentDX(this CanvasView canvasView, double viewportDX) => 
            CanvasCoordinateConversions.ConvertViewportDXToExtentDX(viewportDX);

        public static double ConvertViewportDYToCanvasDY(this CanvasView canvasView, double viewportDY) => 
            CanvasCoordinateConversions.ConvertViewportDYToCanvasDY(viewportDY, canvasView.ScaleRatio);

        public static double ConvertViewportDYToExtentDY(this CanvasView canvasView, double viewportDY) => 
            CanvasCoordinateConversions.ConvertViewportDYToExtentDY(viewportDY);

        public static double ConvertViewportHeightToCanvasHeight(this CanvasView canvasView, double viewportHeight) => 
            CanvasCoordinateConversions.ConvertViewportHeightToCanvasHeight(viewportHeight, canvasView.ScaleRatio);

        public static double ConvertViewportHeightToExtentHeight(this CanvasView canvasView, double viewportHeight) => 
            CanvasCoordinateConversions.ConvertViewportHeightToExtentHeight(viewportHeight);

        public static PointDouble ConvertViewportToCanvas(this CanvasView canvasView, PointDouble viewportPt) => 
            CanvasCoordinateConversions.ConvertViewportToCanvas(viewportPt, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static RectDouble ConvertViewportToCanvas(this CanvasView canvasView, RectDouble viewportRect) => 
            CanvasCoordinateConversions.ConvertViewportToCanvas(viewportRect, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static SizeDouble ConvertViewportToCanvas(this CanvasView canvasView, SizeDouble viewportSize) => 
            CanvasCoordinateConversions.ConvertViewportToCanvas(viewportSize, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static VectorDouble ConvertViewportToCanvas(this CanvasView canvasView, VectorDouble viewportVec) => 
            CanvasCoordinateConversions.ConvertViewportToCanvas(viewportVec, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static PointDouble ConvertViewportToExtent(this CanvasView canvasView, PointDouble viewportPt) => 
            CanvasCoordinateConversions.ConvertViewportToExtent(viewportPt, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static RectDouble ConvertViewportToExtent(this CanvasView canvasView, RectDouble viewportRect) => 
            CanvasCoordinateConversions.ConvertViewportToExtent(viewportRect, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static SizeDouble ConvertViewportToExtent(this CanvasView canvasView, SizeDouble viewportSize) => 
            CanvasCoordinateConversions.ConvertViewportToExtent(viewportSize);

        public static VectorDouble ConvertViewportToExtent(this CanvasView canvasView, VectorDouble viewportVec) => 
            CanvasCoordinateConversions.ConvertViewportToExtent(viewportVec);

        public static double ConvertViewportWidthToCanvasWidth(this CanvasView canvasView, double viewportWidth) => 
            CanvasCoordinateConversions.ConvertViewportWidthToCanvasWidth(viewportWidth, canvasView.ScaleRatio);

        public static double ConvertViewportWidthToExtentWidth(this CanvasView canvasView, double viewportWidth) => 
            CanvasCoordinateConversions.ConvertViewportWidthToExtentWidth(viewportWidth);

        public static double ConvertViewportXToCanvasX(this CanvasView canvasView, double viewportX) => 
            CanvasCoordinateConversions.ConvertViewportXToCanvasX(viewportX, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertViewportXToExtentX(this CanvasView canvasView, double viewportX) => 
            CanvasCoordinateConversions.ConvertViewportXToExtentX(viewportX, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertViewportYToCanvasY(this CanvasView canvasView, double viewportY) => 
            CanvasCoordinateConversions.ConvertViewportYToCanvasY(viewportY, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static double ConvertViewportYToExtentY(this CanvasView canvasView, double viewportY) => 
            CanvasCoordinateConversions.ConvertViewportYToExtentY(viewportY, canvasView.ScaleRatio, canvasView.ViewportCanvasOffset);

        public static RectDouble GetCanvasBounds(this CanvasView canvasView) => 
            new RectDouble(PointDouble.Zero, canvasView.CanvasSize);

        public static RectDouble GetCanvasViewportBounds(this CanvasView canvasView)
        {
            RectDouble canvasBounds = canvasView.GetCanvasBounds();
            RectDouble extentRect = canvasView.ConvertCanvasToExtent(canvasBounds);
            return canvasView.ConvertExtentToViewport(extentRect);
        }

        public static RectDouble GetVisibleCanvasBounds(this CanvasView canvasView)
        {
            RectDouble visibleCanvasViewportBounds = canvasView.GetVisibleCanvasViewportBounds();
            RectDouble extentRect = canvasView.ConvertViewportToExtent(visibleCanvasViewportBounds);
            return canvasView.ConvertExtentToCanvas(extentRect);
        }

        public static RectDouble GetVisibleCanvasViewportBounds(this CanvasView canvasView)
        {
            RectDouble viewportCanvasBounds = canvasView.ViewportCanvasBounds;
            RectDouble extentRect = canvasView.ConvertCanvasToExtent(viewportCanvasBounds);
            RectDouble a = canvasView.ConvertExtentToViewport(extentRect);
            RectDouble canvasViewportBounds = canvasView.GetCanvasViewportBounds();
            return RectDouble.Intersect(a, canvasViewportBounds);
        }
    }
}

