namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal sealed class MemoryStorageHandler : SettingsStorageHandler
    {
        private bool canSetSystemWideValues;
        private Dictionary<string, string> currentUserValues;
        private Dictionary<string, string> systemWideValues;

        public MemoryStorageHandler(bool allowSettingSystemWideValues = false)
        {
            this.canSetSystemWideValues = allowSettingSystemWideValues;
            this.currentUserValues = new Dictionary<string, string>(SettingPath.PathEqualityComparer);
            this.systemWideValues = new Dictionary<string, string>(SettingPath.PathEqualityComparer);
        }

        private Dictionary<string, string> GetHive(SettingsHive hive)
        {
            if (hive != SettingsHive.CurrentUser)
            {
                if (hive != SettingsHive.SystemWide)
                {
                    throw new InternalErrorException();
                }
                return this.systemWideValues;
            }
            return this.currentUserValues;
        }

        protected override bool OnQueryCanSetSystemWideValue() => 
            this.canSetSystemWideValues;

        protected override bool OnTryGet(SettingsHive hive, string name, out string value)
        {
            object sync = base.Sync;
            lock (sync)
            {
                return this.GetHive(hive).TryGetValue(name, out value);
            }
        }

        protected override bool OnTrySet(SettingsHive hive, string name, string value)
        {
            object sync = base.Sync;
            lock (sync)
            {
                this.GetHive(hive)[name] = value;
                return true;
            }
        }

        public bool Remove(SettingsHive hive, string name)
        {
            object sync = base.Sync;
            lock (sync)
            {
                return this.GetHive(hive).Remove(name);
            }
        }

        public string[] CurrentUserAppSettingNames
        {
            get
            {
                object sync = base.Sync;
                lock (sync)
                {
                    return this.currentUserValues.Keys.ToArrayEx<string>();
                }
            }
        }

        public string[] SystemWideAppSettingNames
        {
            get
            {
                object sync = base.Sync;
                lock (sync)
                {
                    return this.systemWideValues.Keys.ToArrayEx<string>();
                }
            }
        }
    }
}

