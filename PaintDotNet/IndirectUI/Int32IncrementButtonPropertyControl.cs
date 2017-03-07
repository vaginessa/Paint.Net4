namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.IncrementButton)]
    internal sealed class Int32IncrementButtonPropertyControl : PropertyControl<int, Int32Property>
    {
        private PdnLabel descriptionText;
        private HeadingLabel header;
        private PdnPushButton incrementButton;

        public Int32IncrementButtonPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.Text = base.DisplayName;
            this.header.RightMargin = 0;
            this.incrementButton = new PdnPushButton();
            this.incrementButton.Name = "incrementButton";
            this.incrementButton.AutoSize = true;
            this.incrementButton.Text = (string) propInfo.ControlProperties[ControlInfoPropertyNames.ButtonText].Value;
            this.incrementButton.Click += new EventHandler(this.OnIncrementButtonClick);
            this.descriptionText = new PdnLabel();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = base.Description;
            Control[] controls = new Control[] { this.header, this.incrementButton, this.descriptionText };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override bool OnFirstSelect()
        {
            this.incrementButton.Select();
            return true;
        }

        private void OnIncrementButtonClick(object sender, EventArgs e)
        {
            long minValue = (long) base.Property.MinValue;
            long maxValue = (long) base.Property.MaxValue;
            long num3 = (maxValue - minValue) + 1L;
            if (num3 != 0)
            {
                long num4 = (long) base.Property.Value;
                long num5 = 1L + num4;
                long num7 = ((num5 - minValue) % num3) + minValue;
                base.Property.Value = (int) num7;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            this.incrementButton.PerformLayout();
            this.incrementButton.Location = new Point(0, this.header.Bottom + (string.IsNullOrEmpty(this.header.Text) ? 0 : num));
            this.descriptionText.SetBounds(0, this.incrementButton.Bottom + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : num), clientSize.Width, string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : this.descriptionText.GetPreferredSize(new Size(clientSize.Width, 1)).Height);
            base.ClientSize = new Size(clientSize.Width, this.descriptionText.Bottom);
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.incrementButton.Enabled = !base.Property.ReadOnly;
            this.descriptionText.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
        }

        [PropertyControlProperty(DefaultValue="+")]
        public string ButtonText
        {
            get => 
                this.incrementButton.Text;
            set
            {
                this.incrementButton.Text = value;
            }
        }
    }
}

