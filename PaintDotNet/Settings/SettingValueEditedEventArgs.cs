namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal sealed class SettingValueEditedEventArgs : PooledEventArgs<SettingValueEditedEventArgs, object, object, bool>
    {
        public static SettingValueEditedEventArgs Get(object initialValue, object finalValue, bool modified) => 
            PooledEventArgs<SettingValueEditedEventArgs, object, object, bool>.Get(initialValue, finalValue, modified);

        public object FinalValue =>
            base.Value2;

        public object InitialValue =>
            base.Value1;

        public bool Modified =>
            base.Value3;
    }
}

