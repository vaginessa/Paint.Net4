namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.PropertySystem;
    using System;

    [PropertyControlInfo(typeof(DoubleProperty), PropertyControlType.Slider, IsDefault=true)]
    internal sealed class DoubleSliderPropertyControl : SliderPropertyControl<double>
    {
        private bool useExponentialScale;

        public DoubleSliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.DecimalPlaces = (int) propInfo.ControlProperties[ControlInfoPropertyNames.DecimalPlaces].Value;
            this.UseExponentialScale = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.UseExponentialScale].Value;
            this.SliderSmallChange = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChange].Value;
            this.SliderLargeChange = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChange].Value;
            this.UpDownIncrement = (double) propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrement].Value;
            base.SetSliderDefaultValue(base.Property.DefaultValue);
            base.ResumeLayout(false);
        }

        protected override double FromNudValue(decimal nudValue) => 
            ((double) nudValue);

        protected override double FromSliderValue(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, base.Property.MinValue, base.Property.MaxValue, this.DecimalPlaces);
            }
            double num = Math.Pow(10.0, (double) -this.DecimalPlaces);
            return (sliderValue * num);
        }

        protected override decimal ToNudValue(double propertyValue) => 
            ((decimal) propertyValue);

        protected override int ToSliderValue(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, base.Property.MinValue, base.Property.MaxValue, this.DecimalPlaces);
            }
            return PropertyControlUtil.ToSliderValue(propertyValue, this.DecimalPlaces);
        }

        [PropertyControlProperty(DefaultValue=2)]
        public int DecimalPlaces
        {
            get => 
                base.DecimalPlaces;
            set
            {
                base.DecimalPlaces = value;
            }
        }

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChange
        {
            get => 
                base.SliderLargeChange;
            set
            {
                base.SliderLargeChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChange
        {
            get => 
                base.SliderSmallChange;
            set
            {
                base.SliderSmallChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrement
        {
            get => 
                base.UpDownIncrement;
            set
            {
                base.UpDownIncrement = value;
            }
        }

        [PropertyControlProperty(DefaultValue=false)]
        public bool UseExponentialScale
        {
            get => 
                this.useExponentialScale;
            set
            {
                this.useExponentialScale = value;
                base.SliderShowTickMarks &= value;
                base.ResetUIRanges();
            }
        }
    }
}

