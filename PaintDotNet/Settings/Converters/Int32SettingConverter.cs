namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class Int32SettingConverter : SettingConverter<int>
    {
        internal static readonly Int32SettingConverter Instance = new Int32SettingConverter();

        protected override int OnConvertFromStorage(Type valueType, string storageValue) => 
            int.Parse(storageValue, CultureInfo.InvariantCulture);

        protected override string OnConvertToStorage(Type valueType, int value) => 
            value.ToString(CultureInfo.InvariantCulture);
    }
}

