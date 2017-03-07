namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal abstract class ParseableValueSettingConverter<T> : SettingConverter<T> where T: struct, IParseString<T>, IFormattable
    {
        protected ParseableValueSettingConverter()
        {
        }

        protected override T OnConvertFromStorage(Type valueType, string storageValue)
        {
            T local = default(T);
            return local.Parse(storageValue, CultureInfo.InvariantCulture);
        }

        protected override string OnConvertToStorage(Type valueType, T value) => 
            value.ToString(null, CultureInfo.InvariantCulture);
    }
}

