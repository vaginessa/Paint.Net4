namespace PaintDotNet.AppModel
{
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class HelpService
    {
        private static HelpService instance = new HelpService();

        public void ShowHelp(IWin32Window owner)
        {
            string str = "http://www.getpaint.net";
            string format = "{0}/doc/latest/index.html";
            string url = string.Format(format, str);
            PdnInfo.OpenUrl2(owner, url);
        }

        public static HelpService Instance =>
            instance;
    }
}

