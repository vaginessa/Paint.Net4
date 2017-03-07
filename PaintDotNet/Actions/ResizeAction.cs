namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal sealed class ResizeAction : DocumentWorkspaceAction
    {
        public ResizeAction() : base(ActionFlags.KeepToolActive)
        {
        }

        private static IRenderer<ColorBgra> CreatePdnResampler(IRenderer<ColorBgra> source, int newWidth, int newHeight, ResamplingAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case ResamplingAlgorithm.NearestNeighbor:
                    return new ResizeNearestNeighborRendererBgra(source, newWidth, newHeight);

                case ResamplingAlgorithm.Bilinear:
                    return new ResizeBilinearRendererBgra(source, newWidth, newHeight);

                case ResamplingAlgorithm.Bicubic:
                    return new ResizeBicubicRendererBgra(source, newWidth, newHeight);

                case ResamplingAlgorithm.SuperSampling:
                case ResamplingAlgorithm.Fant:
                    if ((newWidth >= source.Width) || (newHeight >= source.Height))
                    {
                        return new ResizeBicubicRendererBgra(source, newWidth, newHeight);
                    }
                    return new ResizeSuperSamplingRendererBgra(source, newWidth, newHeight);
            }
            throw ExceptionUtil.InvalidEnumArgumentException<ResamplingAlgorithm>(algorithm, "algorithm");
        }

        private static IRenderer<ColorBgra> CreateResampler(IRenderer<ColorBgra> source, int newWidth, int newHeight, ResamplingAlgorithm algorithm)
        {
            IRenderer<ColorBgra> renderer2;
            IRenderer<ColorBgra> renderer = CreateWicResampler(source, newWidth, newHeight, algorithm);
            ISurface<ColorBgra> dst = new Surface(1, 1);
            try
            {
                renderer.Render(dst, PointInt32.Zero);
                renderer2 = renderer;
            }
            catch (OverflowException)
            {
                renderer2 = CreatePdnResampler(source, newWidth, newHeight, algorithm);
            }
            finally
            {
                if (dst != null)
                {
                    dst.Dispose();
                }
            }
            return renderer2;
        }

        private static IRenderer<ColorBgra> CreateWicResampler(IRenderer<ColorBgra> source, int newWidth, int newHeight, ResamplingAlgorithm algorithm)
        {
            BitmapInterpolationMode nearestNeighbor;
            switch (algorithm)
            {
                case ResamplingAlgorithm.NearestNeighbor:
                    nearestNeighbor = BitmapInterpolationMode.NearestNeighbor;
                    break;

                case ResamplingAlgorithm.Bilinear:
                    nearestNeighbor = BitmapInterpolationMode.Linear;
                    break;

                case ResamplingAlgorithm.Bicubic:
                    nearestNeighbor = BitmapInterpolationMode.Cubic;
                    break;

                case ResamplingAlgorithm.SuperSampling:
                case ResamplingAlgorithm.Fant:
                    if ((newWidth >= source.Width) || (newHeight >= source.Height))
                    {
                        nearestNeighbor = BitmapInterpolationMode.Cubic;
                        break;
                    }
                    nearestNeighbor = BitmapInterpolationMode.Fant;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<ResamplingAlgorithm>(algorithm, "algorithm");
            }
            return new ResizeWicRendererBgra(source, newWidth, newHeight, nearestNeighbor);
        }

        public override HistoryMemento PerformAction(DocumentWorkspace docWorkspace)
        {
            int newWidth;
            int newHeight;
            MeasurementUnit newDpuUnit;
            double newDpu;
            ResamplingAlgorithm algorithm = AppSettings.Instance.Workspace.LastResamplingMethod.Value;
            if (algorithm == ResamplingAlgorithm.SuperSampling)
            {
                algorithm = ResamplingAlgorithm.Fant;
            }
            bool flag = AppSettings.Instance.Workspace.LastMaintainAspectRatio.Value;
            using (ResizeDialog dialog = new ResizeDialog())
            {
                dialog.OriginalSize = docWorkspace.Document.Size;
                dialog.OriginalDpuUnit = docWorkspace.Document.DpuUnit;
                dialog.OriginalDpu = docWorkspace.Document.DpuX;
                dialog.ImageHeight = docWorkspace.Document.Height;
                dialog.ImageWidth = docWorkspace.Document.Width;
                dialog.ResamplingAlgorithm = algorithm;
                dialog.LayerCount = docWorkspace.Document.Layers.Count;
                dialog.Units = dialog.OriginalDpuUnit;
                dialog.Resolution = docWorkspace.Document.DpuX;
                dialog.Units = AppSettings.Instance.Workspace.LastNonPixelUnits.Value;
                dialog.ConstrainToAspect = flag;
                if (dialog.ShowDialog(docWorkspace) == DialogResult.Cancel)
                {
                    return null;
                }
                AppSettings.Instance.Workspace.LastResamplingMethod.Value = dialog.ResamplingAlgorithm;
                AppSettings.Instance.Workspace.LastMaintainAspectRatio.Value = dialog.ConstrainToAspect;
                newDpuUnit = dialog.Units;
                newWidth = dialog.ImageWidth;
                newHeight = dialog.ImageHeight;
                newDpu = dialog.Resolution;
                algorithm = dialog.ResamplingAlgorithm;
                if (newDpuUnit != MeasurementUnit.Pixel)
                {
                    AppSettings.Instance.Workspace.LastNonPixelUnits.Value = newDpuUnit;
                    if (docWorkspace.AppWorkspace.Units != MeasurementUnit.Pixel)
                    {
                        docWorkspace.AppWorkspace.Units = newDpuUnit;
                    }
                }
                if (((docWorkspace.Document.Size == new Size(dialog.ImageWidth, dialog.ImageHeight)) && (docWorkspace.Document.DpuX == newDpu)) && (docWorkspace.Document.DpuUnit == newDpuUnit))
                {
                    return null;
                }
            }
            if ((newWidth == docWorkspace.Document.Width) && (newHeight == docWorkspace.Document.Height))
            {
                MetadataHistoryMemento memento2 = new MetadataHistoryMemento(StaticName, PdnResources.GetImageResource(StaticImageName), docWorkspace);
                docWorkspace.Document.DpuUnit = newDpuUnit;
                docWorkspace.Document.DpuX = newDpu;
                docWorkspace.Document.DpuY = newDpu;
                return memento2;
            }
            docWorkspace.FlushTool();
            using (TaskProgressDialog progressDialog = new TaskProgressDialog())
            {
                <>c__DisplayClass7_2 class_;
                string str = PdnResources.GetString("TaskProgressDialog.Initializing.Text");
                string renderingText = PdnResources.GetString("ResizeProgressDialog.Resizing.Text");
                string renderingWithPercentTextFormat = PdnResources.GetString("ResizeProgressDialog.ResizingWithPercent.Text.Format");
                string cancelingText = PdnResources.GetString("TaskProgressDialog.Canceling.Text");
                string committingText = PdnResources.GetString("ApplyRendererToBitmapLayerHistoryFunction.ProgressDialog.HeaderText");
                progressDialog.CloseOnFinished = true;
                progressDialog.HeaderText = str;
                progressDialog.Text = StaticName;
                progressDialog.Icon = PdnResources.GetImageResource(StaticImageName).Reference.ToIcon();
                List<HistoryMemento> mementos = new List<HistoryMemento>(2);
                SelectionHistoryMemento item = new SelectionHistoryMemento(null, null, docWorkspace);
                mementos.Add(item);
                TileMathHelper tileMath = new TileMathHelper(newWidth, newHeight, 7);
                Document oldDocument = docWorkspace.Document;
                object progressSync = new object();
                long pixelsSoFar = 0L;
                long totalPixels = (newWidth * newHeight) * oldDocument.Layers.Count;
                Exception renderEx = null;
                Document newDocument = null;
                ThreadTask<Unit> resizeTask = null;
                Action updateStatusFn = delegate {
                    if (resizeTask != null)
                    {
                        long num1;
                        double? nullable2;
                        string text1;
                        double? progress = resizeTask.Progress;
                        object obj1 = progressSync;
                        lock (obj1)
                        {
                            num1 = pixelsSoFar;
                        }
                        if (resizeTask.IsCancelRequested)
                        {
                            nullable2 = null;
                            text1 = cancelingText;
                        }
                        else if (num1 == 0)
                        {
                            nullable2 = null;
                            text1 = renderingText;
                        }
                        else if (num1 == totalPixels)
                        {
                            nullable2 = null;
                            text1 = committingText;
                        }
                        else
                        {
                            nullable2 = new double?(DoubleUtil.Clamp(((double) num1) / ((double) totalPixels), 0.0, 1.0));
                            text1 = string.Format(renderingWithPercentTextFormat, (nullable2.Value * 100.0).ToString("N0"));
                        }
                        double? nullable3 = nullable2;
                        double? nullable4 = progress;
                        if (((nullable3.GetValueOrDefault() == nullable4.GetValueOrDefault()) ? (nullable3.HasValue != nullable4.HasValue) : true) && ((progress.HasValue != nullable2.HasValue) || ((progress.HasValue && nullable2.HasValue) && ((nullable2.Value - progress.Value) > 0.003))))
                        {
                            resizeTask.Progress = nullable2;
                        }
                        progressDialog.HeaderText = text1;
                    }
                };
                ManualResetEvent resizeTaskEvent = new ManualResetEvent(false);
                resizeTask = docWorkspace.TaskManager.CreateThreadTask(delegate (PaintDotNet.Threading.Tasks.Task task) {
                    try
                    {
                        PdnSynchronizationContext.Instance.EnsurePosted(updateStatusFn);
                        System.Threading.Tasks.Task<ReplaceDocumentHistoryMemento> task2 = System.Threading.Tasks.Task.Factory.StartNew<ReplaceDocumentHistoryMemento>(new Func<ReplaceDocumentHistoryMemento>(class_.<PerformAction>b__2));
                        try
                        {
                            newDocument = new Document(newWidth, newHeight);
                            newDocument.ReplaceMetadataFrom(oldDocument);
                            newDocument.DpuUnit = newDpuUnit;
                            newDocument.DpuX = newDpu;
                            newDocument.DpuY = newDpu;
                            for (int j = 0; j < oldDocument.Layers.Count; j++)
                            {
                                BitmapLayer layer = (BitmapLayer) oldDocument.Layers[j];
                                Surface source = layer.Surface;
                                IRenderer<ColorBgra> render1 = CreateResampler(source, newWidth, newHeight, algorithm);
                                Surface newSurface = null;
                                try
                                {
                                    if (task.IsCancelRequested)
                                    {
                                        throw new OperationCanceledException();
                                    }
                                    newSurface = new Surface(newWidth, newHeight);
                                    Work.ParallelForEach<RectInt32>(WaitType.Blocking, tileMath.EnumerateTileRows(), delegate (RectInt32 renderRect) {
                                        if (task.IsCancelRequested)
                                        {
                                            throw new OperationCanceledException();
                                        }
                                        using (ISurface<ColorBgra> surface = newSurface.CreateWindow(renderRect))
                                        {
                                            render1.Render(surface, renderRect.Location);
                                            object obj1 = progressSync;
                                            lock (obj1)
                                            {
                                                pixelsSoFar += renderRect.Width * renderRect.Height;
                                            }
                                            PdnSynchronizationContext.Instance.EnsurePosted(updateStatusFn);
                                        }
                                    }, WorkItemQueuePriority.Normal, null);
                                }
                                catch (Exception exception)
                                {
                                    renderEx = exception;
                                }
                                if (renderEx != null)
                                {
                                    DisposableUtil.Free<Surface>(ref newSurface);
                                    goto Label_022D;
                                }
                                BitmapLayer layer2 = new BitmapLayer(newSurface, true);
                                layer2.LoadProperties(layer.SaveProperties());
                                newDocument.Layers.Add(layer2);
                            }
                        }
                        finally
                        {
                            mementos.Add(task2.Result);
                        }
                    Label_022D:
                        if (renderEx != null)
                        {
                            DisposableUtil.Free<Document>(ref newDocument);
                        }
                    }
                    finally
                    {
                        resizeTaskEvent.Set();
                    }
                }, ApartmentState.MTA);
                resizeTask.CancelRequested += delegate (object sender, EventArgs e) {
                    PdnSynchronizationContext.Instance.EnsurePosted(updateStatusFn);
                };
                resizeTask.Start();
                if (!resizeTaskEvent.WaitOne(0x3e8))
                {
                    progressDialog.Task = resizeTask;
                    progressDialog.ShowDialog(docWorkspace);
                }
                if (!resizeTask.IsCancelRequested && !(renderEx is OperationCanceledException))
                {
                    if ((renderEx == null) && !resizeTask.IsCancelRequested)
                    {
                        docWorkspace.Document = newDocument;
                        return new CompoundHistoryMemento(StaticName, PdnResources.GetImageResource(StaticImageName), mementos);
                    }
                    ExceptionDialog.ShowErrorDialog(docWorkspace, renderEx ?? new Exception());
                }
                return null;
            }
        }

        public static string StaticImageName =>
            "Icons.MenuImageResizeIcon.png";

        public static string StaticName =>
            PdnResources.GetString("ResizeAction.Name");
    }
}

