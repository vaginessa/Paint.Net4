namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    internal sealed class PrintAction : DocumentWorkspaceAction
    {
        public PrintAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (!ScanningAndPrinting.CanPrint)
            {
                ShowWiaError(documentWorkspace);
                return null;
            }
            using (new PushNullToolMode(documentWorkspace))
            {
                Surface surface = documentWorkspace.BorrowScratchSurface(base.GetType().Name + ".PerformAction()");
                try
                {
                    surface.Clear();
                    RenderArgs args = new RenderArgs(surface);
                    documentWorkspace.QueueUpdate();
                    using (new WaitCursorChanger(documentWorkspace))
                    {
                        args.Surface.Clear(ColorBgra.White);
                        documentWorkspace.Document.Render(args, false);
                    }
                    string filename = Path.GetTempFileName() + ".bmp";
                    args.Bitmap.Save(filename, ImageFormat.Bmp);
                    try
                    {
                        ScanningAndPrinting.Print(documentWorkspace, filename);
                    }
                    catch (Exception)
                    {
                        ShowWiaError(documentWorkspace);
                    }
                    bool flag = FileSystem.TryDeleteFile(filename);
                }
                catch (Exception exception)
                {
                    ExceptionDialog.ShowErrorDialog(documentWorkspace, exception);
                }
                finally
                {
                    documentWorkspace.ReturnScratchSurface(surface);
                }
            }
            return null;
        }

        public static void ShowWiaError(IWin32Window owner)
        {
            if (OS.OSType == OSType.Server)
            {
                MessageBoxUtil.ErrorBox(owner, PdnResources.GetString("WIA.Error.EnableMe"));
            }
            else
            {
                MessageBoxUtil.ErrorBox(owner, PdnResources.GetString("WIA.Error.UnableToLoad"));
            }
        }
    }
}

