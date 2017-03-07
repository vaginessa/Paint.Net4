namespace PaintDotNet.Rendering
{
    using PaintDotNet.Collections;
    using System;
    using System.Collections.Generic;

    internal static class RectInt32Util
    {
        public static RectInt32 FromPixelPoints(PointInt32 a, PointInt32 b)
        {
            RectInt32 num = FromPoints(a, b);
            int num2 = num.Width + 1;
            num.Width = num2;
            num2 = num.Height + 1;
            num.Height = num2;
            return num;
        }

        public static RectInt32 FromPoints(PointInt32 a, PointInt32 b)
        {
            int left = Math.Min(a.X, b.X);
            int top = Math.Min(a.Y, b.Y);
            int right = Math.Max(a.X, b.X);
            int bottom = Math.Max(a.Y, b.Y);
            return RectInt32.FromEdges(left, top, right, bottom);
        }

        public static RectInt32[] SimplifyRegion(IList<RectInt32> rects) => 
            SimplifyRegion(rects, 50);

        public static RectInt32[] SimplifyRegion(IList<RectInt32> rects, int maxRects)
        {
            if (maxRects < 1)
            {
                throw new ArgumentOutOfRangeException($"maxRects={maxRects} but must be >= 1");
            }
            if (rects.Count < maxRects)
            {
                return rects.ToArrayEx<RectInt32>();
            }
            RectInt32[] numArray = new RectInt32[maxRects];
            for (int i = 0; i < maxRects; i++)
            {
                int startIndex = (i * rects.Count) / maxRects;
                int length = Math.Min(rects.Count, ((i + 1) * rects.Count) / maxRects) - startIndex;
                numArray[i] = rects.Bounds(startIndex, length);
            }
            return numArray;
        }

        public static void Split(RectInt32 rect, RectInt32[] rects)
        {
            int height = rect.Height;
            for (int i = 0; i < rects.Length; i++)
            {
                int left = rect.Left;
                int y = rect.Top + ((height * i) / rects.Length);
                int right = rect.Right;
                int num6 = rect.Top + ((height * (i + 1)) / rects.Length);
                rects[i] = new RectInt32(left, y, right - left, num6 - y);
            }
        }

        public static RectInt32 Union(RectInt32? rect1, RectInt32 rect2)
        {
            if (!rect1.HasValue)
            {
                return rect2;
            }
            return RectInt32.Union(rect1.Value, rect2);
        }

        public static RectInt32? Union(RectInt32? rect1, RectInt32? rect2)
        {
            if (!rect1.HasValue && !rect2.HasValue)
            {
                return null;
            }
            if (rect1.HasValue && !rect2.HasValue)
            {
                return new RectInt32?(rect1.Value);
            }
            if (!rect1.HasValue && rect2.HasValue)
            {
                return new RectInt32?(rect2.Value);
            }
            return new RectInt32?(RectInt32.Union(rect1.Value, rect2.Value));
        }
    }
}

