namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class UISettingsSection : SettingsDialogSection
    {
        public UISettingsSection(AppSettings appSettings) : base(appSettings, PdnResources.GetString("SettingsDialog.UI.DisplayName"), PdnResources.GetImageResource("Icons.Settings.UI.24.png").Reference)
        {
            base.AddSetting(base.AppSettings.UI.Language);
            base.AddSetting(base.AppSettings.UI.ShowTaskbarPreviews);
            base.AddSetting(base.AppSettings.UI.TranslucentWindows);
            base.AddSetting(base.AppSettings.UI.AeroColorScheme);
            base.AddSetting(base.AppSettings.UI.DefaultTextAntialiasMode);
            base.AddSetting(base.AppSettings.UI.DefaultTextRenderingMode);
            base.AddSetting(base.AppSettings.UI.EnableHardwareAcceleration);
            base.AddSetting(base.AppSettings.UI.EnableAnimations);
            base.AddSetting(base.AppSettings.UI.EnableOverscroll);
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new UISettingsPage(this);
    }
}

