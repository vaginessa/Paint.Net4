namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Imaging.Proxies;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    internal abstract class SettingsDialogSection
    {
        private PaintDotNet.Settings.App.AppSettings appSettings;
        private BitmapProxy bitmapIcon;
        private DeviceBitmap deviceIcon;
        private string displayName;
        private Image icon;
        private HashSet<Setting> sectionSettings;

        protected SettingsDialogSection(PaintDotNet.Settings.App.AppSettings appSettings, string displayName, Image icon)
        {
            Validate.Begin().IsNotNull<PaintDotNet.Settings.App.AppSettings>(appSettings, "appSettings").IsNotNullOrWhiteSpace(displayName, "displayName").IsNotNull<Image>(icon, "icon").Check();
            this.appSettings = appSettings;
            this.displayName = displayName;
            this.icon = icon;
            Surface cleanupObject = Surface.CopyFromGdipImage(this.icon);
            this.bitmapIcon = new BitmapProxy(cleanupObject.CreateAliasedImagingBitmap(), ObjectRefProxyOptions.AssumeOwnership);
            this.bitmapIcon.AddCleanupObject(cleanupObject);
            this.deviceIcon = new DeviceBitmap(this.bitmapIcon);
            this.sectionSettings = new HashSet<Setting>();
        }

        protected void AddSetting(Setting setting)
        {
            this.sectionSettings.Add(setting);
        }

        public SettingsDialogPage CreateUI() => 
            this.OnCreateUI();

        protected abstract SettingsDialogPage OnCreateUI();

        public PaintDotNet.Settings.App.AppSettings AppSettings =>
            this.appSettings;

        public DeviceBitmap DeviceIcon =>
            this.deviceIcon;

        public string DisplayName =>
            this.displayName;

        public Image Icon =>
            this.icon;

        public Setting[] Settings =>
            this.sectionSettings.ToArrayEx<Setting>();
    }
}

