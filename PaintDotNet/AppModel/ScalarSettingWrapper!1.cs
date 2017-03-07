namespace PaintDotNet.AppModel
{
    using PaintDotNet.Settings;
    using System;
    using System.Runtime.InteropServices;

    internal class ScalarSettingWrapper<TValue> : SettingWrapper<TValue>, IScalarAppSetting<TValue>, ISetting<TValue>, ISetting where TValue: struct, IComparable<TValue>
    {
        private ScalarSetting<TValue> setting;

        public ScalarSettingWrapper(ScalarSetting<TValue> setting, bool forceReadOnly = true) : base(setting, forceReadOnly)
        {
            this.setting = setting;
        }

        public TValue MaxValue =>
            this.setting.MaxValue;

        public TValue MinValue =>
            this.setting.MinValue;
    }
}

