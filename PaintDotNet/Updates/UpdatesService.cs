namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class UpdatesService : Disposable
    {
        private bool calledFinish;
        private IUpdatesServiceHost host;
        private bool installOnExit;
        private static UpdatesService instance;
        private System.Windows.Forms.Timer retryDialogTimer;
        private StateMachineExecutor stateMachineExecutor;
        private static readonly object sync = new object();
        private UpdatesDialog updatesDialog;
        private UpdatesStateMachine updatesStateMachine;

        private UpdatesService(IUpdatesServiceHost host)
        {
            Validate.IsNotNull<IUpdatesServiceHost>(host, "host");
            this.host = host;
            if (((this.updatesStateMachine == null) && !PdnInfo.IsExpired) && (Security.IsAdministrator || Security.CanElevateToAdministrator))
            {
                this.StartUpdates();
            }
        }

        private DialogResult AskInstallNowOrOnExit(IWin32Window owner, string newVersionName, string moreInfoUrl)
        {
            Image reference;
            Func<bool> <>9__1;
            Icon icon = PdnResources.GetImageResource("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference.ToIcon();
            string str = PdnResources.GetString("UpdatePromptTaskDialog.Title");
            ImageResource imageResource = PdnResources.GetImageResource("Images.UpdatePromptTaskDialog.TaskImage.png");
            try
            {
                reference = imageResource.Reference;
            }
            catch (Exception)
            {
                reference = null;
            }
            string str2 = PdnResources.GetString("UpdatePromptTaskDialog.IntroText");
            TaskAuxLabel label = new TaskAuxLabel {
                Text = newVersionName,
                TextFont = new Font(this.host.UISite.Font.FontFamily, this.host.UISite.Font.Size * 1.35f, FontStyle.Regular)
            };
            TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.Updates.InstallAtExit.png").Reference, PdnResources.GetString("UpdatePromptTaskDialog.InstallOnExitTB.ActionText"), PdnResources.GetString("UpdatePromptTaskDialog.InstallOnExitTB.DescriptionText"));
            TaskButton tail = new TaskButton(PdnResources.GetImageResource("Icons.Updates.InstallNow.png").Reference, PdnResources.GetString("UpdatePromptTaskDialog.InstallNowTB.ActionText"), PdnResources.GetString("UpdatePromptTaskDialog.InstallNowTB.DescriptionText"));
            string str3 = PdnResources.GetString("UpdatePromptTaskDialog.AuxButtonText");
            Action auxButtonClickHandler = delegate {
                (<>9__1 ?? (<>9__1 = () => ShellUtil.LaunchUrl2(this.host.UISite.Win32Window, moreInfoUrl))).Eval<bool>().Observe();
            };
            TaskAuxButton button3 = new TaskAuxButton {
                Text = str3
            };
            button3.Clicked += delegate (object s, EventArgs e) {
                auxButtonClickHandler();
            };
            TaskButton[] buttonArray = (from tb in Enumerable.Empty<TaskButton>().Concat<TaskButton>(((PdnInfo.IsExpired || ShellUtil.IsActivityQueuedForRestart) ? null : button)).Concat<TaskButton>(tail)
                where tb > null
                select tb).ToArrayEx<TaskButton>();
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = reference,
                IntroText = str2,
                TaskButtons = buttonArray,
                AcceptButton = tail,
                CancelButton = null,
                PixelWidth96Dpi = (TaskDialog.DefaultPixelWidth96Dpi * 3) / 2
            };
            dialog2.AuxControls = new TaskAuxControl[] { label, button3 };
            TaskButton button4 = dialog2.Show(owner);
            if (button4 == tail)
            {
                return DialogResult.Yes;
            }
            if (button4 == button)
            {
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeUpdates();
            }
            base.Dispose(disposing);
        }

        private void DisposeUpdates()
        {
            if (this.stateMachineExecutor != null)
            {
                this.stateMachineExecutor.StateMachineFinished -= new EventHandler(this.OnStateMachineFinished);
                this.stateMachineExecutor.StateBegin -= new ValueEventHandler<PaintDotNet.Updates.State>(this.OnStateBegin);
                this.stateMachineExecutor.StateWaitingForInput -= new ValueEventHandler<PaintDotNet.Updates.State>(this.OnStateWaitingForInput);
                this.stateMachineExecutor.Dispose();
                this.stateMachineExecutor = null;
            }
            this.updatesStateMachine = null;
        }

        public static IDisposable Initialize(IUpdatesServiceHost host)
        {
            object sync = UpdatesService.sync;
            lock (sync)
            {
                if (instance != null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Initialize has already been called");
                }
                instance = new UpdatesService(host);
                return Disposable.FromAction(delegate {
                    Shutdown();
                }, false);
            }
        }

        private void InitUpdates()
        {
            this.updatesStateMachine = new UpdatesStateMachine();
            this.updatesStateMachine.UIContext = this.host.UISite.Win32Window;
            this.stateMachineExecutor = new StateMachineExecutor(this.updatesStateMachine);
            this.stateMachineExecutor.SyncContext = this.host.SyncContext;
            this.stateMachineExecutor.StateMachineFinished += new EventHandler(this.OnStateMachineFinished);
            this.stateMachineExecutor.StateBegin += new ValueEventHandler<PaintDotNet.Updates.State>(this.OnStateBegin);
            this.stateMachineExecutor.StateWaitingForInput += new ValueEventHandler<PaintDotNet.Updates.State>(this.OnStateWaitingForInput);
        }

        private void OnMainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel)
            {
                switch (e.CloseReason)
                {
                    case CloseReason.WindowsShutDown:
                        return;
                }
                if (this.installOnExit)
                {
                    this.ShowUpdatesDialog(true);
                }
            }
        }

        private void OnStateBegin(object sender, ValueEventArgs<PaintDotNet.Updates.State> e)
        {
            if ((e.Value is UpdateAvailableState) && (this.updatesDialog == null))
            {
                bool flag = true;
                IUpdatesMainFormSite site = this.host.FindMainForm();
                if ((site != null) && !site.IsCurrentModalForm)
                {
                    flag = false;
                }
                if (flag)
                {
                    this.ShowUpdatesDialog();
                }
                else
                {
                    if (this.retryDialogTimer != null)
                    {
                        this.retryDialogTimer.Enabled = false;
                        this.retryDialogTimer.Dispose();
                        this.retryDialogTimer = null;
                    }
                    this.retryDialogTimer = new System.Windows.Forms.Timer();
                    this.retryDialogTimer.Interval = 0xbb8;
                    this.retryDialogTimer.Tick += delegate (object sender2, EventArgs e2) {
                        bool flag = false;
                        if (base.IsDisposed)
                        {
                            flag = true;
                        }
                        IUpdatesMainFormSite site = this.host.FindMainForm();
                        if (site == null)
                        {
                            flag = true;
                        }
                        else if (this.updatesDialog != null)
                        {
                            flag = true;
                        }
                        else if (site.IsCurrentModalForm && site.Enabled)
                        {
                            this.ShowUpdatesDialog();
                            flag = true;
                        }
                        if (flag && (this.retryDialogTimer != null))
                        {
                            this.retryDialogTimer.Enabled = false;
                            this.retryDialogTimer.Dispose();
                            this.retryDialogTimer = null;
                        }
                    };
                    this.retryDialogTimer.Enabled = true;
                }
            }
            else if ((e.Value is ReadyToCheckState) && (this.updatesDialog == null))
            {
                this.DisposeUpdates();
            }
        }

        private void OnStateMachineFinished(object sender, EventArgs e)
        {
            if (!this.installOnExit)
            {
                this.DisposeUpdates();
            }
        }

        private void OnStateWaitingForInput(object sender, ValueEventArgs<PaintDotNet.Updates.State> e)
        {
            InstallingState state = e.Value as InstallingState;
            if (state != null)
            {
                state.Finish(this.host);
                this.calledFinish = true;
            }
        }

        private void OnUISiteHandleCreated(object sender, EventArgs e)
        {
            this.host.UISite.HandleCreated -= new EventHandler(this.OnUISiteHandleCreated);
            this.StartUpdates();
        }

        public void PerformUpdateCheck()
        {
            if (this.updatesStateMachine == null)
            {
                this.InitUpdates();
            }
            this.ShowUpdatesDialog();
        }

        private void ShowUpdatesDialog()
        {
            this.ShowUpdatesDialog(false);
        }

        private void ShowUpdatesDialog(bool calledFromExit)
        {
            if (!calledFromExit)
            {
                if (this.installOnExit && ShellUtil.IsActivityQueuedForRestart)
                {
                    ShellUtil.IsActivityQueuedForRestart = false;
                }
                this.installOnExit = false;
            }
            IUpdatesMainFormSite site = this.host.FindMainForm();
            if (site != null)
            {
                site.FormClosing -= new FormClosingEventHandler(this.OnMainFormFormClosing);
            }
            if (this.retryDialogTimer != null)
            {
                this.retryDialogTimer.Enabled = false;
                this.retryDialogTimer.Dispose();
                this.retryDialogTimer = null;
            }
            bool flag = true;
            UpdateAvailableState currentState = this.stateMachineExecutor.CurrentState as UpdateAvailableState;
            if (currentState != null)
            {
                PdnVersionInfo newVersionInfo = currentState.NewVersionInfo;
                string friendlyName = newVersionInfo.FriendlyName;
                string infoUrl = newVersionInfo.InfoUrl;
                switch (this.AskInstallNowOrOnExit(this.host.UISite.Win32Window, friendlyName, infoUrl))
                {
                    case DialogResult.Yes:
                        this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                        flag = true;
                        goto Label_0134;

                    case DialogResult.OK:
                    {
                        this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                        flag = false;
                        this.installOnExit = true;
                        ShellUtil.IsActivityQueuedForRestart = true;
                        IUpdatesMainFormSite site2 = this.host.FindMainForm();
                        if (site2 != null)
                        {
                            site2.FormClosing += new FormClosingEventHandler(this.OnMainFormFormClosing);
                        }
                        else
                        {
                            flag = true;
                        }
                        goto Label_0134;
                    }
                }
                this.stateMachineExecutor.ProcessInput(UpdatesAction.Cancel);
                flag = false;
                this.DisposeUpdates();
            }
        Label_0134:
            if (flag)
            {
                IWin32Window window;
                if (this.updatesDialog != null)
                {
                    this.updatesDialog.Close();
                    this.updatesDialog = null;
                }
                this.updatesDialog = new UpdatesDialog();
                this.updatesDialog.InstallingOnExit = calledFromExit;
                this.updatesDialog.UpdatesStateMachine = this.stateMachineExecutor;
                if (!this.stateMachineExecutor.IsStarted)
                {
                    this.stateMachineExecutor.Start();
                }
                try
                {
                    IntPtr handle = this.host.UISite.Win32Window.Handle;
                    window = this.host.UISite.Win32Window;
                }
                catch (Exception)
                {
                    window = null;
                }
                this.updatesDialog.StartPosition = calledFromExit ? FormStartPosition.CenterScreen : this.updatesDialog.StartPosition;
                this.updatesDialog.ShowInTaskbar = calledFromExit;
                this.updatesDialog.Shown += (s, e) => UIUtil.FlashForm(this.updatesDialog);
                this.updatesDialog.ShowDialog(window);
                DialogResult dialogResult = this.updatesDialog.DialogResult;
                this.updatesDialog.Dispose();
                this.updatesDialog = null;
                if (((this.stateMachineExecutor != null) && (dialogResult == DialogResult.Yes)) && (this.stateMachineExecutor.CurrentState is ReadyToInstallState))
                {
                    this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                    while (!this.calledFinish)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private static void Shutdown()
        {
            object sync = UpdatesService.sync;
            lock (sync)
            {
                if (instance != null)
                {
                    instance.Dispose();
                    instance = null;
                }
            }
        }

        private void StartUpdates()
        {
            if (!this.host.UISite.IsHandleCreated)
            {
                this.host.UISite.HandleCreated += new EventHandler(this.OnUISiteHandleCreated);
            }
            else
            {
                this.InitUpdates();
                this.stateMachineExecutor.Start();
            }
        }

        public bool InstallingOnExit =>
            this.installOnExit;

        public static UpdatesService Instance
        {
            get
            {
                if (instance == null)
                {
                    object sync = UpdatesService.sync;
                    lock (sync)
                    {
                        if (instance == null)
                        {
                            ExceptionUtil.ThrowInvalidOperationException("Initialize must be called first");
                        }
                    }
                }
                return instance;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UpdatesService.<>c <>9 = new UpdatesService.<>c();
            public static Func<TaskButton, bool> <>9__18_3;
            public static Action <>9__4_0;

            internal bool <AskInstallNowOrOnExit>b__18_3(TaskButton tb) => 
                (tb > null);

            internal void <Initialize>b__4_0()
            {
                UpdatesService.Shutdown();
            }
        }
    }
}

