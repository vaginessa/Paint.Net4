namespace PaintDotNet.Settings.Converters
{
    using PaintDotNet.Serialization;
    using PaintDotNet.Settings;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    internal sealed class SerializableObjectSettingConverter<T> : SettingConverter<T>
    {
        internal static readonly SerializableObjectSettingConverter<T> Instance;

        static SerializableObjectSettingConverter()
        {
            SerializableObjectSettingConverter<T>.Instance = new SerializableObjectSettingConverter<T>();
        }

        protected override T OnConvertFromStorage(Type valueType, string storageValue)
        {
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(storageValue)))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                SerializationFallbackBinder binder = new SerializationFallbackBinder();
                binder.SetNextRequiredBaseType(typeof(T));
                formatter.Binder = binder;
                return (T) formatter.Deserialize(stream);
            }
        }

        protected override string OnConvertToStorage(Type valueType, T value)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, value);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
    }
}

