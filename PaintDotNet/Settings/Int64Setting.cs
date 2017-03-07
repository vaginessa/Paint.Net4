namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal class Int64Setting : ScalarSetting<long>
    {
        public Int64Setting(string path, SettingScope scope, long defaultValue, long minValue, long maxValue) : base(path, scope, defaultValue, minValue, maxValue, Int64SettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new Int64Setting(base.Path, base.Scope, base.DefaultValue, base.MinValue, base.MaxValue);

        protected override IBoxPolicy<long> OnGetStaticBoxPolicy() => 
            BoxPolicy.Int64.Instance;
    }
}

