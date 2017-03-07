namespace PaintDotNet.Tools.CloneStamp
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class CloneStampToolContentRenderer : BrushToolContentRendererBase<CloneStampTool, CloneStampToolChanges, CloneStampToolUI>
    {
        private byte opacity;
        private IRenderer<ColorBgra> sampleSource;
        private IRenderer<ColorBgra> txSampleSource;

        public CloneStampToolContentRenderer(IRenderer<ColorBgra> sampleSource, CloneStampToolChanges changes) : base(sampleSource.Width, sampleSource.Height, changes)
        {
            this.sampleSource = sampleSource;
            this.txSampleSource = new OffsetRenderer<ColorBgra>(sampleSource, changes.SourceSamplingOffset);
            this.opacity = changes.Color.A;
        }

        protected override void OnRenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset)
        {
            SizeInt32 num = dstContent.Size<ColorBgra>();
            this.txSampleSource.Render(dstContent, renderOffset);
        }

        protected override void OnRenderMask(ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.OnRenderMask(dstMask, renderOffset);
            dstMask.Multiply(this.opacity);
        }
    }
}

