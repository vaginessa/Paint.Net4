namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class DiagnosticsSettingsSection : SettingsDialogSection
    {
        public DiagnosticsSettingsSection(AppSettings appSettings) : base(appSettings, PdnResources.GetString("SettingsDialog.Diagnostics.DisplayName"), PdnResources.GetImageResource("Icons.Settings.Diagnostics.24.png").Reference)
        {
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new DiagnosticsSettingsPage(this);
    }
}

