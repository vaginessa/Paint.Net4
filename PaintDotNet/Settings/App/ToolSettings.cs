namespace PaintDotNet.Settings.App
{
    using PaintDotNet.Settings;
    using System;

    internal sealed class ToolSettings : SettingsBase
    {
        private static ToolSettings nullInstance;
        public readonly AppSettings.ToolsSection Tools;

        public ToolSettings(SettingsStorageHandler storageHandler) : base(storageHandler)
        {
            this.Tools = base.RegisterSectionDuringCtor<AppSettings.ToolsSection>(new AppSettings.ToolsSection("Tools"));
            base.EndInit();
        }

        public static AppSettings.ToolsSection Null
        {
            get
            {
                if (nullInstance == null)
                {
                    nullInstance = new ToolSettings(NullStorageHandler.Instance);
                }
                return nullInstance.Tools;
            }
        }
    }
}

