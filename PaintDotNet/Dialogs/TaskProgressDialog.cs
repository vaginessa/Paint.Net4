namespace PaintDotNet.Dialogs
{
    using Microsoft.WindowsAPICodePack.Taskbar;
    using PaintDotNet;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class TaskProgressDialog : PdnBaseFormInternal
    {
        private PdnPushButton cancelButton;
        private bool closeOnFinished = true;
        private ControlDispatcher dispatcher;
        private Label headerLabel;
        private ProgressBar progressBar;
        private Action queueSyncCallback;
        private PaintDotNet.Controls.SeparatorLine separator;
        private PaintDotNet.Threading.Tasks.Task task;
        private List<Action> taskEventTickets;

        public TaskProgressDialog()
        {
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            this.dispatcher = new ControlDispatcher(this);
            this.headerLabel = new PdnLabel();
            this.progressBar = new ProgressBar();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            this.cancelButton = new PdnPushButton();
            this.headerLabel.Name = "headerLabel";
            this.progressBar.Name = "progressBar";
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
            this.separator.Name = "separator";
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.AutoSize = true;
            this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AutoScaleMode = AutoScaleMode.None;
            base.AcceptButton = null;
            base.CancelButton = this.cancelButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            Control[] controls = new Control[] { this.headerLabel, this.progressBar, this.separator, this.cancelButton };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Task = null;
            }
            base.Dispose(disposing);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            if (this.Task != null)
            {
                this.Task.RequestCancel();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            TaskbarManager.Instance.SetProgressValue(0, 0);
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            TaskbarManager.Instance.SetOverlayIcon(null, null);
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((this.Task != null) && (this.Task.State != TaskState.Finished))
            {
                this.Task.RequestCancel();
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (this.Task != null)
            {
                this.QueueSync();
            }
            base.OnHandleCreated(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!base.IsDisposed)
            {
                int x = UIUtil.ScaleWidth(7);
                int y = UIUtil.ScaleHeight(7);
                int num3 = base.IsGlassEffectivelyEnabled ? 0 : y;
                int width = UIUtil.ScaleWidth(300);
                this.headerLabel.Location = new Point(x - 3, y);
                this.headerLabel.Size = this.headerLabel.GetPreferredSize(new Size(width, 1));
                this.progressBar.Bounds = new Rectangle(x + 1, this.headerLabel.Bottom + y, (width - (2 * x)) - 2, UIUtil.ScaleHeight(0x12));
                this.separator.Visible = this.cancelButton.Visible;
                this.separator.Location = new Point(x, (1 + this.progressBar.Bottom) + y);
                if (this.separator.Visible)
                {
                    this.separator.Size = this.separator.GetPreferredSize(new Size(width - (2 * x), 1));
                }
                else
                {
                    this.separator.Size = new Size(width - x, 0);
                }
                this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
                this.cancelButton.Size = UIUtil.ScaleSize(0x4b, 0x17);
                this.cancelButton.PerformLayout();
                int num5 = base.IsGlassEffectivelyEnabled ? -1 : x;
                this.cancelButton.Location = new Point((width - num5) - this.cancelButton.Width, this.separator.Bottom + y);
                if (!this.cancelButton.Visible)
                {
                    this.cancelButton.Height = 0;
                    this.cancelButton.Location = new Point(x, this.progressBar.Bottom + y);
                }
                int height = this.cancelButton.Bottom + num3;
                base.ClientSize = new Size(width, height);
                if (base.IsGlassEffectivelyEnabled && this.cancelButton.Visible)
                {
                    this.separator.Visible = false;
                    base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
                }
                else
                {
                    this.separator.Visible = this.separator.Visible;
                    base.GlassInset = new Padding(0);
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnShown(EventArgs e)
        {
            TaskbarManager.Instance.SetOverlayIcon(base.Icon, null);
            if (this.Task != null)
            {
                this.QueueSync();
            }
            if (((this.Task != null) && (this.Task.State == TaskState.Finished)) && this.CloseOnFinished)
            {
                base.Close();
            }
            UIUtil.EnableCloseBox(this, this.ShowCancelButton);
            base.OnShown(e);
        }

        private void QueueSync()
        {
            if (this.queueSyncCallback == null)
            {
                this.queueSyncCallback = delegate {
                    if (base.IsHandleCreated)
                    {
                        this.Sync();
                    }
                };
            }
            PdnSynchronizationContext.Instance.EnsurePosted(this.queueSyncCallback);
        }

        public void SetHeaderTextAsync(string newHeaderText)
        {
            if (base.InvokeRequired)
            {
                base.Dispatcher.BeginTry(() => this.HeaderText = newHeaderText).Observe();
            }
            else
            {
                this.HeaderText = newHeaderText;
            }
        }

        private void Sync()
        {
            this.VerifyThreadAccess();
            PaintDotNet.Threading.Tasks.Task task = this.Task;
            if (task != null)
            {
                double? progress;
                TaskState state = task.State;
                bool isCancelRequested = task.IsCancelRequested;
                this.cancelButton.Enabled = !isCancelRequested && (state != TaskState.Finished);
                switch (state)
                {
                    case TaskState.Finished:
                        progress = 1.0;
                        break;

                    case TaskState.NotYetRunning:
                        progress = null;
                        break;

                    default:
                        if (isCancelRequested)
                        {
                            progress = null;
                        }
                        else
                        {
                            if (state != TaskState.Running)
                            {
                                throw ExceptionUtil.InvalidEnumArgumentException<TaskState>(state, "state");
                            }
                            progress = task.Progress;
                        }
                        break;
                }
                if (progress.HasValue)
                {
                    double num = this.progressBar.Minimum + (progress.Value * (this.progressBar.Maximum - this.progressBar.Minimum));
                    int currentValue = (int) num;
                    this.progressBar.Value = currentValue;
                    this.progressBar.Style = ProgressBarStyle.Continuous;
                    TaskbarManager.Instance.SetProgressValue(currentValue, 100);
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                }
                else
                {
                    this.progressBar.Style = ProgressBarStyle.Marquee;
                    TaskbarManager.Instance.SetProgressValue(0, 0);
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                }
                if (((state == TaskState.Finished) && this.closeOnFinished) && (base.IsShown && base.IsHandleCreated))
                {
                    base.Close();
                }
                if (((state == TaskState.Finished) && !this.closeOnFinished) && (base.IsShown && base.IsHandleCreated))
                {
                    this.cancelButton.Text = PdnResources.GetString("Form.CloseButton.Text");
                }
            }
        }

        public bool CloseOnFinished
        {
            get => 
                this.closeOnFinished;
            set
            {
                base.VerifyAccess();
                this.closeOnFinished = value;
                if (((value && (this.Task != null)) && ((this.Task.State == TaskState.Finished) && base.IsShown)) && base.IsHandleCreated)
                {
                    base.Close();
                }
            }
        }

        public string HeaderText
        {
            get => 
                this.headerLabel.Text;
            set
            {
                this.VerifyThreadAccess();
                if (!string.Equals(this.headerLabel.Text, value, StringComparison.Ordinal))
                {
                    this.headerLabel.Text = value;
                    base.PerformLayout();
                }
            }
        }

        public bool ShowCancelButton
        {
            get => 
                this.cancelButton.Visible;
            set
            {
                this.VerifyThreadAccess();
                UIUtil.EnableCloseBox(this, value);
                this.cancelButton.Visible = value;
                base.PerformLayout();
            }
        }

        public PaintDotNet.Threading.Tasks.Task Task
        {
            get => 
                this.task;
            set
            {
                this.VerifyThreadAccess();
                if ((this.task != null) && (this.taskEventTickets != null))
                {
                    this.taskEventTickets.ForEach(f => f());
                    this.taskEventTickets = null;
                }
                if (value != null)
                {
                    this.task = value;
                    List<Action> list = new List<Action>();
                    EventHandler cancelRequestedHandler = (s2, e2) => this.QueueSync();
                    ValueEventHandler<double?> progressChangedHandler = (s2, e2) => this.QueueSync();
                    ValueEventHandler<TaskState> taskStateChangedHandler = (s2, e2) => this.QueueSync();
                    this.task.CancelRequested += cancelRequestedHandler;
                    this.task.ProgressChanged += progressChangedHandler;
                    this.task.StateChanged += taskStateChangedHandler;
                    list.Add(delegate {
                        this.task.CancelRequested -= cancelRequestedHandler;
                    });
                    list.Add(delegate {
                        this.task.ProgressChanged -= progressChangedHandler;
                    });
                    list.Add(delegate {
                        this.task.StateChanged -= taskStateChangedHandler;
                    });
                    this.taskEventTickets = list;
                    if (base.IsHandleCreated)
                    {
                        this.QueueSync();
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TaskProgressDialog.<>c <>9 = new TaskProgressDialog.<>c();
            public static Action<Action> <>9__18_0;

            internal void <set_Task>b__18_0(Action f)
            {
                f();
            }
        }
    }
}

