namespace PaintDotNet.VisualStyling
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal sealed class ShadowDecorationRenderer : RendererBgraBase
    {
        private int shadowExtent;
        private IRenderer<ColorBgra> source;
        private IRenderer<ColorBgra> sourceOutset;

        public ShadowDecorationRenderer(IRenderer<ColorBgra> source, int shadowExtent) : base(source.Width + (shadowExtent * 2), source.Height + (shadowExtent * 2), true)
        {
            this.shadowExtent = shadowExtent;
            this.source = source;
            this.sourceOutset = this.source.Crop(new RectInt32(-shadowExtent, -shadowExtent, this.source.Width + (2 * shadowExtent), this.source.Height + (2 * shadowExtent)));
        }

        protected override void OnRender(ISurface<ColorBgra> dstCropped, PointInt32 renderOffset)
        {
            RectInt32 num = this.Bounds<ColorBgra>();
            using (RenderArgs args = new RenderArgs(dstCropped))
            {
                args.Graphics.Clear(Color.Transparent);
                this.sourceOutset.Render(dstCropped, renderOffset);
                args.Graphics.TranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixOrder.Append);
                DropShadow.DrawInside(args.Graphics, new Rectangle(0, 0, base.Width, base.Height), this.shadowExtent);
            }
        }
    }
}

