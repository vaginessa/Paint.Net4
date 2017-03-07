namespace PaintDotNet.IndirectUI
{
    using PaintDotNet.PropertySystem;
    using System;

    [PropertyControlInfo(typeof(DoubleVector3Property), PropertyControlType.Slider, IsDefault=true)]
    internal sealed class DoubleVector3SliderPropertyControl : Vector3SliderPropertyControl<double>
    {
        private bool useExponentialScale;

        public DoubleVector3SliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            this.DecimalPlaces = (int) propInfo.ControlProperties[ControlInfoPropertyNames.DecimalPlaces].Value;
            this.SliderSmallChangeX = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChangeX].Value;
            this.SliderLargeChangeX = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChangeX].Value;
            this.UpDownIncrementX = (double) propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrementX].Value;
            this.SliderSmallChangeY = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChangeY].Value;
            this.SliderLargeChangeY = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChangeY].Value;
            this.UpDownIncrementY = (double) propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrementY].Value;
            this.SliderSmallChangeZ = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChangeZ].Value;
            this.SliderLargeChangeZ = (double) propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChangeZ].Value;
            this.UpDownIncrementZ = (double) propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrementZ].Value;
            this.UseExponentialScale = (bool) propInfo.ControlProperties[ControlInfoPropertyNames.UseExponentialScale].Value;
            base.SetSliderDefaultValueX(base.Property.DefaultValueX);
            base.SetSliderDefaultValueY(base.Property.DefaultValueY);
            base.SetSliderDefaultValueZ(base.Property.DefaultValueZ);
            base.ResetUIRanges();
        }

        private double FromNudValue(decimal nudValue) => 
            ((double) nudValue);

        protected override double FromNudValueX(decimal nudValue) => 
            this.FromNudValue(nudValue);

        protected override double FromNudValueY(decimal nudValue) => 
            this.FromNudValue(nudValue);

        protected override double FromNudValueZ(decimal nudValue) => 
            this.FromNudValue(nudValue);

        protected override double FromSliderValueX(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, base.Property.MinValueX, base.Property.MaxValueX, this.DecimalPlaces);
            }
            double num = Math.Pow(10.0, (double) -this.DecimalPlaces);
            return (sliderValue * num);
        }

        protected override double FromSliderValueY(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, base.Property.MinValueY, base.Property.MaxValueY, this.DecimalPlaces);
            }
            double num = Math.Pow(10.0, (double) -this.DecimalPlaces);
            return (sliderValue * num);
        }

        protected override double FromSliderValueZ(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, base.Property.MinValueZ, base.Property.MaxValueZ, this.DecimalPlaces);
            }
            double num = Math.Pow(10.0, (double) -this.DecimalPlaces);
            return (sliderValue * num);
        }

        protected override double RoundPropertyValue(double value)
        {
            double num = Math.Pow(10.0, (double) (this.DecimalPlaces + 1));
            double num2 = Math.Pow(10.0, (double) -(this.DecimalPlaces + 1));
            double num3 = value * num;
            return (Math.Round(num3, MidpointRounding.AwayFromZero) * num2);
        }

        private decimal ToNudValue(double propertyValue) => 
            ((decimal) propertyValue);

        protected override decimal ToNudValueX(double propertyValue) => 
            this.ToNudValue(propertyValue);

        protected override decimal ToNudValueY(double propertyValue) => 
            this.ToNudValue(propertyValue);

        protected override decimal ToNudValueZ(double propertyValue) => 
            this.ToNudValue(propertyValue);

        protected override int ToSliderValueX(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, base.Property.MinValueX, base.Property.MaxValueX, this.DecimalPlaces);
            }
            return PropertyControlUtil.ToSliderValue(propertyValue, this.DecimalPlaces);
        }

        protected override int ToSliderValueY(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, base.Property.MinValueY, base.Property.MaxValueY, this.DecimalPlaces);
            }
            return PropertyControlUtil.ToSliderValue(propertyValue, this.DecimalPlaces);
        }

        protected override int ToSliderValueZ(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, base.Property.MinValueZ, base.Property.MaxValueZ, this.DecimalPlaces);
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
        public double SliderLargeChangeX
        {
            get => 
                base.SliderLargeChangeX;
            set
            {
                base.SliderLargeChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChangeY
        {
            get => 
                base.SliderLargeChangeY;
            set
            {
                base.SliderLargeChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=5.0)]
        public double SliderLargeChangeZ
        {
            get => 
                base.SliderLargeChangeZ;
            set
            {
                base.SliderLargeChangeZ = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeX
        {
            get => 
                base.SliderSmallChangeX;
            set
            {
                base.SliderSmallChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeY
        {
            get => 
                base.SliderSmallChangeY;
            set
            {
                base.SliderSmallChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double SliderSmallChangeZ
        {
            get => 
                base.SliderSmallChangeZ;
            set
            {
                base.SliderSmallChangeZ = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementX
        {
            get => 
                base.UpDownIncrementX;
            set
            {
                base.UpDownIncrementX = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementY
        {
            get => 
                base.UpDownIncrementY;
            set
            {
                base.UpDownIncrementY = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1.0)]
        public double UpDownIncrementZ
        {
            get => 
                base.UpDownIncrementZ;
            set
            {
                base.UpDownIncrementZ = value;
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
                base.ResetUIRanges();
            }
        }
    }
}

