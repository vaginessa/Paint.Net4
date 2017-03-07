namespace PaintDotNet.Tools.FloodFill
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class FeatheredMaskRenderer : IRenderer<ColorAlpha8>
    {
        private ColorBgra basis;
        private IRenderer<ColorBgra> colorSource;
        private byte invTolerance;
        private Func<bool> isCancellationRequestedFn;
        private IRenderer<ColorAlpha8> stencilSource;
        private byte tolerance;
        private static readonly Func<byte, byte[]> toleranceToMaskValueLookupTable = Func.Memoize<byte, byte[]>(new Func<byte, byte[]>(FeatheredMaskRenderer.CreateMaskValueLookupTable));

        public FeatheredMaskRenderer(IRenderer<ColorBgra> colorSource, ColorBgra basis, IRenderer<ColorAlpha8> stencilSource, byte tolerance, Func<bool> isCancellationRequestedFn)
        {
            Validate.Begin().IsNotNull<IRenderer<ColorBgra>>(colorSource, "colorSource").IsNotNull<IRenderer<ColorAlpha8>>(stencilSource, "stencilSource").IsNotNull<Func<bool>>(isCancellationRequestedFn, "isCancellationRequestedFn").Check();
            this.colorSource = colorSource;
            this.basis = basis;
            this.stencilSource = stencilSource;
            this.tolerance = tolerance;
            this.isCancellationRequestedFn = isCancellationRequestedFn;
            this.invTolerance = (byte) (0xff - tolerance);
        }

        private byte CombineMaskAndCoverageValues(byte mask, byte coverage)
        {
            byte x = (byte) (0xff - ByteUtil.FastScale((byte) (0xff - mask), (byte) (0xff - mask)));
            byte frac = (byte) (0xff - ByteUtil.FastScale((byte) (0xff - coverage), (byte) (0xff - coverage)));
            return ByteUtil.FastScale(x, frac);
        }

        private static byte[] CreateMaskValueLookupTable(byte tolerance)
        {
            byte[] buffer = new byte[0x100];
            for (int i = 0; i <= 0xff; i++)
            {
                buffer[i] = GetMaskValue(tolerance, (byte) i);
            }
            return buffer;
        }

        private byte GetCoverageValue(bool s00, bool s01, bool s02, bool s10, bool s12, bool s20, bool s21, bool s22)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            if (s00)
            {
                num = 1;
            }
            if (s01)
            {
                num = 1;
                num5 = 1;
                num2 = 1;
                num6 = 1;
            }
            if (s02)
            {
                num2 = 1;
            }
            if (s10)
            {
                num5 = 1;
                num = 1;
                num7 = 1;
                num3 = 1;
            }
            if (s12)
            {
                num6 = 1;
                num2 = 1;
                num8 = 1;
                num4 = 1;
            }
            if (s20)
            {
                num3 = 1;
            }
            if (s21)
            {
                num7 = 1;
                num3 = 1;
                num8 = 1;
                num4 = 1;
            }
            if (s22)
            {
                num4 = 1;
            }
            int num9 = ((((((num + num2) + num3) + num4) + num5) + num6) + num7) + num8;
            return (byte) ((num9 * 0xff) >> 3);
        }

        private byte GetCoverageValue(byte s00, byte s01, byte s02, byte s10, byte s12, byte s20, byte s21, byte s22) => 
            this.GetCoverageValue(s00 == 0xff, s01 == 0xff, s02 == 0xff, s10 == 0xff, s12 == 0xff, s20 == 0xff, s21 == 0xff, s22 == 0xff);

        private static byte GetMaskValue(byte tolerance, byte distance)
        {
            if (tolerance == 0xff)
            {
                return 0xff;
            }
            byte num = (byte) Math.Max(0, distance - tolerance);
            byte num2 = (byte) (0xff - tolerance);
            double d = ((double) num) / ((double) num2);
            double num4 = 1.0 - d;
            double num5 = Math.Sqrt(num4);
            double num6 = Math.Sqrt(d);
            double num7 = 1.0 - num6;
            double num8 = 255.0 * num7;
            return DoubleUtil.ClampToByte(Math.Round(num8, MidpointRounding.AwayFromZero));
        }

        public unsafe void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            if (!this.isCancellationRequestedFn())
            {
                int width = dst.Width;
                int height = dst.Height;
                RectInt32 bounds = RectInt32.Inflate(new RectInt32(renderOffset, new SizeInt32(width, height)), 1, 1);
                byte[] buffer = toleranceToMaskValueLookupTable(this.tolerance);
                if (!this.isCancellationRequestedFn())
                {
                    using (ISurface<ColorAlpha8> surface = UseTileOrToSurfaceWithEdgePadding(this.stencilSource, bounds, ColorAlpha8.Transparent))
                    {
                        for (int i = 0; i < height; i++)
                        {
                            if (this.isCancellationRequestedFn())
                            {
                                return;
                            }
                            byte* rowPointer = (byte*) dst.GetRowPointer<ColorAlpha8>(i);
                            byte* numPtr2 = (byte*) surface.GetPointPointer<ColorAlpha8>(0, i);
                            byte* numPtr3 = (byte*) surface.GetPointPointer<ColorAlpha8>(0, (i + 1));
                            byte* numPtr4 = (byte*) surface.GetPointPointer<ColorAlpha8>(0, (i + 2));
                            for (int j = 0; j < width; j++)
                            {
                                byte num6;
                                byte num7 = numPtr3[1];
                                if (num7 != 0)
                                {
                                    num6 = num7;
                                }
                                else
                                {
                                    byte num8 = numPtr2[0];
                                    byte num9 = numPtr2[1];
                                    byte num10 = numPtr2[2];
                                    byte num11 = numPtr3[0];
                                    byte num12 = numPtr3[2];
                                    byte num13 = numPtr4[0];
                                    byte num14 = numPtr4[1];
                                    byte num15 = numPtr4[2];
                                    if ((((num8 == 0xff) || (num9 == 0xff)) || ((num10 == 0xff) || (num11 == 0xff))) || (((num12 == 0xff) || (num13 == 0xff)) || ((num14 == 0xff) || (num15 == 0xff))))
                                    {
                                        ColorBgra b = this.colorSource.GetPointSlow(j + renderOffset.X, i + renderOffset.Y);
                                        byte distance = FloodFillAlgorithm.GetDistance(this.basis, b);
                                        byte mask = buffer[distance];
                                        byte coverage = this.GetCoverageValue(num8, num9, num10, num11, num12, num13, num14, num15);
                                        num6 = this.CombineMaskAndCoverageValues(mask, coverage);
                                    }
                                    else
                                    {
                                        num6 = 0;
                                    }
                                }
                                rowPointer[0] = num6;
                                rowPointer++;
                                numPtr2++;
                                numPtr3++;
                                numPtr4++;
                            }
                        }
                    }
                }
            }
        }

        private static ISurface<ColorAlpha8> UseTileOrToSurfaceWithEdgePadding(IRenderer<ColorAlpha8> source, RectInt32 bounds, ColorAlpha8 padding)
        {
            if (source.Bounds<ColorAlpha8>().Contains(bounds))
            {
                return source.UseTileOrToSurface(bounds);
            }
            ISurface<ColorAlpha8> surface = SurfaceAllocator.Alpha8.Allocate<ColorAlpha8>(bounds.Size, AllocationOptions.ZeroFillNotRequired);
            surface.Clear(padding);
            RectInt32 num2 = RectInt32.Intersect(source.Bounds<ColorAlpha8>(), bounds);
            PointInt32 location = new PointInt32(num2.Location.X - bounds.Location.X, num2.Location.Y - bounds.Location.Y);
            RectInt32 num4 = new RectInt32(location, num2.Size);
            using (ISurfaceWindow<ColorAlpha8> window = surface.CreateWindow<ColorAlpha8>(num4))
            {
                source.Render(window, num2.Location);
            }
            return surface;
        }

        public int Height =>
            this.stencilSource.Height;

        public int Width =>
            this.stencilSource.Width;
    }
}

