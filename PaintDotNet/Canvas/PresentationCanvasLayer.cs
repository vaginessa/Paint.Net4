namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class PresentationCanvasLayer : CanvasLayer, IDirect2DCompositionTargetHost
    {
        private List<CanvasView> canvasViews;
        private Direct2DCompositionTarget compositionTarget;

        public PresentationCanvasLayer()
        {
            this.compositionTarget = new Direct2DCompositionTarget(this);
            this.canvasViews = new List<CanvasView>(2);
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<Direct2DCompositionTarget>(ref this.compositionTarget, disposing);
            base.Dispose(disposing);
        }

        private Matrix3x2Double GetMatrixToDevice()
        {
            base.VerifyAccess();
            CanvasView view = this.canvasViews.FirstOrDefault<CanvasView>();
            if (view == null)
            {
                return Matrix3x2Double.Identity;
            }
            double scaleRatio = view.ScaleRatio;
            Matrix3x2Double num2 = Matrix3x2Double.Scaling(scaleRatio, scaleRatio);
            PointDouble viewportCanvasOffset = view.ViewportCanvasOffset;
            Matrix3x2Double num4 = Matrix3x2Double.Translation(-viewportCanvasOffset.X, -viewportCanvasOffset.Y);
            return (num2 * num4);
        }

        protected override void OnBeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            if (!LayoutManager.IsUpdatingLayout)
            {
                Visual rootVisual = this.compositionTarget.RootVisual;
                if ((rootVisual != null) && (PresentationSource.FromVisual(rootVisual) != null))
                {
                    LayoutManager.UpdateLayout();
                }
            }
            base.OnBeforeRender(clipRect, canvasView);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            this.compositionTarget.Render(dc, clipRect);
            base.OnRender(dc, clipRect, canvasView);
        }

        protected override void OnViewRegistered(CanvasView canvasView)
        {
            base.VerifyAccess();
            if (!this.canvasViews.Contains(canvasView))
            {
                this.canvasViews.Add(canvasView);
            }
            base.OnViewRegistered(canvasView);
        }

        protected override void OnViewUnregistered(CanvasView canvasView)
        {
            base.VerifyAccess();
            this.canvasViews.Remove(canvasView);
            base.OnViewUnregistered(canvasView);
        }

        void IDirect2DCompositionTargetHost.NotifyInvalidated(RectDouble canvasRect)
        {
            base.Invalidate(canvasRect);
        }

        public Direct2DCompositionTarget CompositionTarget =>
            this.compositionTarget;

        Matrix3x2Double IDirect2DCompositionTargetHost.MatrixFromDevice =>
            this.GetMatrixToDevice().Inverse;

        Matrix3x2Double IDirect2DCompositionTargetHost.MatrixToDevice =>
            this.GetMatrixToDevice();
    }
}

