namespace PaintDotNet.AppModel
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings;
    using System;
    using System.Collections.Generic;

    internal sealed class SettingsService : ISettingsService
    {
        private HashSet<string> allowedSettings;
        private SettingsContainer settings;

        public SettingsService(SettingsContainer settings, IEnumerable<string> allowedSettings)
        {
            Validate.IsNotNull<SettingsContainer>(settings, "settings");
            this.settings = settings;
            this.allowedSettings = new HashSet<string>(allowedSettings, SettingPath.PathEqualityComparer);
        }

        public SettingsService(SettingsContainer settings, params string[] allowedSettings) : this(settings, (IEnumerable<string>) allowedSettings)
        {
        }

        public ISetting GetSetting(string path)
        {
            if (!this.allowedSettings.Contains(path))
            {
                throw new KeyNotFoundException(path);
            }
            Setting setting = this.settings[path];
            return SettingWrapper.CreateWrapper(setting, true);
        }
    }
}

