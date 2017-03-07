namespace PaintDotNet.Tools.Eraser
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class EraserToolContentRenderer : BrushToolContentRendererBase<EraserTool, EraserToolChanges, EraserToolUI>
    {
        private byte opacity;

        public EraserToolContentRenderer(int width, int height, EraserToolChanges changes) : base(width, height, changes)
        {
            this.opacity = changes.Color.A;
        }

        protected override void OnRenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset)
        {
            dstContent.Clear();
        }

        protected override void OnRenderMask(ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.OnRenderMask(dstMask, renderOffset);
            dstMask.Multiply(this.opacity);
        }
    }
}

