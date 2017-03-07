namespace PaintDotNet.Settings
{
    using PaintDotNet.Diagnostics;
    using System;

    internal class SettingsContainer : SettingsView
    {
        private readonly SettingsStorageHandler storageHandler;

        public SettingsContainer(SettingsStorageHandler storageHandler)
        {
            Validate.IsNotNull<SettingsStorageHandler>(storageHandler, "storageHandler");
            this.storageHandler = storageHandler;
        }

        protected override void OnSettingAddedDuringCtor(Setting setting)
        {
            setting.Container = this;
            base.OnSettingAddedDuringCtor(setting);
        }

        public bool CanSetSystemWideValues =>
            this.storageHandler.CanSetSystemWideValue;

        public SettingsStorageHandler StorageHandler =>
            this.storageHandler;
    }
}

