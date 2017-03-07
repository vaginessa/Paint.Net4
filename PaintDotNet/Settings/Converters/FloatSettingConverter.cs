namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class FloatSettingConverter : SettingConverter<float>
    {
        internal static readonly FloatSettingConverter Instance = new FloatSettingConverter();

        protected override float OnConvertFromStorage(Type valueType, string storageValue) => 
            float.Parse(storageValue, CultureInfo.InvariantCulture);

        protected override string OnConvertToStorage(Type valueType, float value) => 
            value.ToString("R", CultureInfo.InvariantCulture);
    }
}

