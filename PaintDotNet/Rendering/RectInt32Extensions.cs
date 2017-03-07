namespace PaintDotNet.Rendering
{
    using PaintDotNet.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class RectInt32Extensions
    {
        public static RectInt32 Bounds(this IEnumerable<RectInt32> rects)
        {
            RectInt32 zero = RectInt32.Zero;
            bool flag = true;
            foreach (RectInt32 num2 in rects)
            {
                if (flag)
                {
                    zero = num2;
                    flag = false;
                }
                else
                {
                    zero = RectInt32.Union(zero, num2);
                }
            }
            if (flag)
            {
                return RectInt32.Empty;
            }
            return zero;
        }

        public static RectInt32 Bounds(this IList<RectInt32> rects) => 
            rects.Bounds(0, rects.Count);

        public static RectInt32 Bounds(this RectInt32[] rects) => 
            rects.Bounds(0, rects.Length);

        public static RectInt32 Bounds(this IList<RectInt32> rects, int startIndex, int length)
        {
            Validate.Begin().IsNotNull<IList<RectInt32>>(rects, "rects").Check().IsRangeValid(rects.Count, startIndex, length, "rects").Check();
            if (length == 0)
            {
                return RectInt32.Zero;
            }
            RectInt32 a = rects[startIndex];
            for (int i = startIndex + 1; i < (startIndex + length); i++)
            {
                a = RectInt32.Union(a, rects[i]);
            }
            return a;
        }

        public static RectInt32 Bounds(this RectInt32[] rects, int startIndex, int length)
        {
            Validate.Begin().IsNotNull<RectInt32[]>(rects, "rects").Check().IsRangeValid(rects.Length, startIndex, length, "rects").Check();
            if (length == 0)
            {
                return RectInt32.Zero;
            }
            RectInt32 a = rects[startIndex];
            for (int i = startIndex + 1; i < (startIndex + length); i++)
            {
                a = RectInt32.Union(a, rects[i]);
            }
            return a;
        }

        public static RectInt32 CoalesceCopy(this RectInt32 rect)
        {
            if (rect.HasZeroArea)
            {
                return RectInt32.Zero;
            }
            return rect;
        }
    }
}

