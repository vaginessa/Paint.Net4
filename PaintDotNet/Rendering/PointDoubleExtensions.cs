namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class PointDoubleExtensions
    {
        public static RectDouble Bounds(this IEnumerable<PointDouble> points)
        {
            using (IEnumerator<PointDouble> enumerator = points.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return RectDouble.Empty;
                }
                PointDouble current = enumerator.Current;
                double x = current.X;
                double y = current.Y;
                double num4 = current.X;
                double num5 = current.Y;
                while (enumerator.MoveNext())
                {
                    PointDouble num7 = enumerator.Current;
                    x = Math.Min(x, num7.X);
                    y = Math.Min(y, num7.Y);
                    num4 = Math.Max(num4, num7.X);
                    num5 = Math.Max(num5, num7.Y);
                }
                return RectDouble.FromEdges(x, y, num4, num5);
            }
        }

        public static bool IsCloseToZero(this PointDouble pt) => 
            (DoubleUtil.IsCloseToZero(pt.X) && DoubleUtil.IsCloseToZero(pt.Y));
    }
}

