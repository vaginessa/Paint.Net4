namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class RectDoubleExtensions
    {
        public static PointDouble BottomCenter(this RectDouble rect) => 
            new PointDouble(rect.Left + (rect.Width / 2.0), rect.Bottom);

        public static RectDouble BottomEdge(this RectDouble rect) => 
            RectDouble.FromEdges(rect.Left, rect.Bottom, rect.Right, rect.Bottom);

        public static RectDouble Bounds(this IEnumerable<RectDouble> rects)
        {
            RectDouble zero = RectDouble.Zero;
            bool flag = true;
            foreach (RectDouble num2 in rects)
            {
                if (flag)
                {
                    zero = num2;
                    flag = false;
                }
                else
                {
                    zero = RectDouble.Union(zero, num2);
                }
            }
            if (flag)
            {
                return RectDouble.Empty;
            }
            return zero;
        }

        public static RectDouble Bounds(this RectDouble[] rects) => 
            rects.Bounds(0, rects.Length);

        public static RectDouble Bounds(this RectDouble[] rects, int startIndex, int length)
        {
            Validate.Begin().IsNotNull<RectDouble[]>(rects, "rects").IsIndexInBounds<RectDouble>(rects, startIndex, "startIndex").IsIndexInBounds<RectDouble>(rects, ((startIndex + length) - 1), "startIndex + length").Check();
            if (length == 0)
            {
                return RectDouble.Empty;
            }
            RectDouble rectA = rects[startIndex];
            for (int i = startIndex + 1; i < (startIndex + length); i++)
            {
                rectA = rectA.UnionCopy(rects[i]);
            }
            return rectA;
        }

        public static PointDouble GetCorner(this RectDouble rect, RectCorner corner)
        {
            switch (corner)
            {
                case RectCorner.TopLeft:
                    return rect.TopLeft;

                case RectCorner.TopRight:
                    return rect.TopRight;

                case RectCorner.BottomLeft:
                    return rect.BottomLeft;

                case RectCorner.BottomRight:
                    return rect.BottomRight;
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<RectCorner>(corner, "corner");
            return new PointDouble();
        }

        public static RectDouble GetEdge(this RectDouble rect, RectEdge edge)
        {
            switch (edge)
            {
                case RectEdge.Left:
                    return rect.LeftEdge();

                case RectEdge.Top:
                    return rect.TopEdge();

                case RectEdge.Right:
                    return rect.RightEdge();

                case RectEdge.Bottom:
                    return rect.BottomEdge();
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<RectEdge>(edge, "edge");
            return new RectDouble();
        }

        public static PointDouble GetEdgeCenter(this RectDouble rect, RectEdge edge)
        {
            switch (edge)
            {
                case RectEdge.Left:
                    return rect.LeftCenter();

                case RectEdge.Top:
                    return rect.TopCenter();

                case RectEdge.Right:
                    return rect.RightCenter();

                case RectEdge.Bottom:
                    return rect.BottomCenter();
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<RectEdge>(edge, "edge");
            return new PointDouble();
        }

        public static RectDouble InflateCopy(this RectDouble rect, double w, double h) => 
            RectDouble.Inflate(rect, w, h);

        public static RectInt32? Int32Inset(this RectDouble rect)
        {
            double num = Math.Ceiling(rect.Left);
            double num2 = Math.Ceiling(rect.Top);
            double num3 = Math.Floor(rect.Right);
            double num4 = Math.Floor(rect.Bottom);
            if ((num <= num3) && (num2 <= num4))
            {
                return new RectInt32((int) num, (int) num2, (int) (num3 - num), (int) (num4 - num2));
            }
            return null;
        }

        public static PointDouble LeftCenter(this RectDouble rect) => 
            new PointDouble(rect.Left, rect.Top + (rect.Height / 2.0));

        public static RectDouble LeftEdge(this RectDouble rect) => 
            RectDouble.FromEdges(rect.Left, rect.Top, rect.Left, rect.Bottom);

        public static PointDouble RightCenter(this RectDouble rect) => 
            new PointDouble(rect.Right, rect.Top + (rect.Height / 2.0));

        public static RectDouble RightEdge(this RectDouble rect) => 
            RectDouble.FromEdges(rect.Right, rect.Top, rect.Right, rect.Bottom);

        public static RectDouble ScaleCopy(this RectDouble rect, double scaleX, double scaleY)
        {
            RectDouble num = rect;
            num.Scale(scaleX, scaleY);
            return num;
        }

        public static PointDouble TopCenter(this RectDouble rect) => 
            new PointDouble(rect.Left + (rect.Width / 2.0), rect.Top);

        public static RectDouble TopEdge(this RectDouble rect) => 
            RectDouble.FromEdges(rect.Left, rect.Top, rect.Right, rect.Top);

        public static RectInt32 TruncateCopy(this RectDouble rect) => 
            new RectInt32((int) Math.Truncate(rect.X), (int) Math.Truncate(rect.Y), (int) Math.Truncate(rect.Width), (int) Math.Truncate(rect.Height));

        public static RectDouble UnionCopy(this RectDouble rectA, RectDouble rectB) => 
            RectDouble.Union(rectA, rectB);
    }
}

