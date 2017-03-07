namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal class Int32Setting : ScalarSetting<int>
    {
        public Int32Setting(string path, SettingScope scope, int defaultValue, int minValue, int maxValue) : base(path, scope, defaultValue, minValue, maxValue, Int32SettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new Int32Setting(base.Path, base.Scope, base.DefaultValue, base.MinValue, base.MaxValue);

        protected override IBoxPolicy<int> OnGetStaticBoxPolicy() => 
            BoxPolicy.Int32.Instance;
    }
}

