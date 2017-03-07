namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(BooleanProperty), PropertyControlType.CheckBox)]
    internal sealed class BooleanCheckBoxPropertyControl : PropertyControl<bool, BooleanProperty>
    {
        private PdnCheckBox checkBox;
        private PdnLabel footnoteLabel;
        private HeadingLabel header;

        public BooleanCheckBoxPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.checkBox = new PdnCheckBox();
            this.checkBox.Name = "checkBox";
            this.checkBox.IsCheckedChanged += new EventHandler(this.OnCheckBoxCheckedChanged);
            this.checkBox.Text = string.IsNullOrEmpty(base.Description) ? base.DisplayName : base.Description;
            this.footnoteLabel = new PdnLabel();
            this.footnoteLabel.Name = "footnoteLabel";
            this.footnoteLabel.AutoSize = false;
            this.footnoteLabel.Text = (string) propInfo.ControlProperties[ControlInfoPropertyNames.Footnote].Value;
            Control[] controls = new Control[] { this.header, this.checkBox, this.footnoteLabel };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        private void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (base.Property.Value != this.checkBox.IsChecked)
            {
                base.Property.Value = this.checkBox.IsChecked;
            }
        }

        protected override void OnDescriptionChanged()
        {
            this.checkBox.Text = string.IsNullOrEmpty(base.Description) ? base.DisplayName : base.Description;
            base.OnDescriptionChanged();
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = base.DisplayName;
            this.checkBox.Text = string.IsNullOrEmpty(base.Description) ? base.DisplayName : base.Description;
            base.OnDisplayNameChanged();
        }

        protected override bool OnFirstSelect()
        {
            this.checkBox.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            this.checkBox.SuspendLayout();
            this.checkBox.Location = new Point(0, this.header.Bottom + num);
            this.checkBox.Size = this.checkBox.GetPreferredSize(clientSize.Width, 1);
            this.checkBox.ResumeLayout();
            this.footnoteLabel.SuspendLayout();
            if (string.IsNullOrWhiteSpace(this.footnoteLabel.Text))
            {
                this.footnoteLabel.Location = new Point(0, this.checkBox.Bottom);
                this.footnoteLabel.Size = new Size(clientSize.Width, 0);
                this.footnoteLabel.Visible = false;
                this.footnoteLabel.Enabled = false;
            }
            else
            {
                this.footnoteLabel.Location = new Point(0, this.checkBox.Bottom + num);
                this.footnoteLabel.Size = this.footnoteLabel.GetPreferredSize(clientSize.Width, 1);
                this.footnoteLabel.Visible = true;
                this.footnoteLabel.Enabled = true;
            }
            this.footnoteLabel.ResumeLayout();
            base.ClientSize = new Size(clientSize.Width, this.footnoteLabel.Bottom);
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.checkBox.Enabled = !base.Property.ReadOnly;
            this.footnoteLabel.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            this.checkBox.IsChecked = base.Property.Value;
        }

        [PropertyControlProperty(DefaultValue="")]
        public string Footnote
        {
            get => 
                this.footnoteLabel.Text;
            set
            {
                this.VerifyThreadAccess();
                this.footnoteLabel.Text = value;
                base.PerformLayout();
            }
        }
    }
}

