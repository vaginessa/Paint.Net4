namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using System;
    using System.Collections.Generic;

    internal sealed class UpdatesSettingsPage : PropertyBasedSettingsPage
    {
        private readonly UpdatesSettingsSection section;

        public UpdatesSettingsPage(UpdatesSettingsSection section) : base(section)
        {
            this.section = section;
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection properties)
        {
            ControlInfo info = base.OnCreateConfigUI(properties);
            IntegerBooleanSetting autoCheck = base.Section.AppSettings.Updates.AutoCheck;
            info.SetPropertyControlValue(autoCheck.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(autoCheck.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.Updates.AutoCheck.Description"));
            info.SetPropertyControlType(PropertyNames.CheckNow, PropertyControlType.IncrementButton);
            info.SetPropertyControlValue(PropertyNames.CheckNow, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.CheckNow, ControlInfoPropertyNames.ButtonText, PdnResources.GetString("SettingsDialog.Updates.CheckNow.ButtonText"));
            IntegerBooleanSetting autoCheckForPrerelease = base.Section.AppSettings.Updates.AutoCheckForPrerelease;
            info.SetPropertyControlValue(autoCheckForPrerelease.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(autoCheckForPrerelease.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.Updates.AutoCheckForPrelease.Description"));
            return info;
        }

        protected override PropertyCollection OnCreatePropertyCollection() => 
            new PropertyCollection(new List<Property> { 
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.Updates.AutoCheck),
                new Int32Property(PropertyNames.CheckNow, 0, 0, 100),
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.Updates.AutoCheckForPrerelease)
            });

        protected override void OnPropertyValueChanged(Property property, object newValue)
        {
            PropertyNames checkNow = PropertyNames.CheckNow;
            if (property.Name == checkNow.ToString())
            {
                this.section.PerformUpdateCheck();
            }
            base.OnPropertyValueChanged(property, newValue);
        }

        private enum PropertyNames
        {
            CheckNow
        }
    }
}

