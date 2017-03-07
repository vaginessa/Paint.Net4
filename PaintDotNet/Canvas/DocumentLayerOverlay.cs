namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;
    using System.Threading;

    internal abstract class DocumentLayerOverlay : IRenderer<ColorBgra>
    {
        private RectInt32 affectedBounds;
        private CancellationTokenSource cancellationTokenSource;
        private PaintDotNet.Layer layer;

        protected DocumentLayerOverlay(PaintDotNet.Layer layer, RectInt32 affectedBounds)
        {
            Validate.IsNotNull<PaintDotNet.Layer>(layer, "layer");
            this.layer = layer;
            this.affectedBounds = affectedBounds;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationTokenSource.Token.Register(new Action(this.OnCancelled));
        }

        public void Cancel()
        {
            this.cancellationTokenSource.Cancel();
        }

        protected abstract void OnCancelled();
        protected abstract void OnRender(ISurface<ColorBgra> dst, PointInt32 renderOffset);
        public void Render(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            if (this.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            this.OnRender(dst, renderOffset);
        }

        public RectInt32 AffectedBounds =>
            this.affectedBounds;

        protected System.Threading.CancellationToken CancellationToken =>
            this.cancellationTokenSource.Token;

        public int Height =>
            this.layer.Height;

        public bool IsCancellationRequested =>
            this.cancellationTokenSource.IsCancellationRequested;

        public PaintDotNet.Layer Layer =>
            this.layer;

        public int Width =>
            this.layer.Width;
    }
}

