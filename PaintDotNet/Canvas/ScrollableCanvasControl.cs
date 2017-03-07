namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ScrollableCanvasControl : Control, IIsDisposed, IDisposable
    {
        private PaintDotNet.Canvas.CanvasControl canvasControl;
        private HScrollBar hScrollBar;
        private VScrollBar vScrollBar;

        public ScrollableCanvasControl()
        {
            base.SuspendLayout();
            base.SetStyle(ControlStyles.Selectable, false);
            this.hScrollBar = new HScrollBar();
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.ValueChanged += new EventHandler(this.OnHScrollBarValueChanged);
            this.hScrollBar.Cursor = Cursors.Arrow;
            this.vScrollBar = new VScrollBar();
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.ValueChanged += new EventHandler(this.OnVScrollBarValueChanged);
            this.vScrollBar.Cursor = Cursors.Arrow;
            this.canvasControl = new PaintDotNet.Canvas.CanvasControl();
            this.canvasControl.Name = "canvasView";
            this.canvasControl.CanvasView.CanvasSizeChanged += new ValueChangedEventHandler<SizeDouble>(this.OnCanvasControlCanvasViewCanvasSizeChanged);
            this.canvasControl.CanvasView.ViewportCanvasOffsetChanged += new ValueChangedEventHandler<PointDouble>(this.OnCanvasControlCanvasViewViewportCanvasOffsetChanged);
            this.canvasControl.CanvasView.ScaleBasisChanged += new ValueChangedEventHandler<ScaleBasis>(this.OnCanvasControlCanvasViewScaleBasisChanged);
            this.canvasControl.CanvasView.ScaleRatioChanged += new ValueChangedEventHandler<double>(this.OnCanvasControlCanvasViewScaleRatioChanged);
            base.Controls.Add(this.hScrollBar);
            base.Controls.Add(this.vScrollBar);
            base.Controls.Add(this.canvasControl);
            base.ResumeLayout(false);
        }

        private double ConvertFromScrollBar(int value) => 
            (((double) value) / ((double) this.GetScrollBarPrecisionFactor()));

        private int ConvertToScrollBar(double value) => 
            ((int) (value * this.GetScrollBarPrecisionFactor()));

        private int GetScrollBarPrecisionFactor()
        {
            double scaleRatio = this.CanvasView.ScaleRatio;
            return Math.Max(1, (int) Math.Ceiling(scaleRatio));
        }

        public void InvalidateLayout()
        {
            base.PerformLayout();
        }

        private void OnCanvasControlCanvasViewCanvasSizeChanged(object sender, ValueChangedEventArgs<SizeDouble> e)
        {
            if (!base.IsDisposed)
            {
                this.InvalidateLayout();
            }
        }

        private void OnCanvasControlCanvasViewScaleBasisChanged(object sender, ValueChangedEventArgs<ScaleBasis> e)
        {
            if (!base.IsDisposed)
            {
                this.InvalidateLayout();
            }
        }

        private void OnCanvasControlCanvasViewScaleRatioChanged(object sender, ValueChangedEventArgs<double> e)
        {
            if (!base.IsDisposed)
            {
                this.InvalidateLayout();
            }
        }

        private void OnCanvasControlCanvasViewViewportCanvasOffsetChanged(object sender, ValueChangedEventArgs<PointDouble> e)
        {
            if (!base.IsDisposed)
            {
                this.InvalidateLayout();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            UIUtil.DisableFlicksAndGestures(this);
            this.UpdateCanvasVisibility();
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.UpdateCanvasVisibility();
            base.OnHandleDestroyed(e);
        }

        private void OnHScrollBarValueChanged(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                double x = this.ConvertFromScrollBar(this.hScrollBar.Value);
                double num2 = this.ConvertFromScrollBar(this.ConvertToScrollBar(this.canvasControl.CanvasView.ViewportCanvasOffset.X));
                if (x != num2)
                {
                    this.canvasControl.CanvasView.ViewportCanvasOffset = new PointDouble(x, this.canvasControl.CanvasView.ViewportCanvasOffset.Y);
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            while (!this.OnLayoutImpl())
            {
            }
            base.OnLayout(levent);
        }

        private bool OnLayoutImpl()
        {
            Rectangle clientRectangle = base.ClientRectangle;
            Size size = clientRectangle.Size;
            if ((size.Width >= 0) && (size.Height >= 0))
            {
                RectDouble viewportRect = clientRectangle.ToRectDouble();
                SizeDouble num2 = size.ToSizeDouble();
                int horizontalScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
                int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                PaintDotNet.Canvas.Canvas canvas = this.canvasControl.Canvas;
                PaintDotNet.Canvas.CanvasView canvasView = this.canvasControl.CanvasView;
                SizeDouble canvasSize = canvasView.CanvasSize;
                RectDouble canvasBounds = canvasView.GetCanvasBounds();
                SizeDouble viewportSize = canvasView.ViewportSize;
                SizeDouble num9 = canvasView.ViewportCanvasBounds.Size;
                PointDouble viewportCanvasOffset = canvasView.ViewportCanvasOffset;
                PointDouble viewportCanvasOffsetMin = canvasView.ViewportCanvasOffsetMin;
                PointDouble viewportCanvasOffsetMax = canvasView.ViewportCanvasOffsetMax;
                SizeDouble num14 = canvasView.ConvertViewportToCanvas(viewportRect).Size;
                SizeDouble num15 = new SizeDouble(Math.Max((double) 0.0, (double) (num2.Width - verticalScrollBarWidth)), Math.Max((double) 0.0, (double) (num2.Height - horizontalScrollBarHeight)));
                RectDouble num16 = new RectDouble(viewportRect.Location, num15);
                SizeDouble num18 = canvasView.ConvertViewportToCanvas(num16).Size;
                ThicknessDouble frameCanvasPadding = canvasView.FrameCanvasPadding;
                RectDouble framedCanvasBounds = canvasView.FramedCanvasBounds;
                bool flag = false;
                bool flag2 = false;
                if ((this.canvasControl == null) || (canvasView.ScaleBasis == ScaleBasis.FitToViewport))
                {
                    flag = false;
                    flag2 = false;
                }
                else
                {
                    if (framedCanvasBounds.Width > num14.Width)
                    {
                        flag = true;
                        if (framedCanvasBounds.Height > num18.Height)
                        {
                            flag2 = true;
                        }
                    }
                    if (framedCanvasBounds.Height > num14.Height)
                    {
                        flag2 = true;
                        if (framedCanvasBounds.Width > num18.Width)
                        {
                            flag = true;
                        }
                    }
                }
                int num21 = size.Width - (flag2 ? verticalScrollBarWidth : 0);
                int width = Math.Max(0, num21);
                int num23 = size.Height - (flag ? horizontalScrollBarHeight : 0);
                int height = Math.Max(0, num23);
                Rectangle rectangle2 = new Rectangle(0, 0, width, height);
                double scaleRatio = canvasView.ScaleRatio;
                this.canvasControl.Bounds = rectangle2;
                this.canvasControl.PerformLayout();
                canvasView.CoerceValue(PaintDotNet.Canvas.CanvasView.ScaleRatioProperty);
                if ((canvasView.ScaleRatio != scaleRatio) || (canvasView.ViewportSize != viewportSize))
                {
                    return false;
                }
                if (flag)
                {
                    Rectangle newBounds = new Rectangle(0, size.Height - horizontalScrollBarHeight, size.Width - (flag2 ? verticalScrollBarWidth : 0), horizontalScrollBarHeight);
                    int min = this.ConvertToScrollBar(viewportCanvasOffsetMin.X);
                    int max = this.ConvertToScrollBar(viewportCanvasOffsetMax.X + num9.Width);
                    int newLargeChange = this.ConvertToScrollBar(num9.Width);
                    int newSmallChange = this.ConvertToScrollBar(num9.Width / 10.0);
                    int newValue = Int32Util.Clamp(this.ConvertToScrollBar(viewportCanvasOffset.X), min, max);
                    UpdateScrollBar(this.hScrollBar, newBounds, min, max, newLargeChange, newSmallChange, newValue);
                }
                if (flag2)
                {
                    Rectangle rectangle4 = new Rectangle(size.Width - verticalScrollBarWidth, 0, verticalScrollBarWidth, size.Height - (flag ? horizontalScrollBarHeight : 0));
                    int num31 = this.ConvertToScrollBar(viewportCanvasOffsetMin.Y);
                    int num32 = this.ConvertToScrollBar(viewportCanvasOffsetMax.Y + num9.Height);
                    int num33 = this.ConvertToScrollBar(num9.Height);
                    int num34 = this.ConvertToScrollBar(num9.Height / 10.0);
                    int num35 = Int32Util.Clamp(this.ConvertToScrollBar(viewportCanvasOffset.Y), num31, num32);
                    UpdateScrollBar(this.vScrollBar, rectangle4, num31, num32, num33, num34, num35);
                }
                this.hScrollBar.Visible = flag;
                this.vScrollBar.Visible = flag2;
            }
            return true;
        }

        protected override void OnParentVisibleChanged(EventArgs e)
        {
            this.UpdateCanvasVisibility();
            base.OnParentVisibleChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this.UpdateCanvasVisibility();
            base.OnVisibleChanged(e);
        }

        private void OnVScrollBarValueChanged(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                double y = this.ConvertFromScrollBar(this.vScrollBar.Value);
                double num2 = this.ConvertFromScrollBar(this.ConvertToScrollBar(this.canvasControl.CanvasView.ViewportCanvasOffset.Y));
                if (y != num2)
                {
                    this.canvasControl.CanvasView.ViewportCanvasOffset = new PointDouble(this.canvasControl.CanvasView.ViewportCanvasOffset.X, y);
                }
            }
        }

        public void UpdateCanvasVisibility()
        {
            this.CanvasView.IsVisible = ((base.IsHandleCreated && base.Visible) && (base.Parent != null)) && base.Parent.Visible;
        }

        private static void UpdateScrollBar(ScrollBar scrollBar, Rectangle newBounds, int newMinimum, int newMaximum, int newLargeChange, int newSmallChange, int newValue)
        {
            Rectangle bounds = scrollBar.Bounds;
            int minimum = scrollBar.Minimum;
            int maximum = scrollBar.Maximum;
            int largeChange = scrollBar.LargeChange;
            int smallChange = scrollBar.SmallChange;
            int num5 = scrollBar.Value;
            scrollBar.Bounds = newBounds;
            bool flag = scrollBar.Visible && (((newMinimum != minimum) || (newMaximum != maximum)) || (newValue != num5));
            if (flag)
            {
                UIUtil.SuspendControlPainting(scrollBar);
            }
            scrollBar.Minimum = newMinimum;
            scrollBar.Maximum = newMaximum;
            scrollBar.LargeChange = newLargeChange;
            scrollBar.SmallChange = newSmallChange;
            scrollBar.Value = newValue;
            if (flag)
            {
                UIUtil.ResumeControlPainting(scrollBar);
                scrollBar.Invalidate();
            }
        }

        private void VerifyAccess()
        {
            this.VerifyThreadAccess();
        }

        public PaintDotNet.Canvas.Canvas Canvas
        {
            get => 
                this.canvasControl.Canvas;
            set
            {
                this.canvasControl.Canvas = value;
            }
        }

        public PaintDotNet.Canvas.CanvasControl CanvasControl =>
            this.canvasControl;

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.canvasControl.CanvasView;

        bool IIsDisposed.IsDisposed =>
            base.IsDisposed;
    }
}

