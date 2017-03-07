namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal class EnumSetting<TEnum> : Setting<TEnum> where TEnum: struct
    {
        public EnumSetting(string path, SettingScope scope, TEnum defaultValue) : base(path, scope, defaultValue, EnumSettingConverter.Instance)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumeration type");
            }
        }

        protected override Setting OnClone() => 
            new EnumSetting<TEnum>(base.Path, base.Scope, base.DefaultValue);

        protected override IBoxPolicy<TEnum> OnGetStaticBoxPolicy() => 
            BoxPolicy.Enum<TEnum>.Instance;
    }
}

