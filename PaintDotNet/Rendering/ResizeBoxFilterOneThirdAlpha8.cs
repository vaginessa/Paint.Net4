namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging;
    using PaintDotNet.SystemLayer;
    using System;

    internal sealed class ResizeBoxFilterOneThirdAlpha8 : IRenderer<ColorAlpha8>
    {
        private static readonly ISurfaceAllocator<ColorAlpha8> alpha8Allocator = SurfaceAllocator.Alpha8;
        private int height;
        private IRenderer<ColorAlpha8> source;
        private int width;

        public ResizeBoxFilterOneThirdAlpha8(IRenderer<ColorAlpha8> source)
        {
            Validate.IsNotNull<IRenderer<ColorAlpha8>>(source, "source");
            int width = source.Width;
            int height = source.Height;
            if (((width % 3) != 0) || ((height % 3) != 0))
            {
                ExceptionUtil.ThrowArgumentException("source must have a width and height that are a multiple of 3");
            }
            this.source = source;
            this.width = width / 3;
            this.height = height / 3;
        }

        public unsafe void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int stride = dst.Stride;
            int x = renderOffset.X * 3;
            int y = renderOffset.Y * 3;
            int dstWidth = width * 3;
            int dstHeight = height * 3;
            IMaskFromScansRenderer source = this.source as IMaskFromScansRenderer;
            if (source != null)
            {
                ISurface<ColorAlpha8> surface = null;
                try
                {
                    int? nullable;
                    source.Render(alpha8Allocator, dstWidth, dstHeight, new PointInt32(x, y), ref surface, out nullable);
                    if (nullable.HasValue)
                    {
                        if (nullable.GetValueOrDefault() != 0)
                        {
                            if (nullable.GetValueOrDefault() != (dstWidth * dstHeight))
                            {
                                throw new UnreachableCodeException();
                            }
                            dst.Clear(ColorAlpha8.Opaque);
                        }
                        else
                        {
                            dst.Clear(ColorAlpha8.Transparent);
                        }
                    }
                    else
                    {
                        RenderingKernels.ResizeBoxFilterOneThirdAlpha8((byte*) dst.Scan0, width, height, stride, (byte*) surface.Scan0, dstWidth, dstHeight, surface.Stride);
                    }
                }
                finally
                {
                    DisposableUtil.Free<ISurface<ColorAlpha8>>(ref surface);
                }
            }
            else
            {
                using (ISurface<ColorAlpha8> surface2 = this.source.UseTileOrToSurface(new RectInt32(x, y, dstWidth, dstHeight)))
                {
                    RenderingKernels.ResizeBoxFilterOneThirdAlpha8((byte*) dst.Scan0, width, height, stride, (byte*) surface2.Scan0, dstWidth, dstHeight, surface2.Stride);
                }
            }
        }

        public int Height =>
            this.height;

        public int Width =>
            this.width;
    }
}

