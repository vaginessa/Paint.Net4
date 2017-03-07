namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class SaveConfigDialog : PdnBaseDialog
    {
        private volatile bool callbackBusy;
        private ManualResetEvent callbackDoneEvent = new ManualResetEvent(true);
        private ScrollableCanvasControl canvasControl;
        private IContainer components;
        private PdnPushButton defaultsButton;
        private bool disposeDocument;
        private PaintDotNet.Document document;
        private DocumentCanvas documentCanvas;
        private string fileSizeTextFormat;
        private System.Threading.Timer fileSizeTimer;
        private PaintDotNet.FileType fileType;
        private Dictionary<PaintDotNet.FileType, PaintDotNet.SaveConfigToken> fileTypeToSaveToken = new Dictionary<PaintDotNet.FileType, PaintDotNet.SaveConfigToken>();
        private PaintDotNet.Controls.SeparatorLine footerSeparator;
        private Cursor handIcon = PdnResources.GetCursor("Cursors.PanToolCursor.cur");
        private Cursor handIconMouseDown = PdnResources.GetCursor("Cursors.PanToolCursorMouseDown.cur");
        private bool isMouseDown;
        private Point lastClientMousePt;
        private HeadingLabel previewHeader;
        private Panel saveConfigPanel;
        private SaveConfigWidget saveConfigWidget;
        private Surface scratchSurface;
        private HeadingLabel settingsHeader;
        private const int timerDelayTime = 100;
        private static readonly Size unscaledMinSize = new Size(600, 380);

        [field: CompilerGenerated]
        public event ProgressEventHandler Progress;

        public SaveConfigDialog()
        {
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            this.fileSizeTimer = new System.Threading.Timer(new TimerCallback(this.FileSizeTimerCallback), null, 0x3e8, -1);
            this.documentCanvas = new DocumentCanvas();
            this.InitializeComponent();
            this.canvasControl.Canvas = this.documentCanvas;
            this.Text = PdnResources.GetString("SaveConfigDialog.Text");
            this.fileSizeTextFormat = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text.Format");
            this.settingsHeader.Text = PdnResources.GetString("SaveConfigDialog.SettingsHeader.Text");
            this.defaultsButton.Text = PdnResources.GetString("SaveConfigDialog.DefaultsButton.Text");
            this.previewHeader.Text = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text");
            base.Icon = PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference.ToIcon();
            this.canvasControl.CanvasControl.Cursor = this.handIcon;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void CleanupTimer()
        {
            if (this.fileSizeTimer != null)
            {
                new Action(this.fileSizeTimer.Dispose).Try().Observe();
                this.fileSizeTimer = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposeDocument && (this.documentCanvas.Document != null))
                {
                    PaintDotNet.Document document = this.documentCanvas.Document;
                    this.documentCanvas.Document = null;
                    document.Dispose();
                }
                this.CleanupTimer();
                if (this.handIcon != null)
                {
                    this.handIcon.Dispose();
                    this.handIcon = null;
                }
                if (this.handIconMouseDown != null)
                {
                    this.handIconMouseDown.Dispose();
                    this.handIconMouseDown = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private void FileSizeProgressEventHandler(object state, ProgressEventArgs e)
        {
            if (base.IsHandleCreated)
            {
                object[] args = new object[] { (int) e.Percent };
                base.BeginInvoke(new Action<int>(this.SetFileSizeProgress), args);
            }
        }

        private void FileSizeTimerCallback(object state)
        {
            try
            {
                if (base.IsHandleCreated)
                {
                    if (this.callbackBusy)
                    {
                        base.Invoke(new Action(this.QueueFileSizeTextUpdate));
                    }
                    else
                    {
                        try
                        {
                            this.FileSizeTimerCallbackImpl(state);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void FileSizeTimerCallbackImpl(object state)
        {
            if ((this.fileSizeTimer != null) && !base.IsDisposed)
            {
                this.callbackBusy = true;
                try
                {
                    if (this.Document != null)
                    {
                        string tempFileName = Path.GetTempFileName();
                        FileStream output = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                        this.FileType.Save(this.Document, output, this.SaveConfigToken, this.scratchSurface, new ProgressEventHandler(this.FileSizeProgressEventHandler), false);
                        output.Flush();
                        output.Close();
                        object[] args = new object[] { tempFileName };
                        base.BeginInvoke(new Action<string>(this.UpdateFileSizeAndPreview), args);
                    }
                }
                catch (Exception)
                {
                    if (!base.IsDisposed)
                    {
                        try
                        {
                            base.BeginInvoke(new Action<string>(this.UpdateFileSizeAndPreview), new object[1]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                finally
                {
                    this.callbackDoneEvent.Set();
                    this.callbackBusy = false;
                    CleanupManager.RequestCleanup();
                }
            }
        }

        private void InitializeComponent()
        {
            this.saveConfigPanel = new Panel();
            this.defaultsButton = new PdnPushButton();
            this.saveConfigWidget = new SaveConfigWidget();
            this.previewHeader = new HeadingLabel();
            this.canvasControl = new ScrollableCanvasControl();
            this.settingsHeader = new HeadingLabel();
            this.footerSeparator = new PaintDotNet.Controls.SeparatorLine();
            base.SuspendLayout();
            base.baseOkButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            base.baseOkButton.Name = "baseOkButton";
            base.baseOkButton.TabIndex = 2;
            base.baseOkButton.Click += new EventHandler(this.OnBaseOkButtonClick);
            base.baseCancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            base.baseCancelButton.Name = "baseCancelButton";
            base.baseCancelButton.TabIndex = 3;
            base.baseCancelButton.Click += new EventHandler(this.OnBaseCancelButtonClick);
            this.footerSeparator.Name = "footerSeparator";
            this.saveConfigPanel.AutoScroll = true;
            this.saveConfigPanel.Name = "saveConfigPanel";
            this.saveConfigPanel.TabIndex = 0;
            this.saveConfigPanel.TabStop = false;
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.AutoSize = true;
            this.defaultsButton.TabIndex = 1;
            this.defaultsButton.Click += new EventHandler(this.OnDefaultsButtonClick);
            this.saveConfigWidget.Name = "saveConfigWidget";
            this.saveConfigWidget.TabIndex = 9;
            this.saveConfigWidget.Token = null;
            this.previewHeader.Name = "previewHeader";
            this.previewHeader.RightMargin = 0;
            this.previewHeader.TabIndex = 11;
            this.previewHeader.TabStop = false;
            this.previewHeader.Text = "Header";
            this.canvasControl.Name = "documentView";
            this.canvasControl.CanvasControl.MouseDown += new MouseEventHandler(this.OnCanvasViewMouseDown);
            this.canvasControl.CanvasControl.MouseMove += new MouseEventHandler(this.OnCanvasViewMouseMove);
            this.canvasControl.CanvasControl.MouseUp += new MouseEventHandler(this.OnCanvasViewMouseUp);
            this.canvasControl.CanvasView.IsCanvasFrameEnabled = false;
            this.settingsHeader.Name = "settingsHeader";
            this.settingsHeader.TabIndex = 13;
            this.settingsHeader.TabStop = false;
            this.settingsHeader.Text = "Header";
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Controls.Add(this.defaultsButton);
            base.Controls.Add(this.settingsHeader);
            base.Controls.Add(this.previewHeader);
            base.Controls.Add(this.canvasControl);
            base.Controls.Add(this.footerSeparator);
            base.Controls.Add(this.saveConfigPanel);
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.MinimizeBox = false;
            base.MaximizeBox = true;
            base.Name = "SaveConfigDialog";
            base.StartPosition = FormStartPosition.Manual;
            base.Controls.SetChildIndex(this.saveConfigPanel, 0);
            base.Controls.SetChildIndex(this.canvasControl, 0);
            base.Controls.SetChildIndex(base.baseOkButton, 0);
            base.Controls.SetChildIndex(base.baseCancelButton, 0);
            base.Controls.SetChildIndex(this.previewHeader, 0);
            base.Controls.SetChildIndex(this.settingsHeader, 0);
            base.Controls.SetChildIndex(this.defaultsButton, 0);
            base.ResumeLayout(false);
        }

        private void LoadPositions()
        {
            Rectangle bounds;
            Size size = UIUtil.ScaleSize(unscaledMinSize);
            Form owner = base.Owner;
            if (owner != null)
            {
                bounds = owner.Bounds;
            }
            else
            {
                bounds = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            }
            Rectangle rectangle2 = new Rectangle((bounds.Width - size.Width) / 2, (bounds.Height - size.Height) / 2, size.Width, size.Height);
            FormWindowState normal = AppSettings.Instance.Window.SaveConfiguration.FormWindowState.Value;
            if (normal == FormWindowState.Minimized)
            {
                normal = FormWindowState.Normal;
            }
            Rectangle rectangle3 = AppSettings.Instance.Window.SaveConfiguration.IsBoundsSpecified ? AppSettings.Instance.Window.SaveConfiguration.Bounds.Value.ToGdipRectangle() : rectangle2;
            Rectangle newClientBounds = new Rectangle(rectangle3.Left + bounds.Left, rectangle3.Top + owner.Top, rectangle3.Width, rectangle3.Height);
            Rectangle defaultClientBounds = new Rectangle(rectangle2.Left + bounds.Left, rectangle2.Top + bounds.Top, rectangle2.Width, rectangle2.Height);
            base.SuspendLayout();
            try
            {
                Rectangle clientBounds = this.ValidateAndAdjustNewBounds(owner, newClientBounds, defaultClientBounds);
                Rectangle rectangle7 = base.ClientBoundsToWindowBounds(clientBounds);
                base.Bounds = rectangle7;
                base.WindowState = normal;
            }
            finally
            {
                base.ResumeLayout(true);
            }
        }

        private void OnBaseCancelButtonClick(object sender, EventArgs e)
        {
        }

        private void OnBaseOkButtonClick(object sender, EventArgs e)
        {
            this.UIWaitForCallbackDoneEvent(WaitActionType.Ok);
            this.CleanupTimer();
        }

        private void OnCanvasViewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.isMouseDown = true;
                this.canvasControl.CanvasControl.Cursor = this.handIconMouseDown;
                this.lastClientMousePt = new Point(e.X, e.Y);
            }
        }

        private void OnCanvasViewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.isMouseDown)
            {
                Point point = new Point(e.X, e.Y);
                int num = point.X - this.lastClientMousePt.X;
                int num2 = point.Y - this.lastClientMousePt.Y;
                if ((num != 0) || (num2 != 0))
                {
                    CanvasView canvasView = this.canvasControl.CanvasView;
                    double num3 = canvasView.ConvertExtentXToCanvasX((double) num);
                    double num4 = canvasView.ConvertExtentYToCanvasY((double) num2);
                    PointDouble viewportCanvasOffset = canvasView.ViewportCanvasOffset;
                    PointDouble num6 = new PointDouble(viewportCanvasOffset.X - num3, viewportCanvasOffset.Y - num4);
                    canvasView.ViewportCanvasOffset = num6;
                    this.lastClientMousePt = point;
                }
            }
        }

        private void OnCanvasViewMouseUp(object sender, MouseEventArgs e)
        {
            this.isMouseDown = false;
            this.canvasControl.CanvasControl.Cursor = this.handIcon;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.UIWaitForCallbackDoneEvent(WaitActionType.Cancel);
            this.CleanupTimer();
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnClosing(e);
        }

        private void OnDefaultsButtonClick(object sender, EventArgs e)
        {
            this.SaveConfigToken = this.FileType.CreateDefaultSaveConfigToken();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(7);
            int num2 = base.IsGlassEffectivelyEnabled ? 0 : num;
            int num3 = UIUtil.ScaleWidth(7);
            int x = base.IsGlassEffectivelyEnabled ? -1 : num3;
            Size size = UIUtil.ScaleSize(0x55, 0x18);
            base.baseCancelButton.Size = size;
            base.baseCancelButton.PerformLayout();
            base.baseOkButton.Size = size;
            base.baseOkButton.PerformLayout();
            base.baseCancelButton.Location = new Point((base.ClientSize.Width - base.baseOkButton.Width) - x, (base.ClientSize.Height - num2) - base.baseCancelButton.Height);
            base.baseOkButton.Location = new Point((base.baseCancelButton.Left - num3) - base.baseOkButton.Width, (base.ClientSize.Height - num2) - base.baseOkButton.Height);
            this.footerSeparator.Size = this.footerSeparator.GetPreferredSize(new Size(base.ClientSize.Width - (2 * x), 1));
            this.footerSeparator.Location = new Point(x, (base.baseOkButton.Top - num) - this.footerSeparator.Height);
            if (base.IsGlassEffectivelyEnabled)
            {
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.footerSeparator.Top);
                this.footerSeparator.Visible = false;
                base.SizeGripStyle = SizeGripStyle.Hide;
            }
            else
            {
                base.GlassInset = new Padding(0);
                this.footerSeparator.Visible = true;
                base.SizeGripStyle = SizeGripStyle.Show;
            }
            int num5 = UIUtil.ScaleHeight(8);
            int y = UIUtil.ScaleHeight(6);
            int num7 = UIUtil.ScaleWidth(8);
            int num8 = UIUtil.ScaleWidth(200);
            int num9 = UIUtil.ScaleWidth(8);
            int num10 = UIUtil.ScaleWidth(8);
            int num11 = (num7 + num8) + num9;
            int width = (base.ClientSize.Width - num11) - num10;
            int num13 = UIUtil.ScaleHeight(12);
            int num14 = -3;
            this.settingsHeader.Location = new Point(num7 + num14, y);
            this.settingsHeader.Width = num8 - num14;
            this.settingsHeader.Size = this.settingsHeader.GetPreferredSize(this.settingsHeader.Width, 1);
            this.settingsHeader.PerformLayout();
            this.saveConfigPanel.Location = new Point(num7, this.settingsHeader.Bottom + num);
            this.saveConfigPanel.Width = num8;
            this.saveConfigPanel.PerformLayout();
            this.saveConfigWidget.Width = this.saveConfigPanel.Width - SystemInformation.VerticalScrollBarWidth;
            this.previewHeader.Location = new Point(num11 + num14, y);
            this.previewHeader.Size = this.previewHeader.GetPreferredSize(width - num14, 1);
            this.canvasControl.Location = new Point(num11, this.previewHeader.Bottom + num);
            this.canvasControl.Size = new Size(width, (this.footerSeparator.Top - num5) - this.canvasControl.Top);
            this.saveConfigPanel.Height = ((this.canvasControl.Bottom - this.saveConfigPanel.Top) - this.defaultsButton.Height) - num13;
            this.saveConfigWidget.PerformLayout();
            int num15 = Math.Min(this.saveConfigPanel.Height, this.saveConfigWidget.Height);
            this.defaultsButton.PerformLayout();
            this.defaultsButton.Location = new Point(num7 + ((num8 - this.defaultsButton.Width) / 2), (this.saveConfigPanel.Top + num15) + num13);
            this.MinimumSize = UIUtil.ScaleSize(unscaledMinSize);
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.scratchSurface == null)
            {
                ExceptionUtil.ThrowInvalidOperationException("ScratchSurface was never set: it is null");
            }
            PaintDotNet.SaveConfigToken saveConfigToken = this.SaveConfigToken;
            this.LoadPositions();
            this.SaveConfigToken = saveConfigToken;
            base.OnLoad(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.canvasControl.CanvasControl.ProcessMouseWheel(this, e);
            base.OnMouseWheel(e);
        }

        private void OnProgress(int percent)
        {
            if (this.Progress != null)
            {
                this.Progress(this, new ProgressEventArgs((double) percent));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnResize(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.baseOkButton.Focus();
            base.OnShown(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnSizeChanged(e);
        }

        private void OnTokenChanged(object sender, EventArgs e)
        {
            this.QueueFileSizeTextUpdate();
        }

        private void QueueFileSizeTextUpdate()
        {
            this.callbackDoneEvent.Reset();
            if (this.fileSizeTimer != null)
            {
                string str = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing");
                this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str);
                this.fileSizeTimer.Change(100, 0);
            }
            this.OnProgress(0);
        }

        private void SavePositions()
        {
            if (base.WindowState != FormWindowState.Minimized)
            {
                if (base.WindowState != FormWindowState.Maximized)
                {
                    Point location;
                    Form owner = base.Owner;
                    if (owner != null)
                    {
                        location = owner.Bounds.Location;
                    }
                    else
                    {
                        location = new Point(0, 0);
                    }
                    Rectangle rectangle = base.WindowBoundsToClientBounds(base.Bounds);
                    int x = rectangle.Left - location.X;
                    int y = rectangle.Top - location.Y;
                    AppSettings.Instance.Window.SaveConfiguration.Bounds.Value = new RectInt32(x, y, rectangle.Width, rectangle.Height);
                }
                AppSettings.Instance.Window.SaveConfiguration.FormWindowState.Value = base.WindowState;
            }
        }

        private void SetFileSizeProgress(int percent)
        {
            string str2 = string.Format(PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing.Format"), percent);
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str2);
            int num = percent.Clamp(0, 100);
            this.OnProgress(num);
        }

        public static string SizeStringFromBytes(long bytes)
        {
            string str;
            string str2;
            double num = bytes;
            if (num > 1073741824.0)
            {
                num /= 1073741824.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.GBFormat");
            }
            else if (num > 1048576.0)
            {
                num /= 1048576.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.MBFormat");
            }
            else if (num > 1024.0)
            {
                num /= 1024.0;
                str = "F1";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.KBFormat");
            }
            else
            {
                str = "F0";
                str2 = PdnResources.GetString("Utility.SizeStringFromBytes.BytesFormat");
            }
            string str3 = num.ToString(str);
            return string.Format(str2, str3);
        }

        private void UIWaitForCallbackDoneEvent(WaitActionType wat)
        {
            if (!this.callbackDoneEvent.WaitOne(0, false))
            {
                using (TaskManager manager = new TaskManager())
                {
                    VirtualTask<Unit> cancelTask = manager.CreateVirtualTask(TaskState.Running);
                    TaskProgressDialog dialog = new TaskProgressDialog {
                        Task = cancelTask,
                        CloseOnFinished = true,
                        ShowCancelButton = false,
                        Icon = base.Icon,
                        Text = this.Text
                    };
                    if (wat != WaitActionType.Ok)
                    {
                        if (wat != WaitActionType.Cancel)
                        {
                            throw ExceptionUtil.InvalidEnumArgumentException<WaitActionType>(wat, "wat");
                        }
                        dialog.HeaderText = PdnResources.GetString("TaskProgressDialog.Canceling.Text");
                    }
                    else
                    {
                        dialog.HeaderText = PdnResources.GetString("SaveConfigDialog.Finishing.Text");
                    }
                    dialog.Shown += (<sender>, <e>) => Work.QueueWorkItem(delegate {
                        try
                        {
                            this.callbackDoneEvent.WaitOne();
                        }
                        finally
                        {
                            cancelTask.SetState(TaskState.Finished);
                        }
                    });
                    dialog.ShowDialog(this);
                }
            }
        }

        private void UpdateFileSizeAndPreview(string tempFileName)
        {
            if (!base.IsDisposed)
            {
                if (tempFileName == null)
                {
                    string str = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Error");
                    this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str);
                }
                else
                {
                    FileInfo info = new FileInfo(tempFileName);
                    long length = info.Length;
                    this.previewHeader.Text = string.Format(this.fileSizeTextFormat, SizeStringFromBytes(length));
                    this.canvasControl.Visible = true;
                    using (PaintDotNet.Document document = null)
                    {
                        if (this.disposeDocument && (this.documentCanvas.Document != null))
                        {
                            document = this.documentCanvas.Document;
                        }
                        if (this.fileType.IsReflexive(this.SaveConfigToken))
                        {
                            this.documentCanvas.Document = this.Document;
                            this.disposeDocument = false;
                        }
                        else
                        {
                            PaintDotNet.Document document2;
                            FileStream input = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                            try
                            {
                                CleanupManager.RequestCleanup();
                                document2 = this.fileType.Load(input);
                            }
                            catch (Exception)
                            {
                                document2 = null;
                                string str2 = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Error");
                                this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str2);
                            }
                            input.Close();
                            if (document2 != null)
                            {
                                this.documentCanvas.Document = document2;
                                this.disposeDocument = true;
                            }
                            CleanupManager.RequestCleanup();
                        }
                        try
                        {
                            info.Delete();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private Rectangle ValidateAndAdjustNewBounds(Form owner, Rectangle newClientBounds, Rectangle defaultClientBounds)
        {
            Rectangle rectangle3;
            System.Windows.Forms.Screen primaryScreen;
            Rectangle rect = base.ClientBoundsToWindowBounds(newClientBounds);
            bool flag = false;
            foreach (System.Windows.Forms.Screen screen2 in System.Windows.Forms.Screen.AllScreens)
            {
                flag |= screen2.Bounds.IntersectsWith(rect);
            }
            if (flag)
            {
                rectangle3 = newClientBounds;
            }
            else
            {
                rectangle3 = defaultClientBounds;
            }
            if (owner != null)
            {
                primaryScreen = System.Windows.Forms.Screen.FromControl(owner);
            }
            else
            {
                primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            }
            Rectangle bounds = base.ClientBoundsToWindowBounds(rectangle3);
            Rectangle windowBounds = PdnBaseForm.EnsureRectIsOnScreen(primaryScreen, bounds);
            return base.WindowBoundsToClientBounds(windowBounds);
        }

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                this.document = value;
            }
        }

        public PaintDotNet.FileType FileType
        {
            get => 
                this.fileType;
            set
            {
                if ((this.fileType == null) || (this.fileType.Name != value.Name))
                {
                    PaintDotNet.SaveConfigToken lastSaveConfigToken;
                    if (this.fileType != null)
                    {
                        this.fileTypeToSaveToken[this.fileType] = this.SaveConfigToken;
                    }
                    this.fileType = value;
                    if (!this.fileTypeToSaveToken.TryGetValue(this.fileType, out lastSaveConfigToken))
                    {
                        lastSaveConfigToken = this.fileType.GetLastSaveConfigToken();
                    }
                    PaintDotNet.SaveConfigToken token2 = this.fileType.CreateDefaultSaveConfigToken();
                    if (lastSaveConfigToken.GetType() != token2.GetType())
                    {
                        lastSaveConfigToken = null;
                    }
                    if (lastSaveConfigToken == null)
                    {
                        lastSaveConfigToken = this.fileType.CreateDefaultSaveConfigToken();
                    }
                    SaveConfigWidget widget = this.fileType.CreateSaveConfigWidget();
                    widget.Token = lastSaveConfigToken;
                    widget.Location = this.saveConfigWidget.Location;
                    this.OnTokenChanged(this, EventArgs.Empty);
                    this.saveConfigWidget.TokenChanged -= new EventHandler(this.OnTokenChanged);
                    base.SuspendLayout();
                    this.saveConfigPanel.Controls.Remove(this.saveConfigWidget);
                    this.saveConfigWidget = widget;
                    this.saveConfigPanel.Controls.Add(this.saveConfigWidget);
                    base.ResumeLayout(true);
                    this.saveConfigWidget.TokenChanged += new EventHandler(this.OnTokenChanged);
                    if (this.saveConfigWidget is NoSaveConfigWidget)
                    {
                        this.defaultsButton.Enabled = false;
                    }
                    else
                    {
                        this.defaultsButton.Enabled = true;
                    }
                }
            }
        }

        public PaintDotNet.SaveConfigToken SaveConfigToken
        {
            get => 
                this.saveConfigWidget.Token;
            set
            {
                this.saveConfigWidget.Token = value;
            }
        }

        public Surface ScratchSurface
        {
            set
            {
                if (this.scratchSurface != null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("May only set ScratchSurface once, and only before the dialog is shown");
                }
                this.scratchSurface = value;
            }
        }

        private enum WaitActionType
        {
            Ok,
            Cancel
        }
    }
}

