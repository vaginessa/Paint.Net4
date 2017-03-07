namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class NewImageAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (appWorkspace.CanSetActiveWorkspace)
            {
                using (NewFileDialog dialog = new NewFileDialog())
                {
                    SizeInt32? clipboardImageSize;
                    SizeInt32 newDocumentSize = appWorkspace.GetNewDocumentSize();
                    using (new WaitCursorChanger(appWorkspace))
                    {
                        CleanupManager.RequestCleanup();
                        try
                        {
                            IPdnDataObject dataObject = PdnClipboard.GetDataObject();
                            clipboardImageSize = ClipboardUtil.GetClipboardImageSize(appWorkspace, dataObject);
                            dataObject = null;
                        }
                        catch (Exception)
                        {
                            clipboardImageSize = null;
                        }
                        CleanupManager.RequestCleanup();
                    }
                    if (clipboardImageSize.HasValue)
                    {
                        newDocumentSize = clipboardImageSize.Value;
                    }
                    dialog.OriginalSize = new Size(newDocumentSize.Width, newDocumentSize.Height);
                    dialog.OriginalDpuUnit = AppSettings.Instance.Workspace.LastNonPixelUnits.Value;
                    dialog.OriginalDpu = Document.GetDefaultDpu(dialog.OriginalDpuUnit);
                    dialog.Units = dialog.OriginalDpuUnit;
                    dialog.Resolution = dialog.OriginalDpu;
                    dialog.ConstrainToAspect = AppSettings.Instance.Workspace.LastMaintainAspectRatioNF.Value;
                    if ((((dialog.ShowDialog(appWorkspace) == DialogResult.OK) && (dialog.ImageWidth > 0)) && ((dialog.ImageHeight > 0) && dialog.Resolution.IsFinite())) && (dialog.Resolution > 0.0))
                    {
                        SizeInt32 size = new SizeInt32(dialog.ImageWidth, dialog.ImageHeight);
                        if (appWorkspace.CreateBlankDocumentInNewWorkspace(size, dialog.Units, dialog.Resolution, false))
                        {
                            appWorkspace.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                            AppSettings.Instance.Workspace.LastMaintainAspectRatioNF.Value = dialog.ConstrainToAspect;
                            if (dialog.Units != MeasurementUnit.Pixel)
                            {
                                AppSettings.Instance.Workspace.LastNonPixelUnits.Value = dialog.Units;
                            }
                            if (appWorkspace.Units != MeasurementUnit.Pixel)
                            {
                                appWorkspace.Units = dialog.Units;
                            }
                        }
                    }
                }
            }
        }
    }
}

