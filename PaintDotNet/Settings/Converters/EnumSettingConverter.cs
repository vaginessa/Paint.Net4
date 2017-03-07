namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;

    internal sealed class EnumSettingConverter : SettingConverter<Enum>
    {
        internal static readonly EnumSettingConverter Instance = new EnumSettingConverter();

        public override bool IsValidValueType(Type valueType)
        {
            if (!base.IsValidValueType(valueType))
            {
                return false;
            }
            Type underlyingType = Enum.GetUnderlyingType(valueType);
            return ((underlyingType == typeof(int)) || (underlyingType == typeof(long)));
        }

        protected override Enum OnConvertFromStorage(Type valueType, string storageValue) => 
            ((Enum) Enum.Parse(valueType, storageValue));

        protected override string OnConvertToStorage(Type valueType, Enum value) => 
            value.ToString();
    }
}

