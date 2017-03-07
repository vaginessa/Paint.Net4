namespace PaintDotNet.AppModel
{
    using PaintDotNet.Dialogs;
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class ExceptionDialogService : IExceptionDialogService
    {
        public void ShowErrorDialog(IWin32Window owner, Exception exception)
        {
            ExceptionDialog.ShowErrorDialog(owner, exception);
        }

        public void ShowErrorDialog(IWin32Window owner, [Optional] string message, Exception exception)
        {
            ExceptionDialog.ShowErrorDialog(owner, message, exception);
        }

        public void ShowErrorDialog(IWin32Window owner, [Optional] string message, string exceptionText)
        {
            ExceptionDialog.ShowErrorDialog(owner, message, exceptionText);
        }
    }
}

