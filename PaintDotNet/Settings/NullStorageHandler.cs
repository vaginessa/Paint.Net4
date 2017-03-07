namespace PaintDotNet.Settings
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class NullStorageHandler : SettingsStorageHandler
    {
        private static readonly NullStorageHandler instance = new NullStorageHandler();

        private NullStorageHandler()
        {
        }

        protected override bool OnQueryCanSetSystemWideValue() => 
            false;

        protected override bool OnTryGet(SettingsHive hive, string name, out string value)
        {
            value = null;
            return false;
        }

        protected override bool OnTrySet(SettingsHive hive, string name, string value) => 
            false;

        public static NullStorageHandler Instance =>
            instance;
    }
}

