namespace PaintDotNet.Dialogs
{
    using Microsoft.Win32;
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Snap;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class FloatingToolForm : PdnBaseFormInternal, ISnapObstacleHost, IThreadAffinitizedObject
    {
        private IContainer components;
        private ControlEventHandler controlAddedDelegate;
        private ControlEventHandler controlRemovedDelegate;
        private bool forceOpaque;
        private HashSet<Control> hookedControls = new HashSet<Control>();
        private bool isMouseInClientArea;
        private bool isMouseInNonClientArea;
        private bool isMoving;
        private bool isReevaluateOpacityQueued;
        private KeyEventHandler keyUpDelegate;
        private EventHandler mouseEnterDelegate;
        private EventHandler mouseLeaveDelegate;
        private Size movingCursorDelta = Size.Empty;
        private AnimatedDouble opacityVariable;
        private const double opaqueAnimationTime = 0.15;
        private const float opaqueOpacity = 1f;
        private bool relinquishFocusOnClick = true;
        private bool relinquishFocusOnResizeEnd = true;
        private bool shouldBeOpaque = true;
        private SnapObstacleController snapObstacle;
        private const double translucentAnimationTime = 0.5;
        private const float translucentOpacity = 0.75f;

        [field: CompilerGenerated]
        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        [field: CompilerGenerated]
        public event EventHandler RelinquishFocus;

        public FloatingToolForm()
        {
            base.KeyPreview = true;
            this.controlAddedDelegate = new ControlEventHandler(this.ControlAddedHandler);
            this.controlRemovedDelegate = new ControlEventHandler(this.ControlRemovedHandler);
            this.keyUpDelegate = new KeyEventHandler(this.KeyUpHandler);
            this.mouseEnterDelegate = new EventHandler(this.MouseEnterHandler);
            this.mouseLeaveDelegate = new EventHandler(this.MouseLeaveHandler);
            this.InitializeComponent();
            try
            {
                SystemEvents.SessionSwitch += new SessionSwitchEventHandler(this.OnSystemEventsSessionSwitch);
                SystemEvents.DisplaySettingsChanged += new EventHandler(this.OnSystemEventsDisplaySettingsChanged);
            }
            catch (Exception)
            {
            }
            base.ControlAdded += this.controlAddedDelegate;
            base.ControlRemoved += this.controlRemovedDelegate;
            this.opacityVariable = new AnimatedDouble(1.0);
            this.opacityVariable.ValueChanged += new ValueChangedEventHandler<double>(this.OnOpacityVariableValueChanged);
        }

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            this.HookControl(e.Control);
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            this.UnhookControl(e.Control);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
                try
                {
                    SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(this.OnSystemEventsSessionSwitch);
                    SystemEvents.DisplaySettingsChanged -= new EventHandler(this.OnSystemEventsDisplaySettingsChanged);
                }
                catch (Exception)
                {
                }
                if (this.hookedControls != null)
                {
                    this.UnhookControl(this);
                    this.hookedControls = null;
                }
                DisposableUtil.Free<AnimatedDouble>(ref this.opacityVariable);
            }
            base.Dispose(disposing);
        }

        private void EvaluateIsMouseInClientArea()
        {
            Point mousePosition = Control.MousePosition;
            bool flag = base.Bounds.Contains(mousePosition);
            this.IsMouseInClientArea = flag;
        }

        private void HookControl(Control control)
        {
            if ((this.hookedControls != null) && this.hookedControls.Add(control))
            {
                control.ControlAdded += this.controlAddedDelegate;
                control.ControlRemoved += this.controlRemovedDelegate;
                if (control != this)
                {
                    control.KeyUp += this.keyUpDelegate;
                }
                control.MouseEnter += this.mouseEnterDelegate;
                control.MouseLeave += this.mouseLeaveDelegate;
                foreach (Control control2 in control.Controls)
                {
                    this.HookControl(control2);
                }
            }
        }

        private void InitializeComponent()
        {
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x124, 0x10f);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FloatingToolForm";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.ForceActiveTitleBar = true;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnKeyUp(e);
            }
        }

        private void MouseEnterHandler(object sender, EventArgs e)
        {
            this.EvaluateIsMouseInClientArea();
            this.QueueReevaluateOpacity();
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            this.EvaluateIsMouseInClientArea();
            this.QueueReevaluateOpacity();
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.relinquishFocusOnClick)
            {
                this.OnRelinquishFocus();
            }
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.Enabled = base.Enabled;
            }
            base.OnEnabledChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.HookControl(this);
            this.EvaluateIsMouseInClientArea();
            this.QueueReevaluateOpacity();
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.UnhookControl(this);
            base.OnHandleDestroyed(e);
        }

        private void OnIsMouseInClientAreaChanged()
        {
            this.QueueReevaluateOpacity();
        }

        private void OnIsMouseInNonClientAreaChanged()
        {
            this.QueueReevaluateOpacity();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (base.IsHandleCreated && (this.snapObstacle != null))
            {
                int snapDistance = this.snapObstacle.SnapDistance;
                int num2 = base.ExtendedFramePadding.Max();
                int newSnapDistance = PaintDotNet.Snap.SnapObstacle.DefaultSnapDistance + num2;
                if (newSnapDistance != snapDistance)
                {
                    this.snapObstacle.SetSnapDistance(newSnapDistance);
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            ISnapManagerHost owner = base.Owner as ISnapManagerHost;
            if (owner != null)
            {
                owner.SnapManager.AddSnapObstacle(this);
            }
            base.OnLoad(e);
        }

        protected override void OnMove(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            base.OnMove(e);
        }

        protected override void OnMoving(MovingEventArgs mea)
        {
            ISnapManagerHost owner = base.Owner as ISnapManagerHost;
            if (owner != null)
            {
                SnapManager snapManager = owner.SnapManager;
                if (!this.isMoving)
                {
                    this.movingCursorDelta = new Size(Cursor.Position.X - mea.Rectangle.X, Cursor.Position.Y - mea.Rectangle.Y);
                    this.isMoving = true;
                }
                mea.Rectangle = new Rectangle(Cursor.Position.X - this.movingCursorDelta.Width, Cursor.Position.Y - this.movingCursorDelta.Height, mea.Rectangle.Width, mea.Rectangle.Height);
                this.snapObstacle.SetBounds(mea.Rectangle.ToRectInt32());
                PointInt32 newLocation = mea.Rectangle.Location.ToPointInt32();
                PointInt32 location = snapManager.AdjustObstacleDestination(this.SnapObstacle, newLocation);
                RectInt32 bounds = new RectInt32(location, mea.Rectangle.Size.ToSizeInt32());
                this.snapObstacle.SetBounds(bounds);
                mea.Rectangle = bounds.ToGdipRectangle();
            }
            base.OnMoving(mea);
        }

        protected override void OnNonClientMouseEnter()
        {
            this.IsMouseInNonClientArea = true;
            base.OnNonClientMouseEnter();
        }

        protected override void OnNonClientMouseLeave()
        {
            this.IsMouseInNonClientArea = false;
            base.OnNonClientMouseLeave();
        }

        private void OnOpacityVariableValueChanged(object sender, EventArgs e)
        {
            base.Opacity = (float) DoubleUtil.Clamp(this.opacityVariable.Value, 0.0, 1.0);
        }

        protected virtual void OnRelinquishFocus()
        {
            if (!MenuStripEx.IsAnyMenuActive)
            {
                this.RelinquishFocus.Raise(this);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            base.OnResize(e);
            this.UpdateParking();
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            this.isMoving = false;
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnResizeEnd(e);
            if (this.relinquishFocusOnResizeEnd)
            {
                this.OnRelinquishFocus();
            }
        }

        private void OnShouldBeOpaqueChanged()
        {
            float num;
            double num2;
            if (this.shouldBeOpaque)
            {
                num = 1f;
                num2 = 0.15;
            }
            else
            {
                num = 0.75f;
                num2 = 0.5;
            }
            this.opacityVariable.AnimateValueTo((double) num, num2, AnimationTransitionType.SmoothStop);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnSizeChanged(e);
        }

        private void OnSnapObstacleBoundsChangeRequested(object sender, HandledEventArgs<RectInt32> e)
        {
            base.Bounds = e.Data.ToGdipRectangle();
        }

        private void OnSystemEventsDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (base.Visible && base.IsShown)
            {
                base.EnsureFormIsOnScreen();
            }
        }

        private void OnSystemEventsSessionSwitch(object sender, EventArgs e)
        {
            if (base.Visible && base.IsShown)
            {
                base.EnsureFormIsOnScreen();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible)
            {
                base.EnsureFormIsOnScreen();
            }
            base.OnVisibleChanged(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool flag = false;
            if (keyData.IsArrowKey())
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                if (msg.Msg == 0x100)
                {
                    this.OnKeyDown(e);
                    return e.Handled;
                }
            }
            else if (this.ProcessCmdKeyEvent != null)
            {
                flag = this.ProcessCmdKeyEvent(this, ref msg, keyData);
            }
            if (!flag)
            {
                flag = base.ProcessCmdKey(ref msg, keyData);
            }
            return flag;
        }

        private void QueueReevaluateOpacity()
        {
            this.VerifyThreadAccess();
            if (base.IsHandleCreated && !this.isReevaluateOpacityQueued)
            {
                this.isReevaluateOpacityQueued = true;
                base.BeginInvoke(delegate {
                    this.isReevaluateOpacityQueued = false;
                    this.ReevaluateOpacity();
                });
            }
        }

        private void ReevaluateOpacity()
        {
            this.VerifyThreadAccess();
            this.EvaluateIsMouseInClientArea();
            this.ShouldBeOpaque = (this.ForceOpaque || ((this.IsMouseInClientArea || this.IsMouseInNonClientArea) && UIUtil.IsFormUnderPoint(this, Control.MousePosition))) || this.IsMouseCapturedSlow();
        }

        private void UnhookControl(Control control)
        {
            if ((this.hookedControls != null) && this.hookedControls.Remove(control))
            {
                control.ControlAdded -= this.controlAddedDelegate;
                control.ControlRemoved -= this.controlRemovedDelegate;
                if (control != this)
                {
                    control.KeyUp -= this.keyUpDelegate;
                }
                control.MouseEnter -= this.mouseEnterDelegate;
                control.MouseLeave -= this.mouseLeaveDelegate;
                foreach (Control control2 in control.Controls)
                {
                    this.UnhookControl(control2);
                }
            }
        }

        private void UpdateParking()
        {
            if (((base.FormBorderStyle == FormBorderStyle.Fixed3D) || (base.FormBorderStyle == FormBorderStyle.FixedDialog)) || (((base.FormBorderStyle == FormBorderStyle.FixedSingle) || (base.FormBorderStyle == FormBorderStyle.FixedToolWindow)) || (base.FormBorderStyle == FormBorderStyle.SizableToolWindow)))
            {
                ISnapManagerHost owner = base.Owner as ISnapManagerHost;
                if (owner != null)
                {
                    owner.SnapManager.ReparkObstacle(this);
                }
            }
        }

        private void UpdateSnapObstacleBounds()
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.SetBounds(this.Bounds());
            }
        }

        public bool ForceOpaque
        {
            get => 
                this.forceOpaque;
            set
            {
                this.VerifyThreadAccess();
                if (value != this.forceOpaque)
                {
                    this.forceOpaque = value;
                    this.QueueReevaluateOpacity();
                }
            }
        }

        private bool IsMouseInClientArea
        {
            get => 
                this.isMouseInClientArea;
            set
            {
                this.VerifyThreadAccess();
                if (value != this.isMouseInClientArea)
                {
                    this.isMouseInClientArea = value;
                    this.OnIsMouseInClientAreaChanged();
                }
            }
        }

        private bool IsMouseInNonClientArea
        {
            get => 
                this.isMouseInNonClientArea;
            set
            {
                this.VerifyThreadAccess();
                if (value != this.isMouseInNonClientArea)
                {
                    this.isMouseInNonClientArea = value;
                    this.OnIsMouseInNonClientAreaChanged();
                }
            }
        }

        protected bool RelinquishFocusOnClick
        {
            get => 
                this.relinquishFocusOnClick;
            set
            {
                this.relinquishFocusOnClick = value;
            }
        }

        protected bool RelinquishFocusOnResizeEnd
        {
            get => 
                this.relinquishFocusOnResizeEnd;
            set
            {
                this.relinquishFocusOnResizeEnd = value;
            }
        }

        private bool ShouldBeOpaque
        {
            get => 
                this.shouldBeOpaque;
            set
            {
                this.VerifyThreadAccess();
                if (value != this.shouldBeOpaque)
                {
                    this.shouldBeOpaque = value;
                    this.OnShouldBeOpaqueChanged();
                }
            }
        }

        public PaintDotNet.Snap.SnapObstacle SnapObstacle
        {
            get
            {
                this.VerifyThreadAccess();
                if (this.snapObstacle == null)
                {
                    int num = base.ExtendedFramePadding.Max();
                    int snapDistance = PaintDotNet.Snap.SnapObstacle.DefaultSnapDistance + num;
                    this.snapObstacle = new SnapObstacleController(this.SnapObstacleName, this.Bounds(), SnapRegion.Exterior, false, PaintDotNet.Snap.SnapObstacle.DefaultSnapProximity, snapDistance, this.SnapObstacleSettings);
                    this.snapObstacle.BoundsChangeRequested += new HandledEventHandler<RectInt32>(this.OnSnapObstacleBoundsChangeRequested);
                }
                return this.snapObstacle;
            }
        }

        protected abstract string SnapObstacleName { get; }

        protected abstract ISnapObstaclePersist SnapObstacleSettings { get; }
    }
}

