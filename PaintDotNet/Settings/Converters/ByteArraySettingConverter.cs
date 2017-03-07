namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;

    internal sealed class ByteArraySettingConverter : SettingConverter<byte[]>
    {
        internal static readonly ByteArraySettingConverter Instance = new ByteArraySettingConverter();

        protected override byte[] OnConvertFromStorage(Type valueType, string storageValue) => 
            Convert.FromBase64String(storageValue);

        protected override string OnConvertToStorage(Type valueType, byte[] value) => 
            Convert.ToBase64String(value);
    }
}

