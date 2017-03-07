namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    [PropertyControlInfo(typeof(DoubleProperty), PropertyControlType.AngleChooser)]
    internal sealed class AngleChooserPropertyControl : PropertyControl<double, DoubleProperty>
    {
        private AngleChooser angleChooser;
        private Label description;
        private HeadingLabel header;
        private PdnPushButton resetButton;
        private PdnNumericUpDown valueNud;

        public AngleChooserPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            AngleChooserPhase offsetByPi;
            DoubleProperty property = (DoubleProperty) propInfo.Property;
            if ((property.MaxValue - property.MinValue) > 360.0)
            {
                throw new ArgumentException($"The difference between the property's minimum and maximum values cannot exceed 360.0. (Property.MinValue={property.MinValue}, Property.MaxValue={property.MaxValue}, Delta={property.MaxValue - property.MinValue})");
            }
            double minValue = property.MinValue;
            double maxValue = property.MaxValue;
            if ((property.MinValue >= -180.0) && (property.MaxValue <= 180.0))
            {
                offsetByPi = AngleChooserPhase.OffsetByPi;
            }
            else
            {
                if ((property.MinValue < 0.0) || (property.MaxValue > 360.0))
                {
                    throw new ArgumentException($"The property minimum and maximum values must either fall into the range [-180, +180] or [0, +360]. (Property.MinValue={property.MinValue}, Property.MaxValue={property.MaxValue})");
                }
                offsetByPi = AngleChooserPhase.Regular;
            }
            this.header = new HeadingLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.angleChooser = new AngleChooser();
            this.angleChooser.Name = "angleChooser";
            this.angleChooser.Phase = offsetByPi;
            this.angleChooser.MinValue = minValue;
            this.angleChooser.MaxValue = maxValue;
            this.angleChooser.ValueChanged += new EventHandler(this.OnAngleChooserValueChanged);
            this.valueNud = new PdnNumericUpDown();
            this.valueNud.Name = "numericUpDown";
            this.valueNud.Minimum = (decimal) base.Property.MinValue;
            this.valueNud.Maximum = (decimal) base.Property.MaxValue;
            this.valueNud.DecimalPlaces = (int) propInfo.ControlProperties[ControlInfoPropertyNames.DecimalPlaces].Value;
            this.valueNud.ValueChanged += new EventHandler(this.OnValueNudValueChanged);
            this.valueNud.TextAlign = HorizontalAlignment.Right;
            if ((maxValue - minValue) == 360.0)
            {
                this.valueNud.RangeWraps = true;
            }
            this.resetButton = new PdnPushButton();
            this.resetButton.Name = "resetButton";
            this.resetButton.Click += new EventHandler(this.OnResetButtonClick);
            this.resetButton.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButton.Visible = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            base.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.description = new PdnLabel();
            this.description.Name = "descriptionText";
            this.description.AutoSize = false;
            this.description.Text = base.Description;
            base.SuspendLayout();
            Control[] controls = new Control[] { this.header, this.angleChooser, this.valueNud, this.resetButton, this.description };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        private double FromAngleChooserValue(double angleChooserValue) => 
            angleChooserValue;

        private void OnAngleChooserValueChanged(object sender, EventArgs e)
        {
            if (base.Property.Value != this.FromAngleChooserValue(this.angleChooser.Value))
            {
                base.Property.Value = this.FromAngleChooserValue(this.angleChooser.Value);
            }
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
            this.valueNud.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            this.resetButton.Width = UIUtil.ScaleWidth(20);
            this.resetButton.Location = new Point(clientSize.Width - this.resetButton.Width, this.header.Bottom + num);
            int num3 = UIUtil.ScaleWidth(70);
            this.valueNud.PerformLayout();
            this.valueNud.Width = num3;
            this.valueNud.Location = new Point((this.resetButton.Left - num2) - this.valueNud.Width, this.header.Bottom + num);
            this.resetButton.Height = this.valueNud.Height;
            this.angleChooser.Size = UIUtil.ScaleSize(new Size(60, 60));
            int num4 = num2;
            int num5 = this.valueNud.Left - num2;
            double num6 = ((double) (num4 + num5)) / 2.0;
            int x = (int) (num6 - (((double) this.angleChooser.Width) / 2.0));
            this.angleChooser.Location = new Point(x, this.header.Bottom + num);
            this.description.Location = new Point(0, Math.Max(this.valueNud.Bottom, Math.Max(this.resetButton.Bottom, this.angleChooser.Bottom)));
            this.description.Width = clientSize.Width;
            this.description.Height = string.IsNullOrEmpty(this.description.Text) ? 0 : this.description.GetPreferredSize(new Size(clientSize.Width, 1)).Height;
            base.ClientSize = new Size(clientSize.Width, this.description.Bottom);
            base.OnLayout(levent);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.angleChooser.Enabled = !base.Property.ReadOnly;
            this.valueNud.Enabled = !base.Property.ReadOnly;
            this.resetButton.Enabled = !base.Property.ReadOnly;
            this.description.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.angleChooser.Value != this.ToAngleChooserValue(base.Property.Value))
            {
                this.angleChooser.Value = this.ToAngleChooserValue(base.Property.Value);
            }
            if (this.valueNud.Value != ((decimal) base.Property.Value))
            {
                this.valueNud.Value = (decimal) base.Property.Value;
            }
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            base.Property.Value = base.Property.DefaultValue;
        }

        private void OnValueNudValueChanged(object sender, EventArgs e)
        {
            if (base.Property.Value != ((double) this.valueNud.Value))
            {
                base.Property.Value = (double) this.valueNud.Value;
            }
        }

        private double ToAngleChooserValue(double nudValue) => 
            nudValue;

        [PropertyControlProperty(DefaultValue=2)]
        public int DecimalPlaces
        {
            get => 
                this.valueNud.DecimalPlaces;
            set
            {
                this.valueNud.DecimalPlaces = value;
            }
        }

        [PropertyControlProperty(DefaultValue=true)]
        public bool ShowResetButton
        {
            get => 
                this.resetButton.Visible;
            set
            {
                this.resetButton.Visible = value;
                base.PerformLayout();
            }
        }
    }
}

