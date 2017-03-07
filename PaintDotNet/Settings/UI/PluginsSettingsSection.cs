namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.AppModel;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Settings.App;
    using System;

    internal sealed class PluginsSettingsSection : SettingsDialogSection
    {
        private IPluginErrorService pluginErrorService;

        public PluginsSettingsSection(AppSettings appSettings, IPluginErrorService pluginErrorService) : base(appSettings, PdnResources.GetString("SettingsDialog.Plugins.DisplayName"), PdnResources.GetImageResource("Icons.Settings.Plugins.24.png").Reference)
        {
            Validate.IsNotNull<IPluginErrorService>(pluginErrorService, "pluginErrorService");
            this.pluginErrorService = pluginErrorService;
        }

        protected override SettingsDialogPage OnCreateUI() => 
            new PluginsSettingsPage(this);

        public IPluginErrorService PluginErrorService =>
            this.pluginErrorService;
    }
}

