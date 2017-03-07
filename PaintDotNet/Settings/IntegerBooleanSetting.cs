namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal sealed class IntegerBooleanSetting : Setting<bool>
    {
        public IntegerBooleanSetting(string path, SettingScope scope, bool defaultValue) : base(path, scope, defaultValue, IntegerBooleanSettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new IntegerBooleanSetting(base.Path, base.Scope, base.DefaultValue);

        protected override IBoxPolicy<bool> OnGetStaticBoxPolicy() => 
            BoxPolicy.Boolean.Instance;
    }
}

