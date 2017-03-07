namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class UISettingsPage : PropertyBasedSettingsPage
    {
        private readonly UISettingsSection section;

        public UISettingsPage(UISettingsSection section) : base(section)
        {
            this.section = section;
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection properties)
        {
            ControlInfo info = base.OnCreateConfigUI(properties);
            BooleanSetting enableHardwareAcceleration = base.Section.AppSettings.UI.EnableHardwareAcceleration;
            info.SetPropertyControlValue(enableHardwareAcceleration.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(enableHardwareAcceleration.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.EnableHardwareAcceleration.Description"));
            BooleanSetting enableAnimations = base.Section.AppSettings.UI.EnableAnimations;
            info.SetPropertyControlValue(enableAnimations.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(enableAnimations.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.EnableAnimations.Description"));
            BooleanSetting enableAntialiasedSelectionOutline = base.Section.AppSettings.UI.EnableAntialiasedSelectionOutline;
            info.SetPropertyControlValue(enableAntialiasedSelectionOutline.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(enableAntialiasedSelectionOutline.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.EnableAntialiasedSelectionOutline.Description"));
            BooleanSetting showTaskbarPreviews = base.Section.AppSettings.UI.ShowTaskbarPreviews;
            info.SetPropertyControlValue(showTaskbarPreviews.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(showTaskbarPreviews.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.ShowTaskbarPreviews.Description"));
            BooleanSetting enableOverscroll = base.Section.AppSettings.UI.EnableOverscroll;
            info.SetPropertyControlValue(enableOverscroll.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(enableOverscroll.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.Overscroll.Description"));
            BooleanSetting translucentWindows = base.Section.AppSettings.UI.TranslucentWindows;
            info.SetPropertyControlValue(translucentWindows.Path, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(translucentWindows.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.TranslucentWindows.Description"));
            EnumSetting<AeroColorScheme> aeroColorScheme = base.Section.AppSettings.UI.AeroColorScheme;
            info.SetPropertyControlValue(aeroColorScheme.Path, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SettingsDialog.UI.AeroColorScheme.DisplayName"));
            PropertyControlInfo info2 = info.FindControlForPropertyName(aeroColorScheme.Path);
            info2.SetValueDisplayName(AeroColorScheme.Blue, PdnResources.GetString("SettingsDialog.UI.AeroColorScheme.Value.Blue"));
            info2.SetValueDisplayName(AeroColorScheme.Light, PdnResources.GetString("SettingsDialog.UI.AeroColorScheme.Value.Light"));
            if (ThemeConfig.EffectiveTheme == PdnTheme.Classic)
            {
                info.SetPropertyControlValue(aeroColorScheme.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.AeroColorScheme.Description.ClassicDisabled"));
            }
            CultureInfoSetting language = base.Section.AppSettings.UI.Language;
            info.SetPropertyControlValue(language.Path, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SettingsDialog.UI.Language.DisplayName"));
            info.SetPropertyControlValue(language.Path, ControlInfoPropertyNames.Description, PdnResources.GetString("SettingsDialog.UI.Language.Description"));
            PropertyControlInfo info3 = info.FindControlForPropertyName(language.Path);
            StaticListChoiceProperty property = (StaticListChoiceProperty) info3.Property;
            CultureInfo info4 = new CultureInfo("en-US");
            foreach (CultureInfo info5 in property.ValueChoices)
            {
                string nativeName;
                if (info5.Equals(info4))
                {
                    nativeName = info5.Parent.NativeName;
                }
                else
                {
                    nativeName = info5.NativeName;
                }
                info3.SetValueDisplayName(info5, nativeName);
            }
            return info;
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> properties = new List<Property> {
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.EnableHardwareAcceleration),
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.EnableAnimations),
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.TranslucentWindows),
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.ShowTaskbarPreviews),
                base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.EnableOverscroll)
            };
            StaticListChoiceProperty item = base.CreatePropertyFromAppSetting<AeroColorScheme>(base.Section.AppSettings.UI.AeroColorScheme);
            if (ThemeConfig.EffectiveTheme == PdnTheme.Classic)
            {
                item.ReadOnly = true;
            }
            properties.Add(item);
            properties.Add(base.CreatePropertyFromAppSetting(base.Section.AppSettings.UI.Language));
            return new PropertyCollection(properties);
        }
    }
}

