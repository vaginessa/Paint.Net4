namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal sealed class SettingValueEditingEventArgs : PooledEventArgs<SettingValueEditingEventArgs, object>
    {
        public static SettingValueEditingEventArgs Get(object currentValue) => 
            PooledEventArgs<SettingValueEditingEventArgs, object>.Get(currentValue);

        public object CurrentValue =>
            base.Value1;
    }
}

