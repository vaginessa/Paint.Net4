namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class PasteInToNewImageAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (appWorkspace.CanSetActiveWorkspace)
            {
                try
                {
                    SizeInt32? nullable;
                    IPdnDataObject dataObject;
                    MaskedSurface clipboardImage;
                    try
                    {
                        using (new WaitCursorChanger(appWorkspace))
                        {
                            CleanupManager.RequestCleanup();
                            dataObject = PdnClipboard.GetDataObject();
                            if (ClipboardUtil.IsClipboardImageMaybeAvailable(appWorkspace, dataObject))
                            {
                                clipboardImage = ClipboardUtil.GetClipboardImage(appWorkspace, dataObject);
                                if (clipboardImage != null)
                                {
                                    nullable = new SizeInt32?(clipboardImage.GetCachedGeometryMaskScansBounds().Size);
                                }
                                else
                                {
                                    nullable = null;
                                }
                            }
                            else
                            {
                                clipboardImage = null;
                                nullable = null;
                            }
                        }
                    }
                    catch (OutOfMemoryException exception)
                    {
                        ExceptionDialog.ShowErrorDialog(appWorkspace, PdnResources.GetString("PasteAction.Error.OutOfMemory"), exception);
                        return;
                    }
                    catch (Exception exception2)
                    {
                        ExceptionDialog.ShowErrorDialog(appWorkspace, PdnResources.GetString("PasteAction.Error.TransferFromClipboard"), exception2);
                        return;
                    }
                    if (!nullable.HasValue)
                    {
                        MessageBoxUtil.ErrorBox(appWorkspace, PdnResources.GetString("PasteInToNewImageAction.Error.NoClipboardImage"));
                    }
                    else
                    {
                        Type defaultToolType;
                        SizeInt32 size = nullable.Value;
                        Document document = null;
                        if ((appWorkspace.ActiveDocumentWorkspace != null) && (appWorkspace.ActiveDocumentWorkspace.Tool != null))
                        {
                            defaultToolType = appWorkspace.ActiveDocumentWorkspace.Tool.GetType();
                        }
                        else
                        {
                            defaultToolType = appWorkspace.DefaultToolType;
                        }
                        using (new WaitCursorChanger(appWorkspace))
                        {
                            document = new Document(size);
                            DocumentWorkspace documentWorkspace = appWorkspace.AddNewDocumentWorkspace();
                            documentWorkspace.Document = document;
                            documentWorkspace.History.PushNewMemento(new NullHistoryMemento(string.Empty, null));
                            PasteInToNewLayerAction action = new PasteInToNewLayerAction(documentWorkspace, dataObject, clipboardImage);
                            if (action.PerformAction())
                            {
                                using (new PushNullToolMode(documentWorkspace))
                                {
                                    documentWorkspace.Selection.Reset();
                                    documentWorkspace.SetDocumentSaveOptions(null, null, null);
                                    documentWorkspace.History.ClearAll();
                                    documentWorkspace.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString("Menu.Edit.PasteInToNewImage.Text"), PdnResources.GetImageResource("Icons.MenuEditPasteInToNewImageIcon.png")));
                                    appWorkspace.ActiveDocumentWorkspace = documentWorkspace;
                                }
                                documentWorkspace.SetToolFromType(defaultToolType);
                            }
                            else
                            {
                                appWorkspace.RemoveDocumentWorkspace(documentWorkspace);
                                document.Dispose();
                            }
                        }
                    }
                }
                catch (ExternalException exception3)
                {
                    ExceptionDialog.ShowErrorDialog(appWorkspace, PdnResources.GetString("AcquireImageAction.Error.Clipboard.TransferError"), exception3);
                }
                catch (OutOfMemoryException exception4)
                {
                    ExceptionDialog.ShowErrorDialog(appWorkspace, PdnResources.GetString("AcquireImageAction.Error.Clipboard.OutOfMemory"), exception4);
                }
                catch (ThreadStateException)
                {
                }
            }
        }
    }
}

