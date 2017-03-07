namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.IndirectUI;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal abstract class PropertyBasedSettingsPage : SettingsDialogPage
    {
        private PropertyCollection propertyCollection;
        private Control propertyUI;
        private ControlInfo propertyUIInfo;

        public PropertyBasedSettingsPage(SettingsDialogSection section) : base(section)
        {
            this.propertyCollection = this.OnCreatePropertyCollection().Clone();
            this.propertyUIInfo = this.OnCreateConfigUI(this.propertyCollection.Clone());
            this.propertyUI = (Control) this.propertyUIInfo.CreateConcreteControl(typeof(Control));
            foreach (Property property in this.propertyCollection)
            {
                PropertyControlInfo info = this.propertyUIInfo.FindControlForPropertyName(property.Name);
                if (info == null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Every property must have a control associated with it");
                }
                else
                {
                    Property controlsProperty = info.Property;
                    controlsProperty.ValueChanged += (s, e) => this.PropertyValueChanged(controlsProperty, e.Value);
                }
            }
            base.SuspendLayout();
            this.propertyUI.Location = new Point(0, 0);
            base.Controls.Add(this.propertyUI);
            base.ResumeLayout(false);
        }

        protected StaticListChoiceProperty CreatePropertyFromAppSetting(CultureInfoSetting appSetting)
        {
            CultureInfo[] items = (from ci in AppSettings.UISection.GetInstalledLanguages()
                orderby ci.NativeName
                select ci).ToArrayEx<CultureInfo>();
            CultureInfo defaultValue = appSetting.DefaultValue;
            int index = items.IndexOf<CultureInfo>(defaultValue);
            int num2 = items.IndexOf<CultureInfo>(new CultureInfo("en"));
            int num3 = items.IndexOf<CultureInfo>(new CultureInfo("en-US"));
            return new StaticListChoiceProperty(appSetting.Path, items, (index != -1) ? index : ((num3 != -1) ? num3 : ((num2 != -1) ? num2 : 0))) { Value = appSetting.Value };
        }

        protected StaticListChoiceProperty CreatePropertyFromAppSetting<TEnum>(EnumSetting<TEnum> appSetting) where TEnum: struct
        {
            object[] valueChoices = Enum.GetValues(typeof(TEnum)).Cast<object>().ToArrayEx<object>();
            return new StaticListChoiceProperty(appSetting.Path, valueChoices, valueChoices.IndexOf<object>(appSetting.DefaultValue)) { Value = appSetting.Value };
        }

        protected Int32Property CreatePropertyFromAppSetting(Int32Setting appSetting) => 
            new Int32Property(appSetting.Path, appSetting.DefaultValue, appSetting.MinValue, appSetting.MaxValue) { Value = appSetting.Value };

        protected BooleanProperty CreatePropertyFromAppSetting(Setting<bool> appSetting) => 
            new BooleanProperty(appSetting.Path, appSetting.DefaultValue) { Value = appSetting.Value };

        protected StaticListChoiceProperty CreatePropertyFromAppSetting<TEnum>(EnumSetting<TEnum> appSetting, TEnum defaultValue, params object[] valueChoices) where TEnum: struct
        {
            int index = valueChoices.IndexOf<object>(defaultValue);
            if (index == -1)
            {
                ExceptionUtil.ThrowArgumentException("defaultValue");
            }
            return new StaticListChoiceProperty(appSetting.Path, valueChoices, index) { Value = appSetting.Value };
        }

        protected virtual ControlInfo OnCreateConfigUI(PropertyCollection properties) => 
            ControlInfo.CreateDefaultConfigUI(properties);

        protected abstract PropertyCollection OnCreatePropertyCollection();
        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.propertyUI.Width = base.ClientSize.Width;
            base.OnLayout(levent);
        }

        protected virtual void OnPropertyValueChanged(Property property, object newValue)
        {
        }

        private void PropertyValueChanged(Property property, object newValue)
        {
            Setting setting;
            if (base.Section.AppSettings.TryFindSetting(property.Name, out setting))
            {
                object obj2 = setting.Value;
                setting.Value = newValue;
            }
            this.OnPropertyValueChanged(property, newValue);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PropertyBasedSettingsPage.<>c <>9 = new PropertyBasedSettingsPage.<>c();
            public static Func<CultureInfo, string> <>9__8_0;

            internal string <CreatePropertyFromAppSetting>b__8_0(CultureInfo ci) => 
                ci.NativeName;
        }
    }
}

