namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;
    using System.Runtime.InteropServices;

    internal class FloatSetting : ScalarSetting<float>
    {
        public FloatSetting(string path, SettingScope scope, float defaultValue, float minValue = -3.402823E+38f, float maxValue = 3.402823E+38f) : base(path, scope, defaultValue, minValue, maxValue, FloatSettingConverter.Instance)
        {
            Validate.Begin().IsFinite(defaultValue, "defaultValue").IsFinite(minValue, "minValue").IsFinite(maxValue, "maxValue").Check();
        }

        protected override Setting OnClone() => 
            new FloatSetting(base.Path, base.Scope, base.DefaultValue, base.MinValue, base.MaxValue);

        protected override bool OnValidateValueT(float potentialValue)
        {
            if (!potentialValue.IsFinite())
            {
                return false;
            }
            return base.OnValidateValueT(potentialValue);
        }
    }
}

