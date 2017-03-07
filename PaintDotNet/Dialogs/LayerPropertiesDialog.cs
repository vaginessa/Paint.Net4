namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class LayerPropertiesDialog : PdnBaseFormInternal
    {
        protected PdnPushButton cancelButton;
        private Container components;
        protected HeadingLabel generalHeader;
        private PaintDotNet.Layer layer;
        protected TextBox nameBox;
        protected PdnLabel nameLabel;
        protected PdnPushButton okButton;
        private object originalProperties;
        protected PaintDotNet.Controls.SeparatorLine separatorLine;
        protected PdnCheckBox visibleCheckBox;

        public LayerPropertiesDialog()
        {
            base.SuspendLayout();
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            this.InitializeComponent();
            base.Icon = PdnResources.GetImage("Icons.MenuLayersLayerPropertiesIcon.png").ToIcon();
            this.Text = PdnResources.GetString("LayerPropertiesDialog.Text");
            this.visibleCheckBox.Text = PdnResources.GetString("LayerPropertiesDialog.VisibleCheckBox.Text");
            this.nameLabel.Text = PdnResources.GetString("LayerPropertiesDialog.NameLabel.Text");
            this.generalHeader.Text = PdnResources.GetString("LayerPropertiesDialog.GeneralHeader.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void InitDialogFromLayer()
        {
            this.nameBox.Text = this.layer.Name;
            this.visibleCheckBox.IsChecked = this.layer.Visible;
        }

        private void InitializeComponent()
        {
            this.separatorLine = new PaintDotNet.Controls.SeparatorLine();
            this.visibleCheckBox = new PdnCheckBox();
            this.nameBox = new TextBox();
            this.nameLabel = new PdnLabel();
            this.cancelButton = new PdnPushButton();
            this.okButton = new PdnPushButton();
            this.generalHeader = new HeadingLabel();
            base.SuspendLayout();
            this.generalHeader.Location = new Point(6, 8);
            this.generalHeader.Name = "generalHeader";
            this.generalHeader.Margin = new Padding(1, 3, 1, 1);
            this.generalHeader.Size = new Size(0x10d, 0x11);
            this.generalHeader.TabIndex = 4;
            this.generalHeader.TabStop = false;
            this.nameLabel.Location = new Point(6, 0x1b);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new Size(50, 0x10);
            this.nameLabel.TabIndex = 2;
            this.nameBox.Location = new Point(0x40, 0x1b);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new Size(200, 20);
            this.nameBox.TabIndex = 2;
            this.nameBox.Text = "";
            this.nameBox.Enter += new EventHandler(this.OnNameBoxEnter);
            this.visibleCheckBox.Location = new Point(14, 0x2e);
            this.visibleCheckBox.Name = "visibleCheckBox";
            this.visibleCheckBox.Size = new Size(90, 0x10);
            this.visibleCheckBox.TabIndex = 3;
            this.visibleCheckBox.IsCheckedChanged += new EventHandler(this.OnVisibleCheckBoxIsCheckedChanged);
            this.okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.okButton.Location = new Point(0x72, 0x48);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0;
            this.okButton.Click += new EventHandler(this.OnOkButtonClick);
            this.okButton.AutoSize = true;
            this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = new Point(0xc2, 0x48);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            this.cancelButton.AutoSize = true;
            base.AcceptButton = this.okButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(0x112, 0x60);
            base.ControlBox = true;
            base.Controls.Add(this.generalHeader);
            base.Controls.Add(this.okButton);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.nameBox);
            base.Controls.Add(this.visibleCheckBox);
            base.Controls.Add(this.nameLabel);
            base.Controls.Add(this.separatorLine);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "LayerPropertiesDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.nameLabel, 0);
            base.Controls.SetChildIndex(this.visibleCheckBox, 0);
            base.Controls.SetChildIndex(this.nameBox, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.okButton, 0);
            base.Controls.SetChildIndex(this.generalHeader, 0);
            base.ResumeLayout(false);
        }

        protected virtual void InitLayerFromDialog()
        {
            this.layer.Name = this.nameBox.Text;
            this.layer.Visible = this.visibleCheckBox.IsChecked;
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                this.layer.PushSuppressPropertyChanged();
                this.layer.LoadProperties(this.originalProperties);
                this.layer.PopSuppressPropertyChanged();
                this.layer.Invalidate();
            }
            base.OnClosed(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(7);
            int num2 = base.IsGlassEffectivelyEnabled ? 0 : num;
            int num3 = UIUtil.ScaleWidth(7);
            int x = base.IsGlassEffectivelyEnabled ? -1 : num3;
            Size size = UIUtil.ScaleSize(0x55, 0x18);
            this.cancelButton.Size = size;
            this.cancelButton.Location = new Point((base.ClientSize.Width - x) - this.cancelButton.Width, (base.ClientSize.Height - num2) - this.cancelButton.Height);
            this.okButton.Size = size;
            this.okButton.Location = new Point((this.cancelButton.Left - num3) - this.okButton.Width, (base.ClientSize.Height - num2) - this.okButton.Height);
            this.separatorLine.Size = this.separatorLine.GetPreferredSize(new Size(base.ClientSize.Width - (2 * x), 1));
            this.separatorLine.Location = new Point(x, (this.okButton.Top - num) - this.separatorLine.Height);
            if (base.IsGlassEffectivelyEnabled)
            {
                this.separatorLine.Visible = false;
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separatorLine.Top);
            }
            else
            {
                this.separatorLine.Visible = true;
                base.GlassInset = new Padding(0);
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.nameBox.Select();
            this.nameBox.Select(0, this.nameBox.Text.Length);
            base.OnLoad(e);
        }

        private void OnNameBoxEnter(object sender, EventArgs e)
        {
            this.nameBox.Select(0, this.nameBox.Text.Length);
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            using (new WaitCursorChanger(this))
            {
                this.layer.PushSuppressPropertyChanged();
                this.InitLayerFromDialog();
                object oldState = this.layer.SaveProperties();
                this.layer.LoadProperties(this.originalProperties);
                this.layer.PopSuppressPropertyChanged();
                this.layer.LoadProperties(oldState);
                this.originalProperties = this.layer.SaveProperties();
            }
            base.Close();
        }

        private void OnVisibleCheckBoxIsCheckedChanged(object sender, EventArgs e)
        {
            this.Layer.PushSuppressPropertyChanged();
            this.Layer.Visible = this.visibleCheckBox.IsChecked;
            this.Layer.PopSuppressPropertyChanged();
        }

        public PaintDotNet.Layer Layer
        {
            get => 
                this.layer;
            set
            {
                this.layer = value;
                this.originalProperties = this.layer.SaveProperties();
                this.InitDialogFromLayer();
            }
        }
    }
}

