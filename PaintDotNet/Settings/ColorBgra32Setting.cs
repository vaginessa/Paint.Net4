namespace PaintDotNet.Settings
{
    using PaintDotNet.Imaging;
    using System;

    internal sealed class ColorBgra32Setting : Setting<ColorBgra32>
    {
        public ColorBgra32Setting(string path, SettingScope scope, ColorBgra32 defaultValue) : base(path, scope, defaultValue, ColorBgra32SettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new ColorBgra32Setting(base.Path, base.Scope, base.DefaultValue);
    }
}

