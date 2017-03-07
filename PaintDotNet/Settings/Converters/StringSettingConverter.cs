namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;

    internal sealed class StringSettingConverter : SettingConverter<string>
    {
        internal static readonly StringSettingConverter Instance = new StringSettingConverter();

        protected override string OnConvertFromStorage(Type valueType, string storageValue) => 
            storageValue;

        protected override string OnConvertToStorage(Type valueType, string value) => 
            value;
    }
}

