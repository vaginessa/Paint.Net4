namespace PaintDotNet.Tools.PaintBrush
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class PaintBrushToolContentRenderer : BrushToolContentRendererBase<PaintBrushTool, PaintBrushToolChanges, PaintBrushToolUI>
    {
        private PdnBrush brush;
        private IRenderer<ColorBgra> brushRenderer;

        public PaintBrushToolContentRenderer(int width, int height, PaintBrushToolChanges changes) : base(width, height, changes)
        {
            this.brush = new PdnLegacyBrush(base.Changes.BrushType, base.Changes.HatchStyle, base.Changes.ForegroundColor, base.Changes.BackgroundColor).EnsureFrozen<PdnLegacyBrush>();
            this.brushRenderer = this.brush.CreateRenderer(base.Width, base.Height);
        }

        protected override void OnRenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset)
        {
            this.brushRenderer.Render(dstContent, renderOffset);
        }
    }
}

