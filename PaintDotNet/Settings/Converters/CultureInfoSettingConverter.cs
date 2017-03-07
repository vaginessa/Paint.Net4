namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class CultureInfoSettingConverter : SettingConverter<CultureInfo>
    {
        internal static readonly CultureInfoSettingConverter Instance = new CultureInfoSettingConverter();

        protected override CultureInfo OnConvertFromStorage(Type valueType, string storageValue) => 
            new CultureInfo(storageValue);

        protected override string OnConvertToStorage(Type valueType, CultureInfo value) => 
            value.Name;
    }
}

