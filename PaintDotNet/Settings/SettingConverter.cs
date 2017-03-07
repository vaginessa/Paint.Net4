namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;

    internal abstract class SettingConverter
    {
        private Type valueType;

        protected SettingConverter(Type valueType)
        {
            Validate.IsNotNull<Type>(valueType, "valueType");
            this.valueType = valueType;
        }

        public abstract object ConvertFromStorage(Type valueType, string storageValue);
        public abstract string ConvertToStorage(Type valueType, object value);
        public bool IsValidValue(Type valueType, object value)
        {
            if (value == null)
            {
                return false;
            }
            if (!valueType.IsAssignableFrom(value.GetType()))
            {
                return false;
            }
            if (!this.IsValidValueType(valueType))
            {
                return false;
            }
            return this.OnValidateValue(valueType, value);
        }

        public virtual bool IsValidValueType(Type valueType)
        {
            Validate.IsNotNull<Type>(valueType, "valueType");
            return this.valueType.IsAssignableFrom(valueType);
        }

        protected virtual bool OnValidateValue(Type valueType, object value) => 
            true;

        public Type ValueType =>
            this.valueType;
    }
}

