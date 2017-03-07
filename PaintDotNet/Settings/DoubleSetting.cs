namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;
    using System.Runtime.InteropServices;

    internal class DoubleSetting : ScalarSetting<double>
    {
        public DoubleSetting(string path, SettingScope scope, double defaultValue, double minValue = -1.7976931348623157E+308, double maxValue = 1.7976931348623157E+308) : base(path, scope, defaultValue, minValue, maxValue, DoubleSettingConverter.Instance)
        {
            Validate.Begin().IsFinite(defaultValue, "defaultValue").IsFinite(minValue, "minValue").IsFinite(maxValue, "maxValue").Check();
        }

        protected override Setting OnClone() => 
            new DoubleSetting(base.Path, base.Scope, base.DefaultValue, base.MinValue, base.MaxValue);

        protected override IBoxPolicy<double> OnGetStaticBoxPolicy() => 
            BoxPolicy.Double.Instance;

        protected override bool OnValidateValueT(double potentialValue)
        {
            if (!potentialValue.IsFinite())
            {
                return false;
            }
            return base.OnValidateValueT(potentialValue);
        }
    }
}

