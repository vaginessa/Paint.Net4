namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Settings;
    using System;

    internal sealed class TypeSettingConverter : SettingConverter<Type>
    {
        internal static readonly TypeSettingConverter Instance = new TypeSettingConverter();

        protected override Type OnConvertFromStorage(Type valueType, string storageValue) => 
            Type.GetType(storageValue, true);

        protected override string OnConvertToStorage(Type valueType, Type value) => 
            value.AssemblyQualifiedName;
    }
}

