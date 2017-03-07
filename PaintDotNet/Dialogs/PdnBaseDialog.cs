namespace PaintDotNet.Dialogs
{
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class PdnBaseDialog : PdnBaseFormInternal
    {
        protected PdnPushButton baseCancelButton;
        protected PdnPushButton baseOkButton;

        public PdnBaseDialog()
        {
            this.InitializeComponent();
            if (!base.DesignMode)
            {
                this.baseOkButton.Text = PdnResources.GetString("Form.OkButton.Text");
                this.baseCancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            }
        }

        private void InitializeComponent()
        {
            this.baseOkButton = new PdnPushButton();
            this.baseCancelButton = new PdnPushButton();
            base.SuspendLayout();
            this.baseOkButton.Location = new Point(0x4d, 0x80);
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 1;
            this.baseOkButton.Click += new EventHandler(this.OnBaseOkButtonClick);
            this.baseCancelButton.DialogResult = DialogResult.Cancel;
            this.baseCancelButton.Location = new Point(0xa5, 0x80);
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 2;
            this.baseCancelButton.Click += new EventHandler(this.OnBaseCancelButtonClick);
            base.AcceptButton = this.baseOkButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.baseCancelButton;
            base.ClientSize = new Size(0xf8, 0x9e);
            base.Controls.Add(this.baseCancelButton);
            base.Controls.Add(this.baseOkButton);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MinimizeBox = false;
            base.Name = "PdnBaseDialog";
            base.ShowInTaskbar = false;
            this.Text = "PdnBaseDialog";
            base.Controls.SetChildIndex(this.baseOkButton, 0);
            base.Controls.SetChildIndex(this.baseCancelButton, 0);
            base.ResumeLayout(false);
        }

        private void OnBaseCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnBaseOkButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
        }
    }
}

