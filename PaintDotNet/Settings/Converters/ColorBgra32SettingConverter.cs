namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Imaging;
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class ColorBgra32SettingConverter : SettingConverter<ColorBgra32>
    {
        internal static readonly ColorBgra32SettingConverter Instance = new ColorBgra32SettingConverter();

        protected override ColorBgra32 OnConvertFromStorage(Type valueType, string storageValue)
        {
            NumberStyles hexNumber = NumberStyles.HexNumber;
            string s = storageValue.Trim();
            if (s.StartsWith("#"))
            {
                s = s.Substring(1);
            }
            return ColorBgra32.FromUInt32(uint.Parse(s, hexNumber, CultureInfo.InvariantCulture));
        }

        protected override string OnConvertToStorage(Type valueType, ColorBgra32 value) => 
            value.Bgra.ToString("X", CultureInfo.InvariantCulture);
    }
}

