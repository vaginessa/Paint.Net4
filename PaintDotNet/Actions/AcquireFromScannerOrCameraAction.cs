namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal sealed class AcquireFromScannerOrCameraAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (appWorkspace.CanSetActiveWorkspace)
            {
                if (!ScanningAndPrinting.CanScan)
                {
                    ShowWiaError(appWorkspace);
                }
                else
                {
                    ScanResult userCancelled;
                    string fileName = Path.ChangeExtension(FileSystem.GetTempFileName(), ".bmp");
                    try
                    {
                        userCancelled = ScanningAndPrinting.Scan(appWorkspace, fileName);
                    }
                    catch (Exception)
                    {
                        userCancelled = ScanResult.UserCancelled;
                    }
                    if (userCancelled == ScanResult.Success)
                    {
                        string str2 = null;
                        try
                        {
                            Image image;
                            Document document;
                            try
                            {
                                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    image = Image.FromStream(stream, false, true);
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                str2 = PdnResources.GetString("LoadImage.Error.FileNotFoundException");
                                throw;
                            }
                            catch (OutOfMemoryException)
                            {
                                str2 = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                                throw;
                            }
                            catch (Exception)
                            {
                                str2 = string.Empty;
                                throw;
                            }
                            try
                            {
                                document = Document.FromGdipImage(image, false);
                            }
                            catch (OutOfMemoryException)
                            {
                                str2 = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                                throw;
                            }
                            catch (Exception)
                            {
                                str2 = string.Empty;
                                throw;
                            }
                            finally
                            {
                                image.Dispose();
                                image = null;
                            }
                            DocumentWorkspace workspace = appWorkspace.AddNewDocumentWorkspace();
                            try
                            {
                                workspace.Document = document;
                            }
                            catch (OutOfMemoryException)
                            {
                                str2 = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                                throw;
                            }
                            document = null;
                            workspace.SetDocumentSaveOptions(null, null, null);
                            workspace.History.ClearAll();
                            HistoryMemento memento = new NullHistoryMemento(PdnResources.GetString("AcquireImageAction.Name"), PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png"));
                            workspace.History.PushNewMemento(memento);
                            appWorkspace.ActiveDocumentWorkspace = workspace;
                            try
                            {
                                File.Delete(fileName);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        catch (Exception exception)
                        {
                            if (str2 == null)
                            {
                                throw;
                            }
                            if (string.IsNullOrEmpty(str2))
                            {
                                ExceptionDialog.ShowErrorDialog(appWorkspace, exception);
                            }
                            else
                            {
                                ExceptionDialog.ShowErrorDialog(appWorkspace, str2, exception);
                            }
                        }
                    }
                }
            }
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

