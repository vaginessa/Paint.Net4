namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.MemoryManagement;
    using System;

    internal sealed class ExtractAlpha8FromBgra32Renderer : IRenderer<ColorAlpha8>
    {
        private IRenderer<ColorBgra> source;

        public ExtractAlpha8FromBgra32Renderer(IRenderer<ColorBgra> source)
        {
            Validate.IsNotNull<IRenderer<ColorBgra>>(source, "source");
            this.source = source;
        }

        public unsafe void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            using (ISurface<ColorBgra> surface = SurfaceAllocator.Bgra.Allocate(width, height, AllocationOptions.ZeroFillNotRequired))
            {
                this.source.Render(surface, renderOffset);
                for (int i = 0; i < height; i++)
                {
                    ColorBgra* rowPointer = (ColorBgra*) surface.GetRowPointer<ColorBgra>(i);
                    byte* numPtr = (byte*) dst.GetRowPointer<ColorAlpha8>(i);
                    int num4 = width;
                    while (num4 >= 4)
                    {
                        uint num5 = ((uint) ((((rowPointer->Bgra & -16777216) >> 0x18) | ((rowPointer[1].Bgra & -16777216) >> 0x10)) | ((rowPointer[2].Bgra & -16777216) >> 8))) | (rowPointer[3].Bgra & 0xff000000);
                        *((int*) numPtr) = num5;
                        rowPointer += 4;
                        numPtr += 4;
                        num4 -= 4;
                    }
                    while (num4 > 0)
                    {
                        numPtr[0] = rowPointer->A;
                        rowPointer++;
                        numPtr++;
                        num4--;
                    }
                }
            }
        }

        public int Height =>
            this.source.Height;

        public int Width =>
            this.source.Width;
    }
}

