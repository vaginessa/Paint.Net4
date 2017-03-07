namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(StringProperty), PropertyControlType.TextBox, IsDefault=true)]
    internal sealed class StringTextBoxPropertyControl : PropertyControl<string, StringProperty>
    {
        private int baseTextBoxHeight;
        private PdnLabel description;
        private HeadingLabel header;
        private TextBox textBox;

        public StringTextBoxPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.textBox = new TextBox();
            this.description = new PdnLabel();
            this.header.Name = "header";
            this.header.Text = base.DisplayName;
            this.header.RightMargin = 0;
            this.description.Name = "description";
            this.description.Text = base.Description;
            this.textBox.Name = "textBox";
            this.textBox.TextChanged += new EventHandler(this.OnTextBoxTextChanged);
            this.textBox.MaxLength = base.Property.MaxLength;
            this.baseTextBoxHeight = this.textBox.Height;
            this.Multiline = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.Multiline].Value;
            Control[] controls = new Control[] { this.header, this.textBox, this.description };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        protected override void OnDescriptionChanged()
        {
            this.description.Text = base.Description;
            base.OnDescriptionChanged();
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = base.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override bool OnFirstSelect()
        {
            this.textBox.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            this.textBox.Location = new Point(0, this.header.Bottom + num2);
            this.textBox.Width = clientSize.Width;
            this.textBox.Height = this.textBox.Multiline ? (this.baseTextBoxHeight * 4) : this.baseTextBoxHeight;
            this.description.Location = new Point(0, (string.IsNullOrEmpty(base.Description) ? 0 : num) + this.textBox.Bottom);
            this.description.Width = clientSize.Width;
            this.description.Height = string.IsNullOrEmpty(this.description.Text) ? 0 : this.description.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
            base.ClientSize = new Size(clientSize.Width, this.description.Bottom);
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.textBox.Enabled = !base.Property.ReadOnly;
            this.textBox.ReadOnly = base.Property.ReadOnly;
            this.description.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.textBox.Text != base.Property.Value)
            {
                this.textBox.Text = base.Property.Value;
            }
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            string text;
            if (this.textBox.Text.Length > base.Property.MaxLength)
            {
                text = this.textBox.Text.Substring(base.Property.MaxLength);
            }
            else
            {
                text = this.textBox.Text;
            }
            if (base.Property.Value != text)
            {
                base.Property.Value = text;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool Multiline
        {
            get => 
                this.textBox.Multiline;
            set
            {
                this.textBox.Multiline = value;
                this.textBox.AcceptsReturn = value;
                base.PerformLayout();
            }
        }
    }
}

