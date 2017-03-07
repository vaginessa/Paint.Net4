namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class DateTimeSettingConverter : SettingConverter<DateTime>
    {
        internal static readonly DateTimeSettingConverter Instance = new DateTimeSettingConverter();

        protected override DateTime OnConvertFromStorage(Type valueType, string storageValue) => 
            new DateTime(long.Parse(storageValue, CultureInfo.InvariantCulture));

        protected override string OnConvertToStorage(Type valueType, DateTime value) => 
            value.Ticks.ToString(CultureInfo.InvariantCulture);
    }
}

