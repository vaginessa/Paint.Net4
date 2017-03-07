namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal sealed class BooleanSetting : Setting<bool>
    {
        public BooleanSetting(string path, SettingScope scope, bool defaultValue) : base(path, scope, defaultValue, BooleanSettingConverter.Instance)
        {
        }

        protected override Setting OnClone() => 
            new BooleanSetting(base.Path, base.Scope, base.DefaultValue);

        protected override IBoxPolicy<bool> OnGetStaticBoxPolicy() => 
            BoxPolicy.Boolean.Instance;
    }
}

