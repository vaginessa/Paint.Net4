namespace PaintDotNet.Canvas
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;

    internal abstract class CanvasLayer<TDerived, TCanvasLayerView> : CanvasLayer where TDerived: CanvasLayer<TDerived, TCanvasLayerView> where TCanvasLayerView: CanvasLayerView<TCanvasLayerView, TDerived>
    {
        private Dictionary<CanvasView, TCanvasLayerView> canvasLayerViews;

        protected CanvasLayer()
        {
            this.canvasLayerViews = new Dictionary<CanvasView, TCanvasLayerView>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.canvasLayerViews != null))
            {
                foreach (CanvasView view in this.canvasLayerViews.Keys.ToArrayEx<CanvasView>())
                {
                    this.TryRemoveCanvasLayerView(view);
                }
                this.canvasLayerViews = null;
            }
            base.Dispose(disposing);
        }

        protected ICollection<TCanvasLayerView> GetCanvasLayerViews()
        {
            base.VerifyAccess();
            return this.canvasLayerViews.Values;
        }

        protected override void OnAfterRender(RectFloat clipRect, CanvasView canvasView)
        {
            TCanvasLayerView local;
            if (this.canvasLayerViews.TryGetValue(canvasView, out local))
            {
                local.AfterRender(clipRect);
            }
            base.OnAfterRender(clipRect, canvasView);
        }

        protected override void OnBeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            TCanvasLayerView local;
            if (this.canvasLayerViews.TryGetValue(canvasView, out local))
            {
                local.BeforeRender(clipRect);
            }
            base.OnBeforeRender(clipRect, canvasView);
        }

        protected override void OnInvalidateDeviceResources(CanvasView canvasView)
        {
            TCanvasLayerView local;
            if (this.canvasLayerViews.TryGetValue(canvasView, out local))
            {
                local.InvalidateDeviceResources();
            }
            base.OnInvalidateDeviceResources(canvasView);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            TCanvasLayerView local;
            if (this.canvasLayerViews.TryGetValue(canvasView, out local))
            {
                local.Render(dc, clipRect);
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        protected abstract TCanvasLayerView OnTryCreateCanvasLayerView(CanvasView canvasView);
        protected override void OnViewRegistered(CanvasView canvasView)
        {
            this.TryRecreateCanvasLayerView(canvasView);
            base.OnViewRegistered(canvasView);
        }

        protected override void OnViewUnregistered(CanvasView canvasView)
        {
            this.TryRemoveCanvasLayerView(canvasView);
            base.OnViewUnregistered(canvasView);
        }

        protected TCanvasLayerView TryGetCanvasLayerView(CanvasView canvasView)
        {
            TCanvasLayerView local;
            base.VerifyAccess();
            Validate.IsNotNull<CanvasView>(canvasView, "canvasView");
            this.canvasLayerViews.TryGetValue(canvasView, out local);
            return local;
        }

        protected bool TryRecreateCanvasLayerView(CanvasView canvasView)
        {
            Validate.IsNotNull<CanvasView>(canvasView, "canvasView");
            base.VerifyAccess();
            this.TryRemoveCanvasLayerView(canvasView);
            TCanvasLayerView local = this.OnTryCreateCanvasLayerView(canvasView);
            if (local != null)
            {
                this.canvasLayerViews.Add(canvasView, local);
                return true;
            }
            return false;
        }

        protected bool TryRemoveCanvasLayerView(CanvasView canvasView)
        {
            TCanvasLayerView local;
            Validate.IsNotNull<CanvasView>(canvasView, "canvasView");
            base.VerifyAccess();
            if (this.canvasLayerViews.TryGetValue(canvasView, out local))
            {
                local.Dispose();
                this.canvasLayerViews.Remove(canvasView);
                return true;
            }
            return false;
        }
    }
}

