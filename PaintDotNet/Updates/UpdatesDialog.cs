namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class UpdatesDialog : PdnBaseFormInternal
    {
        private PdnPushButton closeButton;
        private bool closeOnDoneState;
        private PdnPushButton continueButton;
        private PdnLabel infoText;
        private bool layoutQueued;
        private PdnLinkLabel moreInfoLink;
        private Uri moreInfoTarget;
        private ProgressBar progressBar;
        private PdnLabel progressLabel;
        private PaintDotNet.Controls.SeparatorLine separator;
        private bool setFonts;
        private StateMachineExecutor updatesStateMachine;
        private PdnLabel versionNameLabel;

        public UpdatesDialog()
        {
            this.InstallingOnExit = false;
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            this.InitializeComponent();
            base.Icon = PdnResources.GetImageResource("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference.ToIcon();
        }

        private void EnqueueLayout()
        {
            if (!this.layoutQueued && base.IsHandleCreated)
            {
                this.layoutQueued = true;
                try
                {
                    base.BeginInvoke(delegate {
                        if (base.IsHandleCreated && !base.IsDisposed)
                        {
                            base.PerformLayout();
                            this.QueueUpdate();
                        }
                    });
                }
                catch (Exception exception)
                {
                    if (!(exception is InvalidOperationException) && !(exception is ObjectDisposedException))
                    {
                        throw;
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.closeButton = new PdnPushButton();
            this.continueButton = new PdnPushButton();
            this.progressBar = new ProgressBar();
            this.infoText = new PdnLabel();
            this.moreInfoLink = new PdnLinkLabel();
            this.versionNameLabel = new PdnLabel();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            this.progressLabel = new PdnLabel();
            base.SuspendLayout();
            this.closeButton.AutoSize = true;
            this.closeButton.Name = "closeButton";
            this.closeButton.TabIndex = 0;
            this.closeButton.Click += new EventHandler(this.OnCloseButtonClick);
            this.continueButton.AutoSize = true;
            this.continueButton.AutoScaleImage = false;
            this.continueButton.Name = "continueButton";
            this.continueButton.TabIndex = 3;
            this.continueButton.Click += new EventHandler(this.OnContinueButtonClick);
            this.progressBar.MarqueeAnimationSpeed = 40;
            this.progressBar.Name = "progressBar";
            this.progressBar.TabIndex = 4;
            this.infoText.Name = "infoText";
            this.infoText.TabIndex = 2;
            this.moreInfoLink.Name = "moreInfoLink";
            this.moreInfoLink.TabIndex = 5;
            this.moreInfoLink.TabStop = true;
            this.moreInfoLink.Click += new EventHandler(this.OnMoreInfoLinkClick);
            this.versionNameLabel.Name = "versionNameLabel";
            this.versionNameLabel.TabIndex = 6;
            this.separator.Name = "headerLabel";
            this.separator.TabIndex = 0;
            this.separator.TabStop = false;
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.TabIndex = 8;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.closeButton;
            base.ClientSize = new Size(0x157, 0xac);
            base.Controls.Add(this.progressLabel);
            base.Controls.Add(this.separator);
            base.Controls.Add(this.versionNameLabel);
            base.Controls.Add(this.moreInfoLink);
            base.Controls.Add(this.continueButton);
            base.Controls.Add(this.infoText);
            base.Controls.Add(this.closeButton);
            base.Controls.Add(this.progressBar);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UpdatesDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            if (this.updatesStateMachine != null)
            {
                this.updatesStateMachine.Abort();
                this.updatesStateMachine = null;
                this.closeButton.Enabled = false;
            }
            base.Close();
        }

        private void OnContinueButtonClick(object sender, EventArgs e)
        {
            if (this.updatesStateMachine.CurrentState is ReadyToInstallState)
            {
                base.DialogResult = DialogResult.Yes;
                base.Hide();
                base.Close();
            }
            else
            {
                this.updatesStateMachine.ProcessInput(UpdatesAction.Continue);
                this.continueButton.Enabled = false;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            UpdatesState currentState;
            int bottom;
            this.layoutQueued = false;
            if (!this.setFonts && base.IsHandleCreated)
            {
                this.setFonts = true;
                this.versionNameLabel.Font = new Font(this.Font.FontFamily, this.Font.Size * 1.25f, FontStyle.Regular);
            }
            this.Text = PdnResources.GetString("UpdatesDialog.Text");
            string str = PdnResources.GetString("UpdatesDialog.CloseButton.Text");
            this.moreInfoLink.Text = PdnResources.GetString("UpdatesDialog.MoreInfoLink.Text");
            if ((this.updatesStateMachine == null) || (this.updatesStateMachine.CurrentState == null))
            {
                currentState = null;
            }
            else
            {
                currentState = (UpdatesState) this.updatesStateMachine.CurrentState;
            }
            INewVersionInfo info = currentState as INewVersionInfo;
            if (currentState == null)
            {
                this.infoText.Text = string.Empty;
                this.continueButton.Text = string.Empty;
                this.continueButton.Enabled = false;
                this.continueButton.Visible = false;
                this.moreInfoLink.Visible = false;
                this.moreInfoLink.Enabled = false;
                this.versionNameLabel.Visible = false;
                this.versionNameLabel.Enabled = false;
                goto Label_0265;
            }
            if (currentState is ReadyToInstallState)
            {
                ((ReadyToInstallState) currentState).InstallingOnExit = this.InstallingOnExit;
            }
            if ((currentState is ReadyToInstallState) || (currentState is UpdateAvailableState))
            {
                using (Icon icon = UIUtil.GetStockIcon(UIUtil.StockIcon.Shield, UIUtil.StockIconFlags.SmallIcon))
                {
                    this.continueButton.Image = icon.ToBitmap();
                    goto Label_017B;
                }
            }
            this.continueButton.Image = null;
        Label_017B:
            this.infoText.Text = currentState.InfoText;
            this.continueButton.Text = currentState.ContinueButtonText;
            this.continueButton.Visible = currentState.ContinueButtonVisible;
            this.continueButton.Enabled = currentState.ContinueButtonVisible;
            this.progressBar.Style = (currentState.MarqueeStyle == MarqueeStyle.Marquee) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            this.progressBar.Visible = currentState.MarqueeStyle > MarqueeStyle.None;
            this.progressLabel.Visible = this.progressBar.Visible;
            if ((this.continueButton.Enabled || (currentState is ErrorState)) || (currentState is DoneState))
            {
                str = PdnResources.GetString("UpdatesDialog.CloseButton.Text");
            }
            else
            {
                str = PdnResources.GetString("Form.CancelButton.Text");
            }
            if (info != null)
            {
                this.versionNameLabel.Text = info.NewVersionInfo.FriendlyName;
                this.moreInfoTarget = new Uri(info.NewVersionInfo.InfoUrl);
            }
        Label_0265:
            this.closeButton.Text = str;
            int num = UIUtil.ScaleWidth(8);
            int num2 = UIUtil.ScaleHeight(8);
            int x = UIUtil.ScaleWidth(8);
            int y = UIUtil.ScaleHeight(8);
            int num5 = UIUtil.ScaleWidth(8);
            int num6 = Math.Max(0, num2);
            int width = (base.ClientSize.Width - x) - num5;
            Size size = UIUtil.ScaleSize(0x55, 0x18);
            int num9 = base.IsGlassEffectivelyEnabled ? -1 : x;
            this.infoText.Location = new Point(x, y);
            this.infoText.Width = width;
            this.infoText.Size = this.infoText.GetPreferredSize(this.infoText.Width, 1);
            if (((currentState is UpdateAvailableState) || (currentState is DownloadingState)) || ((currentState is ReadyToInstallState) || (info != null)))
            {
                this.versionNameLabel.Size = this.versionNameLabel.GetPreferredSize(width, 1);
                this.versionNameLabel.Enabled = true;
                this.versionNameLabel.Visible = true;
                this.moreInfoLink.Size = this.moreInfoLink.GetPreferredSize(width, 1);
                this.moreInfoLink.Enabled = true;
                this.moreInfoLink.Visible = true;
            }
            else
            {
                this.versionNameLabel.Size = new Size(width, 0);
                this.versionNameLabel.Enabled = false;
                this.versionNameLabel.Visible = false;
                this.moreInfoLink.Size = new Size(width, 0);
                this.moreInfoLink.Enabled = false;
                this.moreInfoLink.Visible = false;
            }
            this.versionNameLabel.Location = new Point(this.infoText.Left, this.infoText.Bottom + num2);
            if ((width - this.versionNameLabel.Width) < (num + this.moreInfoLink.Width))
            {
                this.moreInfoLink.Location = new Point(this.versionNameLabel.Left, this.versionNameLabel.Bottom + (num2 / 2));
            }
            else
            {
                this.moreInfoLink.Location = new Point(this.versionNameLabel.Right + num, (this.versionNameLabel.Bottom - this.moreInfoLink.Height) - 1);
            }
            int num10 = this.versionNameLabel.Visible ? (this.moreInfoLink.Bottom + ((num2 * 3) / 2)) : (this.infoText.Bottom + num2);
            if (((currentState is CheckingState) || (currentState is DownloadingState)) || (currentState is ExtractingState))
            {
                if (currentState is CheckingState)
                {
                    this.progressLabel.Enabled = false;
                    this.progressLabel.Visible = false;
                }
                else
                {
                    this.progressLabel.Enabled = true;
                    this.progressLabel.Visible = true;
                }
                this.progressBar.Enabled = true;
                this.progressBar.Visible = true;
            }
            else
            {
                this.progressLabel.Enabled = false;
                this.progressLabel.Visible = false;
                this.progressBar.Enabled = false;
                this.progressBar.Visible = false;
            }
            this.progressLabel.Size = this.progressLabel.GetPreferredSize(width, 1);
            if (!this.progressLabel.Visible)
            {
                this.progressLabel.Width = 0;
            }
            this.progressLabel.Location = new Point((base.ClientSize.Width - num5) - this.progressLabel.Width, num10);
            this.progressBar.Location = new Point(x, this.progressLabel.Top + ((this.progressLabel.Height - this.progressBar.Height) / 2));
            this.progressBar.Height = UIUtil.ScaleHeight(0x12);
            this.progressBar.Width = this.progressLabel.Visible ? ((this.progressLabel.Left - x) - num) : width;
            int[] vals = new int[] { this.versionNameLabel.Visible ? (this.versionNameLabel.Bottom + (num2 / 2)) : 0, this.moreInfoLink.Visible ? this.moreInfoLink.Bottom : 0, this.progressLabel.Visible ? this.progressLabel.Bottom : 0, this.progressBar.Visible ? this.progressBar.Bottom : 0 };
            int num11 = num2 + Int32Util.Max(this.infoText.Bottom, vals);
            this.separator.Location = new Point(x, num11);
            this.separator.Size = this.separator.GetPreferredSize(width, 1);
            if ((currentState is ReadyToInstallState) || (currentState is UpdateAvailableState))
            {
                this.closeButton.Enabled = false;
                this.closeButton.Visible = false;
                this.continueButton.Enabled = true;
                this.continueButton.Visible = true;
                this.continueButton.Size = size;
                this.continueButton.PerformLayout();
                this.continueButton.Location = new Point((base.ClientSize.Width - num9) - this.continueButton.Width, this.separator.Bottom + num2);
                bottom = this.continueButton.Bottom;
            }
            else
            {
                this.closeButton.Enabled = true;
                this.closeButton.Visible = true;
                this.closeButton.Size = size;
                this.closeButton.PerformLayout();
                this.closeButton.Location = new Point((base.ClientSize.Width - num9) - this.closeButton.Width, this.separator.Bottom + num2);
                this.continueButton.Enabled = false;
                this.continueButton.Visible = false;
                bottom = this.closeButton.Bottom;
            }
            base.ClientSize = new Size(base.ClientSize.Width, bottom + num6);
            if (base.IsGlassEffectivelyEnabled)
            {
                this.separator.Visible = false;
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
            }
            else
            {
                this.separator.Visible = true;
                base.GlassInset = new Padding(0);
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.updatesStateMachine.CurrentState is ReadyToInstallState)
            {
                ValueEventArgs<PaintDotNet.Updates.State> args = ValueEventArgs.Get<PaintDotNet.Updates.State>(this.updatesStateMachine.CurrentState);
                this.OnUpdatesStateMachineStateBegin(this, args);
                args.Return();
            }
            base.OnLoad(e);
        }

        private void OnMoreInfoLinkClick(object sender, EventArgs e)
        {
            PdnInfo.OpenUrl2(this, this.moreInfoTarget.ToString());
        }

        private void OnUpdatesStateMachineStateBegin(object sender, ValueEventArgs<PaintDotNet.Updates.State> e)
        {
            this.progressBar.Value = 0;
            this.UpdateDynamicUI();
            if ((e.Value is DoneState) && this.closeOnDoneState)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
            else if (e.Value is ReadyToCheckState)
            {
                this.updatesStateMachine.ProcessInput(UpdatesAction.Continue);
            }
            else if (e.Value is AbortedState)
            {
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
        }

        private void OnUpdatesStateMachineStateMachineBegin(object sender, EventArgs e)
        {
            this.UpdateDynamicUI();
        }

        private void OnUpdatesStateMachineStateMachineFinished(object sender, EventArgs e)
        {
            this.UpdateDynamicUI();
        }

        private void OnUpdatesStateMachineStateProgress(object sender, ProgressEventArgs e)
        {
            int num = ((int) e.Percent).Clamp(this.progressBar.Minimum, this.progressBar.Maximum);
            this.progressBar.Value = num;
            string str2 = string.Format(PdnResources.GetString("UpdatesDialog.ProgressLabel.Text.Format"), num.ToString());
            this.progressLabel.Text = str2;
            this.UpdateDynamicUI();
        }

        private void OnUpdatesStateMachineStateWaitingForInput(object sender, ValueEventArgs<PaintDotNet.Updates.State> e)
        {
            this.continueButton.Enabled = true;
            this.UpdateDynamicUI();
        }

        private void UpdateDynamicUI()
        {
            this.EnqueueLayout();
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UIUtil.AddCompositedExStyleToCreateParams(createParams);
                return createParams;
            }
        }

        public bool InstallingOnExit { get; set; }

        public StateMachineExecutor UpdatesStateMachine
        {
            get => 
                this.updatesStateMachine;
            set
            {
                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin -= new ValueEventHandler<PaintDotNet.Updates.State>(this.OnUpdatesStateMachineStateBegin);
                    this.updatesStateMachine.StateMachineBegin -= new EventHandler(this.OnUpdatesStateMachineStateMachineBegin);
                    this.updatesStateMachine.StateMachineFinished -= new EventHandler(this.OnUpdatesStateMachineStateMachineFinished);
                    this.updatesStateMachine.StateProgress -= new ProgressEventHandler(this.OnUpdatesStateMachineStateProgress);
                    this.updatesStateMachine.StateWaitingForInput -= new ValueEventHandler<PaintDotNet.Updates.State>(this.OnUpdatesStateMachineStateWaitingForInput);
                }
                this.updatesStateMachine = value;
                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin += new ValueEventHandler<PaintDotNet.Updates.State>(this.OnUpdatesStateMachineStateBegin);
                    this.updatesStateMachine.StateMachineBegin += new EventHandler(this.OnUpdatesStateMachineStateMachineBegin);
                    this.updatesStateMachine.StateMachineFinished += new EventHandler(this.OnUpdatesStateMachineStateMachineFinished);
                    this.updatesStateMachine.StateProgress += new ProgressEventHandler(this.OnUpdatesStateMachineStateProgress);
                    this.updatesStateMachine.StateWaitingForInput += new ValueEventHandler<PaintDotNet.Updates.State>(this.OnUpdatesStateMachineStateWaitingForInput);
                }
                this.UpdateDynamicUI();
            }
        }
    }
}

