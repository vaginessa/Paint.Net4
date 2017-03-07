namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class Int64SettingConverter : SettingConverter<long>
    {
        internal static readonly Int64SettingConverter Instance = new Int64SettingConverter();

        protected override long OnConvertFromStorage(Type valueType, string storageValue) => 
            long.Parse(storageValue, CultureInfo.InvariantCulture);

        protected override string OnConvertToStorage(Type valueType, long value) => 
            value.ToString(CultureInfo.InvariantCulture);
    }
}

