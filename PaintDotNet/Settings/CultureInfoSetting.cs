namespace PaintDotNet.Settings
{
    using System;
    using System.Globalization;

    internal sealed class CultureInfoSetting : Setting<CultureInfo>
    {
        public CultureInfoSetting(string path, SettingScope scope, CultureInfo defaultValue) : base(path, scope, defaultValue, CultureInfoSettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new CultureInfoSetting(base.Path, base.Scope, base.DefaultValue);
    }
}

