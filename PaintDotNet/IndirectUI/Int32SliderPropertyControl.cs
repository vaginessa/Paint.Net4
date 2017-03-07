namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.PropertySystem;
    using System;

    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.Slider, IsDefault=true)]
    internal sealed class Int32SliderPropertyControl : SliderPropertyControl<int>
    {
        private const int maxMax = 0x5f5e100;
        private const int minMin = -100000000;

        public Int32SliderPropertyControl(PropertyControlInfo propInfo) : base(propInfo)
        {
            base.SuspendLayout();
            this.SliderSmallChange = (int) propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChange].Value;
            this.SliderLargeChange = (int) propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChange].Value;
            this.UpDownIncrement = (int) propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrement].Value;
            base.SetSliderDefaultValue(base.Property.DefaultValue);
            base.ResumeLayout(false);
        }

        protected override int FromNudValue(decimal nudValue) => 
            ((int) nudValue).Clamp(-100000000, 0x5f5e100);

        protected override int FromSliderValue(int sliderValue) => 
            sliderValue.Clamp(-100000000, 0x5f5e100);

        protected override decimal ToNudValue(int propertyValue) => 
            propertyValue.Clamp(-100000000, 0x5f5e100);

        protected override int ToSliderValue(int propertyValue) => 
            propertyValue.Clamp(-100000000, 0x5f5e100);

        [PropertyControlProperty(DefaultValue=5)]
        public int SliderLargeChange
        {
            get => 
                base.SliderLargeChange;
            set
            {
                base.SliderLargeChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1)]
        public int SliderSmallChange
        {
            get => 
                base.SliderSmallChange;
            set
            {
                base.SliderSmallChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue=1)]
        public int UpDownIncrement
        {
            get => 
                base.UpDownIncrement;
            set
            {
                base.UpDownIncrement = value;
            }
        }
    }
}

