namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class Tool : IDispatcherObject, IDisposable, IHotKeyTarget, IDisposedEvent, IFormAssociate
    {
        private bool active;
        private bool autoScroll = true;
        private bool canCommit;
        private System.Windows.Forms.Cursor cursor;
        private const char decPenSizeBy5Shortcut = '\x001b';
        private const char decPenSizeShortcut = '[';
        private const char decRadiusShortcut = '\'';
        public static readonly System.Type DefaultToolType = typeof(PaintBrushTool);
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;
        private System.Windows.Forms.Cursor handCursor;
        private System.Windows.Forms.Cursor handCursorInvalid;
        private System.Windows.Forms.Cursor handCursorMouseDown;
        private int ignoreMouseMove;
        private const char incPenSizeBy5Shortcut = '\x001d';
        private const char incPenSizeShortcut = ']';
        private const char incRadiusShortcut = ';';
        private bool isCommitSupported;
        private bool isPulseEnabled;
        private int keyboardEnterCount;
        private int keyboardMoveRepeats;
        private int keyboardMoveSpeed = 1;
        private Dictionary<Keys, KeyTimeInfo> keysThatAreDown = new Dictionary<Keys, KeyTimeInfo>();
        private DateTime lastAutoScrollTime = DateTime.MinValue;
        private MouseButtons lastButton;
        private Keys lastKey;
        private DateTime lastKeyboardMove = DateTime.MinValue;
        private PointInt32 lastMouseXY;
        private PointInt32 lastPanMouseXY;
        private static DateTime lastToolSwitch = DateTime.MinValue;
        private int mouseDown;
        private int mouseEnterCount;
        private bool panMode;
        private System.Windows.Forms.Cursor panOldCursor;
        private bool panTracking;
        private int pulseCounter;
        private const char swapUserColorsShortcut = 'x';
        private const char togglePenDashStyleShortcut = '.';
        private const char togglePenEndCapShortcut = '/';
        private const char togglePenStartCapShortcut = ',';
        private const char toggleShapeShortcut = 'a';
        private const char toggleWhichUserColorShortcut = 'c';
        private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
        private ImageResource toolBarImage;
        private static readonly char[] toolConfigStripShortcuts = new char[] { '[', '\x001b', ']', '\x001d', 'x', 'c', ',', '.', '/', 'a' };
        private ToolInfo toolInfo;
        private static readonly TimeSpan toolSwitchReset = new TimeSpan(0, 0, 0, 2, 0);

        [field: CompilerGenerated]
        public event EventHandler CanCommitChanged;

        [field: CompilerGenerated]
        public event EventHandler CursorChanged;

        [field: CompilerGenerated]
        public event EventHandler CursorChanging;

        [field: CompilerGenerated]
        public event EventHandler Disposed;

        [field: CompilerGenerated]
        public event EventHandler IsCommitSupportedChanged;

        [field: CompilerGenerated]
        public event EventHandler IsPulseEnabledChanged;

        [field: CompilerGenerated]
        public event EventHandler ToolBarConfigItemsChanged;

        public Tool(PaintDotNet.Controls.DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, PaintDotNet.ToolBarConfigItems toolBarConfigItems)
        {
            this.documentWorkspace = documentWorkspace;
            this.toolBarImage = toolBarImage;
            this.toolInfo = new ToolInfo(name, helpText, toolBarImage, this.LargeImage, hotKey, skipIfActiveOnHotKey, base.GetType());
            this.toolBarConfigItems = toolBarConfigItems;
        }

        private void Activate()
        {
            this.active = true;
            this.panTracking = false;
            this.panMode = false;
            this.mouseDown = 0;
            this.Selection.Changing += new EventHandler(this.SelectionChangingHandler);
            this.Selection.Changed += new EventHandler<SelectionChangedEventArgs>(this.SelectionChangedHandler);
            this.HistoryStack.ExecutingHistoryMemento += new ExecutingHistoryMementoEventHandler(this.ExecutingHistoryMemento);
            this.HistoryStack.ExecutedHistoryMemento += new ExecutedHistoryMementoEventHandler(this.ExecutedHistoryMemento);
            this.HistoryStack.FinishedStepGroup += new EventHandler(this.FinishedHistoryStepGroup);
            this.IsPulseEnabled = false;
            this.documentWorkspace.UpdateStatusBarToToolHelpText(this);
            this.OnActivate();
        }

        private bool AutoScrollIfNecessary(PointDouble position)
        {
            if (!this.autoScroll || !this.CanPan())
            {
                return false;
            }
            RectDouble viewportCanvasBounds = this.CanvasView.ViewportCanvasBounds;
            RectDouble canvasBounds = this.CanvasView.GetCanvasBounds();
            double x = DoubleUtil.Lerp((viewportCanvasBounds.Left + viewportCanvasBounds.Right) / 2.0, position.X, 1.02);
            PointDouble num3 = new PointDouble(x, DoubleUtil.Lerp((viewportCanvasBounds.Top + viewportCanvasBounds.Bottom) / 2.0, position.Y, 1.02));
            double num15 = (num3.X < viewportCanvasBounds.Left) ? ((double) (-1)) : ((num3.X > viewportCanvasBounds.Right) ? ((double) 1) : ((double) 0));
            double y = (num3.Y < viewportCanvasBounds.Top) ? ((double) (-1)) : ((num3.Y > viewportCanvasBounds.Bottom) ? ((double) 1) : ((double) 0));
            VectorDouble vec = new VectorDouble(num15, y);
            if (vec.IsCloseToZero())
            {
                return false;
            }
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.lastAutoScrollTime);
            double num5 = DoubleUtil.Clamp(span.TotalSeconds, 0.0, 0.1);
            if (DoubleUtil.IsCloseToZero(num5))
            {
                return false;
            }
            double num6 = (double) AppSettings.Instance.Workspace.AutoScrollViewportPxPerSecond.Value;
            VectorDouble viewportVec = (VectorDouble) ((num5 * vec) * num6);
            PointDouble viewportCanvasOffset = this.CanvasView.ViewportCanvasOffset;
            VectorDouble num9 = this.CanvasView.ConvertViewportToCanvas(viewportVec);
            double left = UIUtil.ScaleWidth((double) 5.0);
            double top = UIUtil.ScaleHeight((double) 5.0);
            ThicknessDouble num12 = new ThicknessDouble(left, top, left, top);
            VectorDouble zero = VectorDouble.Zero;
            if ((num9.X < 0.0) && (viewportCanvasBounds.Left > -num12.Left))
            {
                zero.X = Math.Max(num9.X, -(viewportCanvasBounds.Left + num12.Left));
            }
            if ((num9.Y < 0.0) && (viewportCanvasBounds.Top > -num12.Top))
            {
                zero.Y = Math.Max(num9.Y, -(viewportCanvasBounds.Top + num12.Top));
            }
            if ((num9.X > 0.0) && (viewportCanvasBounds.Right < (canvasBounds.Right + num12.Right)))
            {
                zero.X = Math.Min(num9.X, (canvasBounds.Right + num12.Right) - viewportCanvasBounds.Right);
            }
            if ((num9.Y > 0.0) && (viewportCanvasBounds.Bottom < (canvasBounds.Bottom + num12.Bottom)))
            {
                zero.Y = Math.Min(num9.Y, (canvasBounds.Bottom + num12.Bottom) - viewportCanvasBounds.Bottom);
            }
            if (zero.IsCloseToZero())
            {
                return false;
            }
            PointDouble num14 = viewportCanvasOffset + zero;
            this.ResetLastAutoScrollTime();
            this.CanvasView.ViewportCanvasOffset = num14;
            this.Update();
            return true;
        }

        protected bool CanPan() => 
            (this.DocumentWorkspace.CanvasView.ScaleBasis != ScaleBasis.FitToViewport);

        private void Click()
        {
            this.OnClick();
        }

        public void Commit()
        {
            this.VerifyAccess<PaintDotNet.Tools.Tool>();
            if (!this.IsCommitSupported || !this.CanCommit)
            {
                throw new InvalidOperationException();
            }
            this.OnCommit();
        }

        private void Deactivate()
        {
            this.active = false;
            this.Selection.Changing -= new EventHandler(this.SelectionChangingHandler);
            this.Selection.Changed -= new EventHandler<SelectionChangedEventArgs>(this.SelectionChangedHandler);
            this.HistoryStack.ExecutingHistoryMemento -= new ExecutingHistoryMementoEventHandler(this.ExecutingHistoryMemento);
            this.HistoryStack.ExecutedHistoryMemento -= new ExecutedHistoryMementoEventHandler(this.ExecutedHistoryMemento);
            this.HistoryStack.FinishedStepGroup -= new EventHandler(this.FinishedHistoryStepGroup);
            this.OnDeactivate();
            DisposableUtil.Free<System.Windows.Forms.Cursor>(ref this.handCursor);
            DisposableUtil.Free<System.Windows.Forms.Cursor>(ref this.handCursorMouseDown);
            DisposableUtil.Free<System.Windows.Forms.Cursor>(ref this.handCursorInvalid);
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
                this.Disposed.Raise(this);
            }
            this.Disposed = null;
        }

        private void Enter()
        {
            this.keyboardEnterCount++;
            if (this.keyboardEnterCount == 1)
            {
                this.OnEnter();
            }
        }

        private void EnterPanMode()
        {
            if (this.panMode)
            {
                throw new InvalidOperationException("Cannot enter pan mode when already in pan mode");
            }
            this.panMode = true;
            this.panOldCursor = this.Cursor;
            this.Cursor = this.CurrentPanModeCursor;
        }

        private void ExecutedHistoryMemento(object sender, ExecutedHistoryMementoEventArgs e)
        {
            this.OnExecutedHistoryMemento(e);
        }

        private void ExecutingHistoryMemento(object sender, ExecutingHistoryMementoEventArgs e)
        {
            this.OnExecutingHistoryMemento(e);
        }

        ~Tool()
        {
            this.Dispose(false);
        }

        private void FinishedHistoryStepGroup(object sender, EventArgs e)
        {
            this.OnFinishedHistoryStepGroup();
        }

        protected object GetStaticData() => 
            this.DocumentWorkspace.GetStaticToolData(base.GetType());

        private void KeyDown(KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        private void KeyPress(KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void KeyPress(Keys key)
        {
            this.OnKeyPress(key);
        }

        private void KeyUp(KeyEventArgs e)
        {
            bool flag = false;
            if (this.panMode && (e.KeyCode == Keys.Space))
            {
                flag = true;
                this.LeavePanMode();
                e.Handled = true;
            }
            this.OnKeyUp(e);
            if (flag)
            {
                PointInt32 screenPt = System.Windows.Forms.Cursor.Position.ToPointInt32();
                PointDouble num2 = this.DocumentWorkspace.ScreenToDocument(screenPt);
                this.PerformMouseMove(new MouseEventArgsF(Control.MouseButtons, 0, num2.X, num2.Y, 0));
            }
        }

        private void Leave()
        {
            if (this.keyboardEnterCount == 1)
            {
                this.keyboardEnterCount = 0;
                this.OnLeave();
            }
            else
            {
                this.keyboardEnterCount = Math.Max(0, this.keyboardEnterCount - 1);
            }
        }

        private void LeavePanMode()
        {
            if (!this.panMode)
            {
                throw new InvalidOperationException("Cannot leave pan mode when not in pan mode");
            }
            this.panMode = false;
            this.panTracking = false;
            this.Cursor = this.panOldCursor;
            this.panOldCursor = null;
        }

        private void MouseDown(MouseEventArgsF e)
        {
            this.mouseDown++;
            this.ResetLastAutoScrollTime();
            if ((!this.panMode && (this.mouseDown == 1)) && (e.Button == MouseButtons.Middle))
            {
                this.EnterPanMode();
            }
            if (this.panMode)
            {
                this.panTracking = true;
                this.lastPanMouseXY = new PointInt32(e.X, e.Y);
                if (this.CanPan())
                {
                    this.Cursor = this.CurrentPanModeCursor;
                }
            }
            else
            {
                this.OnMouseDown(e);
            }
            this.lastMouseXY = new PointInt32(e.X, e.Y);
        }

        private void MouseEnter()
        {
            this.mouseEnterCount++;
            if (this.mouseEnterCount == 1)
            {
                this.OnMouseEnter();
            }
        }

        private void MouseLeave()
        {
            if (this.mouseEnterCount == 1)
            {
                this.mouseEnterCount = 0;
                this.OnMouseLeave();
            }
            else
            {
                this.mouseEnterCount = Math.Max(0, this.mouseEnterCount - 1);
            }
        }

        private void MouseMove(MouseEventArgsF e)
        {
            if (this.ignoreMouseMove > 0)
            {
                this.ignoreMouseMove--;
            }
            else if (this.panTracking && ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Middle)))
            {
                switch (UIUtil.AsyncMouseButtons)
                {
                    case MouseButtons.Left:
                    case MouseButtons.Middle:
                    {
                        PointInt32 num = new PointInt32(e.X, e.Y);
                        PointDouble center = this.DocumentWorkspace.VisibleDocumentRect.Center;
                        PointDouble num4 = new PointDouble((double) (e.X - this.lastPanMouseXY.X), (double) (e.Y - this.lastPanMouseXY.Y));
                        PointDouble documentScrollPosition = this.DocumentWorkspace.DocumentScrollPosition;
                        if ((num4.X != 0.0) || (num4.Y != 0.0))
                        {
                            documentScrollPosition.X -= num4.X;
                            documentScrollPosition.Y -= num4.Y;
                            this.lastPanMouseXY = new PointInt32(e.X, e.Y);
                            this.lastPanMouseXY.X -= (int) Math.Truncate(num4.X);
                            this.lastPanMouseXY.Y -= (int) Math.Truncate(num4.Y);
                            using (this.UseIgnoreMouseMove())
                            {
                                this.DocumentWorkspace.DocumentScrollPosition = documentScrollPosition;
                            }
                            this.Update();
                        }
                        break;
                    }
                }
            }
            else if (!this.panMode)
            {
                this.OnMouseMove(e);
            }
            this.lastMouseXY = new PointInt32(e.X, e.Y);
            this.lastButton = e.Button;
        }

        private void MouseUp(MouseEventArgsF e)
        {
            this.mouseDown--;
            if (this.panMode)
            {
                this.Cursor = this.CurrentPanModeCursor;
            }
            else
            {
                this.OnMouseUp(e);
            }
            if (((this.mouseDown == 0) && this.panMode) && (e.Button == MouseButtons.Middle))
            {
                this.LeavePanMode();
            }
            this.lastMouseXY = new PointInt32(e.X, e.Y);
        }

        protected virtual void OnActivate()
        {
        }

        protected virtual void OnClick()
        {
        }

        protected virtual void OnCommit()
        {
        }

        protected virtual void OnCursorChanged()
        {
            this.CursorChanged.Raise(this);
        }

        protected virtual void OnCursorChanging()
        {
            this.CursorChanging.Raise(this);
        }

        protected virtual void OnDeactivate()
        {
            this.IsPulseEnabled = false;
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
        }

        protected virtual void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
        }

        protected virtual void OnFinishedHistoryStepGroup()
        {
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (!this.keysThatAreDown.ContainsKey(e.KeyData))
                {
                    this.keysThatAreDown.Add(e.KeyData, new KeyTimeInfo());
                }
                if ((!this.IsMouseDown && !this.panMode) && (e.KeyCode == Keys.Space))
                {
                    this.EnterPanMode();
                }
                else if ((this.panMode && e.Control) && e.KeyData.IsArrowKey())
                {
                    VectorInt32 num;
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            num = new VectorInt32(-1, 0);
                            break;

                        case Keys.Up:
                            num = new VectorInt32(0, -1);
                            break;

                        case Keys.Right:
                            num = new VectorInt32(1, 0);
                            break;

                        case Keys.Down:
                            num = new VectorInt32(0, 1);
                            break;

                        default:
                            throw ExceptionUtil.InvalidEnumArgumentException<Keys>(e.KeyData, "e.KeyData");
                    }
                    PaintDotNet.Canvas.CanvasView canvasView = this.CanvasView;
                    canvasView.ViewportCanvasOffset += this.CanvasView.ConvertViewportToCanvas(num * 100);
                }
                this.OnKeyPress(e.KeyData);
            }
        }

        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && this.DocumentWorkspace.Focused)
            {
                if ((toolConfigStripShortcuts.IndexOf<char>(e.KeyChar) != -1) || (toolConfigStripShortcuts.IndexOf<char>(char.ToLower(e.KeyChar)) != -1))
                {
                    e.Handled = this.OnTranslateToolConfigStripHotKey(e.KeyChar);
                }
                else
                {
                    ToolInfo[] toolInfos = PaintDotNet.Controls.DocumentWorkspace.ToolInfos;
                    System.Type toolType = this.DocumentWorkspace.GetToolType();
                    int num = 0;
                    if ((this.ModifierKeys & Keys.Shift) != Keys.None)
                    {
                        Array.Reverse(toolInfos);
                    }
                    if ((char.ToLower(this.HotKey) != char.ToLower(e.KeyChar)) || ((DateTime.Now - lastToolSwitch) > toolSwitchReset))
                    {
                        num = -1;
                    }
                    else
                    {
                        for (int j = 0; j < toolInfos.Length; j++)
                        {
                            if (toolInfos[j].ToolType == toolType)
                            {
                                num = j;
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < toolInfos.Length; i++)
                    {
                        int index = ((i + num) + 1) % toolInfos.Length;
                        ToolInfo info = toolInfos[index];
                        if (((info.ToolType != this.DocumentWorkspace.GetToolType()) || !info.SkipIfActiveOnHotKey) && (char.ToLower(info.HotKey) == char.ToLower(e.KeyChar)))
                        {
                            if (!this.IsMouseDown)
                            {
                                this.AppWorkspace.Widgets.ToolsControl.SelectTool(info.ToolType);
                            }
                            e.Handled = true;
                            lastToolSwitch = DateTime.Now;
                            break;
                        }
                    }
                    if (!e.Handled)
                    {
                        char keyChar = e.KeyChar;
                        if (((keyChar == '\r') || (keyChar == '\x001b')) && (!this.IsMouseDown && !this.Selection.IsEmpty))
                        {
                            e.Handled = true;
                            this.DocumentWorkspace.ApplyFunction(new DeselectFunction());
                        }
                    }
                }
            }
        }

        protected virtual void OnKeyPress(Keys key)
        {
            int num2;
            PointInt32 zero = PointInt32.Zero;
            if (key != this.lastKey)
            {
                this.lastKeyboardMove = DateTime.MinValue;
            }
            this.lastKey = key;
            switch (key)
            {
                case Keys.Left:
                    num2 = zero.X - 1;
                    zero.X = num2;
                    break;

                case Keys.Up:
                    num2 = zero.Y - 1;
                    zero.Y = num2;
                    break;

                case Keys.Right:
                    num2 = zero.X + 1;
                    zero.X = num2;
                    break;

                case Keys.Down:
                    num2 = zero.Y + 1;
                    zero.Y = num2;
                    break;
            }
            if (!zero.Equals(PointInt32.Zero))
            {
                if (this.panMode)
                {
                    PaintDotNet.Canvas.CanvasView canvasView = this.CanvasView;
                    canvasView.ViewportCanvasOffset += this.CanvasView.ConvertViewportToCanvas(((VectorInt32) zero) * 10);
                }
                else
                {
                    long num3 = DateTime.Now.Ticks - this.lastKeyboardMove.Ticks;
                    if ((num3 * 4L) > 0x989680L)
                    {
                        this.keyboardMoveRepeats = 0;
                        this.keyboardMoveSpeed = 1;
                    }
                    else
                    {
                        this.keyboardMoveRepeats++;
                        if ((this.keyboardMoveRepeats > 15) && ((this.keyboardMoveRepeats % 4) == 0))
                        {
                            this.keyboardMoveSpeed++;
                        }
                    }
                    this.lastKeyboardMove = DateTime.Now;
                    int num4 = (int) Math.Ceiling((double) (this.DocumentWorkspace.ScaleFactor.Ratio * this.keyboardMoveSpeed));
                    Point position = System.Windows.Forms.Cursor.Position;
                    System.Windows.Forms.Cursor.Position = new PointInt32(position.X + (num4 * zero.X), position.Y + (num4 * zero.Y)).ToGdipPoint();
                }
            }
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            this.keysThatAreDown.Clear();
        }

        protected virtual void OnLeave()
        {
        }

        protected virtual void OnMouseDown(MouseEventArgsF e)
        {
            this.lastButton = e.Button;
        }

        protected virtual void OnMouseEnter()
        {
        }

        protected virtual void OnMouseLeave()
        {
        }

        protected virtual void OnMouseMove(MouseEventArgsF e)
        {
            if (this.panMode || ((this.mouseDown > 0) && (UIUtil.AsyncMouseButtons != MouseButtons.None)))
            {
                this.AutoScrollIfNecessary(e.Point);
            }
        }

        protected virtual void OnMouseUp(MouseEventArgsF e)
        {
            this.lastButton = e.Button;
        }

        protected virtual void OnPaste(IPdnDataObject data, out bool handled)
        {
            handled = false;
        }

        protected virtual void OnPasteQuery(IPdnDataObject data, out bool canHandle)
        {
            canHandle = false;
        }

        protected virtual void OnPulse()
        {
        }

        protected virtual void OnSelectionChanged()
        {
        }

        protected virtual void OnSelectionChanging()
        {
        }

        protected virtual bool OnToolConfigStripHotKey(ToolConfigStripHotKey key)
        {
            switch (key)
            {
                case ToolConfigStripHotKey.DecrementPenSize:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(-1f);
                    return true;

                case ToolConfigStripHotKey.DecrementPenSizeBy5:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(-5f);
                    return true;

                case ToolConfigStripHotKey.IncrementPenSize:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(1f);
                    return true;

                case ToolConfigStripHotKey.IncrementPenSizeBy5:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenWidth))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(5f);
                    return true;

                case ToolConfigStripHotKey.SwapUserColors:
                    this.AppWorkspace.Widgets.ColorsForm.SwapUserColors();
                    return true;

                case ToolConfigStripHotKey.ToggleWhichUserColor:
                    this.AppWorkspace.Widgets.ColorsForm.ToggleWhichUserColor();
                    return true;

                case ToolConfigStripHotKey.TogglePenStartCap:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenStartCap)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenStartCap))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.CyclePenStartCap();
                    return true;

                case ToolConfigStripHotKey.TogglePenDashStyle:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenDashStyle)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenDashStyle))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.CyclePenDashStyle();
                    return true;

                case ToolConfigStripHotKey.TogglePenEndCap:
                    if ((this.ToolBarConfigItems & (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenEndCap)) != (PaintDotNet.ToolBarConfigItems.None | PaintDotNet.ToolBarConfigItems.PenEndCap))
                    {
                        break;
                    }
                    this.AppWorkspace.Widgets.ToolConfigStrip.CyclePenEndCap();
                    return true;

                case ToolConfigStripHotKey.NextShape:
                case ToolConfigStripHotKey.PreviousShape:
                    return false;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<ToolConfigStripHotKey>(key, "key");
            }
            return false;
        }

        protected virtual bool OnTranslateToolConfigStripHotKey(char keyChar)
        {
            char ch = char.ToLower(keyChar);
            switch (ch)
            {
                case 'x':
                    return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.SwapUserColors);

                case 'c':
                    return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.ToggleWhichUserColor);
            }
            if (keyChar == '[')
            {
                return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.DecrementPenSize);
            }
            if ((keyChar == '\x001b') && ((this.ModifierKeys & Keys.Control) != Keys.None))
            {
                return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.DecrementPenSizeBy5);
            }
            if (keyChar == ']')
            {
                return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.IncrementPenSize);
            }
            if ((keyChar == '\x001d') && ((this.ModifierKeys & Keys.Control) != Keys.None))
            {
                return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.IncrementPenSizeBy5);
            }
            switch (ch)
            {
                case ',':
                    return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.TogglePenStartCap);

                case '.':
                    return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.TogglePenDashStyle);

                case '/':
                    return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.TogglePenEndCap);
            }
            if (ch != 'a')
            {
                return false;
            }
            if ((this.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.PreviousShape);
            }
            return this.OnToolConfigStripHotKey(ToolConfigStripHotKey.NextShape);
        }

        private void Paste(IPdnDataObject data, out bool handled)
        {
            this.OnPaste(data, out handled);
        }

        private void PasteQuery(IPdnDataObject data, out bool canHandle)
        {
            this.OnPasteQuery(data, out canHandle);
        }

        public void PerformActivate()
        {
            this.Activate();
        }

        public void PerformClick()
        {
            this.Click();
        }

        public void PerformDeactivate()
        {
            this.Deactivate();
        }

        public void PerformEnter()
        {
            this.Enter();
        }

        public void PerformKeyDown(KeyEventArgs e)
        {
            this.KeyDown(e);
        }

        public void PerformKeyPress(KeyPressEventArgs e)
        {
            this.KeyPress(e);
        }

        public void PerformKeyPress(Keys key)
        {
            this.KeyPress(key);
        }

        public void PerformKeyUp(KeyEventArgs e)
        {
            this.KeyUp(e);
        }

        public void PerformLeave()
        {
            this.Leave();
        }

        public void PerformMouseDown(MouseEventArgsF e)
        {
            this.DocumentCanvas.MouseLocation = e.Point;
            this.DocumentWorkspace.Focus();
            this.MouseDown(e);
        }

        public void PerformMouseEnter()
        {
            this.MouseEnter();
        }

        public void PerformMouseLeave()
        {
            this.MouseLeave();
        }

        public void PerformMouseMove(MouseEventArgsF e)
        {
            this.DocumentCanvas.MouseLocation = e.Point;
            this.MouseMove(e);
        }

        public void PerformMouseUp(MouseEventArgsF e)
        {
            this.MouseUp(e);
        }

        public void PerformPaste(IPdnDataObject data, out bool handled)
        {
            this.Paste(data, out handled);
        }

        public void PerformPasteQuery(IPdnDataObject data, out bool canHandle)
        {
            this.PasteQuery(data, out canHandle);
        }

        public void PerformPulse()
        {
            this.Pulse();
        }

        private void Pulse()
        {
            this.pulseCounter++;
            if (this.IsPulseEnabled)
            {
                if (this.IsFormActive)
                {
                    this.OnPulse();
                }
                else if ((this.pulseCounter % 4) == 0)
                {
                    this.OnPulse();
                }
            }
        }

        private void RaiseIsPulseEnabledChanged()
        {
            this.IsPulseEnabledChanged.Raise(this);
        }

        private void ResetLastAutoScrollTime()
        {
            this.lastAutoScrollTime = DateTime.UtcNow;
        }

        private void SelectionChanged()
        {
            this.OnSelectionChanged();
        }

        private void SelectionChangedHandler(object sender, EventArgs e)
        {
            this.OnSelectionChanged();
        }

        private void SelectionChanging()
        {
            this.OnSelectionChanging();
        }

        private void SelectionChangingHandler(object sender, EventArgs e)
        {
            this.OnSelectionChanging();
        }

        protected void SetStaticData(object data)
        {
            this.DocumentWorkspace.SetStaticToolData(base.GetType(), data);
        }

        protected void SetStatus(ImageResource statusIcon, string statusText)
        {
            if ((statusIcon == null) && (statusText != null))
            {
                statusIcon = PdnResources.GetImageResource("Icons.MenuHelpHelpTopicsIcon.png");
            }
            this.DocumentWorkspace.SetStatus(statusText, statusIcon);
        }

        protected PointDouble SnapPoint(PointDouble canvasPoint) => 
            PointDouble.Truncate(canvasPoint);

        protected PointDouble[] SnapPoints(IList<PointDouble> canvasPoints)
        {
            PointDouble[] numArray = new PointDouble[canvasPoints.Count];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = this.SnapPoint(canvasPoints[i]);
            }
            return numArray;
        }

        protected void Update()
        {
            this.DocumentWorkspace.QueueUpdate();
        }

        private PushIgnoreMouseMoveScope UseIgnoreMouseMove() => 
            new PushIgnoreMouseMoveScope(this);

        public bool Active =>
            this.active;

        protected Layer ActiveLayer =>
            this.DocumentWorkspace.ActiveLayer;

        protected int ActiveLayerIndex
        {
            get => 
                this.DocumentWorkspace.ActiveLayerIndex;
            set
            {
                this.DocumentWorkspace.ActiveLayerIndex = value;
            }
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace =>
            this.DocumentWorkspace.AppWorkspace;

        public Form AssociatedForm =>
            this.AppWorkspace.FindForm();

        public bool CanCommit
        {
            get
            {
                this.VerifyAccess<PaintDotNet.Tools.Tool>();
                return this.canCommit;
            }
            protected set
            {
                this.VerifyAccess<PaintDotNet.Tools.Tool>();
                if (value != this.canCommit)
                {
                    this.canCommit = value;
                    this.CanCommitChanged.Raise(this);
                }
            }
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.DocumentWorkspace.CanvasView;

        protected System.Windows.Forms.Cursor CurrentPanModeCursor
        {
            get
            {
                if (!this.CanPan())
                {
                    return this.HandCursorInvalid;
                }
                if (this.IsMouseDown)
                {
                    return this.HandCursorMouseDown;
                }
                return this.HandCursor;
            }
        }

        public System.Windows.Forms.Cursor Cursor
        {
            get => 
                this.cursor;
            set
            {
                this.OnCursorChanging();
                this.cursor = value;
                this.OnCursorChanged();
            }
        }

        public bool DeactivateOnLayerChange =>
            true;

        public IDispatcher Dispatcher =>
            this.documentWorkspace?.Dispatcher;

        protected PaintDotNet.Document Document =>
            this.DocumentWorkspace.Document;

        public PaintDotNet.Canvas.DocumentCanvas DocumentCanvas =>
            this.DocumentWorkspace.DocumentCanvas;

        public PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace =>
            this.documentWorkspace;

        public bool Focused =>
            this.DocumentWorkspace.Focused;

        protected System.Windows.Forms.Cursor HandCursor
        {
            get
            {
                if (!this.active)
                {
                    return null;
                }
                if (this.handCursor == null)
                {
                    this.handCursor = PdnResources.GetCursor("Cursors.PanToolCursor.cur");
                }
                return this.handCursor;
            }
        }

        protected System.Windows.Forms.Cursor HandCursorInvalid
        {
            get
            {
                if (!this.active)
                {
                    return null;
                }
                if (this.handCursorInvalid == null)
                {
                    this.handCursorInvalid = PdnResources.GetCursor("Cursors.PanToolCursorInvalid.cur");
                }
                return this.handCursorInvalid;
            }
        }

        protected System.Windows.Forms.Cursor HandCursorMouseDown
        {
            get
            {
                if (!this.active)
                {
                    return null;
                }
                if (this.handCursorMouseDown == null)
                {
                    this.handCursorMouseDown = PdnResources.GetCursor("Cursors.PanToolCursorMouseDown.cur");
                }
                return this.handCursorMouseDown;
            }
        }

        public string HelpText =>
            this.toolInfo.HelpText;

        protected PaintDotNet.HistoryStack HistoryStack =>
            this.DocumentWorkspace.History;

        public char HotKey =>
            this.toolInfo.HotKey;

        public ImageResource Image =>
            this.toolBarImage;

        public ToolInfo Info =>
            this.toolInfo;

        protected bool IsAutoScrollEnabled
        {
            get => 
                this.autoScroll;
            set
            {
                this.autoScroll = value;
            }
        }

        public bool IsCommitSupported
        {
            get
            {
                this.VerifyAccess<PaintDotNet.Tools.Tool>();
                return this.isCommitSupported;
            }
            protected set
            {
                if (this.Dispatcher != null)
                {
                    this.VerifyAccess<PaintDotNet.Tools.Tool>();
                }
                if (value != this.isCommitSupported)
                {
                    this.isCommitSupported = value;
                    this.IsCommitSupportedChanged.Raise(this);
                }
            }
        }

        protected bool IsFormActive =>
            (Form.ActiveForm == this.DocumentWorkspace.FindForm());

        protected bool IsInPanMode =>
            this.panMode;

        public bool IsMouseDown =>
            (this.mouseDown > 0);

        public bool IsMouseEntered =>
            (this.mouseEnterCount > 0);

        public bool IsPulseEnabled
        {
            get => 
                this.isPulseEnabled;
            protected set
            {
                this.VerifyAccess<PaintDotNet.Tools.Tool>();
                if (value != this.isPulseEnabled)
                {
                    this.isPulseEnabled = value;
                    this.RaiseIsPulseEnabledChanged();
                }
            }
        }

        public virtual ImageResource LargeImage =>
            null;

        public Keys ModifierKeys =>
            Control.ModifierKeys;

        public string Name =>
            this.toolInfo.DisplayName;

        protected PaintDotNet.Selection Selection =>
            this.DocumentWorkspace.Selection;

        public PaintDotNet.ToolBarConfigItems ToolBarConfigItems
        {
            get => 
                this.toolBarConfigItems;
            protected set
            {
                this.VerifyAccess<PaintDotNet.Tools.Tool>();
                if (value != this.toolBarConfigItems)
                {
                    this.toolBarConfigItems = value;
                    this.ToolBarConfigItemsChanged.Raise(this);
                }
            }
        }

        public AppSettings.ToolsSection ToolSettings =>
            this.DocumentWorkspace.ToolSettings;

        private sealed class KeyTimeInfo
        {
            public KeyTimeInfo()
            {
                this.KeyDownTime = DateTime.Now;
                this.LastKeyPressPulse = this.KeyDownTime;
                this.Repeats = 0;
            }

            public DateTime KeyDownTime { get; private set; }

            public DateTime LastKeyPressPulse { get; private set; }

            public int Repeats { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PushIgnoreMouseMoveScope : IDisposable
        {
            private PaintDotNet.Tools.Tool owner;
            private int oldIgnoreMouseMove;
            public PushIgnoreMouseMoveScope(PaintDotNet.Tools.Tool owner)
            {
                this.owner = owner;
                this.oldIgnoreMouseMove = this.owner.ignoreMouseMove;
                this.owner.ignoreMouseMove++;
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    if (this.owner.ignoreMouseMove > this.oldIgnoreMouseMove)
                    {
                        this.owner.ignoreMouseMove = this.oldIgnoreMouseMove;
                    }
                    this.owner = null;
                }
            }
        }
    }
}

