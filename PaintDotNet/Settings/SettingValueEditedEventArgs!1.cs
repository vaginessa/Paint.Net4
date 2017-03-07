namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;

    internal sealed class SettingValueEditedEventArgs<TValue> : PooledEventArgs<SettingValueEditedEventArgs<TValue>, TValue, TValue, bool>
    {
        public static SettingValueEditedEventArgs<TValue> Get(TValue initialValue, TValue finalValue, bool modified) => 
            PooledEventArgs<SettingValueEditedEventArgs<TValue>, TValue, TValue, bool>.Get(initialValue, finalValue, modified);

        public TValue FinalValue =>
            base.Value2;

        public TValue InitialValue =>
            base.Value1;

        public bool Modified =>
            base.Value3;
    }
}

