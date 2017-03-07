namespace PaintDotNet.Updates
{
    using System;
    using System.Collections;

    internal sealed class PdnVersionManifest
    {
        private string downloadPageUrl;
        private PdnVersionInfo[] versionInfos;

        public PdnVersionManifest(string downloadPageUrl, PdnVersionInfo[] versionInfos)
        {
            this.downloadPageUrl = downloadPageUrl;
            this.versionInfos = (PdnVersionInfo[]) versionInfos.Clone();
        }

        public int GetLatestBetaVersionIndex()
        {
            PdnVersionInfo[] versionInfos = this.VersionInfos;
            Array.Sort(versionInfos, new PdnVersionInfoComparer());
            for (int i = versionInfos.Length - 1; i >= 0; i--)
            {
                if (!versionInfos[i].IsFinal)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetLatestStableVersionIndex()
        {
            PdnVersionInfo[] versionInfos = this.VersionInfos;
            Array.Sort(versionInfos, new PdnVersionInfoComparer());
            for (int i = versionInfos.Length - 1; i >= 0; i--)
            {
                if (versionInfos[i].IsFinal)
                {
                    return i;
                }
            }
            return -1;
        }

        public string DownloadPageUrl =>
            this.downloadPageUrl;

        public PdnVersionInfo[] VersionInfos =>
            this.versionInfos;

        private class PdnVersionInfoComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                PdnVersionInfo info = (PdnVersionInfo) x;
                PdnVersionInfo info2 = (PdnVersionInfo) y;
                if (info.Version < info2.Version)
                {
                    return -1;
                }
                if (info.Version == info2.Version)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}

