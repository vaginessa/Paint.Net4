namespace PaintDotNet.Canvas
{
    using PaintDotNet.Rendering;
    using System;

    internal static class CanvasCoordinateConversions
    {
        public static double ConvertCanvasDXToExtentDX(double canvasDX, double scaleRatio) => 
            (canvasDX * scaleRatio);

        public static double ConvertCanvasDXToViewportDX(double canvasDX, double scaleRatio) => 
            ConvertExtentDXToViewportDX(ConvertCanvasDXToExtentDX(canvasDX, scaleRatio));

        public static double ConvertCanvasDYToExtentDY(double canvasDY, double scaleRatio) => 
            (canvasDY * scaleRatio);

        public static double ConvertCanvasDYToViewportDY(double canvasDY, double scaleRatio) => 
            ConvertExtentDYToViewportDY(ConvertCanvasDXToExtentDX(canvasDY, scaleRatio));

        public static double ConvertCanvasHeightToExtentHeight(double canvasHeight, double scaleRatio) => 
            (canvasHeight * scaleRatio);

        public static double ConvertCanvasHeightToViewportHeight(double canvasHeight, double scaleRatio) => 
            ConvertExtentHeightToViewportHeight(ConvertCanvasHeightToExtentHeight(canvasHeight, scaleRatio));

        public static PointDouble ConvertCanvasToExtent(PointDouble canvasPt, double scaleRatio) => 
            new PointDouble(ConvertCanvasXToExtentX(canvasPt.X, scaleRatio), ConvertCanvasYToExtentY(canvasPt.Y, scaleRatio));

        public static RectDouble ConvertCanvasToExtent(RectDouble canvasRect, double scaleRatio)
        {
            PointDouble location = ConvertCanvasToExtent(canvasRect.Location, scaleRatio);
            return new RectDouble(location, ConvertCanvasToExtent(canvasRect.Size, scaleRatio));
        }

        public static SizeDouble ConvertCanvasToExtent(SizeDouble canvasSize, double scaleRatio) => 
            new SizeDouble(ConvertCanvasWidthToExtentWidth(canvasSize.Width, scaleRatio), ConvertCanvasHeightToExtentHeight(canvasSize.Height, scaleRatio));

        public static VectorDouble ConvertCanvasToExtent(VectorDouble canvasVec, double scaleRatio) => 
            new VectorDouble(ConvertCanvasDXToExtentDX(canvasVec.X, scaleRatio), ConvertCanvasDYToExtentDY(canvasVec.Y, scaleRatio));

        public static PointDouble ConvertCanvasToViewport(PointDouble canvasPt, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToViewport(ConvertCanvasToExtent(canvasPt, scaleRatio), scaleRatio, viewportCanvasOffset);

        public static RectDouble ConvertCanvasToViewport(RectDouble canvasRect, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToViewport(ConvertCanvasToExtent(canvasRect, scaleRatio), scaleRatio, viewportCanvasOffset);

        public static SizeDouble ConvertCanvasToViewport(SizeDouble canvasSize, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToViewport(ConvertCanvasToExtent(canvasSize, scaleRatio));

        public static VectorDouble ConvertCanvasToViewport(VectorDouble canvasVec, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToViewport(ConvertCanvasToExtent(canvasVec, scaleRatio));

        public static double ConvertCanvasWidthToExtentWidth(double canvasWidth, double scaleRatio) => 
            (canvasWidth * scaleRatio);

        public static double ConvertCanvasWidthToViewportWidth(double canvasWidth, double scaleRatio) => 
            ConvertExtentWidthToViewportWidth(ConvertCanvasWidthToExtentWidth(canvasWidth, scaleRatio));

        public static double ConvertCanvasXToExtentX(double canvasX, double scaleRatio) => 
            (canvasX * scaleRatio);

        public static double ConvertCanvasXToViewportX(double canvasX, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentXToViewportX(ConvertCanvasXToExtentX(canvasX, scaleRatio), scaleRatio, viewportCanvasOffset);

        public static double ConvertCanvasYToExtentY(double canvasY, double scaleRatio) => 
            (canvasY * scaleRatio);

        public static double ConvertCanvasYToViewportY(double canvasY, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentYToViewportY(ConvertCanvasYToExtentY(canvasY, scaleRatio), scaleRatio, viewportCanvasOffset);

        public static double ConvertExtentDXToCanvasDX(double extentDX, double scaleRatio) => 
            (extentDX / scaleRatio);

        public static double ConvertExtentDXToViewportDX(double extentDX) => 
            extentDX;

        public static double ConvertExtentDYToCanvasDY(double extentDY, double scaleRatio) => 
            (extentDY / scaleRatio);

        public static double ConvertExtentDYToViewportDY(double extentDY) => 
            extentDY;

        public static double ConvertExtentHeightToCanvasHeight(double extentHeight, double scaleRatio) => 
            (extentHeight / scaleRatio);

        public static double ConvertExtentHeightToViewportHeight(double extentHeight) => 
            extentHeight;

        public static PointDouble ConvertExtentToCanvas(PointDouble extentPt, double scaleRatio)
        {
            double x = ConvertExtentXToCanvasX(extentPt.X, scaleRatio);
            return new PointDouble(x, ConvertExtentYToCanvasY(extentPt.Y, scaleRatio));
        }

        public static RectDouble ConvertExtentToCanvas(RectDouble extentRect, double scaleRatio)
        {
            PointDouble location = ConvertExtentToCanvas(extentRect.Location, scaleRatio);
            return new RectDouble(location, ConvertExtentToCanvas(extentRect.Size, scaleRatio));
        }

        public static SizeDouble ConvertExtentToCanvas(SizeDouble extentSize, double scaleRatio)
        {
            double width = ConvertExtentWidthToCanvasWidth(extentSize.Width, scaleRatio);
            return new SizeDouble(width, ConvertExtentHeightToCanvasHeight(extentSize.Height, scaleRatio));
        }

        public static VectorDouble ConvertExtentToCanvas(VectorDouble extentVec, double scaleRatio)
        {
            double x = ConvertExtentDXToCanvasDX(extentVec.X, scaleRatio);
            return new VectorDouble(x, ConvertExtentDYToCanvasDY(extentVec.Y, scaleRatio));
        }

        public static SizeDouble ConvertExtentToViewport(SizeDouble extentSize) => 
            extentSize;

        public static VectorDouble ConvertExtentToViewport(VectorDouble extentVec) => 
            extentVec;

        public static PointDouble ConvertExtentToViewport(PointDouble extentPt, double scaleRatio, PointDouble viewportCanvasOffset)
        {
            double x = ConvertExtentXToViewportX(extentPt.X, scaleRatio, viewportCanvasOffset);
            return new PointDouble(x, ConvertExtentYToViewportY(extentPt.Y, scaleRatio, viewportCanvasOffset));
        }

        public static RectDouble ConvertExtentToViewport(RectDouble extentRect, double scaleRatio, PointDouble viewportCanvasOffset) => 
            new RectDouble(ConvertExtentToViewport(extentRect.Location, scaleRatio, viewportCanvasOffset), extentRect.Size);

        public static double ConvertExtentWidthToCanvasWidth(double extentWidth, double scaleRatio) => 
            (extentWidth / scaleRatio);

        public static double ConvertExtentWidthToViewportWidth(double extentWidth) => 
            extentWidth;

        public static double ConvertExtentXToCanvasX(double extentX, double scaleRatio) => 
            (extentX / scaleRatio);

        public static double ConvertExtentXToViewportX(double extentX, double scaleRatio, PointDouble viewportCanvasOffset)
        {
            double num = ConvertCanvasXToExtentX(viewportCanvasOffset.X, scaleRatio);
            return (extentX - num);
        }

        public static double ConvertExtentYToCanvasY(double extentY, double scaleRatio) => 
            (extentY / scaleRatio);

        public static double ConvertExtentYToViewportY(double extentY, double scaleRatio, PointDouble viewportCanvasOffset)
        {
            double num = ConvertCanvasYToExtentY(viewportCanvasOffset.Y, scaleRatio);
            return (extentY - num);
        }

        public static double ConvertViewportDXToCanvasDX(double viewportDX, double scaleRatio) => 
            ConvertExtentDXToCanvasDX(ConvertViewportDXToExtentDX(viewportDX), scaleRatio);

        public static double ConvertViewportDXToExtentDX(double viewportDX) => 
            viewportDX;

        public static double ConvertViewportDYToCanvasDY(double viewportDY, double scaleRatio) => 
            ConvertExtentDYToCanvasDY(ConvertViewportDXToExtentDX(viewportDY), scaleRatio);

        public static double ConvertViewportDYToExtentDY(double viewportDY) => 
            viewportDY;

        public static double ConvertViewportHeightToCanvasHeight(double viewportHeight, double scaleRatio) => 
            ConvertExtentHeightToCanvasHeight(ConvertViewportHeightToExtentHeight(viewportHeight), scaleRatio);

        public static double ConvertViewportHeightToExtentHeight(double viewportHeight) => 
            viewportHeight;

        public static PointDouble ConvertViewportToCanvas(PointDouble viewportPt, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToCanvas(ConvertViewportToExtent(viewportPt, scaleRatio, viewportCanvasOffset), scaleRatio);

        public static RectDouble ConvertViewportToCanvas(RectDouble viewportRect, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToCanvas(ConvertViewportToExtent(viewportRect, scaleRatio, viewportCanvasOffset), scaleRatio);

        public static SizeDouble ConvertViewportToCanvas(SizeDouble viewportSize, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToCanvas(ConvertViewportToExtent(viewportSize), scaleRatio);

        public static VectorDouble ConvertViewportToCanvas(VectorDouble viewportVec, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentToCanvas(ConvertViewportToExtent(viewportVec), scaleRatio);

        public static SizeDouble ConvertViewportToExtent(SizeDouble viewportSize) => 
            viewportSize;

        public static VectorDouble ConvertViewportToExtent(VectorDouble viewportVec) => 
            viewportVec;

        public static PointDouble ConvertViewportToExtent(PointDouble viewportPt, double scaleRatio, PointDouble viewportCanvasOffset) => 
            new PointDouble(ConvertViewportXToExtentX(viewportPt.X, scaleRatio, viewportCanvasOffset), ConvertViewportYToExtentY(viewportPt.Y, scaleRatio, viewportCanvasOffset));

        public static RectDouble ConvertViewportToExtent(RectDouble viewportRect, double scaleRatio, PointDouble viewportCanvasOffset) => 
            new RectDouble(ConvertViewportToExtent(viewportRect.Location, scaleRatio, viewportCanvasOffset), viewportRect.Size);

        public static double ConvertViewportWidthToCanvasWidth(double viewportWidth, double scaleRatio) => 
            ConvertExtentWidthToCanvasWidth(ConvertViewportWidthToExtentWidth(viewportWidth), scaleRatio);

        public static double ConvertViewportWidthToExtentWidth(double viewportWidth) => 
            viewportWidth;

        public static double ConvertViewportXToCanvasX(double viewportX, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentXToCanvasX(ConvertViewportXToExtentX(viewportX, scaleRatio, viewportCanvasOffset), scaleRatio);

        public static double ConvertViewportXToExtentX(double viewportX, double scaleRatio, PointDouble viewportCanvasOffset) => 
            (viewportX + ConvertCanvasXToExtentX(viewportCanvasOffset.X, scaleRatio));

        public static double ConvertViewportYToCanvasY(double viewportY, double scaleRatio, PointDouble viewportCanvasOffset) => 
            ConvertExtentYToCanvasY(ConvertViewportYToExtentY(viewportY, scaleRatio, viewportCanvasOffset), scaleRatio);

        public static double ConvertViewportYToExtentY(double viewportY, double scaleRatio, PointDouble viewportCanvasOffset) => 
            (viewportY + ConvertCanvasYToExtentY(viewportCanvasOffset.Y, scaleRatio));
    }
}

