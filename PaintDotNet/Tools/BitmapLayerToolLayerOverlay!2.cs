namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;

    internal sealed class BitmapLayerToolLayerOverlay<TTool, TChanges> : DocumentBitmapLayerOverlay where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private ContentBlendMode blendMode;
        private TChanges changes;
        private IRenderer<ColorAlpha8> clipMaskRenderer;
        private IMaskedRenderer<ColorBgra, ColorAlpha8>[] contentRenderers;
        private ContentRendererBgra renderer;

        public BitmapLayerToolLayerOverlay(BitmapLayer layer, RectInt32 affectedBounds, TChanges changes, ContentBlendMode blendMode, IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> contentRenderers, IRenderer<ColorAlpha8> clipMaskRenderer) : base(layer, affectedBounds)
        {
            Validate.IsNotNull<TChanges>(changes, "changes");
            this.changes = changes;
            this.blendMode = blendMode;
            this.contentRenderers = contentRenderers.ToArrayEx<IMaskedRenderer<ColorBgra, ColorAlpha8>>();
            this.clipMaskRenderer = clipMaskRenderer;
            this.renderer = new ContentRendererBgra(base.Layer.Surface, this.blendMode, this.contentRenderers, this.clipMaskRenderer);
        }

        protected override void OnCancelled()
        {
            CancellableUtil.TryCancel(this.renderer);
            foreach (IMaskedRenderer<ColorBgra, ColorAlpha8> renderer in this.contentRenderers)
            {
                CancellableUtil.TryCancel(renderer);
            }
        }

        protected override void OnRender(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            this.renderer.Render(dst, renderOffset);
        }

        public TChanges Changes =>
            this.changes;
    }
}

