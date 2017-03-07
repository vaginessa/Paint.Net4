namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Diagnostics;
    using System;

    internal sealed class ReadOnlySetting<T> : ISetting<T>, ISetting
    {
        private readonly Setting<T> setting;

        public ReadOnlySetting(Setting<T> setting)
        {
            Validate.IsNotNull<Setting<T>>(setting, "setting");
            this.setting = setting;
        }

        public T DefaultValue =>
            this.setting.DefaultValue;

        public bool IsReadOnly =>
            true;

        object ISetting.DefaultValue =>
            this.setting.DefaultValue;

        object ISetting.Value
        {
            get => 
                this.setting.Value;
            set
            {
                throw new ReadOnlyException();
            }
        }

        public string Path =>
            this.setting.Path;

        public T Value
        {
            get => 
                this.setting.Value;
            set
            {
                throw new ReadOnlyException();
            }
        }

        public Type ValueType =>
            this.setting.ValueType;
    }
}

