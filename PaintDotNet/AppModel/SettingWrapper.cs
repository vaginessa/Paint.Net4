namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings;
    using System;
    using System.Runtime.InteropServices;

    internal class SettingWrapper : ISetting
    {
        private bool forceReadOnly;
        private Setting setting;

        public SettingWrapper(Setting setting, bool forceReadOnly)
        {
            Validate.IsNotNull<Setting>(setting, "setting");
            this.setting = setting;
            this.forceReadOnly = forceReadOnly;
        }

        public static ISetting CreateWrapper(Setting setting, bool forceReadOnly = true) => 
            (TryCreateScalarWrapper<int>(setting, forceReadOnly) ?? (TryCreateScalarWrapper<bool>(setting, forceReadOnly) ?? (TryCreateScalarWrapper<long>(setting, forceReadOnly) ?? new SettingWrapper(setting, forceReadOnly))));

        private static ISetting TryCreateScalarWrapper<TValue>(Setting setting, bool forceReadOnly) where TValue: struct, IComparable<TValue>
        {
            ScalarSetting<TValue> setting2 = setting as ScalarSetting<TValue>;
            if (setting2 != null)
            {
                return new ScalarSettingWrapper<TValue>(setting2, forceReadOnly);
            }
            return null;
        }

        public object DefaultValue =>
            this.setting.DefaultValue;

        public bool IsReadOnly
        {
            get
            {
                if (!this.forceReadOnly)
                {
                    return this.setting.IsReadOnly;
                }
                return true;
            }
        }

        public string Path =>
            this.setting.Path;

        public object Value
        {
            get => 
                this.setting.Value;
            set
            {
                if (this.IsReadOnly)
                {
                    ExceptionUtil.ThrowInvalidOperationException("This property is read only");
                }
                else
                {
                    this.setting.Value = value;
                }
            }
        }

        public Type ValueType =>
            this.setting.ValueType;
    }
}

