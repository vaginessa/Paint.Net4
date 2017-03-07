namespace PaintDotNet.Dialogs
{
    using Microsoft.WindowsAPICodePack.Taskbar;
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class ExceptionDialog : PdnBaseFormInternal
    {
        private PaintDotNet.Controls.SeparatorLine bottomSeparator;
        private PdnPushButton button1;
        private PdnPushButton button2;
        private PdnPushButton copyToClipboardButton;
        private string crashLogDir;
        private TextBox crashLogTextBox;
        private PdnPushButton detailsButton;
        private PictureBox errorIconBox;
        private string exceptionText;
        private PictureBox folderIconBox;
        private Size? maxClientSize;
        private PdnLabel message2Label;
        private PdnLabel messageLabel;
        private Size? minClientSize;
        private PdnLinkLabel openFolderLink;
        private ToolTip toolTip;

        public ExceptionDialog()
        {
            base.SuspendLayout();
            this.toolTip = new ToolTip();
            base.Icon = PdnInfo.AppIcon;
            this.errorIconBox = new PictureBox();
            this.errorIconBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.errorIconBox.Image = this.GetScaledWarningIcon();
            this.messageLabel = new PdnLabel();
            this.message2Label = new PdnLabel();
            this.bottomSeparator = new PaintDotNet.Controls.SeparatorLine();
            this.detailsButton = new PdnPushButton();
            this.detailsButton.Click += new EventHandler(this.OnDetailsButtonClick);
            this.detailsButton.AutoSize = true;
            this.button1 = new PdnPushButton();
            this.button1.AutoSize = true;
            this.button1.DialogResult = DialogResult.OK;
            this.button1.Visible = false;
            this.button2 = new PdnPushButton();
            this.button2.Text = PdnResources.GetString("Form.OkButton.Text");
            this.button2.AutoSize = true;
            this.button2.DialogResult = DialogResult.Cancel;
            this.copyToClipboardButton = new PdnPushButton();
            this.copyToClipboardButton.Text = PdnResources.GetString("ExceptionDialog.CopyToClipboardButton.Text");
            this.copyToClipboardButton.AutoSize = true;
            this.copyToClipboardButton.Click += new EventHandler(this.OnCopyToClipboardButtonClick);
            this.folderIconBox = new PictureBox();
            this.folderIconBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.folderIconBox.Image = PdnResources.GetImageResource("Icons.FolderShortcut.png").Reference;
            this.folderIconBox.Visible = false;
            this.openFolderLink = new PdnLinkLabel();
            this.openFolderLink.Text = PdnResources.GetString("ExceptionDialog.OpenFolderLink.Text");
            this.openFolderLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnOpenFolderLinkLinkClicked);
            this.openFolderLink.Visible = false;
            this.openFolderLink.AutoSize = true;
            this.crashLogTextBox = new TextBox();
            this.crashLogTextBox.Multiline = true;
            this.crashLogTextBox.ReadOnly = true;
            this.crashLogTextBox.ScrollBars = ScrollBars.Vertical;
            this.crashLogTextBox.Font = new Font(FontFamily.GenericMonospace, this.crashLogTextBox.Font.Size);
            this.crashLogTextBox.Visible = false;
            this.crashLogTextBox.KeyPress += new KeyPressEventHandler(this.OnCrashLogTextBoxKeyPress);
            this.detailsButton.Text = this.GetDetailsButtonText();
            this.detailsButton.TabIndex = 0;
            this.button1.TabIndex = 1;
            this.button2.TabIndex = 2;
            this.copyToClipboardButton.TabIndex = 3;
            this.openFolderLink.TabIndex = 4;
            this.crashLogTextBox.TabIndex = 5;
            base.Controls.Add(this.errorIconBox);
            base.Controls.Add(this.messageLabel);
            base.Controls.Add(this.message2Label);
            base.Controls.Add(this.bottomSeparator);
            base.Controls.Add(this.detailsButton);
            base.Controls.Add(this.button2);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.copyToClipboardButton);
            base.Controls.Add(this.folderIconBox);
            base.Controls.Add(this.openFolderLink);
            base.Controls.Add(this.crashLogTextBox);
            this.Text = PdnInfo.BareProductName;
            this.BackColor = SystemColors.Window;
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.AcceptButton = this.button1;
            base.CancelButton = this.button2;
            base.ClientSize = new Size(UIUtil.ScaleWidth(450), base.ClientSize.Height);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            base.ShowInTaskbar = false;
            base.ResumeLayout(false);
            base.PerformLayout();
            this.MinimumSize = base.Size;
            this.button1.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.toolTip != null))
            {
                this.toolTip.Dispose();
                this.toolTip = null;
            }
            base.Dispose(disposing);
        }

        private string GetDetailsButtonText()
        {
            if (!this.crashLogTextBox.Visible)
            {
                return PdnResources.GetString("ExceptionDialog.DetailsButton.Show.Text");
            }
            return PdnResources.GetString("ExceptionDialog.DetailsButton.Hide.Text");
        }

        private Image GetScaledWarningIcon()
        {
            int num2;
            double num = UIUtil.ScaleWidth((double) 16.0);
            if (num >= 32.0)
            {
                num2 = 0x20;
            }
            else if (num >= 24.0)
            {
                num2 = 0x18;
            }
            else
            {
                num2 = 0x10;
            }
            return PdnResources.GetImageResource($"Images.Warning{num2}.png").Reference;
        }

        private void OnCopyToClipboardButtonClick(object sender, EventArgs e)
        {
            try
            {
                PdnClipboard.SetText(this.crashLogTextBox.Text);
            }
            catch (Exception)
            {
            }
        }

        private void OnCrashLogTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\x0001')
            {
                this.crashLogTextBox.SelectAll();
                e.Handled = true;
            }
        }

        private void OnDetailsButtonClick(object sender, EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                this.crashLogTextBox.Visible = !this.crashLogTextBox.Visible;
                this.copyToClipboardButton.Visible = this.crashLogTextBox.Visible;
                this.folderIconBox.Visible = !string.IsNullOrEmpty(this.crashLogDir) && this.openFolderLink.Visible;
                this.openFolderLink.Visible = this.folderIconBox.Visible;
                this.detailsButton.Text = this.GetDetailsButtonText();
                base.PerformLayout();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num15;
            int num16;
            int y = UIUtil.ScaleWidth(8);
            int x = UIUtil.ScaleHeight(8);
            Size size = UIUtil.ScaleSize(0x55, 0x18);
            this.errorIconBox.Location = new Point(x, y);
            this.errorIconBox.Size = this.errorIconBox.Image.Size;
            this.errorIconBox.PerformLayout();
            this.messageLabel.Location = new Point(this.errorIconBox.Right + x, this.errorIconBox.Top);
            this.messageLabel.Width = (base.ClientSize.Width - this.messageLabel.Left) - x;
            this.messageLabel.Height = this.messageLabel.GetPreferredSize(new Size(this.messageLabel.Width, 1)).Height;
            bool flag = !string.IsNullOrWhiteSpace(this.message2Label.Text);
            this.message2Label.Location = new Point(this.messageLabel.Left, this.messageLabel.Bottom + (flag ? y : 0));
            this.message2Label.Width = (base.ClientSize.Width - this.message2Label.Left) - x;
            this.message2Label.Height = flag ? this.message2Label.GetPreferredSize(new Size(this.message2Label.Width, 1)).Height : 0;
            int num3 = y + Math.Max(this.errorIconBox.Bottom, this.message2Label.Bottom);
            this.bottomSeparator.Location = new Point(x, num3);
            int width = base.ClientSize.Width - (2 * x);
            this.bottomSeparator.Width = width;
            this.bottomSeparator.Size = this.bottomSeparator.GetPreferredSize(new Size(width, 1));
            this.button2.Size = size;
            this.button2.PerformLayout();
            int num5 = this.bottomSeparator.Bottom + y;
            this.button2.Location = new Point((base.ClientSize.Width - x) - this.button2.Width, num5);
            int num6 = this.bottomSeparator.Bottom + y;
            this.button1.Size = size;
            this.button1.PerformLayout();
            int top = this.button2.Top;
            this.button1.Location = new Point((this.button2.Left - x) - this.button1.Width, top);
            this.detailsButton.Size = size;
            this.detailsButton.PerformLayout();
            int num8 = this.bottomSeparator.Bottom + y;
            this.detailsButton.Location = new Point(x, num8);
            int num9 = y + Int32Util.Max(this.detailsButton.Bottom, this.button2.Bottom, this.button1.Bottom);
            this.copyToClipboardButton.Location = new Point(x, num9);
            this.copyToClipboardButton.Size = size;
            this.copyToClipboardButton.PerformLayout();
            this.copyToClipboardButton.Size = this.copyToClipboardButton.GetPreferredSize(this.copyToClipboardButton.Size);
            this.folderIconBox.Size = UIUtil.ScaleSize(this.folderIconBox.Image.Size);
            this.folderIconBox.Location = new Point(this.copyToClipboardButton.Right + x, this.copyToClipboardButton.Top + ((this.copyToClipboardButton.Height - this.folderIconBox.Height) / 2));
            this.openFolderLink.Size = this.openFolderLink.GetPreferredSize(new Size(1, 1));
            this.openFolderLink.Location = new Point(this.folderIconBox.Right + (x / 2), this.folderIconBox.Top + ((this.folderIconBox.Height - this.openFolderLink.Height) / 2));
            int num10 = UIUtil.ScaleHeight(250);
            if (this.crashLogTextBox.Visible)
            {
                int num19 = y + Int32Util.Max(this.copyToClipboardButton.Bottom, this.folderIconBox.Bottom, this.openFolderLink.Bottom);
                this.crashLogTextBox.Location = new Point(x, num19);
                this.crashLogTextBox.Width = base.ClientSize.Width - (2 * x);
                int num20 = (base.ClientSize.Height - y) - this.crashLogTextBox.Top;
                this.crashLogTextBox.Height = Math.Max(num10, num20);
            }
            int num11 = y + Int32Util.Max(this.detailsButton.Bottom, this.button1.Bottom, this.button2.Bottom, this.crashLogTextBox.Visible ? this.crashLogTextBox.Bottom : 0);
            int num12 = (((x + (this.detailsButton.Visible ? (this.detailsButton.Width + x) : 0)) + (this.button1.Visible ? (this.button1.Width + x) : 0)) + this.button2.Width) + x;
            int num13 = (x + (this.copyToClipboardButton.Visible ? (this.copyToClipboardButton.Width + x) : 0)) + (this.openFolderLink.Visible ? (this.openFolderLink.Width + x) : 0);
            int num14 = Math.Max(num12, num13);
            if (this.crashLogTextBox.Visible)
            {
                num15 = (num11 - this.crashLogTextBox.Height) + num10;
                num16 = 0x7d0;
            }
            else
            {
                num15 = num11;
                num16 = num11;
            }
            int[] vals = new int[] { base.ClientSize.Width };
            int num17 = Int32Util.Max(num14, vals);
            int height = Int32Util.Clamp(base.ClientSize.Height, num15, num16);
            base.ClientSize = new Size(num17, height);
            this.minClientSize = new Size(num14, num15);
            this.maxClientSize = new Size(0x7d0, num16);
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.IsButton1Visible)
            {
                this.button1.Select();
            }
            else
            {
                this.button2.Select();
            }
            base.OnLoad(e);
        }

        private void OnOpenFolderLinkLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellUtil.BrowseFolder2(this, this.crashLogDir);
        }

        protected override void OnShown(EventArgs e)
        {
            base.PerformLayout();
            if (base.ShowInTaskbar)
            {
                Icon icon = PdnResources.GetImage("Images.Warning16.png").ToIcon(true);
                TaskbarManager.Instance.SetOverlayIcon(base.Handle, icon, null);
            }
            UIUtil.FlashForm(this);
            base.OnShown(e);
        }

        protected override void OnSizing(SizingEventArgs sea)
        {
            base.PerformLayout();
            Size clientSize = base.WindowSizeToClientSize(sea.ScreenWindowRectangle.Size);
            if (this.minClientSize.HasValue)
            {
                clientSize.Width = Math.Max(clientSize.Width, this.minClientSize.Value.Width);
                clientSize.Height = Math.Max(clientSize.Height, this.minClientSize.Value.Height);
            }
            if (this.maxClientSize.HasValue)
            {
                clientSize.Width = Math.Min(clientSize.Width, this.maxClientSize.Value.Width);
                clientSize.Height = Math.Min(clientSize.Height, this.maxClientSize.Value.Height);
            }
            Size size = base.ClientSizeToWindowSize(clientSize);
            Rectangle rectangle = new Rectangle(sea.ScreenWindowRectangle.Location, size);
            sea.ScreenWindowRectangle = rectangle;
            base.OnSizing(sea);
        }

        public static void ShowErrorDialog(IWin32Window owner, Exception exception)
        {
            ShowErrorDialog(owner, GenericErrorMessage, exception);
        }

        public static void ShowErrorDialog(IWin32Window owner, [Optional] string message, Exception exception)
        {
            ShowErrorDialog(owner, message, exception.ToString());
        }

        public static void ShowErrorDialog(IWin32Window owner, [Optional] string message, string exceptionText)
        {
            using (ExceptionDialog dialog = new ExceptionDialog())
            {
                dialog.Message = message ?? GenericErrorMessage;
                dialog.ExceptionText = exceptionText;
                dialog.ShowDialog(owner);
            }
        }

        public string Button1Text
        {
            get => 
                this.button1.Text;
            set
            {
                this.button1.Text = value;
                base.PerformLayout();
            }
        }

        public string Button2Text
        {
            get => 
                this.button2.Text;
            set
            {
                this.button2.Text = value;
            }
        }

        public string CrashLogDirectory
        {
            get => 
                this.crashLogDir;
            set
            {
                this.crashLogDir = value;
                this.toolTip.SetToolTip(this.openFolderLink, this.crashLogDir);
                this.openFolderLink.Visible = !string.IsNullOrEmpty(value);
                this.folderIconBox.Visible = this.openFolderLink.Visible;
                base.PerformLayout();
            }
        }

        public string ExceptionText
        {
            get => 
                this.exceptionText;
            set
            {
                this.exceptionText = value;
                string str = this.exceptionText.Replace("\n", "\r\n");
                this.crashLogTextBox.Text = str;
            }
        }

        public static string GenericErrorMessage =>
            PdnResources.GetString("ExceptionDialog.GenericErrorMessage");

        public bool IsButton1Visible
        {
            get => 
                this.button1.Visible;
            set
            {
                this.button1.Visible = value;
                base.PerformLayout();
            }
        }

        public string Message
        {
            get => 
                this.messageLabel.Text;
            set
            {
                this.messageLabel.Text = value;
                base.PerformLayout();
            }
        }

        public string Message2
        {
            get => 
                this.message2Label.Text;
            set
            {
                this.message2Label.Text = value;
                base.PerformLayout();
            }
        }
    }
}

