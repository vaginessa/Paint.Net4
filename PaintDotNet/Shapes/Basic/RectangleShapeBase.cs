namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using System;

    internal abstract class RectangleShapeBase : PdnGeometryShapeBase
    {
        protected RectangleShapeBase(string displayName) : base(displayName, ShapeCategory.Basic)
        {
        }

        protected static RectDouble? TryGetInsetInteriorFillBounds(RectDouble bounds, double penWidth)
        {
            double left = bounds.Left + penWidth;
            double top = bounds.Top + penWidth;
            double right = bounds.Right - penWidth;
            double bottom = bounds.Bottom - penWidth;
            if ((left <= right) && (top <= bottom))
            {
                return new RectDouble?(RectDouble.FromEdges(left, top, right, bottom));
            }
            return null;
        }

        protected static RectDouble? TryGetInsetOutlineDrawBounds(RectDouble bounds, double penWidth)
        {
            double num = penWidth / 2.0;
            double left = bounds.Left + num;
            double top = bounds.Top + num;
            double right = bounds.Right - num;
            double bottom = bounds.Bottom - num;
            if ((left <= right) && (top <= bottom))
            {
                return new RectDouble?(RectDouble.FromEdges(left, top, right, bottom));
            }
            return null;
        }
    }
}

