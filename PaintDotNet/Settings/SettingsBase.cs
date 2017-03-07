namespace PaintDotNet.Settings
{
    using System;

    internal abstract class SettingsBase : SettingsContainer
    {
        protected SettingsBase(SettingsStorageHandler storageHandler) : base(storageHandler)
        {
        }

        protected TSection RegisterSectionDuringCtor<TSection>(TSection section) where TSection: SettingsSection
        {
            base.AddSettingsDuringCtor<IEnumerable<Setting>>(section.Settings);
            return section;
        }
    }
}

