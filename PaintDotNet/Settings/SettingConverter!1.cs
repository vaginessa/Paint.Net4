namespace PaintDotNet.Settings
{
    using System;

    internal abstract class SettingConverter<T> : SettingConverter
    {
        protected SettingConverter() : base(typeof(T))
        {
        }

        public sealed override object ConvertFromStorage(Type valueType, string storageValue) => 
            this.OnConvertFromStorage(valueType, storageValue);

        public sealed override string ConvertToStorage(Type valueType, object value) => 
            this.OnConvertToStorage(valueType, (T) value);

        protected abstract T OnConvertFromStorage(Type valueType, string storageValue);
        protected abstract string OnConvertToStorage(Type valueType, T value);
        protected sealed override bool OnValidateValue(Type valueType, object value) => 
            this.OnValidateValueT(valueType, (T) value);

        protected virtual bool OnValidateValueT(Type valueType, T value) => 
            true;
    }
}

