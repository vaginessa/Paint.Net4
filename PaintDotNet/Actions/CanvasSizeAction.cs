namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Settings.App;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class CanvasSizeAction : DocumentWorkspaceAction
    {
        public CanvasSizeAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            AnchorEdge initialAnchor = AppSettings.Instance.Workspace.LastCanvasSizeAnchorEdge.Value;
            Document document = ResizeDocument(documentWorkspace, initialAnchor, documentWorkspace.ToolSettings.SecondaryColor.Value, true, true);
            if (document != null)
            {
                using (new PushNullToolMode(documentWorkspace))
                {
                    List<HistoryMemento> actions = new List<HistoryMemento>(2);
                    SelectionHistoryMemento item = new SelectionHistoryMemento(null, null, documentWorkspace);
                    actions.Add(item);
                    if (document.DpuUnit != MeasurementUnit.Pixel)
                    {
                        AppSettings.Instance.Workspace.LastNonPixelUnits.Value = document.DpuUnit;
                        if (documentWorkspace.AppWorkspace.Units != MeasurementUnit.Pixel)
                        {
                            documentWorkspace.AppWorkspace.Units = document.DpuUnit;
                        }
                    }
                    ReplaceDocumentHistoryMemento memento2 = new ReplaceDocumentHistoryMemento(null, null, documentWorkspace);
                    actions.Add(memento2);
                    documentWorkspace.Document = document;
                    return new CompoundHistoryMemento(StaticName, StaticImage, actions);
                }
            }
            return null;
        }

        public static Document ResizeDocument(Document document, Size newSize, AnchorEdge edge, ColorBgra background)
        {
            Document document2 = new Document(newSize.Width, newSize.Height);
            document2.ReplaceMetadataFrom(document);
            for (int i = 0; i < document.Layers.Count; i++)
            {
                Layer layer = (Layer) document.Layers[i];
                if (layer is BitmapLayer)
                {
                    Layer layer2;
                    try
                    {
                        layer2 = ResizeLayer((BitmapLayer) layer, newSize, edge, background);
                    }
                    catch (OutOfMemoryException)
                    {
                        document2.Dispose();
                        throw;
                    }
                    document2.Layers.Add(layer2);
                }
                else
                {
                    ExceptionUtil.ThrowInvalidOperationException("Canvas Size does not support Layers that are not BitmapLayers");
                }
            }
            return document2;
        }

        private static Document ResizeDocument(DocumentWorkspace documentWorkspace, AnchorEdge initialAnchor, ColorBgra background, bool loadAndSaveMaintainAspect, bool saveAnchor)
        {
            Document document2;
            CleanupManager.RequestCleanup();
            IWin32Window owner = documentWorkspace;
            Document document = documentWorkspace.Document;
            Size size = document.Size;
            using (CanvasSizeDialog dialog = new CanvasSizeDialog())
            {
                bool flag;
                if (loadAndSaveMaintainAspect)
                {
                    flag = AppSettings.Instance.Workspace.LastMaintainAspectRatioCS.Value;
                }
                else
                {
                    flag = false;
                }
                dialog.OriginalSize = document.Size;
                dialog.OriginalDpuUnit = document.DpuUnit;
                dialog.OriginalDpu = document.DpuX;
                dialog.ImageWidth = size.Width;
                dialog.ImageHeight = size.Height;
                dialog.LayerCount = document.Layers.Count;
                dialog.AnchorEdge = initialAnchor;
                dialog.Units = dialog.OriginalDpuUnit;
                dialog.Resolution = document.DpuX;
                dialog.Units = AppSettings.Instance.Workspace.LastNonPixelUnits.Value;
                dialog.ConstrainToAspect = flag;
                DialogResult result = dialog.ShowDialog(owner);
                Size newSize = new Size(dialog.ImageWidth, dialog.ImageHeight);
                MeasurementUnit units = dialog.Units;
                double resolution = dialog.Resolution;
                if (result == DialogResult.Cancel)
                {
                    return null;
                }
                if (loadAndSaveMaintainAspect)
                {
                    AppSettings.Instance.Workspace.LastMaintainAspectRatioCS.Value = dialog.ConstrainToAspect;
                }
                if (saveAnchor)
                {
                    AppSettings.Instance.Workspace.LastCanvasSizeAnchorEdge.Value = dialog.AnchorEdge;
                }
                if (((newSize == document.Size) && (units == document.DpuUnit)) && (resolution == document.DpuX))
                {
                    document2 = null;
                }
                else
                {
                    try
                    {
                        documentWorkspace.FlushTool();
                        CleanupManager.RequestCleanup();
                        Document document3 = ResizeDocument(document, newSize, dialog.AnchorEdge, background);
                        document3.DpuUnit = units;
                        document3.DpuX = resolution;
                        document3.DpuY = resolution;
                        document2 = document3;
                    }
                    catch (OutOfMemoryException exception)
                    {
                        ExceptionDialog.ShowErrorDialog(owner, PdnResources.GetString("CanvasSizeAction.ResizeDocument.OutOfMemory"), exception.ToString());
                        document2 = null;
                    }
                    catch (Exception)
                    {
                        document2 = null;
                    }
                }
            }
            return document2;
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, Size newSize, AnchorEdge anchor, ColorBgra background)
        {
            BitmapLayer layer2 = new BitmapLayer(newSize.Width, newSize.Height);
            new UnaryPixelOps.Constant(background).Apply(layer2.Surface, layer2.Surface.Bounds);
            if (!layer.IsBackground)
            {
                new UnaryPixelOps.SetAlphaChannel(0).Apply(layer2.Surface, layer2.Surface.Bounds);
            }
            int num = 0;
            int num2 = 0;
            int num3 = newSize.Width - layer.Width;
            int num4 = newSize.Height - layer.Height;
            int num5 = (newSize.Width - layer.Width) / 2;
            int num6 = (newSize.Height - layer.Height) / 2;
            int x = 0;
            int y = 0;
            switch (anchor)
            {
                case AnchorEdge.TopLeft:
                    x = num2;
                    y = num;
                    break;

                case AnchorEdge.Top:
                    x = num5;
                    y = num;
                    break;

                case AnchorEdge.TopRight:
                    x = num3;
                    y = num;
                    break;

                case AnchorEdge.Left:
                    x = num2;
                    y = num6;
                    break;

                case AnchorEdge.Middle:
                    x = num5;
                    y = num6;
                    break;

                case AnchorEdge.Right:
                    x = num3;
                    y = num6;
                    break;

                case AnchorEdge.BottomLeft:
                    x = num2;
                    y = num4;
                    break;

                case AnchorEdge.Bottom:
                    x = num5;
                    y = num4;
                    break;

                case AnchorEdge.BottomRight:
                    x = num3;
                    y = num4;
                    break;
            }
            layer2.Surface.CopySurface(layer.Surface, new Point(x, y));
            layer2.LoadProperties(layer.SaveProperties());
            return layer2;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuImageCanvasSizeIcon.png");

        public static string StaticName =>
            PdnResources.GetString("CanvasSizeAction.Name");
    }
}

