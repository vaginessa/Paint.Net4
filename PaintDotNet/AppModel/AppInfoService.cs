namespace PaintDotNet.AppModel
{
    using PaintDotNet.Resources;
    using System;

    internal sealed class AppInfoService : IAppInfoService
    {
        public Version AppVersion =>
            PdnInfo.Version;

        public string InstallDirectory =>
            PdnInfo.ApplicationDir;

        [Obsolete("Use the UserFilesService instead.", true)]
        public string UserDataDirectory =>
            UserFilesService.Instance.TryGetUserFilesPath();
    }
}

