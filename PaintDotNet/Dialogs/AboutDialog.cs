namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class AboutDialog : PdnBaseFormInternal
    {
        private PdnPushButton closeButton;
        private PdnLabel copyrightLabel;
        private PdnBanner pdnBanner;
        private PaintDotNet.Controls.SeparatorLine separator;
        private PdnLabel versionLabel;

        public AboutDialog()
        {
            this.DoubleBuffered = true;
            base.SuspendLayout();
            this.InitializeComponent();
            string format = PdnResources.GetString("AboutDialog.Text.Format");
            this.Text = string.Format(format, PdnInfo.BareProductName);
            this.copyrightLabel.Text = PdnInfo.CopyrightString;
            base.Icon = PdnResources.GetIconFromImage("Icons.MenuHelpAboutIcon.png");
            this.closeButton.Text = PdnResources.GetString("Form.CloseButton.Text");
            this.versionLabel.Text = PdnInfo.FullAppName;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeComponent()
        {
            this.closeButton = new PdnPushButton();
            this.copyrightLabel = new PdnLabel();
            this.pdnBanner = new PdnBanner();
            this.versionLabel = new PdnLabel();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            base.SuspendLayout();
            this.closeButton.DialogResult = DialogResult.Cancel;
            this.closeButton.AutoSize = true;
            this.closeButton.Name = "okButton";
            this.closeButton.TabIndex = 0;
            this.copyrightLabel.BorderStyle = BorderStyle.None;
            this.copyrightLabel.Location = new Point(10, 0x5f);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new Size(0x1e1, 0x24);
            this.copyrightLabel.TabIndex = 4;
            this.pdnBanner.Location = new Point(0, 0);
            this.pdnBanner.Name = "pdnBanner";
            this.pdnBanner.Size = new Size(0x1ef, 0x47);
            this.pdnBanner.TabIndex = 7;
            this.versionLabel.BorderStyle = BorderStyle.None;
            this.versionLabel.Location = new Point(10, 0x4d);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new Size(0x1e1, 13);
            this.versionLabel.TabIndex = 8;
            this.separator.Name = "separator";
            base.AcceptButton = this.closeButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.closeButton;
            base.ClientSize = new Size(0x1ef, 0xa8);
            base.Controls.Add(this.versionLabel);
            base.Controls.Add(this.copyrightLabel);
            base.Controls.Add(this.pdnBanner);
            base.Controls.Add(this.separator);
            base.Controls.Add(this.closeButton);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AboutDialog";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.closeButton, 0);
            base.Controls.SetChildIndex(this.pdnBanner, 0);
            base.Controls.SetChildIndex(this.copyrightLabel, 0);
            base.Controls.SetChildIndex(this.versionLabel, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num3;
            int num4;
            int x = UIUtil.ScaleWidth(7);
            int num2 = UIUtil.ScaleHeight(8);
            if (base.IsGlassEffectivelyEnabled)
            {
                num3 = 0;
                num4 = -1;
                this.separator.Visible = false;
            }
            else
            {
                num3 = num2;
                num4 = UIUtil.ScaleWidth(7);
                this.separator.Visible = true;
            }
            int width = base.ClientSize.Width - (x * 2);
            this.pdnBanner.PerformLayout();
            this.versionLabel.Location = new Point(x, this.pdnBanner.Bottom + num2);
            this.versionLabel.Size = this.versionLabel.GetPreferredSize(width, 1);
            this.copyrightLabel.Location = new Point(x, this.versionLabel.Bottom + num2);
            this.copyrightLabel.Size = this.copyrightLabel.GetPreferredSize(width, 1);
            this.separator.Size = this.separator.GetPreferredSize(width, 1);
            this.separator.Location = new Point(x, this.copyrightLabel.Bottom + num2);
            this.closeButton.Size = UIUtil.ScaleSize(0x55, 0x18);
            this.closeButton.PerformLayout();
            this.closeButton.Location = new Point((base.ClientSize.Width - num4) - this.closeButton.Width, this.separator.Bottom + num2);
            base.ClientSize = new Size(base.ClientSize.Width, this.closeButton.Bottom + num3);
            base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
            base.OnLayout(levent);
        }
    }
}

