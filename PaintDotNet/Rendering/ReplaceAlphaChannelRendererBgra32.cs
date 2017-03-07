namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.MemoryManagement;
    using System;

    internal sealed class ReplaceAlphaChannelRendererBgra32 : IRenderer<ColorBgra>
    {
        private IRenderer<ColorAlpha8> alphaSource;
        private IRenderer<ColorBgra> colorSource;

        public ReplaceAlphaChannelRendererBgra32(IRenderer<ColorBgra> colorSource, IRenderer<ColorAlpha8> alphaSource)
        {
            Validate.Begin().IsNotNull<IRenderer<ColorBgra>>(colorSource, "colorSource").IsNotNull<IRenderer<ColorAlpha8>>(alphaSource, "alphaSource").Check().AreEqual<SizeInt32>(colorSource.Size<ColorBgra>(), "colorSource.Size()", alphaSource.Size<ColorAlpha8>(), "alphaSource.Size()").Check();
            this.colorSource = colorSource;
            this.alphaSource = alphaSource;
        }

        public unsafe void Render(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            using (ISurface<ColorAlpha8> surface = SurfaceAllocator.Alpha8.Allocate(width, height, AllocationOptions.ZeroFillNotRequired))
            {
                this.colorSource.Render(dst, renderOffset);
                this.alphaSource.Render(surface, renderOffset);
                for (int i = 0; i < height; i++)
                {
                    ColorBgra* rowPointer = (ColorBgra*) dst.GetRowPointer<ColorBgra>(i);
                    byte* numPtr = (byte*) surface.GetRowPointer<ColorAlpha8>(i);
                    int num4 = width;
                    while (num4 >= 4)
                    {
                        uint num5 = *((uint*) numPtr);
                        rowPointer->Bgra = (rowPointer->Bgra & 0xffffff) | ((uint) ((num5 & 0xff) << 0x18));
                        rowPointer[1].Bgra = (rowPointer[1].Bgra & 0xffffff) | ((uint) ((num5 & 0xff00) << 0x10));
                        rowPointer[2].Bgra = (rowPointer[2].Bgra & 0xffffff) | ((uint) ((num5 & 0xff0000) << 8));
                        rowPointer[3].Bgra = (rowPointer[3].Bgra & 0xffffff) | (num5 & 0xff000000);
                        rowPointer += 4;
                        numPtr += 4;
                        num4 -= 4;
                    }
                    while (num4 > 0)
                    {
                        rowPointer->A = numPtr[0];
                        rowPointer++;
                        numPtr++;
                        num4--;
                    }
                }
            }
        }

        public int Height =>
            this.colorSource.Height;

        public int Width =>
            this.colorSource.Width;
    }
}

