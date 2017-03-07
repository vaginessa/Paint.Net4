namespace PaintDotNet.Settings
{
    using System;

    internal sealed class ByteArraySetting : Setting<byte[]>
    {
        public ByteArraySetting(string path, SettingScope scope, byte[] defaultValue) : base(path, scope, defaultValue, ByteArraySettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new ByteArraySetting(base.Path, base.Scope, base.DefaultValue);
    }
}

