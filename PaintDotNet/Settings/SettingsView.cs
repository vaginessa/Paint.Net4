namespace PaintDotNet.Settings
{
    using PaintDotNet;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class SettingsView : IEnumerable<Setting>, IEnumerable
    {
        private bool isInitialized;
        private Dictionary<string, Setting> pathToSettingMap = new Dictionary<string, Setting>(SettingPath.PathEqualityComparer);
        private List<Setting> settings = new List<Setting>();
        private object sync = new object();

        protected TSetting AddSettingDuringCtor<TSetting>(TSetting setting) where TSetting: Setting
        {
            this.settings.Add(setting);
            this.pathToSettingMap.Add(setting.Path, setting);
            this.OnSettingAddedDuringCtor(setting);
            return setting;
        }

        protected TSettings AddSettingsDuringCtor<TSettings>(TSettings settings) where TSettings: IEnumerable<Setting>
        {
            List<Setting> list = settings as List<Setting>;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    this.AddSettingDuringCtor<Setting>(list[i]);
                }
                return settings;
            }
            foreach (Setting setting in settings)
            {
                this.AddSettingDuringCtor<Setting>(setting);
            }
            return settings;
        }

        protected void EndInit()
        {
            if (this.isInitialized)
            {
                ExceptionUtil.ThrowInvalidOperationException("Already finished initialization");
            }
            this.isInitialized = true;
        }

        public IEnumerator<Setting> GetEnumerator()
        {
            this.VerifyIsInitialized();
            return this.settings.GetEnumerator();
        }

        protected virtual void OnSettingAddedDuringCtor(Setting setting)
        {
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryFindSetting(string path, out Setting result)
        {
            object sync = this.Sync;
            lock (sync)
            {
                return this.pathToSettingMap.TryGetValue(path, out result);
            }
        }

        private void VerifyIsInitialized()
        {
            if (!this.isInitialized)
            {
                ExceptionUtil.ThrowInvalidOperationException("Haven't finished initialization");
            }
        }

        public int Count =>
            this.settings.Count;

        public Setting this[int index]
        {
            get
            {
                this.VerifyIsInitialized();
                return this.settings[index];
            }
        }

        public Setting this[string path]
        {
            get
            {
                object sync = this.Sync;
                lock (sync)
                {
                    this.VerifyIsInitialized();
                    return this.pathToSettingMap[path];
                }
            }
        }

        protected object Sync =>
            this.sync;
    }
}

