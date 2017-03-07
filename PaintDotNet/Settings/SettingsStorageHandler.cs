namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;
    using System.Runtime.InteropServices;

    internal abstract class SettingsStorageHandler
    {
        private readonly object sync = new object();

        public bool CanDelete(Setting appSetting)
        {
            switch (appSetting.Scope)
            {
                case SettingScope.CurrentUser:
                    return true;

                case SettingScope.CurrentUserWithSystemWideOverride:
                case SettingScope.SystemWideWithCurrentUserOverride:
                    return false;

                case SettingScope.SystemWide:
                    return this.CanSetSystemWideValue;
            }
            throw new InternalErrorException();
        }

        public object Get(Setting appSetting)
        {
            SettingsHive hive;
            SettingsHive? nullable;
            object obj2;
            object obj3;
            Validate.IsNotNull<Setting>(appSetting, "appSetting");
            SettingConverter converter = appSetting.Converter;
            if (!converter.IsValidValueType(appSetting.ValueType))
            {
                throw new ArgumentException("appSetting.ValueType is not a supported Type");
            }
            GetHiveQueryOrder(appSetting.Scope, out hive, out nullable);
            if (this.TryGetValue(hive, appSetting.Path, appSetting.ValueType, converter, out obj2))
            {
                return obj2;
            }
            if (nullable.HasValue && this.TryGetValue(nullable.Value, appSetting.Path, appSetting.ValueType, converter, out obj3))
            {
                return obj3;
            }
            return appSetting.DefaultValue;
        }

        private static void GetHiveQueryOrder(SettingScope scope, out SettingsHive firstTryHive, out SettingsHive? secondTryHive)
        {
            switch (scope)
            {
                case SettingScope.CurrentUser:
                    firstTryHive = SettingsHive.CurrentUser;
                    break;

                case SettingScope.CurrentUserWithSystemWideOverride:
                    firstTryHive = SettingsHive.SystemWide;
                    break;

                case SettingScope.SystemWide:
                    firstTryHive = SettingsHive.SystemWide;
                    break;

                case SettingScope.SystemWideWithCurrentUserOverride:
                    firstTryHive = SettingsHive.CurrentUser;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<SettingScope>(scope, "scope");
            }
            switch (scope)
            {
                case SettingScope.CurrentUser:
                case SettingScope.SystemWide:
                    secondTryHive = 0;
                    return;

                case SettingScope.CurrentUserWithSystemWideOverride:
                    secondTryHive = 0;
                    return;

                case SettingScope.SystemWideWithCurrentUserOverride:
                    secondTryHive = 1;
                    return;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<SettingScope>(scope, "scope");
        }

        protected abstract bool OnQueryCanSetSystemWideValue();
        protected abstract bool OnTryGet(SettingsHive hive, string path, out string storageValue);
        protected abstract bool OnTrySet(SettingsHive hive, string name, string value);
        public bool TryGet(SettingsHive hive, string path, out string storageValue) => 
            this.OnTryGet(hive, path, out storageValue);

        private bool TryGetValue(SettingsHive hive, string path, Type valueType, SettingConverter converter, out object value)
        {
            string str;
            if (!this.TryGet(hive, path, out str))
            {
                value = null;
                return false;
            }
            try
            {
                value = converter.ConvertFromStorage(valueType, str);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public bool TrySet(Setting appSetting, object value)
        {
            SettingsHive hive;
            SettingsHive? nullable;
            Validate.IsNotNull<Setting>(appSetting, "appSetting");
            SettingConverter converter = appSetting.Converter;
            if (!converter.IsValidValueType(appSetting.ValueType))
            {
                throw new ArgumentException("appSetting.ValueType is not a supported Type");
            }
            if (!converter.IsValidValue(appSetting.ValueType, value))
            {
                throw new ArgumentException("value is not suitable for appSetting");
            }
            GetHiveQueryOrder(appSetting.Scope, out hive, out nullable);
            bool flag = this.OnQueryCanSetSystemWideValue();
            string str = converter.ConvertToStorage(appSetting.ValueType, value);
            bool flag2 = false;
            if (!flag2)
            {
                if ((hive == SettingsHive.SystemWide) && !flag)
                {
                    flag2 = false;
                }
                else
                {
                    flag2 = this.UnsafeTrySet(hive, appSetting.Path, str);
                }
            }
            if (flag2 || !nullable.HasValue)
            {
                return flag2;
            }
            if ((((SettingsHive) nullable.Value) == SettingsHive.SystemWide) && !flag)
            {
                return false;
            }
            return this.UnsafeTrySet(nullable.Value, appSetting.Path, str);
        }

        public bool UnsafeTrySet(SettingsHive hive, string name, string value) => 
            this.OnTrySet(hive, name, value);

        public bool CanSetSystemWideValue =>
            this.OnQueryCanSetSystemWideValue();

        public object Sync =>
            this.sync;
    }
}

