namespace PaintDotNet.Rendering
{
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class MaskFromScansRenderer<TList> : IRenderer<ColorAlpha8>, IMaskFromScansRenderer where TList: IReadOnlyList<RectInt32>
    {
        private PointInt32 origin;
        private TList scans;
        private SizeInt32 size;

        internal MaskFromScansRenderer(TList sortedScans, RectInt32 bounds)
        {
            this.origin = bounds.Location;
            this.size = bounds.Size;
            this.scans = sortedScans;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RectInt32 GetScanAt(int index) => 
            RectInt32.Offset(this.scans[index], -this.origin.X, -this.origin.Y);

        private int GetScansStartIndex(int srcTop, int srcBottom)
        {
            int num11;
            int count = this.scans.Count;
            int num2 = 0;
            int num3 = count - 1;
            int index = 0;
            while (num2 <= num3)
            {
                int num5 = num2 + ((num3 - num2) >> 1);
                RectInt32 scanAt = this.GetScanAt(num5);
                int y = scanAt.Y;
                int num8 = scanAt.Y + scanAt.Height;
                if (y > srcBottom)
                {
                    num3 = num5 - 1;
                }
                else
                {
                    if (num8 < srcTop)
                    {
                        num2 = num5 + 1;
                        continue;
                    }
                    index = num5;
                    break;
                }
            }
            do
            {
                index--;
                if (index == -1)
                {
                    return 0;
                }
                RectInt32 num9 = this.GetScanAt(index);
                int num10 = num9.Y;
                num11 = num9.Y + num9.Height;
            }
            while (num11 >= srcTop);
            index++;
            return index;
        }

        public void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int? nullable;
            this.Render(null, dst.Width, dst.Height, renderOffset, ref dst, out nullable);
        }

        public unsafe void Render(ISurfaceAllocator<ColorAlpha8> allocator, int dstWidth, int dstHeight, PointInt32 renderOffset, ref ISurface<ColorAlpha8> dst, out int? fill255Count)
        {
            int stride;
            if ((allocator == null) && (dst == null))
            {
                throw new ArgumentException();
            }
            if (dst != null)
            {
                dst.Clear(ColorAlpha8.Transparent);
            }
            int x = renderOffset.X;
            int y = renderOffset.Y;
            int right = renderOffset.X + dstWidth;
            int bottom = renderOffset.Y + dstHeight;
            RectInt32 a = RectInt32.FromEdges(x, y, right, bottom);
            int count = this.scans.Count;
            int scansStartIndex = this.GetScansStartIndex(y, bottom);
        Label_006C:
            stride = -1;
            byte* numPtr = null;
            if (dst != null)
            {
                stride = dst.Stride;
                numPtr = (byte*) dst.Scan0;
            }
            int num9 = 0;
            for (int i = scansStartIndex; i < count; i++)
            {
                RectInt32 scanAt = this.GetScanAt(i);
                if (scanAt.Y >= bottom)
                {
                    break;
                }
                int left = Math.Max(x, scanAt.X);
                int top = Math.Max(y, scanAt.Y);
                int num14 = Math.Min(right, scanAt.X + scanAt.Width);
                int num15 = Math.Min(bottom, scanAt.Y + scanAt.Height);
                RectInt32 b = RectInt32.FromEdges(left, top, num14, num15);
                if (RectInt32.Intersect(a, b).HasPositiveArea)
                {
                    uint num18 = (uint) (num14 - left);
                    int num19 = left - renderOffset.X;
                    int num20 = num14 - renderOffset.X;
                    if (dst == null)
                    {
                        num9 += (int) ((num15 - top) * num18);
                    }
                    else
                    {
                        byte* prgBuffer = numPtr + ((byte*) (((top - renderOffset.Y) * stride) + num19));
                        for (int j = top; j < num15; j++)
                        {
                            Memory.FillMemory(prgBuffer, 0xff, (ulong) num18);
                            prgBuffer += stride;
                        }
                    }
                }
            }
            if (((dst != null) || (num9 == 0)) || (num9 == (dstWidth * dstHeight)))
            {
                if (dst == null)
                {
                    fill255Count = new int?(num9);
                }
                else
                {
                    fill255Count = 0;
                }
            }
            else
            {
                dst = allocator.Allocate(dstWidth, dstHeight, AllocationOptions.Default);
                goto Label_006C;
            }
        }

        public int Height =>
            this.size.Height;

        public int Width =>
            this.size.Width;
    }
}

