namespace PaintDotNet.Tools.BrushBase
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;

    internal abstract class BrushToolContentRendererBase<TTool, TChanges, TUI> : CancellableMaskedRendererBgraBase where TTool: BrushToolBase<TTool, TChanges, TUI> where TChanges: BrushToolChangesBase<TChanges, TTool> where TUI: BrushToolUIBase<TUI, TTool, TChanges>, new()
    {
        private readonly TChanges changes;

        protected BrushToolContentRendererBase(int width, int height, TChanges changes) : base(width, height, true)
        {
            Validate.IsNotNull<TChanges>(changes, "changes");
            this.changes = changes;
        }

        protected sealed override void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            this.OnRenderContent(dstContent, renderOffset);
            base.ThrowIfCancellationRequested();
            this.OnRenderMask(dstMask, renderOffset);
            base.ThrowIfCancellationRequested();
        }

        protected abstract void OnRenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset);
        protected virtual void OnRenderMask(ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            this.changes.RenderCache.RenderMask(dstMask, renderOffset);
        }

        protected TChanges Changes =>
            this.changes;
    }
}

