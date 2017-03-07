namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools.Move;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal sealed class PasteAction
    {
        private IPdnDataObject clipData;
        private DocumentWorkspace documentWorkspace;
        private MaskedSurface maskedSurface;

        public PasteAction(DocumentWorkspace documentWorkspace) : this(documentWorkspace, null, null)
        {
        }

        public PasteAction(DocumentWorkspace documentWorkspace, IPdnDataObject clipData, MaskedSurface maskedSurface)
        {
            this.documentWorkspace = documentWorkspace;
            this.clipData = clipData;
            this.maskedSurface = maskedSurface;
        }

        private static Surface CreateThumbnail(MaskedSurface maskedSurface)
        {
            int thumbSideLength = UIUtil.ScaleWidth(120);
            Surface surfaceReadOnly = maskedSurface.SurfaceReadOnly;
            GeometryList geometryMaskCopy = maskedSurface.GetGeometryMaskCopy();
            RectInt32 maskBounds = maskedSurface.GeometryMaskBounds.Int32Bound;
            return CreateThumbnail(surfaceReadOnly, geometryMaskCopy, maskBounds, thumbSideLength);
        }

        public static Surface CreateThumbnail(Surface sourceSurface, GeometryList maskGeometry, RectInt32 maskBounds, int thumbSideLength)
        {
            Surface dst = new Surface(ThumbnailHelpers.ComputeThumbnailSize(sourceSurface.Size<ColorBgra>(), thumbSideLength));
            dst.Clear(ColorBgra.Transparent);
            sourceSurface.ResizeFant(dst.Size<ColorBgra>()).Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 5, WorkItemQueuePriority.Normal).Render<ColorBgra>(dst);
            Surface surface = new Surface(dst.Size<ColorBgra>());
            surface.Clear(ColorBgra.Black);
            using (PdnGraphicsPath path = new PdnGraphicsPath())
            {
                path.AddGeometryList(maskGeometry);
                double scaleX = (maskBounds.Width == 0) ? 0.0 : (((double) dst.Width) / ((double) maskBounds.Width));
                double scaleY = (maskBounds.Height == 0) ? 0.0 : (((double) dst.Height) / ((double) maskBounds.Height));
                Matrix3x2Double m = Matrix3x2Double.Translation((double) -maskBounds.X, (double) -maskBounds.Y) * Matrix3x2Double.Scaling(scaleX, scaleY);
                using (RenderArgs args = new RenderArgs(surface))
                {
                    args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Matrix matrix = m.ToGdipMatrix())
                    {
                        args.Graphics.Transform = matrix;
                    }
                    args.Graphics.FillPath(Brushes.White, (GraphicsPath) path);
                    args.Graphics.DrawPath(Pens.White, (GraphicsPath) path);
                }
            }
            new IntensityMaskOp().Apply(surface, dst, surface);
            RendererBgra.Checkers(dst.Size<ColorBgra>()).Render<ColorBgra>(dst);
            CompositionOps.Normal.Static.Apply(dst, dst, surface);
            surface.Dispose();
            surface = null;
            int recommendedExtent = DropShadow.GetRecommendedExtent(dst.Size<ColorBgra>());
            ShadowDecorationRenderer renderer = new ShadowDecorationRenderer(dst, recommendedExtent);
            Surface surface3 = new Surface(renderer.Size<ColorBgra>());
            renderer.Render<ColorBgra>(surface3);
            return surface3;
        }

        public bool PerformAction()
        {
            bool flag;
            try
            {
                flag = this.PerformActionImpl();
            }
            finally
            {
                this.clipData = null;
                this.maskedSurface = null;
            }
            return flag;
        }

        private bool PerformActionImpl()
        {
            PointInt32 num2;
            RectInt32 num3;
            if (this.clipData == null)
            {
                try
                {
                    using (new WaitCursorChanger(this.documentWorkspace))
                    {
                        CleanupManager.RequestCleanup();
                        this.clipData = PdnClipboard.GetDataObject();
                    }
                }
                catch (OutOfMemoryException exception)
                {
                    ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("PasteAction.Error.OutOfMemory"), exception);
                    return false;
                }
                catch (Exception exception2)
                {
                    ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("PasteAction.Error.TransferFromClipboard"), exception2);
                    return false;
                }
            }
            bool handled = false;
            if (this.documentWorkspace.Tool != null)
            {
                this.documentWorkspace.Tool.PerformPaste(this.clipData, out handled);
            }
            if (handled)
            {
                return true;
            }
            if (this.maskedSurface == null)
            {
                try
                {
                    using (new WaitCursorChanger(this.documentWorkspace))
                    {
                        this.maskedSurface = ClipboardUtil.GetClipboardImage(this.documentWorkspace, this.clipData);
                    }
                }
                catch (OutOfMemoryException exception3)
                {
                    ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("PasteAction.Error.OutOfMemory"), exception3);
                    return false;
                }
                catch (Exception exception4)
                {
                    ExceptionDialog.ShowErrorDialog(this.documentWorkspace, PdnResources.GetString("PasteAction.Error.TransferFromClipboard"), exception4);
                    return false;
                }
            }
            if (this.maskedSurface == null)
            {
                MessageBoxUtil.ErrorBox(this.documentWorkspace, PdnResources.GetString("PasteAction.Error.NoImage"));
                return false;
            }
            RectInt32 cachedGeometryMaskScansBounds = this.maskedSurface.GetCachedGeometryMaskScansBounds();
            if ((cachedGeometryMaskScansBounds.Width > this.documentWorkspace.Document.Width) || (cachedGeometryMaskScansBounds.Height > this.documentWorkspace.Document.Height))
            {
                Surface surface;
                try
                {
                    using (new WaitCursorChanger(this.documentWorkspace))
                    {
                        surface = CreateThumbnail(this.maskedSurface);
                    }
                }
                catch (OutOfMemoryException)
                {
                    surface = null;
                }
                DialogResult result = ShowExpandCanvasTaskDialog(this.documentWorkspace, surface);
                int activeLayerIndex = this.documentWorkspace.ActiveLayerIndex;
                ColorBgra background = this.documentWorkspace.ToolSettings.SecondaryColor.Value;
                if (result != DialogResult.Cancel)
                {
                    if (result != DialogResult.Yes)
                    {
                        if (result != DialogResult.No)
                        {
                            throw ExceptionUtil.InvalidEnumArgumentException<DialogResult>(result, "dr");
                        }
                        goto Label_031D;
                    }
                    using (new PushNullToolMode(this.documentWorkspace))
                    {
                        int width = Math.Max(cachedGeometryMaskScansBounds.Width, this.documentWorkspace.Document.Width);
                        Size newSize = new Size(width, Math.Max(cachedGeometryMaskScansBounds.Height, this.documentWorkspace.Document.Height));
                        Document document = CanvasSizeAction.ResizeDocument(this.documentWorkspace.Document, newSize, AnchorEdge.TopLeft, background);
                        if (document == null)
                        {
                            return false;
                        }
                        SelectionHistoryMemento memento = new SelectionHistoryMemento(null, null, this.documentWorkspace);
                        ReplaceDocumentHistoryMemento memento2 = new ReplaceDocumentHistoryMemento(CanvasSizeAction.StaticName, CanvasSizeAction.StaticImage, this.documentWorkspace);
                        this.documentWorkspace.Document = document;
                        HistoryMemento[] actions = new HistoryMemento[] { memento, memento2 };
                        CompoundHistoryMemento memento3 = new CompoundHistoryMemento(CanvasSizeAction.StaticName, CanvasSizeAction.StaticImage, actions);
                        this.documentWorkspace.History.PushNewMemento(memento3);
                        this.documentWorkspace.ActiveLayer = (Layer) this.documentWorkspace.Document.Layers[activeLayerIndex];
                        goto Label_031D;
                    }
                }
                return false;
            }
        Label_031D:
            num3 = this.documentWorkspace.Document.Bounds();
            RectDouble visibleDocumentRect = this.documentWorkspace.VisibleDocumentRect;
            RectInt32? nullable = visibleDocumentRect.Int32Inset();
            RectDouble num5 = nullable.HasValue ? ((RectDouble) nullable.Value) : visibleDocumentRect;
            RectInt32 num6 = num5.Int32Bound;
            if (num5.Contains(cachedGeometryMaskScansBounds))
            {
                num2 = new PointInt32(0, 0);
            }
            else
            {
                int num12;
                int num13;
                int num16;
                int num17;
                if (cachedGeometryMaskScansBounds.X < num5.Left)
                {
                    num12 = -cachedGeometryMaskScansBounds.X + num6.X;
                }
                else if (cachedGeometryMaskScansBounds.Right > num6.Right)
                {
                    num12 = (-cachedGeometryMaskScansBounds.X + num6.Right) - cachedGeometryMaskScansBounds.Width;
                }
                else
                {
                    num12 = 0;
                }
                if (cachedGeometryMaskScansBounds.Y < num5.Top)
                {
                    num13 = -cachedGeometryMaskScansBounds.Y + num6.Y;
                }
                else if (cachedGeometryMaskScansBounds.Bottom > num6.Bottom)
                {
                    num13 = (-cachedGeometryMaskScansBounds.Y + num6.Bottom) - cachedGeometryMaskScansBounds.Height;
                }
                else
                {
                    num13 = 0;
                }
                PointInt32 num14 = new PointInt32(num12, num13);
                RectInt32 num15 = new RectInt32(cachedGeometryMaskScansBounds.X + num14.X, cachedGeometryMaskScansBounds.Y + num14.Y, cachedGeometryMaskScansBounds.Width, cachedGeometryMaskScansBounds.Height);
                if (num15.X < 0)
                {
                    num16 = num12 - num15.X;
                }
                else
                {
                    num16 = num12;
                }
                if (num15.Y < 0)
                {
                    num17 = num13 - num15.Y;
                }
                else
                {
                    num17 = num13;
                }
                PointInt32 num18 = new PointInt32(num16, num17);
                RectInt32 rect = new RectInt32(cachedGeometryMaskScansBounds.X + num18.X, cachedGeometryMaskScansBounds.Y + num18.Y, cachedGeometryMaskScansBounds.Width, cachedGeometryMaskScansBounds.Height);
                if (num3.Contains(rect))
                {
                    num2 = num18;
                }
                else
                {
                    PointInt32 num20 = num18;
                    if (rect.Right > num3.Right)
                    {
                        int num21 = rect.Right - num3.Right;
                        int num22 = Math.Min(num21, rect.Left);
                        num20.X -= num22;
                    }
                    if (rect.Bottom > num3.Bottom)
                    {
                        int num23 = rect.Bottom - num3.Bottom;
                        int num24 = Math.Min(num23, rect.Top);
                        num20.Y -= num24;
                    }
                    num2 = num20;
                }
            }
            RectInt32 b = this.documentWorkspace.VisibleDocumentRect.Int32Bound;
            RectInt32 a = new RectInt32(cachedGeometryMaskScansBounds.X + num2.X, cachedGeometryMaskScansBounds.Y + num2.Y, cachedGeometryMaskScansBounds.Width, cachedGeometryMaskScansBounds.Height);
            bool hasZeroArea = RectInt32.Intersect(a, b).HasZeroArea;
            MoveTool.BeginPaste(this.documentWorkspace, PdnResources.GetString("CommonAction.Paste"), PdnResources.GetImageResource("Icons.MenuEditPasteIcon.png"), this.maskedSurface.SurfaceReadOnly, this.maskedSurface.GeometryMask, num2);
            if (hasZeroArea)
            {
                PointInt32 num25 = new PointInt32(b.Left + (b.Width / 2), b.Top + (b.Height / 2));
                PointInt32 num26 = new PointInt32(a.Left + (a.Width / 2), a.Top + (a.Height / 2));
                SizeInt32 num27 = new SizeInt32(num26.X - num25.X, num26.Y - num25.Y);
                PointDouble documentScrollPosition = this.documentWorkspace.DocumentScrollPosition;
                PointDouble num29 = new PointDouble(documentScrollPosition.X + num27.Width, documentScrollPosition.Y + num27.Height);
                this.documentWorkspace.DocumentScrollPosition = num29;
            }
            return true;
        }

        private static DialogResult ShowExpandCanvasTaskDialog(IWin32Window owner, Surface thumbnail)
        {
            DialogResult yes;
            Icon icon = PdnResources.GetImageResource("Icons.MenuEditPasteIcon.png").Reference.ToIcon();
            string str = PdnResources.GetString("ExpandCanvasQuestion.Title");
            RenderArgs args = new RenderArgs(thumbnail);
            Image bitmap = args.Bitmap;
            string str2 = PdnResources.GetString("ExpandCanvasQuestion.IntroText");
            TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.ExpandCanvasQuestion.YesTB.Image.png").Reference, PdnResources.GetString("ExpandCanvasQuestion.YesTB.ActionText"), PdnResources.GetString("ExpandCanvasQuestion.YesTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.ExpandCanvasQuestion.NoTB.Image.png").Reference, PdnResources.GetString("ExpandCanvasQuestion.NoTB.ActionText"), PdnResources.GetString("ExpandCanvasQuestion.NoTB.ExplanationText"));
            TaskButton button3 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("ExpandCanvasQuestion.CancelTB.ActionText"), PdnResources.GetString("ExpandCanvasQuestion.CancelTB.ExplanationText"));
            int num = (TaskDialog.DefaultPixelWidth96Dpi * 3) / 2;
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = bitmap,
                ScaleTaskImageWithDpi = true,
                IntroText = str2
            };
            dialog2.TaskButtons = new TaskButton[] { button, button2, button3 };
            dialog2.AcceptButton = button;
            dialog2.CancelButton = button3;
            dialog2.PixelWidth96Dpi = num;
            TaskButton button4 = dialog2.Show(owner);
            if (button4 == button)
            {
                yes = DialogResult.Yes;
            }
            else if (button4 == button2)
            {
                yes = DialogResult.No;
            }
            else
            {
                yes = DialogResult.Cancel;
            }
            args.Dispose();
            args = null;
            return yes;
        }

        private sealed class IntensityMaskOp : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                byte intensityByte = rhs.GetIntensityByte();
                return ColorBgra.FromBgra(lhs.B, lhs.G, lhs.R, ByteUtil.FastScale(intensityByte, lhs.A));
            }
        }
    }
}

