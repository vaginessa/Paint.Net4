namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class VectorSliderPropertyControl<TValue> : PropertyControl<Pair<TValue, TValue>, VectorProperty<TValue>> where TValue: struct, IComparable<TValue>
    {
        private int decimalPlaces;
        private PdnLabel descriptionText;
        private HeadingLabel header;
        private PdnNumericUpDown numericUpDownX;
        private PdnNumericUpDown numericUpDownY;
        private PdnPushButton resetButtonX;
        private PdnPushButton resetButtonY;
        private PdnTrackBar sliderX;
        private PdnTrackBar sliderY;

        public VectorSliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            this.decimalPlaces = 2;
            base.SuspendLayout();
            this.header = new HeadingLabel();
            this.sliderX = new PdnTrackBar();
            this.numericUpDownX = new PdnNumericUpDown();
            this.resetButtonX = new PdnPushButton();
            this.sliderY = new PdnTrackBar();
            this.numericUpDownY = new PdnNumericUpDown();
            this.resetButtonY = new PdnPushButton();
            this.descriptionText = new PdnLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = base.DisplayName;
            this.sliderX.Name = "sliderX";
            this.sliderX.AutoSize = false;
            this.sliderX.ValueChanged += new EventHandler(this.OnSliderXValueChanged);
            this.SliderShowTickMarksX = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.SliderShowTickMarksX].Value;
            this.sliderX.ResetRequested += new EventHandler(this.OnResetButtonXClick);
            this.numericUpDownX.Name = "numericUpDownX";
            this.numericUpDownX.ValueChanged += new EventHandler(this.OnNumericUpDownXValueChanged);
            this.numericUpDownX.TextAlign = HorizontalAlignment.Right;
            this.resetButtonX.Name = "resetButtonX";
            this.resetButtonX.AutoSize = false;
            this.resetButtonX.Click += new EventHandler(this.OnResetButtonXClick);
            this.resetButtonX.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButtonX.Visible = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            base.ToolTip.SetToolTip(this.resetButtonX, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.sliderY.Name = "sliderY";
            this.sliderY.AutoSize = false;
            this.sliderY.ValueChanged += new EventHandler(this.OnSliderYValueChanged);
            this.SliderShowTickMarksY = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.SliderShowTickMarksY].Value;
            this.sliderY.ResetRequested += new EventHandler(this.OnResetButtonYClick);
            this.numericUpDownY.Name = "numericUpDownY";
            this.numericUpDownY.ValueChanged += new EventHandler(this.OnNumericUpDownYValueChanged);
            this.numericUpDownY.TextAlign = HorizontalAlignment.Right;
            this.resetButtonY.Name = "resetButtonY";
            this.resetButtonY.AutoSize = false;
            this.resetButtonY.Click += new EventHandler(this.OnResetButtonYClick);
            this.resetButtonY.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButtonY.Visible = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            base.ToolTip.SetToolTip(this.resetButtonY, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = base.Description;
            this.ValidateUIRanges();
            this.ResetUIRanges();
            Control[] controls = new Control[] { this.header, this.sliderX, this.numericUpDownX, this.resetButtonX, this.sliderY, this.numericUpDownY, this.resetButtonY, this.descriptionText };
            base.Controls.AddRange(controls);
            base.ResumeLayout(false);
        }

        protected abstract TValue FromNudValueX(decimal nudValue);
        protected abstract TValue FromNudValueY(decimal nudValue);
        protected abstract TValue FromSliderValueX(int sliderValue);
        protected abstract TValue FromSliderValueY(int sliderValue);
        private bool IsEqualTo(TValue lhs, TValue rhs) => 
            ScalarProperty<TValue>.IsEqualTo(lhs, rhs);

        protected virtual void OnDecimalPlacesChanged()
        {
            this.ResetUIRanges();
        }

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
            this.numericUpDownX.Select();
            return true;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(5);
            int num2 = UIUtil.ScaleWidth(4);
            Size clientSize = base.ClientSize;
            this.header.SetBounds(0, 0, clientSize.Width, this.header.GetPreferredSize(new Size(clientSize.Width, 0)).Height);
            int width = UIUtil.ScaleWidth(70);
            int y = (this.header.Bottom + num) - 1;
            int num5 = UIUtil.ScaleWidth(20);
            this.resetButtonX.SuspendLayout();
            this.resetButtonX.SetBounds(clientSize.Width - this.resetButtonX.Width, y, num5, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            this.numericUpDownX.SetBounds((this.resetButtonX.Visible ? (this.resetButtonX.Left - num2) : clientSize.Width) - this.numericUpDownX.Width, y, width, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            this.resetButtonX.Height = this.numericUpDownX.Height;
            this.resetButtonX.ResumeLayout();
            this.sliderX.SetBounds(0, y - (num / 2), this.numericUpDownX.Left - num2, this.numericUpDownX.Height + num);
            int num6 = num + Int32Util.Max(this.resetButtonX.Bottom, this.numericUpDownX.Bottom, this.sliderX.Bottom);
            this.resetButtonY.SetBounds(clientSize.Width - this.resetButtonY.Width, num6, num5, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            this.numericUpDownY.SetBounds((this.resetButtonY.Visible ? (this.resetButtonY.Left - num2) : clientSize.Width) - this.numericUpDownY.Width, num6, width, -1, BoundsSpecified.Width | BoundsSpecified.Location);
            this.resetButtonY.Height = this.numericUpDownY.Height;
            this.resetButtonY.ResumeLayout();
            this.sliderY.SetBounds(0, num6 - (num / 2), this.numericUpDownY.Left - num2, this.numericUpDownY.Height + num);
            this.descriptionText.SetBounds(0, (string.IsNullOrEmpty(base.Description) ? 0 : num) + Int32Util.Max(this.resetButtonY.Bottom, this.sliderY.Bottom, this.numericUpDownY.Bottom), clientSize.Width, string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : this.descriptionText.GetPreferredSize(new Size(clientSize.Width, 1)).Height);
            base.ClientSize = new Size(clientSize.Width, this.descriptionText.Bottom);
            base.OnLayout(levent);
        }

        private void OnNumericUpDownXValueChanged(object sender, EventArgs e)
        {
            if (!this.IsEqualTo(this.RoundPropertyValue(base.Property.ValueX), this.RoundPropertyValue(this.FromNudValueX(this.numericUpDownX.Value))))
            {
                base.Property.ValueX = this.RoundPropertyValue(this.FromNudValueX(this.numericUpDownX.Value));
            }
        }

        private void OnNumericUpDownYValueChanged(object sender, EventArgs e)
        {
            if (!this.IsEqualTo(this.RoundPropertyValue(base.Property.ValueY), this.RoundPropertyValue(this.FromNudValueY(this.numericUpDownY.Value))))
            {
                base.Property.ValueY = this.RoundPropertyValue(this.FromNudValueY(this.numericUpDownY.Value));
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !base.Property.ReadOnly;
            this.numericUpDownX.Enabled = !base.Property.ReadOnly;
            this.sliderX.Enabled = !base.Property.ReadOnly;
            this.resetButtonX.Enabled = !base.Property.ReadOnly;
            this.numericUpDownY.Enabled = !base.Property.ReadOnly;
            this.sliderY.Enabled = !base.Property.ReadOnly;
            this.resetButtonY.Enabled = !base.Property.ReadOnly;
            this.descriptionText.Enabled = !base.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            if (!this.IsEqualTo(this.RoundPropertyValue(this.FromNudValueX(this.numericUpDownX.Value)), this.RoundPropertyValue(base.Property.ValueX)))
            {
                this.numericUpDownX.Value = this.ToNudValueX(this.RoundPropertyValue(base.Property.ValueX));
            }
            if (this.sliderX.Value != this.ToSliderValueX(this.RoundPropertyValue(base.Property.ValueX)))
            {
                this.sliderX.Value = this.ToSliderValueX(this.RoundPropertyValue(base.Property.ValueX));
            }
            if (!this.IsEqualTo(this.RoundPropertyValue(this.FromNudValueY(this.numericUpDownY.Value)), this.RoundPropertyValue(base.Property.ValueY)))
            {
                this.numericUpDownY.Value = this.ToNudValueY(this.RoundPropertyValue(base.Property.ValueY));
            }
            if (this.sliderY.Value != this.ToSliderValueY(this.RoundPropertyValue(base.Property.ValueY)))
            {
                this.sliderY.Value = this.ToSliderValueY(this.RoundPropertyValue(base.Property.ValueY));
            }
        }

        private void OnResetButtonXClick(object sender, EventArgs e)
        {
            base.Property.ValueX = base.Property.DefaultValueX;
        }

        private void OnResetButtonYClick(object sender, EventArgs e)
        {
            base.Property.ValueY = base.Property.DefaultValueY;
        }

        private void OnSliderXValueChanged(object sender, EventArgs e)
        {
            if (this.ToSliderValueX(base.Property.ValueX) != this.ToSliderValueX(this.FromSliderValueX(this.sliderX.Value)))
            {
                base.Property.ValueX = this.FromSliderValueX(this.sliderX.Value);
            }
        }

        private void OnSliderYValueChanged(object sender, EventArgs e)
        {
            if (this.ToSliderValueY(base.Property.ValueY) != this.ToSliderValueY(this.FromSliderValueY(this.sliderY.Value)))
            {
                base.Property.ValueY = this.FromSliderValueY(this.sliderY.Value);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            this.header.Text = this.Text;
            base.OnTextChanged(e);
        }

        protected void ResetUIRanges()
        {
            this.sliderX.Minimum = this.ToSliderValueX(base.Property.MinValueX);
            this.sliderX.Maximum = this.ToSliderValueX(base.Property.MaxValueX);
            this.sliderX.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.sliderX);
            this.numericUpDownX.Minimum = this.ToNudValueX(base.Property.MinValueX);
            this.numericUpDownX.Maximum = this.ToNudValueX(base.Property.MaxValueX);
            this.sliderY.Minimum = this.ToSliderValueY(base.Property.MinValueY);
            this.sliderY.Maximum = this.ToSliderValueY(base.Property.MaxValueY);
            this.sliderY.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.sliderY);
            this.numericUpDownY.Minimum = this.ToNudValueY(base.Property.MinValueY);
            this.numericUpDownY.Maximum = this.ToNudValueY(base.Property.MaxValueY);
        }

        protected abstract TValue RoundPropertyValue(TValue value);
        protected void SetSliderDefaultValueX(TValue defaultValue)
        {
            this.sliderX.DefaultValue = new int?(this.ToSliderValueX(defaultValue));
        }

        protected void SetSliderDefaultValueY(TValue defaultValue)
        {
            this.sliderY.DefaultValue = new int?(this.ToSliderValueY(defaultValue));
        }

        protected abstract decimal ToNudValueX(TValue propertyValue);
        protected abstract decimal ToNudValueY(TValue propertyValue);
        protected abstract int ToSliderValueX(TValue propertyValue);
        protected abstract int ToSliderValueY(TValue propertyValue);
        private void ValidateUIRanges()
        {
            try
            {
                int num = this.ToSliderValueX(base.Property.MinValueX);
                int num2 = this.ToSliderValueX(base.Property.MaxValueX);
                int num3 = this.ToSliderValueY(base.Property.MinValueY);
                int num4 = this.ToSliderValueY(base.Property.MaxValueY);
                decimal num5 = this.ToNudValueX(base.Property.MinValueX);
                decimal num6 = this.ToNudValueX(base.Property.MaxValueX);
                decimal num7 = this.ToNudValueY(base.Property.MinValueY);
                decimal num8 = this.ToNudValueY(base.Property.MaxValueY);
                TValue local = this.FromSliderValueX(this.ToSliderValueX(base.Property.MinValueX));
                TValue local2 = this.FromSliderValueX(this.ToSliderValueX(base.Property.MaxValueX));
                TValue local3 = this.FromSliderValueY(this.ToSliderValueY(base.Property.MinValueY));
                TValue local4 = this.FromSliderValueY(this.ToSliderValueY(base.Property.MaxValueY));
            }
            catch (Exception exception)
            {
                throw new ArgumentOutOfRangeException($"The property's range, [({base.Property.MinValueX},{base.Property.MinValueY}), ({base.Property.MaxValueX},{base.Property.MaxValueY})], cannot be accomodated. Try a smaller range, or a smaller value for DecimalPlaces.", exception);
            }
        }

        protected int DecimalPlaces
        {
            get => 
                this.decimalPlaces;
            set
            {
                this.decimalPlaces = value;
                this.numericUpDownX.DecimalPlaces = value;
                this.numericUpDownY.DecimalPlaces = value;
                this.OnDecimalPlacesChanged();
            }
        }

        [PropertyControlProperty(DefaultValue=true)]
        public bool ShowResetButton
        {
            get => 
                (this.resetButtonX.Visible && this.resetButtonY.Visible);
            set
            {
                this.resetButtonX.Visible = value;
                this.resetButtonY.Visible = value;
                base.PerformLayout();
            }
        }

        protected TValue SliderLargeChangeX
        {
            get => 
                this.FromSliderValueX(this.sliderX.LargeChange);
            set
            {
                this.sliderX.LargeChange = this.ToSliderValueX(value);
            }
        }

        protected TValue SliderLargeChangeY
        {
            get => 
                this.FromSliderValueY(this.sliderY.LargeChange);
            set
            {
                this.sliderY.LargeChange = this.ToSliderValueY(value);
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarksX
        {
            get => 
                (this.sliderX.TickStyle > TickStyle.None);
            set
            {
                this.sliderX.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool SliderShowTickMarksY
        {
            get => 
                (this.sliderY.TickStyle > TickStyle.None);
            set
            {
                this.sliderY.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        protected TValue SliderSmallChangeX
        {
            get => 
                this.FromSliderValueX(this.sliderX.SmallChange);
            set
            {
                this.sliderX.SmallChange = this.ToSliderValueX(value);
            }
        }

        protected TValue SliderSmallChangeY
        {
            get => 
                this.FromSliderValueY(this.sliderY.SmallChange);
            set
            {
                this.sliderY.SmallChange = this.ToSliderValueY(value);
            }
        }

        protected TValue UpDownIncrementX
        {
            get => 
                this.FromNudValueX(this.numericUpDownX.Increment);
            set
            {
                this.numericUpDownX.Increment = this.ToNudValueX(value);
            }
        }

        protected TValue UpDownIncrementY
        {
            get => 
                this.FromNudValueY(this.numericUpDownY.Increment);
            set
            {
                this.numericUpDownY.Increment = this.ToNudValueY(value);
            }
        }
    }
}

