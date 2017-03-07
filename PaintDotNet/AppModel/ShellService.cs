namespace PaintDotNet.AppModel
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal sealed class ShellService : IShellService
    {
        public bool LaunchFolder(IWin32Window owner, string folderPath)
        {
            Validate.Begin().IsNotNull<IWin32Window>(owner, "owner").IsNotNull<string>(folderPath, "folderPath").Check().IsNotZero(owner.Handle, "owner.Handle").IsNotEmpty(folderPath, "folderPath").Check();
            Result result = delegate {
                ShellUtil.BrowseFolder2(owner, folderPath);
            }.Try();
            result.Observe();
            return !result.IsError;
        }

        public bool LaunchUrl(IWin32Window owner, string url)
        {
            Validate.Begin().IsNotNull<string>(url, "url").Check().IsNotEmpty(url, "url").Check();
            return ShellUtil.LaunchUrl2(owner, url);
        }
    }
}

