namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class DocumentView : UserControl2
    {
        private PaintDotNet.Document document;
        private PaintDotNet.Canvas.DocumentCanvas documentCanvas = new PaintDotNet.Canvas.DocumentCanvas();
        private Action forceMouseMoveImpl;
        private bool hookedMouseEvents;
        private Ruler leftRuler;
        private FormWindowState oldWindowState = FormWindowState.Minimized;
        private MouseMovePoint? previousMovePoint;
        private bool raiseFirstInputAfterGotFocus;
        private int refreshSuspended;
        private bool rulersEnabled = true;
        private ScrollableCanvasControl scrollableCanvasControl;
        private Ruler topRuler;

        [field: CompilerGenerated]
        public event EventHandler CanvasLayout;

        [field: CompilerGenerated]
        public event EventHandler CompositionUpdated;

        [field: CompilerGenerated]
        public event EventHandler DocumentChanged;

        [field: CompilerGenerated]
        public event ValueEventHandler<PaintDotNet.Document> DocumentChanging;

        [field: CompilerGenerated]
        public event EventHandler DocumentClick;

        [field: CompilerGenerated]
        public event EventHandler DocumentEnter;

        [field: CompilerGenerated]
        public event KeyEventHandler DocumentKeyDown;

        [field: CompilerGenerated]
        public event KeyPressEventHandler DocumentKeyPress;

        [field: CompilerGenerated]
        public event KeyEventHandler DocumentKeyUp;

        [field: CompilerGenerated]
        public event EventHandler DocumentLeave;

        [field: CompilerGenerated]
        public event EventHandler<MouseEventArgsF> DocumentMouseDown;

        [field: CompilerGenerated]
        public event EventHandler DocumentMouseEnter;

        [field: CompilerGenerated]
        public event EventHandler DocumentMouseLeave;

        [field: CompilerGenerated]
        public event EventHandler<MouseEventArgsF> DocumentMouseMove;

        [field: CompilerGenerated]
        public event EventHandler<MouseEventArgsF> DocumentMouseUp;

        [field: CompilerGenerated]
        public event EventHandler DrawGridChanged;

        [field: CompilerGenerated]
        public event EventHandler FirstInputAfterGotFocus;

        [field: CompilerGenerated]
        public event EventHandler RulersEnabledChanged;

        [field: CompilerGenerated]
        public event EventHandler ScaleFactorChanged;

        public DocumentView()
        {
            this.InitializeComponent();
            this.documentCanvas.CompositionIdle += new EventHandler(this.OnDocumentCanvasCompositionIdle);
            this.scrollableCanvasControl.Canvas = this.documentCanvas;
            this.scrollableCanvasControl.CanvasView.ViewportCanvasOffsetChanged += new ValueChangedEventHandler<PointDouble>(this.OnViewportCanvasOffsetChanged);
            this.scrollableCanvasControl.CanvasView.ScaleRatioChanged += new ValueChangedEventHandler<double>(this.OnScaleRatioChanged);
            PixelGridCanvasLayer.AddIsPixelGridEnabledChangedHandler(this.scrollableCanvasControl.CanvasView, new ValueChangedEventHandler<bool>(this.OnCanvasViewIsPixelGridEnabledChanged));
            this.document = null;
        }

        private void CheckForFirstInputAfterGotFocus()
        {
            if (this.raiseFirstInputAfterGotFocus)
            {
                this.raiseFirstInputAfterGotFocus = false;
                this.OnFirstInputAfterGotFocus();
            }
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            this.OnDocumentClick();
        }

        public PointDouble ClientToDocument(PointDouble clientPt)
        {
            PointDouble screenPt = this.PointToScreen(clientPt);
            PointDouble viewportPt = this.scrollableCanvasControl.CanvasControl.PointToClient(screenPt);
            PointDouble extentPt = this.CanvasView.ConvertViewportToExtent(viewportPt);
            return this.CanvasView.ConvertExtentToCanvas(extentPt);
        }

        public PointDouble ClientToDocument(PointInt32 clientPt)
        {
            PointInt32 screenPt = this.PointToScreen(clientPt);
            PointInt32 viewportPt = this.scrollableCanvasControl.CanvasControl.PointToClient(screenPt);
            PointDouble extentPt = this.CanvasView.ConvertViewportToExtent(viewportPt);
            return this.CanvasView.ConvertExtentToCanvas(extentPt);
        }

        public RectDouble ClientToDocument(RectInt32 clientRect)
        {
            RectInt32 screenRect = this.RectangleToScreen(clientRect);
            RectInt32 viewportRect = this.scrollableCanvasControl.CanvasControl.RectangleToClient(screenRect);
            RectDouble extentRect = this.CanvasView.ConvertViewportToExtent(viewportRect);
            return this.CanvasView.ConvertExtentToCanvas(extentRect);
        }

        private static IList<PointInt32> ConvertIntermediateScreenPointsToClient(Control target, IList<MouseMovePoint> intermediateScreenPoints) => 
            intermediateScreenPoints.Select<MouseMovePoint, PointInt32>(pt => target.PointToClient(new Point(pt.X, pt.Y)).ToPointInt32());

        private IList<PointDouble> ConvertIntermediateScreenPointsToDocument(Control source, IList<MouseMovePoint> intermediateScreenPoints) => 
            intermediateScreenPoints.Select<MouseMovePoint, PointInt32>(pt => source.PointToClient(new Point(pt.X, pt.Y)).ToPointInt32()).Select<PointInt32, PointDouble>(pt => this.MouseToDocument(source, pt));

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.scrollableCanvasControl != null)
                {
                    this.scrollableCanvasControl.CanvasView.ViewportCanvasOffsetChanged -= new ValueChangedEventHandler<PointDouble>(this.OnViewportCanvasOffsetChanged);
                    this.scrollableCanvasControl.CanvasView.ScaleRatioChanged -= new ValueChangedEventHandler<double>(this.OnScaleRatioChanged);
                    PixelGridCanvasLayer.RemoveIsPixelGridEnabledChangedHandler(this.scrollableCanvasControl.CanvasView, new ValueChangedEventHandler<bool>(this.OnCanvasViewIsPixelGridEnabledChanged));
                    this.scrollableCanvasControl.Canvas = null;
                }
                if (this.documentCanvas != null)
                {
                    this.documentCanvas.CompositionIdle -= new EventHandler(this.OnDocumentCanvasCompositionIdle);
                    this.scrollableCanvasControl.Canvas = null;
                }
                DisposableUtil.Free<PaintDotNet.Canvas.DocumentCanvas>(ref this.documentCanvas);
            }
            base.Dispose(disposing);
        }

        private void DocumentInvalidated(object sender, RectInt32InvalidatedEventArgs e)
        {
        }

        private void DocumentMetadataChangedHandler(object sender, EventArgs e)
        {
            if (this.document != null)
            {
                this.leftRuler.Dpu = 1.0 / this.document.PixelToPhysicalY(1.0, this.leftRuler.MeasurementUnit);
                this.topRuler.Dpu = 1.0 / this.document.PixelToPhysicalY(1.0, this.topRuler.MeasurementUnit);
            }
        }

        private void DocumentSetImpl(PaintDotNet.Document value)
        {
            PointDouble documentScrollPosition = this.DocumentScrollPosition;
            this.OnDocumentChanging(value);
            this.SuspendRefresh();
            try
            {
                if (this.document != null)
                {
                    this.document.Metadata.Changed -= new EventHandler(this.DocumentMetadataChangedHandler);
                    this.document.Invalidated -= new EventHandler<RectInt32InvalidatedEventArgs>(this.DocumentInvalidated);
                }
                this.document = value;
                this.documentCanvas.Document = value;
                if (this.document != null)
                {
                    this.document.Metadata.Changed += new EventHandler(this.DocumentMetadataChangedHandler);
                    this.document.Invalidated += new EventHandler<RectInt32InvalidatedEventArgs>(this.DocumentInvalidated);
                }
                base.Invalidate(true);
                this.DocumentMetadataChangedHandler(this, EventArgs.Empty);
                this.OnResize(EventArgs.Empty);
                this.OnDocumentChanged();
            }
            finally
            {
                this.ResumeRefresh();
            }
            this.DocumentScrollPosition = documentScrollPosition;
            this.RaiseCanvasLayout();
        }

        public PointDouble DocumentToClient(PointDouble documentPt)
        {
            PointDouble extentPt = this.CanvasView.ConvertCanvasToExtent(documentPt);
            PointDouble clientPt = this.CanvasView.ConvertExtentToViewport(extentPt);
            PointDouble screenPt = this.scrollableCanvasControl.CanvasControl.PointToScreen(clientPt);
            return this.PointToClient(screenPt);
        }

        public RectDouble DocumentToClient(RectDouble documentRect)
        {
            RectDouble extentRect = this.CanvasView.ConvertCanvasToExtent(documentRect);
            RectDouble clientRect = this.CanvasView.ConvertExtentToViewport(extentRect);
            RectDouble screenRect = this.scrollableCanvasControl.CanvasControl.RectangleToScreen(clientRect);
            return this.RectangleToClient(screenRect);
        }

        protected void ForceMouseMoveAsync()
        {
            if (this.forceMouseMoveImpl == null)
            {
                this.forceMouseMoveImpl = new Action(this.ForceMouseMoveImpl);
            }
            PdnSynchronizationContext.Instance.EnsurePosted(this.forceMouseMoveImpl);
        }

        private void ForceMouseMoveImpl()
        {
            if (!base.IsDisposed && base.IsHandleCreated)
            {
                MouseButtons mouseButtons = Control.MouseButtons;
                Point mousePosition = Control.MousePosition;
                PointDouble docPoint = this.ScreenToDocument(mousePosition.ToPointDouble());
                PointDouble[] intermediatePoints = new PointDouble[] { docPoint };
                this.MouseMoveHandlerImpl(docPoint, mouseButtons, 0, 0, intermediatePoints);
                base.Update();
            }
        }

        private static IList<MouseMovePoint> GetIntermediateScreenPoints(MouseMovePoint? previousMovePoint, MouseMovePoint latestMovePoint)
        {
            List<MouseMovePoint> list = new List<MouseMovePoint>();
            if (previousMovePoint.HasValue)
            {
                IList<MouseMovePoint> collection = UIUtil.TryGetMouseMoveScreenPoints(previousMovePoint.Value, latestMovePoint);
                if (collection != null)
                {
                    list.AddRange(collection);
                }
            }
            if (list.Count == 0)
            {
                list.Add(latestMovePoint);
            }
            return list;
        }

        private void HookMouseEvents(Control c)
        {
            if (!(c is ScrollBar) && !(c is ScrollableCanvasControl))
            {
                c.MouseEnter += new EventHandler(this.MouseEnterHandler);
                c.MouseLeave += new EventHandler(this.MouseLeaveHandler);
                c.MouseUp += new MouseEventHandler(this.MouseUpHandler);
                c.MouseMove += new MouseEventHandler(this.MouseMoveHandler);
                c.MouseDown += new MouseEventHandler(this.MouseDownHandler);
                c.Click += new EventHandler(this.ClickHandler);
            }
            foreach (Control control in c.Controls)
            {
                this.HookMouseEvents(control);
            }
        }

        private void InitializeComponent()
        {
            this.topRuler = new Ruler();
            this.leftRuler = new Ruler();
            this.scrollableCanvasControl = new ScrollableCanvasControl();
            base.SuspendLayout();
            this.topRuler.Dock = DockStyle.Top;
            this.topRuler.Location = new Point(0, 0);
            this.topRuler.Name = "topRuler";
            this.topRuler.Size = UIUtil.ScaleSize(new Size(0x180, 0x11));
            this.topRuler.TabIndex = 3;
            this.topRuler.MouseWheel += (s, e) => this.PerformMouseWheel(this.topRuler, e);
            this.leftRuler.Dock = DockStyle.Left;
            this.leftRuler.Location = new Point(0, 0x11);
            this.leftRuler.Name = "leftRuler";
            this.leftRuler.Orientation = Orientation.Vertical;
            this.leftRuler.Size = UIUtil.ScaleSize(new Size(0x11, 0x130));
            this.leftRuler.TabIndex = 4;
            this.leftRuler.MouseWheel += (s, e) => this.PerformMouseWheel(this.leftRuler, e);
            this.scrollableCanvasControl.Name = "scrollableCanvasControl";
            this.scrollableCanvasControl.Dock = DockStyle.Fill;
            this.scrollableCanvasControl.CanvasControl.KeyDown += new KeyEventHandler(this.OnCanvasViewKeyDown);
            this.scrollableCanvasControl.CanvasControl.KeyUp += new KeyEventHandler(this.OnCanvasViewKeyUp);
            this.scrollableCanvasControl.CanvasControl.KeyPress += new KeyPressEventHandler(this.OnCanvasViewKeyPress);
            this.scrollableCanvasControl.CanvasControl.GotFocus += new EventHandler(this.OnCanvasViewGotFocus);
            this.scrollableCanvasControl.CanvasControl.LostFocus += new EventHandler(this.OnCanvasViewLostFocus);
            base.Controls.Add(this.scrollableCanvasControl);
            base.Controls.Add(this.leftRuler);
            base.Controls.Add(this.topRuler);
            base.Name = "DocumentView";
            base.Size = new Size(0x180, 320);
            base.ResumeLayout(false);
        }

        public override bool IsMouseCaptured()
        {
            if ((!base.Capture && !this.scrollableCanvasControl.Capture) && (!this.scrollableCanvasControl.CanvasControl.Capture && !this.leftRuler.Capture))
            {
                return this.topRuler.Capture;
            }
            return true;
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is Ruler))
            {
                PointDouble num = this.MouseToDocument((Control) sender, new PointInt32(e.X, e.Y));
                this.OnDocumentMouseDown(new MouseEventArgsF(e.Button, e.Clicks, num.X, num.Y, e.Delta));
            }
        }

        private void MouseEnterHandler(object sender, EventArgs e)
        {
            this.OnDocumentMouseEnter(EventArgs.Empty);
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            this.OnDocumentMouseLeave(EventArgs.Empty);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            Control source = (Control) sender;
            MouseButtons button = e.Button;
            int clicks = e.Clicks;
            int delta = e.Delta;
            Point location = e.Location;
            Point point2 = source.PointToScreen(location);
            int inputMessageTime = UIUtil.GetInputMessageTime();
            MouseMovePoint latestMovePoint = new MouseMovePoint(point2.X, point2.Y, inputMessageTime);
            IList<MouseMovePoint> intermediateScreenPoints = GetIntermediateScreenPoints(this.previousMovePoint, latestMovePoint);
            IList<PointDouble> intermediatePoints = this.ConvertIntermediateScreenPointsToDocument(source, intermediateScreenPoints);
            this.previousMovePoint = new MouseMovePoint?(latestMovePoint);
            this.MouseMoveHandlerImpl(intermediatePoints.Last<PointDouble>(), button, clicks, delta, intermediatePoints);
        }

        private void MouseMoveHandlerImpl(PointDouble docPoint, MouseButtons button, int clicks, int delta, IEnumerable<PointDouble> intermediatePoints)
        {
            if (this.RulersEnabled)
            {
                int num = (docPoint.X > 0.0) ? ((int) Math.Truncate(docPoint.X)) : ((docPoint.X < 0.0) ? ((int) Math.Truncate((double) (docPoint.X - 1.0))) : 0);
                int num2 = (docPoint.Y > 0.0) ? ((int) Math.Truncate(docPoint.Y)) : ((docPoint.Y < 0.0) ? ((int) Math.Truncate((double) (docPoint.Y - 1.0))) : 0);
                this.topRuler.Value = num;
                this.leftRuler.Value = num2;
                this.UpdateRulerOffsets();
            }
            MouseEventArgsF e = new MouseEventArgsF(button, clicks, docPoint.X, docPoint.Y, delta, intermediatePoints);
            this.OnDocumentMouseMove(e);
        }

        public PointDouble MouseToDocument(Control sender, PointInt32 mouse)
        {
            Point p = sender.PointToScreen(mouse.ToGdipPoint());
            Point pt = this.scrollableCanvasControl.CanvasControl.PointToClient(p);
            PointDouble extentPt = this.scrollableCanvasControl.CanvasView.ConvertViewportToExtent(pt.ToPointDouble());
            return this.scrollableCanvasControl.CanvasView.ConvertExtentToCanvas(extentPt);
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is Ruler))
            {
                PointDouble num = this.MouseToDocument((Control) sender, new PointInt32(e.X, e.Y));
                this.OnDocumentMouseUp(new MouseEventArgsF(e.Button, e.Clicks, num.X, num.Y, e.Delta));
            }
        }

        private void OnCanvasViewGotFocus(object sender, EventArgs e)
        {
            this.raiseFirstInputAfterGotFocus = true;
        }

        private void OnCanvasViewIsPixelGridEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            this.OnDrawGridChanged();
        }

        private void OnCanvasViewKeyDown(object sender, KeyEventArgs e)
        {
            this.OnDocumentKeyDown(e);
        }

        private void OnCanvasViewKeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnDocumentKeyPress(e);
        }

        private void OnCanvasViewKeyUp(object sender, KeyEventArgs e)
        {
            this.OnDocumentKeyUp(e);
        }

        private void OnCanvasViewLostFocus(object sender, EventArgs e)
        {
            this.raiseFirstInputAfterGotFocus = false;
        }

        protected virtual void OnCompositionUpdated()
        {
            this.CompositionUpdated.Raise(this);
        }

        private void OnDocumentCanvasCompositionIdle(object sender, EventArgs e)
        {
            this.OnCompositionUpdated();
        }

        protected virtual void OnDocumentChanged()
        {
            this.DocumentChanged.Raise(this);
        }

        protected virtual void OnDocumentChanging(PaintDotNet.Document newDocument)
        {
            this.DocumentChanging.Raise<PaintDotNet.Document>(this, newDocument);
        }

        protected void OnDocumentClick()
        {
            this.CheckForFirstInputAfterGotFocus();
            this.DocumentClick.Raise(this);
        }

        protected void OnDocumentEnter(EventArgs e)
        {
            this.DocumentEnter.Raise(this);
        }

        protected void OnDocumentKeyDown(KeyEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyDown != null)
            {
                this.DocumentKeyDown(this, e);
            }
        }

        protected void OnDocumentKeyPress(KeyPressEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyPress != null)
            {
                this.DocumentKeyPress(this, e);
            }
        }

        protected void OnDocumentKeyUp(KeyEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyUp != null)
            {
                this.DocumentKeyUp(this, e);
            }
        }

        protected void OnDocumentLeave(EventArgs e)
        {
            this.DocumentLeave.Raise(this);
        }

        protected virtual void OnDocumentMouseDown(MouseEventArgsF e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentMouseDown != null)
            {
                this.DocumentMouseDown(this, e);
            }
        }

        protected virtual void OnDocumentMouseEnter(EventArgs e)
        {
            if (this.DocumentMouseEnter != null)
            {
                this.DocumentMouseEnter(this, e);
            }
        }

        protected virtual void OnDocumentMouseLeave(EventArgs e)
        {
            if (this.DocumentMouseLeave != null)
            {
                this.DocumentMouseLeave(this, e);
            }
        }

        protected virtual void OnDocumentMouseMove(MouseEventArgsF e)
        {
            if (this.DocumentMouseMove != null)
            {
                this.DocumentMouseMove(this, e);
            }
        }

        protected virtual void OnDocumentMouseUp(MouseEventArgsF e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentMouseUp != null)
            {
                this.DocumentMouseUp(this, e);
            }
        }

        protected virtual void OnDrawGridChanged()
        {
            this.DrawGridChanged.Raise(this);
        }

        protected override void OnEnter(EventArgs e)
        {
            this.OnDocumentEnter(e);
            base.OnEnter(e);
        }

        protected virtual void OnFirstInputAfterGotFocus()
        {
            this.FirstInputAfterGotFocus.Raise(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.UpdateCanvasViewVisibility();
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.UpdateCanvasViewVisibility();
            base.OnHandleDestroyed(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.UpdateRulerOffsets();
            base.OnLayout(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            this.OnDocumentLeave(e);
            base.OnLeave(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.hookedMouseEvents)
            {
                this.hookedMouseEvents = true;
                foreach (Control control in base.Controls)
                {
                    this.HookMouseEvents(control);
                }
            }
            this.scrollableCanvasControl.CanvasControl.Select();
        }

        protected override void OnParentVisibleChanged(EventArgs e)
        {
            this.UpdateCanvasViewVisibility();
            base.OnParentVisibleChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Form parentForm = base.ParentForm;
            if (parentForm != null)
            {
                if (parentForm.WindowState != this.oldWindowState)
                {
                    base.PerformLayout();
                }
                this.oldWindowState = parentForm.WindowState;
            }
            base.OnResize(e);
            this.UpdateRulerOffsets();
            this.RaiseCanvasLayout();
        }

        protected void OnRulersEnabledChanged()
        {
            this.RulersEnabledChanged.Raise(this);
        }

        protected virtual void OnScaleFactorChanged()
        {
            this.ScaleFactorChanged.Raise(this);
        }

        private void OnScaleRatioChanged(object sender, ValueChangedEventArgs<double> e)
        {
            PaintDotNet.ScaleFactor factor = PaintDotNet.ScaleFactor.FromRatio(this.scrollableCanvasControl.CanvasView.ScaleRatio);
            this.topRuler.ScaleFactor = factor;
            this.leftRuler.ScaleFactor = factor;
            this.UpdateRulerOffsets();
            this.OnScaleFactorChanged();
            this.OnScroll(null);
            this.scrollableCanvasControl.QueueUpdate();
        }

        protected virtual void OnUnitsChanged()
        {
        }

        protected virtual void OnUnitsChanging()
        {
        }

        private void OnViewportCanvasOffsetChanged(object sender, ValueChangedEventArgs<PointDouble> e)
        {
            this.ForceMouseMoveAsync();
            this.UpdateRulerOffsets();
            this.OnScroll(null);
            this.RaiseCanvasLayout();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this.UpdateCanvasViewVisibility();
            base.OnVisibleChanged(e);
        }

        public void PerformDocumentMouseDown(MouseEventArgsF e)
        {
            this.OnDocumentMouseDown(e);
        }

        public void PerformDocumentMouseMove(MouseEventArgsF e)
        {
            this.OnDocumentMouseMove(e);
        }

        public void PerformDocumentMouseUp(MouseEventArgsF e)
        {
            this.OnDocumentMouseUp(e);
        }

        public void PerformMouseWheel(Control sender, MouseEventArgs e)
        {
            this.scrollableCanvasControl.CanvasControl.ProcessMouseWheel(this, e);
        }

        public void PopCacheStandby()
        {
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if ((keyData.IsArrowKey() || (keys == Keys.Delete)) || (keys == Keys.Tab))
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                if (msg.Msg == 0x100)
                {
                    if (base.ContainsFocus)
                    {
                        this.OnDocumentKeyDown(e);
                        if (keyData.IsArrowKey())
                        {
                            e.Handled = true;
                        }
                    }
                    if (e.Handled)
                    {
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void PushCacheStandby()
        {
        }

        protected void RaiseCanvasLayout()
        {
            this.VerifyThreadAccess();
            this.CanvasLayout.Raise(this);
        }

        public void ResumeRefresh()
        {
            this.refreshSuspended--;
        }

        public PointDouble ScreenToDocument(PointDouble screenPt)
        {
            PointDouble viewportPt = this.scrollableCanvasControl.CanvasControl.PointToClient(screenPt);
            PointDouble extentPt = this.CanvasView.ConvertViewportToExtent(viewportPt);
            return this.CanvasView.ConvertExtentToCanvas(extentPt);
        }

        public void SetHighlightRectangle(RectDouble rectF)
        {
            if ((rectF.Width == 0.0) || (rectF.Height == 0.0))
            {
                this.leftRuler.HighlightEnabled = false;
                this.topRuler.HighlightEnabled = false;
            }
            else
            {
                if (this.topRuler != null)
                {
                    this.topRuler.HighlightEnabled = true;
                    this.topRuler.HighlightStart = rectF.Left;
                    this.topRuler.HighlightLength = rectF.Width;
                }
                if (this.leftRuler != null)
                {
                    this.leftRuler.HighlightEnabled = true;
                    this.leftRuler.HighlightStart = rectF.Top;
                    this.leftRuler.HighlightLength = rectF.Height;
                }
            }
        }

        public void SuspendRefresh()
        {
            this.refreshSuspended++;
        }

        private void UpdateCanvasViewVisibility()
        {
            this.scrollableCanvasControl.UpdateCanvasVisibility();
        }

        private void UpdateRulerOffsets()
        {
            if (base.IsHandleCreated)
            {
                PointInt32 screenPt = this.scrollableCanvasControl.CanvasControl.PointToScreen(PointInt32.Zero);
                PointDouble num2 = this.ScreenToDocument(screenPt);
                this.topRuler.Offset = -num2.X;
                if (this.topRuler.Visible)
                {
                    this.topRuler.QueueUpdate();
                }
                this.leftRuler.Offset = -num2.Y;
                if (this.leftRuler.Visible)
                {
                    this.leftRuler.QueueUpdate();
                }
            }
        }

        public void ZoomIn()
        {
            this.ZoomViaCurrentMousePosition(this.ScaleFactor.GetNextLarger());
        }

        public void ZoomIn(double factor)
        {
            this.ZoomViaCurrentMousePosition(PaintDotNet.ScaleFactor.FromRatio(this.ScaleFactor.Ratio * factor));
        }

        public void ZoomOut()
        {
            this.ZoomViaCurrentMousePosition(this.ScaleFactor.GetNextSmaller());
        }

        public void ZoomOut(double factor)
        {
            this.ZoomViaCurrentMousePosition(PaintDotNet.ScaleFactor.FromRatio(this.ScaleFactor.Ratio / factor));
        }

        public void ZoomTo(PaintDotNet.ScaleFactor newScaleFactor)
        {
            this.ZoomViaCurrentMousePosition(newScaleFactor);
        }

        protected virtual void ZoomToWithCentering(PaintDotNet.ScaleFactor newScaleFactor, Func<PointDouble> anchorPtCanvasFn)
        {
            PaintDotNet.ScaleFactor scaleFactor = this.ScaleFactor;
            PointDouble num = anchorPtCanvasFn();
            this.ScaleFactor = newScaleFactor;
            VectorDouble num3 = anchorPtCanvasFn() - num;
            PaintDotNet.Canvas.CanvasView canvasView = this.CanvasView;
            canvasView.ViewportCanvasOffset -= num3;
        }

        protected void ZoomViaCurrentMousePosition(PaintDotNet.ScaleFactor newScaleFactor)
        {
            PointDouble center;
            PointInt32 pt = Control.MousePosition.ToPointInt32();
            RectDouble visibleCanvasBounds = this.CanvasView.GetVisibleCanvasBounds();
            RectDouble num3 = this.RectangleToScreen(this.DocumentToClient(visibleCanvasBounds));
            if (num3.Contains(pt))
            {
                double x = DoubleUtil.Clamp((double) pt.X, num3.Left, num3.Right);
                center = new PointDouble(x, DoubleUtil.Clamp((double) pt.Y, num3.Top, num3.Bottom));
            }
            else
            {
                center = num3.Center;
            }
            PointDouble clientPt = this.PointToClient(center);
            PointDouble anchorPtViewport = this.CanvasView.ConvertCanvasToViewport(this.ClientToDocument(clientPt));
            this.ZoomToWithCentering(newScaleFactor, () => this.CanvasView.ConvertViewportToCanvas(anchorPtViewport));
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.scrollableCanvasControl.CanvasView;

        public RectInt32 ClientRectangleMax =>
            base.RectangleToClient(this.scrollableCanvasControl.RectangleToScreen(this.scrollableCanvasControl.Bounds)).ToRectInt32();

        public RectInt32 ClientRectangleMin
        {
            get
            {
                RectInt32 clientRectangleMax = this.ClientRectangleMax;
                clientRectangleMax.Width -= SystemInformation.VerticalScrollBarWidth;
                clientRectangleMax.Height -= SystemInformation.HorizontalScrollBarHeight;
                return clientRectangleMax;
            }
        }

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                if (base.InvokeRequired)
                {
                    object[] args = new object[] { value };
                    base.Invoke(new Action<PaintDotNet.Document>(this.DocumentSetImpl), args);
                }
                else
                {
                    this.DocumentSetImpl(value);
                }
            }
        }

        public PaintDotNet.Canvas.DocumentCanvas DocumentCanvas =>
            this.documentCanvas;

        public PointDouble DocumentCenterPoint
        {
            get => 
                this.CanvasView.ViewportCanvasBounds.Center;
            set
            {
                SizeDouble size = this.CanvasView.ViewportCanvasBounds.Size;
                PointDouble num2 = new PointDouble(value.X - (size.Width / 2.0), value.Y - (size.Height / 2.0));
                this.DocumentScrollPosition = num2;
            }
        }

        public PointDouble DocumentScrollPosition
        {
            get => 
                this.scrollableCanvasControl.CanvasView.ViewportCanvasOffset;
            set
            {
                this.scrollableCanvasControl.CanvasView.ViewportCanvasOffset = value;
            }
        }

        public bool DrawGrid
        {
            get => 
                PixelGridCanvasLayer.GetIsPixelGridEnabled(this.scrollableCanvasControl.CanvasView);
            set
            {
                PixelGridCanvasLayer.SetIsPixelGridEnabled(this.scrollableCanvasControl.CanvasView, value);
            }
        }

        public override bool Focused
        {
            get
            {
                if ((!base.Focused && !this.scrollableCanvasControl.Focused) && (!this.scrollableCanvasControl.CanvasControl.Focused && !this.leftRuler.Focused))
                {
                    return this.topRuler.Focused;
                }
                return true;
            }
        }

        public PaintDotNet.ScaleFactor MaxScaleFactor =>
            PaintDotNet.ScaleFactor.FromRatio(32.0);

        public PaintDotNet.ScaleFactor MinScaleFactor =>
            PaintDotNet.ScaleFactor.FromRatio(0.01);

        public bool RulersEnabled
        {
            get => 
                this.rulersEnabled;
            set
            {
                if (this.rulersEnabled != value)
                {
                    this.rulersEnabled = value;
                    if (this.topRuler != null)
                    {
                        this.topRuler.Enabled = value;
                        this.topRuler.Visible = value;
                    }
                    if (this.leftRuler != null)
                    {
                        this.leftRuler.Enabled = value;
                        this.leftRuler.Visible = value;
                    }
                    base.PerformLayout();
                    this.OnRulersEnabledChanged();
                    this.RaiseCanvasLayout();
                }
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                PaintDotNet.ScaleFactor.FromRatio(this.CanvasView.ScaleRatio);
            set
            {
                PointDouble documentCenterPoint = this.DocumentCenterPoint;
                this.CanvasView.ScaleRatio = value.Ratio;
                this.DocumentCenterPoint = documentCenterPoint;
                this.RaiseCanvasLayout();
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.leftRuler.MeasurementUnit;
            set
            {
                this.OnUnitsChanging();
                this.leftRuler.MeasurementUnit = value;
                this.topRuler.MeasurementUnit = value;
                this.DocumentMetadataChangedHandler(this, EventArgs.Empty);
                this.OnUnitsChanged();
            }
        }

        public RectDouble VisibleDocumentBounds
        {
            get
            {
                RectDouble clientRect = this.DocumentToClient(this.VisibleDocumentRect);
                return this.RectangleToScreen(clientRect);
            }
        }

        public RectDouble VisibleDocumentRect =>
            this.scrollableCanvasControl.CanvasView.GetVisibleCanvasBounds();

        public RectInt32 VisibleViewRect
        {
            get
            {
                Rectangle clientRectangle = this.scrollableCanvasControl.CanvasControl.ClientRectangle;
                Rectangle r = this.scrollableCanvasControl.CanvasControl.RectangleToScreen(clientRectangle);
                return base.RectangleToClient(r).ToRectInt32();
            }
        }
    }
}

