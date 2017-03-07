namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;

    internal abstract class CanvasLayerView<TDerived, TCanvasLayer> : ThreadAffinitizedObjectBase, IDisposable, IIsDisposed where TDerived: CanvasLayerView<TDerived, TCanvasLayer> where TCanvasLayer: CanvasLayer<TCanvasLayer, TDerived>
    {
        private PaintDotNet.Canvas.CanvasView canvasView;
        private TCanvasLayer owner;

        protected CanvasLayerView(TCanvasLayer owner, PaintDotNet.Canvas.CanvasView canvasView)
        {
            Validate.Begin().IsNotNull<TCanvasLayer>(owner, "owner").IsNotNull<PaintDotNet.Canvas.CanvasView>(canvasView, "canvasView").Check();
            this.owner = owner;
            this.canvasView = canvasView;
            this.canvasView.IsVisibleChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
        }

        public void AfterRender(RectFloat clipRect)
        {
            this.OnAfterRender(clipRect);
        }

        public void BeforeRender(RectFloat clipRect)
        {
            this.OnBeforeRender(clipRect);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.InvalidateDeviceResources();
                if (this.canvasView != null)
                {
                    this.canvasView.IsVisibleChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
                    this.canvasView = null;
                }
            }
        }

        public void InvalidateDeviceResources()
        {
            this.OnInvalidateDeviceResources();
        }

        protected virtual void OnAfterRender(RectFloat clipRect)
        {
        }

        protected virtual void OnBeforeRender(RectFloat clipRect)
        {
        }

        protected virtual void OnCanvasViewIsVisibleChanged(object sender, ValueChangedEventArgs<bool> e)
        {
        }

        protected virtual void OnInvalidateDeviceResources()
        {
        }

        protected virtual void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
        }

        public void Render(IDrawingContext dc, RectFloat clipRect)
        {
            this.OnRender(dc, clipRect);
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.canvasView;

        public bool IsDisposed =>
            (this.canvasView == null);

        public TCanvasLayer Owner =>
            this.owner;
    }
}

