namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class BooleanSettingConverter : SettingConverter<bool>
    {
        internal static readonly BooleanSettingConverter Instance = new BooleanSettingConverter();

        protected override bool OnConvertFromStorage(Type valueType, string storageValue) => 
            bool.Parse(storageValue);

        protected override string OnConvertToStorage(Type valueType, bool value) => 
            value.ToString(CultureInfo.InvariantCulture);
    }
}

