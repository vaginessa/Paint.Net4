namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;
    using System.Globalization;

    internal sealed class IntegerBooleanSettingConverter : SettingConverter<bool>
    {
        internal static readonly IntegerBooleanSettingConverter Instance = new IntegerBooleanSettingConverter();

        protected override bool OnConvertFromStorage(Type valueType, string storageValue)
        {
            switch (int.Parse(storageValue, CultureInfo.InvariantCulture))
            {
                case 0:
                    return false;

                case 1:
                    return true;
            }
            throw new ArgumentOutOfRangeException();
        }

        protected override string OnConvertToStorage(Type valueType, bool value)
        {
            int num = value ? 1 : 0;
            return num.ToString(CultureInfo.InvariantCulture);
        }
    }
}

