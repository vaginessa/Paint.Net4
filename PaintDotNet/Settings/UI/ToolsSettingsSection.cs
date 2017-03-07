namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class ToolsSettingsSection : SettingsDialogSection
    {
        private AppSettings.ToolsSection toolBarSettings;

        public ToolsSettingsSection(AppSettings appSettings, AppSettings.ToolsSection toolBarSettings) : base(appSettings, PdnResources.GetString("SettingsDialog.Tools.DisplayName"), PdnResources.GetImageResource("Icons.Settings.Tools.24.png").Reference)
        {
            this.toolBarSettings = toolBarSettings;
            foreach (Setting setting in appSettings.ToolDefaults.Settings)
            {
                base.AddSetting(setting);
            }
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new ToolsSettingsPage(this);

        public AppSettings.ToolsSection ToolBarSettings =>
            this.toolBarSettings;
    }
}

