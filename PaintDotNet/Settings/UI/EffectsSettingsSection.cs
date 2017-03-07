namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class EffectsSettingsSection : SettingsDialogSection
    {
        public EffectsSettingsSection(AppSettings appSettings) : base(appSettings, PdnResources.GetString("SettingsDialog.Effects.DisplayName"), PdnResources.GetImageResource("Icons.Settings.Effects.24.png").Reference)
        {
            base.AddSetting(base.AppSettings.Effects.DefaultQualityLevel);
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new EffectsSettingsPage(this);
    }
}

