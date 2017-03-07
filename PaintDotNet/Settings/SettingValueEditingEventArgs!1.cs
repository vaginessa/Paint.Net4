namespace PaintDotNet.Settings
{
    using PaintDotNet;

    internal sealed class SettingValueEditingEventArgs<TValue> : PooledEventArgs<SettingValueEditingEventArgs<TValue>, TValue>
    {
        public static SettingValueEditingEventArgs<TValue> Get(TValue currentValue) => 
            PooledEventArgs<SettingValueEditingEventArgs<TValue>, TValue>.Get(currentValue);

        public TValue CurrentValue =>
            base.Value1;
    }
}

