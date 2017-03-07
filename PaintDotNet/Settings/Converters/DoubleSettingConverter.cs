namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class DoubleSettingConverter : SettingConverter<double>
    {
        internal static readonly DoubleSettingConverter Instance = new DoubleSettingConverter();

        protected override double OnConvertFromStorage(Type valueType, string storageValue) => 
            double.Parse(storageValue, CultureInfo.InvariantCulture);

        protected override string OnConvertToStorage(Type valueType, double value) => 
            value.ToString("R", CultureInfo.InvariantCulture);
    }
}

