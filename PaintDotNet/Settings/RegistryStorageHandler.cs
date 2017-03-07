namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class RegistryStorageHandler : SettingsStorageHandler
    {
        private static readonly RegistryStorageHandler instance = new RegistryStorageHandler();

        private RegistryStorageHandler()
        {
        }

        private RegistrySettings GetHive(SettingsHive hive)
        {
            if (hive != SettingsHive.CurrentUser)
            {
                if (hive != SettingsHive.SystemWide)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<SettingsHive>(hive, "hive");
                }
                return RegistrySettings.SystemWide;
            }
            return RegistrySettings.CurrentUser;
        }

        protected override bool OnQueryCanSetSystemWideValue() => 
            Security.IsAdministrator;

        protected override bool OnTryGet(SettingsHive hive, string name, out string value)
        {
            RegistrySettings settings = this.GetHive(hive);
            try
            {
                value = settings.GetString(name);
                return (value > null);
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        protected override bool OnTrySet(SettingsHive hive, string name, string value)
        {
            RegistrySettings settings = this.GetHive(hive);
            try
            {
                settings.SetString(name, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static RegistryStorageHandler Instance =>
            instance;
    }
}

