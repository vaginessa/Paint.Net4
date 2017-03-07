namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ApplyRendererToBitmapLayerHistoryFunction : HistoryFunction
    {
        public const int DefaultDelayUntilProgressDialogMs = 0x7d0;
        public const int DefaultRendererTileEdgeLog2 = 4;
        private int delayUntilProgressDialogMs;
        private ImageResource historyMementoImage;
        private string historyMementoName;
        private int layerIndex;
        private IRenderer<ColorBgra> renderer;
        private RectInt32 rendererClipRect;
        private int rendererTileEdgeLog2;
        public const int RenderTileGroupEdgeLog2 = 3;

        public ApplyRendererToBitmapLayerHistoryFunction(string historyMementoName, ImageResource historyMementoImage, int layerIndex, IRenderer<ColorBgra> renderer, RectInt32 rendererClipRect, int rendererTileEdgeLog2 = 4, int delayUntilProgressDialogMs = 0x7d0, ActionFlags actionFlags = 0) : base(actionFlags)
        {
            this.historyMementoName = historyMementoName;
            this.historyMementoImage = historyMementoImage;
            this.layerIndex = layerIndex;
            this.renderer = renderer;
            this.rendererClipRect = rendererClipRect;
            this.rendererTileEdgeLog2 = rendererTileEdgeLog2;
            this.delayUntilProgressDialogMs = delayUntilProgressDialogMs;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            historyWorkspace.VerifyAccess();
            BitmapLayer layer = (BitmapLayer) historyWorkspace.Document.Layers[this.layerIndex];
            Surface layerSurface = layer.Surface;
            TileMathHelper tileMathHelper = new TileMathHelper(layerSurface.Width, layerSurface.Height, this.rendererTileEdgeLog2);
            TileMathHelper tileGroupMathHelper = new TileMathHelper(layerSurface.Width, layerSurface.Height, this.rendererTileEdgeLog2 + 3);
            HistoryMemento actionHM = null;
            ManualResetEvent taskFinishedEvent = new ManualResetEvent(false);
            ThreadTask<Unit> commitTask = historyWorkspace.TaskManager.StartNewThreadTask(delegate (Task task) {
                SegmentedList<RectInt32> list = tileGroupMathHelper.EnumerateTilesClippedToSourceRect(this.rendererClipRect).ToSegmentedList<RectInt32>();
                ListUtil.FisherYatesShuffle<RectInt32>(list);
                ConcurrentQueue<ISurface<ColorBgra>> queue = new ConcurrentQueue<ISurface<ColorBgra>>();
                ConcurrentQueue<RectInt32> changedTiles = new ConcurrentQueue<RectInt32>();
                ConcurrentQueue<TupleStruct<RectInt32, ISurface<ColorBgra>>> changedTileGroups = new ConcurrentQueue<TupleStruct<RectInt32, ISurface<ColorBgra>>>();
                double progressIncrement = 1.0 / ((double) Math.Max(1, list.Count));
                task.Progress = 0.0;
                Work.ParallelForEach<RectInt32>(WaitType.Blocking, list, delegate (RectInt32 tileGroupRect) {
                    ISurface<ColorBgra> dst = RetryManager.RunMemorySensitiveOperation<ISurfaceRef<ColorBgra>>(() => SurfaceAllocator.Bgra.Allocate<ColorBgra>(tileGroupRect.Size, AllocationOptions.ZeroFillNotRequired));
                    this.renderer.Render(dst, tileGroupRect.Location);
                    bool flag = false;
                    foreach (RectInt32 num in tileMathHelper.EnumerateTilesClippedToSourceRect(tileGroupRect))
                    {
                        RectInt32 bounds = RectInt32.Offset(num, -tileGroupRect.Location);
                        using (ISurface<ColorBgra> surface2 = layerSurface.CreateWindow(num))
                        {
                            using (ISurface<ColorBgra> surface3 = dst.CreateWindow<ColorBgra>(bounds))
                            {
                                if (!SurfaceBgraUtil.ArePixelsEqual(surface2, surface3))
                                {
                                    flag = true;
                                    changedTiles.Enqueue(num);
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        changedTileGroups.Enqueue(TupleStruct.Create<RectInt32, ISurface<ColorBgra>>(tileGroupRect, dst));
                    }
                    else
                    {
                        DisposableUtil.Free<ISurface<ColorBgra>>(ref dst);
                    }
                    task.IncrementProgressBy(progressIncrement);
                }, WorkItemQueuePriority.Normal, null);
                task.Progress = null;
                if (changedTiles.Count == 0)
                {
                    actionHM = null;
                }
                else
                {
                    SegmentedList<RectInt32> scans = new SegmentedList<RectInt32>(changedTiles.Count, 7);
                    scans.AddRange(changedTiles);
                    ScansHelpers.SortScansByTopLeft(scans);
                    ScansHelpers.ConsolidateSortedScansInPlace(scans);
                    actionHM = new BitmapHistoryMemento(this.historyMementoName, this.historyMementoImage, historyWorkspace, this.layerIndex, scans);
                    this.EnterCriticalRegion();
                    Work.ParallelForEach<TupleStruct<RectInt32, ISurface<ColorBgra>>>(WaitType.Blocking, changedTileGroups, delegate (TupleStruct<RectInt32, ISurface<ColorBgra>> tileInfo) {
                        using (ISurface<ColorBgra> surface = layerSurface.CreateWindow(tileInfo.Item1))
                        {
                            tileInfo.Item2.Render(surface, PointInt32.Zero);
                        }
                        tileInfo.Item2.Dispose();
                    }, WorkItemQueuePriority.Normal, null);
                    foreach (RectInt32 num in scans)
                    {
                        layer.Invalidate(num);
                    }
                }
            }, ApartmentState.MTA);
            commitTask.ResultAsync<Unit>().Receive(delegate (Result<Unit> r) {
                taskFinishedEvent.Set();
            }).Observe();
            if (!taskFinishedEvent.WaitOne(this.delayUntilProgressDialogMs))
            {
                string headerText = PdnResources.GetString("ApplyRendererToBitmapLayerHistoryFunction.ProgressDialog.HeaderText");
                string headerTextFormat = PdnResources.GetString("ApplyRendererToBitmapLayerHistoryFunction.ProgressDialog.HeaderText.Format");
                using (TaskProgressDialog progressDialog = new TaskProgressDialog())
                {
                    Action updateHeaderText = delegate {
                        progressDialog.VerifyAccess();
                        if (!progressDialog.IsDisposed)
                        {
                            string text1;
                            double? progress = commitTask.Progress;
                            if (!progress.HasValue)
                            {
                                text1 = headerText;
                            }
                            else
                            {
                                text1 = string.Format(headerTextFormat, (progress.Value * 100.0).ToString("N0"));
                            }
                            progressDialog.HeaderText = text1;
                        }
                    };
                    progressDialog.Text = this.historyMementoName;
                    progressDialog.Icon = this.historyMementoImage.Reference.ToIcon();
                    progressDialog.CloseOnFinished = true;
                    progressDialog.ShowCancelButton = false;
                    progressDialog.Task = commitTask;
                    updateHeaderText();
                    commitTask.ProgressChanged += delegate (object s, ValueEventArgs<double?> e) {
                        PdnSynchronizationContext.Instance.EnsurePosted(updateHeaderText);
                    };
                    progressDialog.ShowDialog(historyWorkspace.Window);
                }
            }
            if (!commitTask.TaskResult.IsError)
            {
                return actionHM;
            }
            if (commitTask.TaskResult.Error is OutOfMemoryException)
            {
                throw new OutOfMemoryException(null, commitTask.TaskResult.Error);
            }
            throw new AggregateException(null, commitTask.TaskResult.Error);
        }
    }
}

