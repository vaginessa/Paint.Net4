namespace PaintDotNet.Updates
{
    using System;

    internal sealed class PdnVersionInfo
    {
        private string[] downloadUrls;
        private string friendlyName;
        private string[] fullDownloadUrls;
        private string infoUrl;
        private bool isFinal;
        private int netFxMajorVersion;
        private int netFxMinorVersion;
        private int netFxServicePack;
        private System.Version version;

        public PdnVersionInfo(System.Version version, string friendlyName, int netFxMajorVersion, int netFxMinorVersion, int netFxServicePack, string infoUrl, string[] downloadUrls, string[] fullDownloadUrls, bool isFinal)
        {
            this.version = version;
            this.friendlyName = friendlyName;
            this.netFxMajorVersion = netFxMajorVersion;
            this.netFxMinorVersion = netFxMinorVersion;
            this.netFxServicePack = netFxServicePack;
            this.infoUrl = infoUrl;
            this.downloadUrls = (string[]) downloadUrls.Clone();
            this.fullDownloadUrls = (string[]) fullDownloadUrls.Clone();
            this.isFinal = isFinal;
        }

        public string ChooseDownloadUrl(bool full)
        {
            string[] fullDownloadUrls;
            DateTime now = DateTime.Now;
            if (full)
            {
                fullDownloadUrls = this.FullDownloadUrls;
            }
            else
            {
                fullDownloadUrls = this.DownloadUrls;
            }
            int index = Math.Abs((int) (now.Second % fullDownloadUrls.Length));
            return fullDownloadUrls[index];
        }

        public string[] DownloadUrls =>
            ((string[]) this.downloadUrls.Clone());

        public string FriendlyName =>
            this.friendlyName;

        public string[] FullDownloadUrls =>
            ((string[]) this.fullDownloadUrls.Clone());

        public string InfoUrl =>
            this.infoUrl;

        public bool IsFinal =>
            this.isFinal;

        public int NetFxMajorVersion =>
            this.netFxMajorVersion;

        public int NetFxMinorVersion =>
            this.netFxMinorVersion;

        public int NetFxServicePack =>
            this.netFxServicePack;

        public System.Version Version =>
            this.version;
    }
}

