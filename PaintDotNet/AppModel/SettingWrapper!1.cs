namespace PaintDotNet.AppModel
{
    using PaintDotNet.Settings;
    using System;

    internal class SettingWrapper<TValue> : SettingWrapper, ISetting<TValue>, ISetting
    {
        private Setting<TValue> setting;

        protected SettingWrapper(Setting<TValue> setting, bool forceReadOnly) : base(setting, forceReadOnly)
        {
            this.setting = setting;
        }

        public TValue DefaultValue =>
            this.setting.DefaultValue;

        public TValue Value
        {
            get => 
                this.setting.Value;
            set
            {
                this.setting.Value = value;
            }
        }
    }
}

