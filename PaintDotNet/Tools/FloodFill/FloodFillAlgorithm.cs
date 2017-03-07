namespace PaintDotNet.Tools.FloodFill
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class FloodFillAlgorithm
    {
        public static bool CheckColor(ColorBgra start, ColorBgra checkMe, byte maxDistance) => 
            (GetDistance(start, checkMe) <= maxDistance);

        public static unsafe void FillStencilByColor<TBitVector2D>(IRenderer<ColorBgra> sampleSource, TBitVector2D stencilBuffer, ColorBgra basis, byte tolerance, Func<bool> isCancellationRequestedFn, RectInt32 clipRect) where TBitVector2D: IBitVector2D
        {
            if (!isCancellationRequestedFn())
            {
                int left = clipRect.Left;
                int top = clipRect.Top;
                int right = clipRect.Right;
                int bottom = clipRect.Bottom;
                using (ISurface<ColorBgra> surface = sampleSource.UseTileOrToSurface(clipRect))
                {
                    for (int i = top; i < bottom; i++)
                    {
                        if (isCancellationRequestedFn())
                        {
                            return;
                        }
                        int row = i - clipRect.Top;
                        ColorBgra* rowPointer = (ColorBgra*) surface.GetRowPointer<ColorBgra>(row);
                        for (int j = left; j < right; j++)
                        {
                            bool flag;
                            ColorBgra b = rowPointer[0];
                            if (b == basis)
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = GetDistance(basis, b) <= tolerance;
                            }
                            stencilBuffer.SetUnchecked(j, i, flag);
                            rowPointer++;
                        }
                    }
                }
            }
        }

        public static unsafe void FillStencilFromPoint<TBitVector2D>(IRenderer<ColorBgra> sampleSource, TBitVector2D stencilBuffer, PointInt32 startPt, byte tolerance, Func<bool> isCancellationRequestedFn, out RectInt32 bounds) where TBitVector2D: IBitVector2D
        {
            Validate.IsNotNull<IRenderer<ColorBgra>>(sampleSource, "sampleSource");
            int width = sampleSource.Width;
            int height = sampleSource.Height;
            if ((width > stencilBuffer.Width) || (height > stencilBuffer.Height))
            {
                throw new ArgumentException();
            }
            if (!sampleSource.CheckPointValue<ColorBgra>(startPt))
            {
                bounds = RectInt32.Empty;
            }
            else
            {
                int num3 = sampleSource.Width;
                int num4 = sampleSource.Height;
                using (RendererRowCache<ColorBgra> cache = new RendererRowCache<ColorBgra>(sampleSource))
                {
                    int num5 = 0x7fffffff;
                    int num6 = 0x7fffffff;
                    int num7 = -2147483648;
                    int num8 = -2147483648;
                    Queue<PointInt32> queue = new Queue<PointInt32>(0x10);
                    queue.Enqueue(startPt);
                    cache.AddRowRef(startPt.Y - 1);
                    cache.AddRowRef(startPt.Y);
                    cache.AddRowRef(startPt.Y + 1);
                    ColorBgra start = *((ColorBgra*) (((void*) cache.GetRow(startPt.Y)) + (startPt.X * sizeof(ColorBgra))));
                    while (queue.Any<PointInt32>())
                    {
                        if (isCancellationRequestedFn())
                        {
                            bounds = RectInt32.Empty;
                            return;
                        }
                        PointInt32 pt = queue.Dequeue();
                        try
                        {
                            if (sampleSource.CheckPointValue<ColorBgra>(pt))
                            {
                                ColorBgra* row = (ColorBgra*) cache.GetRow(pt.Y);
                                int x = pt.X - 1;
                                int num11 = pt.X;
                                while (((x >= 0) && !stencilBuffer.Get(x, pt.Y)) && CheckColor(start, row[x], tolerance))
                                {
                                    stencilBuffer.Set(x, pt.Y, true);
                                    x--;
                                }
                                while (((num11 < num3) && !stencilBuffer.Get(num11, pt.Y)) && CheckColor(start, row[num11], tolerance))
                                {
                                    stencilBuffer.Set(num11, pt.Y, true);
                                    num11++;
                                }
                                x++;
                                num11--;
                                if (pt.Y > 0)
                                {
                                    cache.AddRowRef(pt.Y - 2);
                                    cache.AddRowRef(pt.Y - 1);
                                    cache.AddRowRef(pt.Y);
                                    try
                                    {
                                        ColorBgra* bgraPtr2 = (ColorBgra*) cache.GetRow(pt.Y - 1);
                                        int num12 = x;
                                        int num13 = x;
                                        for (int i = x; i <= num11; i++)
                                        {
                                            if (!stencilBuffer.Get(i, pt.Y - 1) && CheckColor(start, bgraPtr2[i], tolerance))
                                            {
                                                num13++;
                                            }
                                            else
                                            {
                                                if ((num13 - num12) > 0)
                                                {
                                                    queue.Enqueue(new PointInt32(num12, pt.Y - 1));
                                                    cache.AddRowRef(pt.Y - 2);
                                                    cache.AddRowRef(pt.Y - 1);
                                                    cache.AddRowRef(pt.Y);
                                                }
                                                num13++;
                                                num12 = num13;
                                            }
                                        }
                                        if ((num13 - num12) > 0)
                                        {
                                            queue.Enqueue(new PointInt32(num12, pt.Y - 1));
                                            cache.AddRowRef(pt.Y - 2);
                                            cache.AddRowRef(pt.Y - 1);
                                            cache.AddRowRef(pt.Y);
                                        }
                                    }
                                    finally
                                    {
                                        cache.ReleaseRowRef(pt.Y - 2);
                                        cache.ReleaseRowRef(pt.Y - 1);
                                        cache.ReleaseRowRef(pt.Y);
                                    }
                                }
                                if (pt.Y < (num4 - 1))
                                {
                                    cache.AddRowRef(pt.Y);
                                    cache.AddRowRef(pt.Y + 1);
                                    cache.AddRowRef(pt.Y + 2);
                                    try
                                    {
                                        ColorBgra* bgraPtr3 = (ColorBgra*) cache.GetRow(pt.Y + 1);
                                        int num15 = x;
                                        int num16 = x;
                                        for (int j = x; j <= num11; j++)
                                        {
                                            if (!stencilBuffer.Get(j, pt.Y + 1) && CheckColor(start, bgraPtr3[j], tolerance))
                                            {
                                                num16++;
                                            }
                                            else
                                            {
                                                if ((num16 - num15) > 0)
                                                {
                                                    queue.Enqueue(new PointInt32(num15, pt.Y + 1));
                                                    cache.AddRowRef(pt.Y);
                                                    cache.AddRowRef(pt.Y + 1);
                                                    cache.AddRowRef(pt.Y + 2);
                                                }
                                                num16++;
                                                num15 = num16;
                                            }
                                        }
                                        if ((num16 - num15) > 0)
                                        {
                                            queue.Enqueue(new PointInt32(num15, pt.Y + 1));
                                            cache.AddRowRef(pt.Y);
                                            cache.AddRowRef(pt.Y + 1);
                                            cache.AddRowRef(pt.Y + 2);
                                        }
                                    }
                                    finally
                                    {
                                        cache.ReleaseRowRef(pt.Y);
                                        cache.ReleaseRowRef(pt.Y + 1);
                                        cache.ReleaseRowRef(pt.Y + 2);
                                    }
                                }
                                num5 = Math.Min(num5, x);
                                num6 = Math.Min(num6, pt.Y);
                                num7 = Math.Max(num7, num11);
                                num8 = Math.Max(num8, pt.Y);
                            }
                            continue;
                        }
                        finally
                        {
                            cache.ReleaseRowRef(pt.Y - 1);
                            cache.ReleaseRowRef(pt.Y);
                            cache.ReleaseRowRef(pt.Y + 1);
                        }
                    }
                    bounds = RectInt32.FromEdges(num5, num6, num7, num8);
                }
            }
        }

        public static byte GetDistance(ColorBgra a, ColorBgra b)
        {
            if (a.Bgra == b.Bgra)
            {
                return 0;
            }
            double num = ByteUtil.ToScalingDouble(a.R);
            double num2 = ByteUtil.ToScalingDouble(a.G);
            double num3 = ByteUtil.ToScalingDouble(a.B);
            double num4 = ByteUtil.ToScalingDouble(a.A);
            double num5 = ByteUtil.ToScalingDouble(b.R);
            double num6 = ByteUtil.ToScalingDouble(b.G);
            double num7 = ByteUtil.ToScalingDouble(b.B);
            double num8 = ByteUtil.ToScalingDouble(b.A);
            double num9 = num - num5;
            double num10 = (num9 * num9) * num4;
            double num11 = num2 - num6;
            double num12 = (num11 * num11) * num4;
            double num13 = num3 - num7;
            double num14 = (num13 * num13) * num4;
            double num15 = num4 - num8;
            double num16 = num15 * num15;
            double d = ((num10 + num12) + num14) + num16;
            double num19 = Math.Sqrt(d) / 2.0;
            double num20 = num19 * 255.0;
            int num21 = (int) Math.Round(num20, MidpointRounding.AwayFromZero);
            byte num22 = (byte) num21;
            return Math.Max(1, num22);
        }
    }
}

