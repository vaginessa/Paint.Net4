namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using System;
    using System.Collections.Generic;

    internal sealed class EffectsSettingsPage : PropertyBasedSettingsPage
    {
        private readonly EffectsSettingsSection section;

        public EffectsSettingsPage(EffectsSettingsSection section) : base(section)
        {
            this.section = section;
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection properties)
        {
            ControlInfo info = base.OnCreateConfigUI(properties);
            Int32Setting defaultQualityLevel = base.Section.AppSettings.Effects.DefaultQualityLevel;
            info.SetPropertyControlValue(defaultQualityLevel.Path, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SettingsDialog.Effects.DefaultQualityLevel.Header"));
            info.SetPropertyControlValue(defaultQualityLevel.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.Effects.DefaultQualityLevel.Description"));
            return info;
        }

        protected override PropertyCollection OnCreatePropertyCollection() => 
            new PropertyCollection(new List<Property> { base.CreatePropertyFromAppSetting(base.Section.AppSettings.Effects.DefaultQualityLevel) });
    }
}

