namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class SettingsSection
    {
        private const int initialCapacity = 0x20;
        private readonly Dictionary<string, Setting> nameToSettingMap;
        private readonly SettingsSection parentSection;
        private readonly string path;
        private readonly List<Setting> settings;

        public SettingsSection(string path)
        {
            this.settings = new List<Setting>(0x20);
            this.nameToSettingMap = new Dictionary<string, Setting>(0x20, SettingPath.PathEqualityComparer);
            Validate.IsNotNullOrWhiteSpace(path, "path");
            this.path = path;
            this.parentSection = null;
        }

        public SettingsSection(SettingsSection parentSection, string subPath)
        {
            this.settings = new List<Setting>(0x20);
            this.nameToSettingMap = new Dictionary<string, Setting>(0x20, SettingPath.PathEqualityComparer);
            Validate.Begin().IsNotNull<SettingsSection>(parentSection, "parentSection").IsNotNullOrWhiteSpace(subPath, "subPath").Check();
            this.parentSection = parentSection;
            this.path = parentSection.Path + "/" + subPath;
        }

        protected string GetSettingPath(string settingName) => 
            SettingPath.Combine(this.Path, settingName);

        protected TSetting Register<TSetting>(TSetting setting) where TSetting: Setting
        {
            this.settings.Add(setting);
            this.nameToSettingMap.Add(setting.Path, setting);
            if (this.parentSection != null)
            {
                this.parentSection.Register<TSetting>(setting);
            }
            return setting;
        }

        public int Count =>
            this.settings.Count;

        public Setting this[int index] =>
            this.settings[index];

        public Setting this[string path] =>
            this.nameToSettingMap[path];

        public SettingsSection Parent =>
            this.parentSection;

        public string Path =>
            this.path;

        public IEnumerable<Setting> Settings =>
            this.settings;
    }
}

