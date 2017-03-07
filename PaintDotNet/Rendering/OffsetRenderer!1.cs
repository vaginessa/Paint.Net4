namespace PaintDotNet.Rendering
{
    using PaintDotNet.Diagnostics;
    using System;

    internal sealed class OffsetRenderer<TPixel> : IRenderer<TPixel> where TPixel: struct, INaturalPixelInfo
    {
        private PointInt32 offset;
        private IRenderer<TPixel> source;

        public OffsetRenderer(IRenderer<TPixel> source, PointInt32 offset)
        {
            Validate.IsNotNull<IRenderer<TPixel>>(source, "source");
            this.source = source;
            this.offset = offset;
        }

        public void Render(ISurface<TPixel> dst, PointInt32 renderOffset)
        {
            dst.Clear<TPixel>();
            SizeInt32 size = dst.Size<TPixel>();
            RectInt32 num2 = new RectInt32(renderOffset, size);
            PointInt32 location = new PointInt32(renderOffset.X - this.offset.X, renderOffset.Y - this.offset.Y);
            RectInt32 a = new RectInt32(location, size);
            RectInt32 num5 = RectInt32.Intersect(a, this.source.Bounds<TPixel>());
            if (num5.HasPositiveArea)
            {
                PointInt32 num6 = new PointInt32(num5.X - a.X, num5.Y - a.Y);
                RectInt32 bounds = new RectInt32(num6, num5.Size);
                if (bounds.Location.IsZero && (bounds.Size == size))
                {
                    this.source.Render(dst, location);
                }
                else
                {
                    using (ISurface<TPixel> surface = dst.CreateWindow<TPixel>(bounds))
                    {
                        this.source.Render(surface, num5.Location);
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

