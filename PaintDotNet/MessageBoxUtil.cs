namespace PaintDotNet
{
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal static class MessageBoxUtil
    {
        public static DialogResult AskYesNo(IWin32Window parent, string question) => 
            UIUtil.MessageBox(parent, question, PdnInfo.BareProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        public static void ErrorBox(IWin32Window parent, string message)
        {
            UIUtil.MessageBox(parent, message, PdnInfo.BareProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }
}

