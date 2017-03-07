namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class UpdatesSettingsSection : SettingsDialogSection
    {
        private readonly SettingsDialog owner;

        public UpdatesSettingsSection(SettingsDialog owner, AppSettings appSettingsContainer) : base(appSettingsContainer, PdnResources.GetString("SettingsDialog.Updates.DisplayName"), PdnResources.GetImageResource("Icons.Settings.Updates.24.png").Reference)
        {
            Validate.IsNotNull<SettingsDialog>(owner, "owner");
            this.owner = owner;
            base.AddSetting(base.AppSettings.Updates.AutoCheck);
            base.AddSetting(base.AppSettings.Updates.AutoCheckForPrerelease);
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new UpdatesSettingsPage(this);

        internal void PerformUpdateCheck()
        {
            this.owner.PerformUpdateCheck();
        }
    }
}

