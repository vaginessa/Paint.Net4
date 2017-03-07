namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class CanvasLayer : ThreadAffinitizedObjectBase, IDisposable, IIsDisposed
    {
        private bool isDisposed;
        private bool isTopMost;
        private bool isVisible = true;
        private PaintDotNet.Canvas.Canvas owner;

        [field: CompilerGenerated]
        public event EventHandler<CanvasInvalidatedEventArgs> Invalidated;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<bool> IsVisibleChanged;

        public void AfterRender(RectFloat clipRect, CanvasView canvasView)
        {
            this.OnAfterRender(clipRect, canvasView);
        }

        public void BeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            this.OnBeforeRender(clipRect, canvasView);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.isDisposed = true;
        }

        ~CanvasLayer()
        {
            this.Dispose(false);
        }

        public void Invalidate()
        {
            base.VerifyAccess();
            this.Invalidate(PaintDotNet.Canvas.Canvas.CanvasMaxBounds);
        }

        public void Invalidate(RectDouble canvasRect)
        {
            base.VerifyAccess();
            this.OnInvalidated(CanvasInvalidatedEventArgs.IdentityCallback, canvasRect);
        }

        public void Invalidate(CalculateInvalidRectCallback callback, RectDouble canvasRect)
        {
            base.VerifyAccess();
            this.OnInvalidated(callback, canvasRect);
        }

        public void InvalidateDeviceResources(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.OnInvalidateDeviceResources(canvasView);
        }

        protected virtual void OnAfterRender(RectFloat clipRect, CanvasView canvasView)
        {
        }

        protected virtual void OnBeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
        }

        protected virtual void OnCanvasChanged(PaintDotNet.Canvas.Canvas oldValue, PaintDotNet.Canvas.Canvas newValue)
        {
        }

        protected virtual void OnInvalidated(CalculateInvalidRectCallback callback, RectDouble canvasRect)
        {
            this.Invalidated.RaisePooled<CanvasInvalidatedEventArgs, CalculateInvalidRectCallback, RectDouble>(this, callback, canvasRect);
        }

        protected virtual void OnInvalidateDeviceResources(CanvasView canvasView)
        {
        }

        protected virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            this.IsVisibleChanged.Raise<bool>(this, oldValue, newValue);
        }

        protected virtual void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
        }

        protected virtual void OnViewRegistered(CanvasView canvasView)
        {
        }

        protected virtual void OnViewUnregistered(CanvasView canvasView)
        {
        }

        public void RegisterView(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.OnViewRegistered(canvasView);
        }

        public void Render(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            this.OnRender(dc, clipRect, canvasView);
        }

        public void UnregisterView(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.OnViewUnregistered(canvasView);
        }

        public bool IsDisposed =>
            this.isDisposed;

        public bool IsTopMost
        {
            get => 
                this.isTopMost;
            set
            {
                base.VerifyAccess();
                if (value != this.isTopMost)
                {
                    this.isTopMost = value;
                    this.Invalidate();
                }
            }
        }

        public bool IsVisible
        {
            get => 
                this.isVisible;
            set
            {
                base.VerifyAccess();
                bool isVisible = this.isVisible;
                if (value != isVisible)
                {
                    this.isVisible = value;
                    this.OnIsVisibleChanged(isVisible, value);
                }
            }
        }

        public PaintDotNet.Canvas.Canvas Owner
        {
            get => 
                this.owner;
            set
            {
                base.VerifyAccess();
                PaintDotNet.Canvas.Canvas owner = this.owner;
                if (owner != null)
                {
                    owner.CanvasLayers.Remove(this);
                }
                this.owner = value;
                if ((this.owner != null) && !this.owner.CanvasLayers.Contains(this))
                {
                    this.owner.CanvasLayers.Add(this);
                }
                this.OnCanvasChanged(owner, value);
            }
        }
    }
}

