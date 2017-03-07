namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal abstract class ScalarSetting<TValue> : Setting<TValue> where TValue: struct, IComparable<TValue>
    {
        private TValue maxValue;
        private TValue minValue;

        protected ScalarSetting(string path, SettingScope scope, TValue defaultValue, TValue minValue, TValue maxValue, SettingConverter<TValue> converter) : base(path, scope, defaultValue, converter)
        {
            if (minValue.IsGreaterThan<TValue>(maxValue) || maxValue.IsLessThan<TValue>(minValue))
            {
                throw new ArgumentException("minValue must be less than or equal to maxValue");
            }
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        protected override bool OnValidateValueT(TValue potentialValue)
        {
            if (potentialValue.IsLessThan<TValue>(this.minValue))
            {
                return false;
            }
            if (potentialValue.IsGreaterThan<TValue>(this.maxValue))
            {
                return false;
            }
            return true;
        }

        public TValue MaxValue =>
            this.maxValue;

        public TValue MinValue =>
            this.minValue;
    }
}

