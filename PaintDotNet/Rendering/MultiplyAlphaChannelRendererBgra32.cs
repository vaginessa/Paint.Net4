namespace PaintDotNet.Rendering
{
    using PaintDotNet.Diagnostics;
    using System;

    internal sealed class MultiplyAlphaChannelRendererBgra32 : IRenderer<ColorBgra>
    {
        private IRenderer<ColorAlpha8> alpha;
        private IRenderer<ColorBgra> source;

        public MultiplyAlphaChannelRendererBgra32(IRenderer<ColorBgra> source, IRenderer<ColorAlpha8> alpha)
        {
            Validate.Begin().IsNotNull<IRenderer<ColorBgra>>(source, "source").IsNotNull<IRenderer<ColorAlpha8>>(alpha, "alpha").Check().AreEqual<SizeInt32>(source.Size<ColorBgra>(), "source.Size()", alpha.Size<ColorAlpha8>(), "alpha.Size()").Check();
            this.source = source;
            this.alpha = alpha;
        }

        public void Render(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            SizeInt32 size = dst.Size<ColorBgra>();
            this.source.Render(dst, renderOffset);
            using (ISurface<ColorAlpha8> surface = this.alpha.UseTileOrToSurface(new RectInt32(renderOffset, size)))
            {
                dst.MultiplyAlphaChannel(surface);
            }
        }

        public int Height =>
            this.source.Height;

        public int Width =>
            this.source.Width;
    }
}

