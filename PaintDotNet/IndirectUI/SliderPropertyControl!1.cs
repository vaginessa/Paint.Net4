namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class SliderPropertyControl<TValue> : PropertyControl<TValue, ScalarProperty<TValue>> where TValue: struct, IComparable<TValue>
    {
        private Label descriptionText;
        private HeadingLabel header;
        private PdnNumericUpDown numericUpDown;
        private PdnPushButton resetButton;
        private PdnTrackBar slider;

        public SliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            this.header = new HeadingLabel();
            this.slider = new PdnTrackBar();
            this.numericUpDown = new PdnNumericUpDown();
            this.resetButton = new PdnPushButton();
            this.descriptionText = new PdnLabel();
            this.slider.BeginInit();
            base.SuspendLayout();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.numericUpDown.DecimalPlaces = 0;
            this.numericUpDown.Name = "numericUpDown";
            this.numericUpDown.TextAlign = HorizontalAlignment.Right;
            this.numericUpDown.TabIndex = 1;
            this.RangeWraps = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.RangeWraps].Value;
            this.slider.Name = "slider";
            this.slider.AutoSize = false;
            this.slider.TabIndex = 0;
            this.SliderShowTickMarks = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.SliderShowTickMarks].Value;
            this.ControlStyle = (int) propInfo.ControlProperties[ControlInfoPropertyNames.ControlStyle].Value;
            this.ControlColors = propInfo.ControlProperties[ControlInfoPropertyNames.ControlColors].Value;
            this.slider.ResetRequested += new EventHandler(this.OnResetButtonClick);
            this.resetButton.AutoSize = false;
            this.resetButton.Name = "resetButton";
            this.resetButton.Click += new EventHandler(this.OnResetButtonClick);
            this.resetButton.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButton.TabIndex = 2;
            this.resetButton.Visible = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            base.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = base.Description;
            this.ValidateUIRanges();
            this.ResetUIRanges();
            Control[] controls = new Control[] { this.header, this.slider, this.numericUpDown, this.resetButton, this.descriptionText };
            base.Controls.AddRange(controls);
            this.slider.EndInit();
            base.ResumeLayout(false);
            this.numericUpDown.ValueChanged += new EventHandler(this.OnNumericUpDownValueChanged);
            this.slider.ValueChanged += new EventHandler(this.OnSliderValueChanged);
        }

        protected abstract TValue FromNudValue(decimal nudValue);
        protected abstract TValue FromSliderValue(int sliderValue);
        protected override void OnDescriptionChanged()
        {
            this.descriptionText.Text = base.Description;
            base.OnDescriptionChanged();
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = base.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override bool OnFirstSelect()
        {
            this.numericUpDown.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(4);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            this.resetButton.SuspendLayout();
            int width = UIUtil.ScaleWidth(20);
            this.resetButton.SetBounds(clientSize.Width - width, this.header.Bottom + num, width, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            int num4 = UIUtil.ScaleWidth(70);
            this.numericUpDown.SetBounds((this.resetButton.Visible ? (this.resetButton.Left - num2) : clientSize.Width) - this.numericUpDown.Width, this.header.Bottom + num, num4, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            this.resetButton.Height = this.numericUpDown.Height;
            this.resetButton.ResumeLayout();
            this.slider.SetBounds(0, this.header.Bottom + (num / 2), this.numericUpDown.Left - num2, this.numericUpDown.Height + num, BoundsSpecified.All);
            this.descriptionText.SetBounds(0, (string.IsNullOrEmpty(base.Description) ? 0 : num) + Int32Util.Max(this.resetButton.Bottom, this.slider.Bottom, this.numericUpDown.Bottom), clientSize.Width, string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : this.descriptionText.GetPreferredSize(new Size(clientSize.Width, 1)).Height);
            base.ClientSize = new Size(clientSize.Width, this.descriptionText.Bottom);
            base.OnLayout(levent);
        }

        private void OnNumericUpDownValueChanged(object sender, EventArgs e)
        {
            if (this.ToNudValue(base.Property.Value) != this.ToNudValue(this.FromNudValue(this.numericUpDown.Value)))
            {
                TValue newValue = this.FromNudValue(this.numericUpDown.Value);
                TValue local2 = base.Property.ClampPotentialValue(newValue);
                base.Property.Value = local2;
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.numericUpDown.Enabled = !base.Property.ReadOnly;
            this.slider.Enabled = !base.Property.ReadOnly;
            this.resetButton.Enabled = !base.Property.ReadOnly;
            this.descriptionText.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.numericUpDown.Value != this.ToNudValue(base.Property.Value))
            {
                decimal num = this.ToNudValue(base.Property.Value);
                this.numericUpDown.Value = num;
            }
            if (this.slider.Value != this.ToSliderValue(base.Property.Value))
            {
                int num3 = this.ToSliderValue(base.Property.Value).Clamp(this.slider.Minimum, this.slider.Maximum);
                this.slider.Value = num3;
            }
        }

        private void OnResetButtonClick(object sender, EventArgs e)
        {
            base.Property.Value = base.Property.DefaultValue;
        }

        private void OnSliderValueChanged(object sender, EventArgs e)
        {
            if (this.ToSliderValue(base.Property.Value) != this.ToSliderValue(this.FromSliderValue(this.slider.Value)))
            {
                TValue newValue = this.FromSliderValue(this.slider.Value);
                TValue local2 = base.Property.ClampPotentialValue(newValue);
                base.Property.Value = local2;
            }
        }

        protected void ResetUIRanges()
        {
            this.numericUpDown.Minimum = this.ToNudValue(base.Property.MinValue);
            this.numericUpDown.Maximum = this.ToNudValue(base.Property.MaxValue);
            this.slider.Minimum = this.ToSliderValue(base.Property.MinValue);
            this.slider.Maximum = this.ToSliderValue(base.Property.MaxValue);
            this.slider.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.slider);
        }

        protected void SetSliderDefaultValue(TValue defaultValue)
        {
            this.slider.DefaultValue = new int?(this.ToSliderValue(defaultValue));
        }

        protected abstract decimal ToNudValue(TValue propertyValue);
        protected abstract int ToSliderValue(TValue propertyValue);
        private void ValidateUIRanges()
        {
            try
            {
                int num = this.ToSliderValue(base.Property.MinValue);
                int num2 = this.ToSliderValue(base.Property.MaxValue);
                decimal num3 = this.ToNudValue(base.Property.MinValue);
                decimal num4 = this.ToNudValue(base.Property.MaxValue);
                TValue local = this.FromSliderValue(this.ToSliderValue(base.Property.MinValue));
                TValue local2 = this.FromSliderValue(this.ToSliderValue(base.Property.MaxValue));
                TValue local3 = this.FromNudValue(this.ToNudValue(base.Property.MinValue));
                TValue local4 = this.FromNudValue(this.ToNudValue(base.Property.MaxValue));
            }
            catch (Exception exception)
            {
                throw new ArgumentOutOfRangeException($"The property's range, [{base.Property.MinValue}, {base.Property.MaxValue}], cannot be accomodated. Try a smaller range, or a smaller value for DecimalPlaces.", exception);
            }
        }

        [PropertyControlProperty(DefaultValue=null)]
        public object ControlColors
        {
            get => 
                this.slider.Colors;
            set
            {
                this.slider.Colors = value as ColorBgra[];
            }
        }

        [PropertyControlProperty(DefaultValue=0)]
        public int ControlStyle
        {
            get => 
                ((int) this.slider.TrackBarFillStyle);
            set
            {
                this.slider.TrackBarFillStyle = (TrackBarFillStyle) value;
            }
        }

        protected int DecimalPlaces
        {
            get => 
                this.numericUpDown.DecimalPlaces;
            set
            {
                this.numericUpDown.DecimalPlaces = value;
                this.ResetUIRanges();
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool RangeWraps
        {
            get => 
                this.numericUpDown.RangeWraps;
            set
            {
                this.numericUpDown.RangeWraps = value;
            }
        }

        [PropertyControlProperty(DefaultValue=true)]
        public bool ShowDefaultValueTick
        {
            get => 
                this.slider.ShowDefaultValueTick;
            set
            {
                this.slider.ShowDefaultValueTick = value;
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
                this.resetButton.AutoSize = value;
                base.PerformLayout();
            }
        }

        protected TValue SliderLargeChange
        {
            get => 
                this.FromSliderValue(this.slider.LargeChange);
            set
            {
                this.slider.LargeChange = this.ToSliderValue(value);
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarks
        {
            get => 
                (this.slider.TickStyle > TickStyle.None);
            set
            {
                this.slider.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        protected TValue SliderSmallChange
        {
            get => 
                this.FromSliderValue(this.slider.SmallChange);
            set
            {
                this.slider.SmallChange = this.ToSliderValue(value);
            }
        }

        protected TValue UpDownIncrement
        {
            get => 
                this.FromNudValue(this.numericUpDown.Increment);
            set
            {
                this.numericUpDown.Increment = this.ToNudValue(value);
            }
        }
    }
}

