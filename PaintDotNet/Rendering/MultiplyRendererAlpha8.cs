namespace PaintDotNet.Rendering
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.SystemLayer;
    using System;

    internal sealed class MultiplyRendererAlpha8 : IRenderer<ColorAlpha8>
    {
        private IRenderer<ColorAlpha8> first;
        private int firstHeight;
        private int firstWidth;
        private IRenderer<ColorAlpha8> second;
        private int secondHeight;
        private int secondWidth;

        public MultiplyRendererAlpha8(IRenderer<ColorAlpha8> first, IRenderer<ColorAlpha8> second)
        {
            Validate.Begin().IsNotNull<IRenderer<ColorAlpha8>>(first, "first").IsNotNull<IRenderer<ColorAlpha8>>(second, "second").Check().AreEqual<SizeInt32>(first.Size<ColorAlpha8>(), "first.Size()", second.Size<ColorAlpha8>(), "second.Size()").Check();
            this.first = first;
            this.firstWidth = first.Width;
            this.firstHeight = first.Height;
            this.second = second;
            this.secondWidth = second.Width;
            this.secondHeight = second.Height;
        }

        public unsafe void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int stride = dst.Stride;
            using (ISurface<ColorAlpha8> surface = SurfaceAllocator.Alpha8.Allocate(width, height, AllocationOptions.ZeroFillNotRequired))
            {
                this.first.Render(dst, renderOffset);
                this.second.Render(surface, renderOffset);
                int num4 = surface.Stride;
                if ((stride == width) && (stride == num4))
                {
                    RenderingKernels.MultiplyAlpha8ByAlpha8Extent((byte*) dst.Scan0, (byte*) surface.Scan0, width * height);
                }
                else
                {
                    for (int i = 0; i < height; i++)
                    {
                        byte* rowPointer = (byte*) dst.GetRowPointer<ColorAlpha8>(i);
                        byte* numPtr2 = (byte*) surface.GetRowPointer<ColorAlpha8>(i);
                        RenderingKernels.MultiplyAlpha8ByAlpha8Extent(rowPointer, numPtr2, width);
                    }
                }
            }
        }

        public int Height =>
            this.firstHeight;

        public int Width =>
            this.firstWidth;
    }
}

