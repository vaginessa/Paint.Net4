namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;

    internal sealed class DateTimeSetting : ScalarSetting<DateTime>
    {
        public DateTimeSetting(string path, SettingScope scope, DateTime defaultValue) : this(path, scope, defaultValue, DateTime.MinValue, DateTime.MaxValue)
        {
        }

        public DateTimeSetting(string path, SettingScope scope, DateTime defaultValue, DateTime minValue, DateTime maxValue) : base(path, scope, defaultValue, minValue, maxValue, DateTimeSettingConverter.Instance)
        {
            Validate.Begin().IsValueInRange<DateTime>(defaultValue, DateTime.MinValue, DateTime.MaxValue, "defaultValue").IsValueInRange<DateTime>(minValue, DateTime.MinValue, DateTime.MaxValue, "minValue").IsValueInRange<DateTime>(maxValue, DateTime.MinValue, DateTime.MaxValue, "maxValue").Check();
        }

        protected override Setting OnClone() => 
            new DateTimeSetting(base.Path, base.Scope, base.DefaultValue, base.MinValue, base.MaxValue);

        protected override bool OnValidateValueT(DateTime potentialValue) => 
            (((potentialValue >= DateTime.MinValue) && (potentialValue <= DateTime.MaxValue)) && base.OnValidateValueT(potentialValue));
    }
}

