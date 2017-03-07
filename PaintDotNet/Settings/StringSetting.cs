namespace PaintDotNet.Settings
{
    using System;
    using System.Runtime.InteropServices;

    internal class StringSetting : Setting<string>
    {
        private bool allowEmptyOrWhiteSpace;

        public StringSetting(string path, SettingScope scope, string defaultValue, bool allowEmptyOrWhiteSpace = true) : base(path, scope, defaultValue, StringSettingConverter.Instance)
        {
            this.allowEmptyOrWhiteSpace = allowEmptyOrWhiteSpace;
        }

        protected override Setting OnClone() => 
            new StringSetting(base.Path, base.Scope, base.DefaultValue, this.AllowEmptyOrWhiteSpace);

        protected override bool OnValidateValueT(string potentialValue)
        {
            if (!this.allowEmptyOrWhiteSpace && string.IsNullOrWhiteSpace(potentialValue))
            {
                return false;
            }
            return base.OnValidateValueT(potentialValue);
        }

        public bool AllowEmptyOrWhiteSpace =>
            this.allowEmptyOrWhiteSpace;
    }
}

