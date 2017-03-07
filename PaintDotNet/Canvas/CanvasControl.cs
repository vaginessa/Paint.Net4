namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Controls;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CanvasControl : Direct2DControl
    {
        private PaintDotNet.Canvas.CanvasView canvasView;
        private int renderCount;
        private ValueChangedEventHandler<RenderingPriority> renderingPriorityChangedHandler;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PaintDotNet.Canvas.Canvas> CanvasChanged;

        public CanvasControl() : base(FactorySource.PerThread)
        {
            base.UseBackColor = false;
            base.UseHwndRenderTarget = AppSettings.Instance.UI.EnableCanvasHwndRenderTarget.Value;
            this.UpdateRenderTargetType();
            AppSettings.Instance.UI.EnableHardwareAcceleration.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnEnableHardwareAccelerationChanged);
            this.canvasView = new PaintDotNet.Canvas.CanvasView();
            this.canvasView.CanvasChanged += new ValueChangedEventHandler<PaintDotNet.Canvas.Canvas>(this.OnCanvasViewCanvasChanged);
            this.canvasView.CanvasSizeChanged += new ValueChangedEventHandler<SizeDouble>(this.OnCanvasViewCanvasSizeChanged);
            this.canvasView.Invalidated += new EventHandler<RectDoubleInvalidatedEventArgs>(this.OnCanvasViewInvalidated);
            this.canvasView.ScaleRatioChanged += new ValueChangedEventHandler<double>(this.OnCanvasViewScaleRatioChanged);
            this.canvasView.ViewportCanvasOffsetChanged += new ValueChangedEventHandler<PointDouble>(this.OnCanvasViewViewportCanvasOffsetChanged);
            this.UpdateOverscroll();
            AppSettings.Instance.UI.EnableOverscroll.ValueChangedT += new ValueChangedEventHandler<bool>(this.OnEnableOverscrollChanged);
            WeakReference<CanvasControl> weakThis = new WeakReference<CanvasControl>(this);
            ValueChangedEventHandler<RenderingPriority> renderingPriorityChangedHandler = null;
            renderingPriorityChangedHandler = delegate (object sender, ValueChangedEventArgs<RenderingPriority> e) {
                CanvasControl control;
                if (weakThis.TryGetTarget(out control))
                {
                    control.OnRenderingPriorityChanged(null, e);
                }
                else
                {
                    RenderingPriorityManager.RenderingPriorityChanged -= renderingPriorityChangedHandler;
                }
            };
            this.renderingPriorityChangedHandler = renderingPriorityChangedHandler;
            RenderingPriorityManager.RenderingPriorityChanged += this.renderingPriorityChangedHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppSettings.Instance.UI.EnableHardwareAcceleration.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnEnableHardwareAccelerationChanged);
                AppSettings.Instance.UI.EnableOverscroll.ValueChangedT -= new ValueChangedEventHandler<bool>(this.OnEnableOverscrollChanged);
                this.canvasView.CanvasChanged -= new ValueChangedEventHandler<PaintDotNet.Canvas.Canvas>(this.OnCanvasViewCanvasChanged);
                this.canvasView.CanvasSizeChanged -= new ValueChangedEventHandler<SizeDouble>(this.OnCanvasViewCanvasSizeChanged);
                this.canvasView.ScaleRatioChanged -= new ValueChangedEventHandler<double>(this.OnCanvasViewScaleRatioChanged);
                this.canvasView.ViewportCanvasOffsetChanged -= new ValueChangedEventHandler<PointDouble>(this.OnCanvasViewViewportCanvasOffsetChanged);
                this.canvasView.Canvas = null;
                if (this.renderingPriorityChangedHandler != null)
                {
                    RenderingPriorityManager.RenderingPriorityChanged -= this.renderingPriorityChangedHandler;
                    this.renderingPriorityChangedHandler = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnAfterRender(RectFloat viewportClipRect)
        {
            if ((this.Canvas != null) && viewportClipRect.HasPositiveArea)
            {
                RectFloat extentRect = (RectFloat) this.canvasView.ConvertViewportToExtent(viewportClipRect);
                RectFloat clipRect = (RectFloat) this.canvasView.ConvertExtentToCanvas(extentRect);
                this.canvasView.Canvas.AfterRender(clipRect, this.canvasView);
            }
            base.OnAfterRender(viewportClipRect);
        }

        protected override void OnBeforeRender(RectFloat viewportClipRect)
        {
            this.canvasView.RenderTarget = base.RenderTarget;
            if ((this.Canvas != null) && viewportClipRect.HasPositiveArea)
            {
                RectFloat extentRect = (RectFloat) this.canvasView.ConvertViewportToExtent(viewportClipRect);
                RectFloat clipRect = (RectFloat) this.canvasView.ConvertExtentToCanvas(extentRect);
                this.canvasView.Canvas.BeforeRender(clipRect, this.canvasView);
            }
            base.OnBeforeRender(viewportClipRect);
        }

        private void OnCanvasChanged(PaintDotNet.Canvas.Canvas oldValue, PaintDotNet.Canvas.Canvas newValue)
        {
            if (oldValue != null)
            {
                oldValue.Invalidated -= new EventHandler<CanvasInvalidatedEventArgs>(this.OnCanvasInvalidated);
            }
            if (newValue != null)
            {
                newValue.Invalidated += new EventHandler<CanvasInvalidatedEventArgs>(this.OnCanvasInvalidated);
            }
            this.CanvasChanged.Raise<PaintDotNet.Canvas.Canvas>(this, oldValue, newValue);
        }

        private void OnCanvasInvalidated(object sender, CanvasInvalidatedEventArgs e)
        {
            base.VerifyAccess();
            if (this.Canvas != sender)
            {
                throw new PaintDotNet.InternalErrorException("this.canvas != sender");
            }
            PaintDotNet.Canvas.CanvasView canvasView = this.CanvasView;
            RectDouble invalidCanvasRect = e.GetInvalidCanvasRect(canvasView);
            RectDouble extentRect = this.canvasView.ConvertCanvasToExtent(invalidCanvasRect);
            RectInt32 rect = this.canvasView.ConvertExtentToViewport(extentRect).Int32Bound;
            base.Invalidate(rect);
        }

        private void OnCanvasViewCanvasChanged(object sender, ValueChangedEventArgs<PaintDotNet.Canvas.Canvas> e)
        {
            this.OnCanvasChanged(e.OldValue, e.NewValue);
            base.Invalidate();
        }

        private void OnCanvasViewCanvasSizeChanged(object sender, ValueChangedEventArgs<SizeDouble> e)
        {
            base.Invalidate();
        }

        private void OnCanvasViewInvalidated(object sender, RectDoubleInvalidatedEventArgs e)
        {
            RectDouble extentRect = this.canvasView.ConvertCanvasToExtent(e.InvalidRect);
            RectInt32 rect = this.canvasView.ConvertExtentToViewport(extentRect).Int32Bound;
            base.Invalidate(rect);
        }

        private void OnCanvasViewScaleRatioChanged(object sender, ValueChangedEventArgs<double> e)
        {
            base.Invalidate();
        }

        private void OnCanvasViewViewportCanvasOffsetChanged(object sender, ValueChangedEventArgs<PointDouble> e)
        {
            base.Invalidate();
        }

        private void OnEnableHardwareAccelerationChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (!base.IsDisposed)
            {
                this.UpdateRenderTargetType();
            }
        }

        private void OnEnableOverscrollChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (!base.IsDisposed)
            {
                this.UpdateOverscroll();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.canvasView.ViewportSize = base.ClientSize.ToSizeDouble();
            UIUtil.DisableFlicksAndGestures(this);
            base.OnHandleCreated(e);
        }

        protected override void OnInvalidateDeviceResources()
        {
            if (this.Canvas != null)
            {
                this.Canvas.InvalidateDeviceResources(this.canvasView);
            }
            this.canvasView.RenderTarget = null;
            base.OnInvalidateDeviceResources();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
            {
                goto Label_0347;
            }
            PaintDotNet.Canvas.CanvasView canvasView = this.CanvasView;
            if (canvasView == null)
            {
                goto Label_0347;
            }
            bool flag = false;
            switch (e.KeyCode)
            {
                case Keys.PageUp:
                    if (!e.Shift)
                    {
                        canvasView.ViewportCanvasOffset -= new VectorDouble(0.0, canvasView.ViewportCanvasBounds.Height);
                    }
                    else
                    {
                        canvasView.ViewportCanvasOffset -= new VectorDouble(canvasView.ViewportCanvasBounds.Width, 0.0);
                    }
                    flag = true;
                    e.Handled = true;
                    goto Label_032E;

                case Keys.Next:
                    if (!e.Shift)
                    {
                        canvasView.ViewportCanvasOffset += new VectorDouble(0.0, canvasView.ViewportCanvasBounds.Height);
                        break;
                    }
                    canvasView.ViewportCanvasOffset += new VectorDouble(canvasView.ViewportCanvasBounds.Width, 0.0);
                    break;

                case Keys.End:
                    if (!e.Control || !e.Alt)
                    {
                        if (e.Control)
                        {
                            canvasView.ViewportCanvasOffset = canvasView.ViewportCanvasOffsetMax;
                        }
                        else
                        {
                            RectDouble visibleCanvasBounds = canvasView.GetVisibleCanvasBounds();
                            SizeDouble size = canvasView.ViewportCanvasBounds.Size;
                            PointDouble bottomRight = canvasView.GetCanvasBounds().BottomRight;
                            if ((visibleCanvasBounds.Left <= (bottomRight.X - 1.0)) && (visibleCanvasBounds.Right >= bottomRight.X))
                            {
                                if ((visibleCanvasBounds.Top > (bottomRight.Y - 1.0)) || (visibleCanvasBounds.Bottom < bottomRight.Y))
                                {
                                    canvasView.ViewportCanvasOffset = new PointDouble(canvasView.ViewportCanvasOffset.X, bottomRight.Y - size.Height);
                                }
                            }
                            else
                            {
                                canvasView.ViewportCanvasOffset = new PointDouble(bottomRight.X - size.Width, canvasView.ViewportCanvasOffset.Y);
                            }
                        }
                    }
                    flag = true;
                    e.Handled = true;
                    goto Label_032E;

                case Keys.Home:
                    if (!e.Control || !e.Alt)
                    {
                        if (e.Control)
                        {
                            canvasView.ViewportCanvasOffset = canvasView.ViewportCanvasOffsetMin;
                        }
                        else
                        {
                            RectDouble num2 = canvasView.GetVisibleCanvasBounds();
                            PointDouble topLeft = canvasView.GetCanvasBounds().TopLeft;
                            if ((num2.Left <= topLeft.X) && (num2.Right >= (topLeft.X + 1.0)))
                            {
                                if ((num2.Top > topLeft.Y) || (num2.Bottom < (topLeft.Y + 1.0)))
                                {
                                    canvasView.ViewportCanvasOffset -= new VectorDouble(0.0, num2.Top - topLeft.Y);
                                }
                            }
                            else
                            {
                                canvasView.ViewportCanvasOffset -= new VectorDouble(num2.Left - topLeft.X, 0.0);
                            }
                        }
                    }
                    flag = true;
                    e.Handled = true;
                    goto Label_032E;

                default:
                    goto Label_032E;
            }
            flag = true;
            e.Handled = true;
        Label_032E:
            if (flag)
            {
                canvasView.SetValue(PaintDotNet.Canvas.CanvasView.ViewportCanvasOffsetProperty, canvasView.GetValue(PaintDotNet.Canvas.CanvasView.ViewportCanvasOffsetProperty));
            }
        Label_0347:
            base.OnKeyDown(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.ProcessMouseWheel(this, e);
            base.OnMouseWheel(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.IsInputKey = true;
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat viewportClipRect)
        {
            if ((this.Canvas != null) && viewportClipRect.HasPositiveArea)
            {
                RectFloat extentRect = (RectFloat) this.canvasView.ConvertViewportToExtent(viewportClipRect);
                RectFloat clipRect = (RectFloat) this.canvasView.ConvertExtentToCanvas(extentRect);
                using (dc.UseSaveDrawingState())
                {
                    this.canvasView.Canvas.Render(dc, clipRect, this.canvasView);
                }
            }
            base.OnRender(dc, viewportClipRect);
            this.renderCount++;
        }

        private void OnRenderingPriorityChanged(object sender, ValueChangedEventArgs<RenderingPriority> e)
        {
            if (!base.CheckAccess())
            {
                base.BeginInvoke(new Action(this.UpdateRenderingPriority));
            }
            else
            {
                this.UpdateRenderingPriority();
            }
        }

        protected override void OnRenderTargetCreated()
        {
            this.UpdateRenderingPriority();
            base.OnRenderTargetCreated();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.canvasView.ViewportSize = base.ClientSize.ToSizeDouble();
            base.OnSizeChanged(e);
        }

        protected override void OnUnhandledExceptionCreateHwndRenderTarget(Exception ex, RenderTargetProperties rtProps, HwndRenderTargetProperties hwndRTProps)
        {
            if (((ex is AccessViolationException) && (rtProps.Type != RenderTargetType.Software)) && AppSettings.Instance.UI.EnableHardwareAcceleration.Value)
            {
                AppSettings.Instance.UI.EnableHardwareAcceleration.Value = false;
                AppSettings.Instance.UI.ErrorFlags.Value = ((AppErrorFlags) AppSettings.Instance.UI.ErrorFlags.Value) | AppErrorFlags.DisabledHardwareAccelerationDueToCreateHwndRenderTargetAccessViolation;
            }
            base.OnUnhandledExceptionCreateHwndRenderTarget(ex, rtProps, hwndRTProps);
        }

        protected override void OnUnhandledExceptionEndDraw(Exception ex)
        {
            base.OnUnhandledExceptionEndDraw(ex);
        }

        public void ProcessMouseWheel(Control sender, MouseEventArgs e)
        {
            double num11;
            double num12;
            double num13;
            RectDouble visibleCanvasViewportBounds = this.canvasView.GetVisibleCanvasViewportBounds();
            PointInt32 point = base.PointToClient(sender.PointToScreen(e.Location)).ToPointInt32();
            PointDouble viewportPt = RectDoubleUtil.Clamp(visibleCanvasViewportBounds, point);
            PointDouble extentPt = this.canvasView.ConvertViewportToExtent(viewportPt);
            PointDouble num5 = this.canvasView.ConvertExtentToCanvas(extentPt);
            double scaleRatio = this.canvasView.ScaleRatio;
            PointDouble viewportCanvasOffset = this.canvasView.ViewportCanvasOffset;
            double num8 = ((double) e.Delta) / scaleRatio;
            double x = viewportCanvasOffset.X;
            double y = viewportCanvasOffset.Y;
            ScaleBasis scaleBasis = this.canvasView.ScaleBasis;
            ScaleBasis ratio = ScaleBasis.Ratio;
            if (Control.ModifierKeys == Keys.Shift)
            {
                num11 = x - num8;
                num12 = y;
                ratio = scaleBasis;
                num13 = scaleRatio;
            }
            else if (Control.ModifierKeys == Keys.Control)
            {
                double num16;
                double num14 = ((double) e.Delta) / 120.0;
                double num15 = Math.Pow(1.12, Math.Abs(num14));
                if (e.Delta > 0)
                {
                    num16 = scaleRatio * num15;
                }
                else
                {
                    num16 = scaleRatio / num15;
                }
                double num17 = this.canvasView.ClampScaleRatio(num16);
                double num18 = Math.Round(num17, MidpointRounding.AwayFromZero);
                if ((Math.Abs((double) (num18 - num17)) < (num18 * 0.1)) && (Math.Abs((double) (num18 - num17)) < Math.Abs((double) (num18 - scaleRatio))))
                {
                    num13 = num18;
                }
                else
                {
                    num13 = num17;
                }
                ratio = ScaleBasis.Ratio;
                num11 = num5.X - (viewportPt.X / num13);
                num12 = num5.Y - (viewportPt.Y / num13);
            }
            else if (Control.ModifierKeys == Keys.None)
            {
                num11 = x;
                num12 = y - num8;
                ratio = scaleBasis;
                num13 = scaleRatio;
            }
            else
            {
                num11 = x;
                num12 = y;
                ratio = scaleBasis;
                num13 = scaleRatio;
            }
            this.canvasView.ViewportCanvasOffset = new PointDouble(num11, num12);
            this.canvasView.ScaleBasis = ratio;
            this.canvasView.ScaleRatio = num13;
        }

        private void UpdateOverscroll()
        {
            base.VerifyAccess();
            this.canvasView.IsCanvasFrameEnabled = AppSettings.Instance.UI.EnableOverscroll.Value;
        }

        private void UpdateRenderingPriority()
        {
            IRenderTarget renderTarget = base.RenderTarget;
            if ((renderTarget != null) && renderTarget.IsSupported(RenderTargetType.Hardware, null, null, null))
            {
                using (CastOrRefHolder<IDeviceContext> holder = renderTarget.TryCastOrCreateRef<IDeviceContext>())
                {
                    if (holder.HasRef)
                    {
                        using (CastOrRefHolder<IDevice1> holder2 = holder.ObjectRef.Device.TryCastOrCreateRef<IDevice1>())
                        {
                            if (holder2.HasRef)
                            {
                                RenderingPriority renderingPriority = holder2.ObjectRef.RenderingPriority;
                                RenderingPriority priority2 = RenderingPriorityManager.RenderingPriority;
                                if (renderingPriority != priority2)
                                {
                                    holder2.ObjectRef.RenderingPriority = priority2;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateRenderTargetType()
        {
            base.VerifyAccess();
            if (AppSettings.Instance.UI.EnableHardwareAcceleration.Value)
            {
                base.HwndRenderTargetType = RenderTargetType.Default;
            }
            else
            {
                base.HwndRenderTargetType = RenderTargetType.Software;
            }
        }

        public PaintDotNet.Canvas.Canvas Canvas
        {
            get => 
                this.canvasView.Canvas;
            set
            {
                this.canvasView.Canvas = value;
            }
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.canvasView;
    }
}

