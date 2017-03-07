namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class DocumentCanvasTileCache : ThreadAffinitizedObjectBase, IDisposable, IIsDisposed
    {
        private readonly ProtectedRegion cancelAllRenderingRegion = new ProtectedRegion("CancelAllRendering", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private readonly ProtectedRegion cancelTileRenderingRegion = new ProtectedRegion("CancelTileRendering", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private readonly ProtectedRegion invalidateRegion = new ProtectedRegion("Invalidate", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private DequeSet<PointInt32> invalidTileOffsets;
        private int isActiveCount;
        private bool isHighQuality;
        private int isProcessTileRenderedQueueQueued;
        private bool isProcessTileRenderQueueQueued;
        private int mipLevel;
        private DocumentCanvasLayer owner;
        private SendOrPostCallback processTileRenderedQueueCallback;
        private readonly ProtectedRegion processTileRenderedQueueRegion = new ProtectedRegion("ProcessTileRenderedQueue", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private SendOrPostCallback processTileRenderQueueCallback;
        private readonly ProtectedRegion processTileRenderQueueRegion = new ProtectedRegion("ProcessTileRenderQueue", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private readonly ProtectedRegion queueInvalidTilesForRenderingRegion = new ProtectedRegion("QueueInvalidTilesForRendering", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private readonly ProtectedRegion queueProcessTileRenderQueueRegion = new ProtectedRegion("QueueProcessTileRenderQueue", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private IBitmapSource<ColorBgra32> source;
        private RectInt32 sourceBounds;
        private SizeInt32 sourceSize;
        private SynchronizationContext syncContext;
        private IBitmap<ColorPbgra32>[][] tileBuffers;
        private bool[][] tileIsValid;
        private PaintDotNet.Rendering.TileMathHelper tileMathHelper;
        private ConcurrentDequeDictionary<PointInt32, RenderedTileInfo> tilesRenderedQueue;
        private ConcurrentSet<PointInt32> tilesRenderingCancelledSet;
        private HashSet<PointInt32> tilesRenderingSet;
        private DequeSet<PointInt32> tilesRenderQueue;
        private WorkItemDispatcher workItemDispatcher;
        private EditableDataWorkItemQueue<PointInt32> workItemQueue;

        public DocumentCanvasTileCache(DocumentCanvasLayer owner, IBitmapSource<ColorBgra32> source, int sourceTileEdgeLog2, int mipLevel, bool highQuality)
        {
            Validate.Begin().IsNotNull<DocumentCanvasLayer>(owner, "owner").IsNotNull<IBitmapSource<ColorBgra32>>(source, "source").Check();
            if (sourceTileEdgeLog2 < 0)
            {
                ExceptionUtil.ThrowArgumentOutOfRangeException("sourceTileEdgeLog2");
            }
            if (mipLevel < 0)
            {
                ExceptionUtil.ThrowArgumentOutOfRangeException("mipLevel");
            }
            this.syncContext = SynchronizationContext.Current;
            this.owner = owner;
            this.source = source;
            this.sourceSize = this.source.Size;
            this.sourceBounds = this.source.Bounds();
            this.isHighQuality = highQuality;
            this.tileMathHelper = new PaintDotNet.Rendering.TileMathHelper(this.sourceSize.Width, this.sourceSize.Height, sourceTileEdgeLog2);
            this.mipLevel = mipLevel;
            this.tileIsValid = ArrayUtil.Create2D<bool>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
            this.invalidTileOffsets = new DequeSet<PointInt32>();
            this.tileBuffers = ArrayUtil.Create2D<IBitmap<ColorPbgra32>>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
            for (int i = 0; i < this.tileMathHelper.TileRows; i++)
            {
                for (int j = 0; j < this.tileMathHelper.TileColumns; j++)
                {
                    this.invalidTileOffsets.TryEnqueue(new PointInt32(j, i));
                }
            }
            this.tilesRenderQueue = new DequeSet<PointInt32>();
            this.tilesRenderingSet = new HashSet<PointInt32>();
            this.tilesRenderingCancelledSet = new ConcurrentSet<PointInt32>();
            this.tilesRenderedQueue = new ConcurrentDequeDictionary<PointInt32, RenderedTileInfo>();
            this.processTileRenderQueueCallback = new SendOrPostCallback(this.ProcessTileRenderQueueCallback);
            this.processTileRenderedQueueCallback = new SendOrPostCallback(this.ProcessTileRenderedQueueCallback);
            this.workItemDispatcher = WorkItemDispatcher.Default;
            this.workItemQueue = new EditableDataWorkItemQueue<PointInt32>(this.workItemDispatcher, new Action<PointInt32>(this.RenderTileWorkItem));
        }

        public void CancelAllRendering()
        {
            base.VerifyAccess();
            using (this.cancelAllRenderingRegion.UseEnterScope())
            {
                foreach (PointInt32 num in this.tileMathHelper.EnumerateTileOffsets(this.sourceBounds))
                {
                    this.CancelTileRendering(num);
                }
            }
        }

        private void CancelTileRendering(PointInt32 tileOffset)
        {
            using (this.cancelTileRenderingRegion.UseEnterScope())
            {
                if (this.tilesRenderingSet.Contains(tileOffset))
                {
                    this.tilesRenderingCancelledSet.Add(tileOffset);
                }
                this.tilesRenderQueue.Remove(tileOffset);
            }
        }

        private void CancelTilesRendering(IEnumerable<PointInt32> tileOffsets)
        {
            using (this.cancelTileRenderingRegion.UseEnterScope())
            {
                foreach (PointInt32 num in tileOffsets)
                {
                    if (this.tilesRenderingSet.Contains(num))
                    {
                        this.tilesRenderingCancelledSet.Add(num);
                    }
                    this.tilesRenderQueue.Remove(num);
                }
            }
        }

        private static IBitmapSource<ColorPbgra32> CreateBufferedTileScaler(IImagingFactory imagingFactory, IBitmapSource<ColorBgra32> source, int dstWidth, int dstHeight, BitmapInterpolationMode interpolationMode)
        {
            IBitmapSource<ColorPbgra32> source5;
            using (IBitmap<ColorBgra32> bitmap = BitmapAllocator.Bgra32.Allocate<ColorBgra32>(source.Size.Width, imagingFactory.GetBufferBitmapHeight(source, 7), AllocationOptions.Default))
            {
                using (IBitmapSource<ColorBgra32> source2 = imagingFactory.CreateBufferedBitmap<ColorBgra32>(source, bitmap, 7))
                {
                    using (IBitmapSource<ColorPbgra32> source3 = imagingFactory.CreateFormatConvertedBitmap<ColorPbgra32>(source2))
                    {
                        source5 = imagingFactory.CreateBitmapScaler<ColorPbgra32>(source3, dstWidth, dstHeight, interpolationMode);
                    }
                }
            }
            return source5;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<EditableDataWorkItemQueue<PointInt32>>(ref this.workItemQueue);
                Work.QueueDisposeStream((IEnumerable<IDisposable>) this.tileBuffers.SelectMany<IBitmap<ColorPbgra32>>());
                this.tileBuffers = null;
                this.source = null;
            }
        }

        private SizeInt32 GetTileBufferSize(PointInt32 tileOffset) => 
            this.GetTileBufferSize(tileOffset.X, tileOffset.Y);

        private SizeInt32 GetTileBufferSize(int tileColumn, int tileRow)
        {
            RectInt32 tileSourceRect = this.tileMathHelper.GetTileSourceRect(tileColumn, tileRow);
            int width = Int32Util.DivLog2RoundUp(tileSourceRect.Width, this.mipLevel);
            return new SizeInt32(width, Int32Util.DivLog2RoundUp(tileSourceRect.Height, this.mipLevel));
        }

        public void Invalidate(RectInt32 sourceRect)
        {
            base.VerifyAccess();
            using (this.invalidateRegion.UseEnterScope())
            {
                if (!this.sourceBounds.Contains(sourceRect))
                {
                    ExceptionUtil.ThrowArgumentOutOfRangeException();
                }
                IEnumerable<PointInt32> tileOffsets = this.tileMathHelper.EnumerateTileOffsets(sourceRect);
                this.CancelTilesRendering(tileOffsets);
                foreach (PointInt32 num in tileOffsets)
                {
                    this.tileIsValid[num.Y][num.X] = false;
                    this.invalidTileOffsets.TryEnqueue(num);
                }
                this.owner.NotifyTileCacheInvalidated(this, sourceRect);
            }
        }

        private void InvalidateCancelledTiles(IEnumerable<PointInt32> tileOffsets)
        {
            if (!this.IsDisposed)
            {
                base.VerifyAccess();
                foreach (PointInt32 num in tileOffsets)
                {
                    RectInt32 tileSourceRect = this.tileMathHelper.GetTileSourceRect(num);
                    this.Invalidate(tileSourceRect);
                }
            }
        }

        private bool IsTileRenderingCancelled(PointInt32 tileOffset) => 
            this.tilesRenderingCancelledSet.Contains(tileOffset);

        private bool IsTileRenderingCancelled(int tileRow, int tileColumn) => 
            this.IsTileRenderingCancelled(new PointInt32(tileRow, tileColumn));

        public bool IsTileValid(PointInt32 tileOffset) => 
            this.IsTileValid(tileOffset.X, tileOffset.Y);

        public bool IsTileValid(int column, int row)
        {
            base.VerifyAccess();
            this.tileMathHelper.VerifyTileOffset(column, row);
            return this.tileIsValid[row][column];
        }

        public void PopActive()
        {
            base.VerifyAccess();
            this.isActiveCount--;
            if (this.isActiveCount == 0)
            {
                for (int i = 0; i < this.tileMathHelper.TileRows; i++)
                {
                    for (int j = 0; j < this.tileMathHelper.TileColumns; j++)
                    {
                        PointInt32 tileOffset = new PointInt32(j, i);
                        this.CancelTileRendering(tileOffset);
                        this.invalidTileOffsets.TryEnqueue(tileOffset);
                    }
                }
                Work.QueueFreeStreamContents<IBitmap<ColorPbgra32>>(this.tileBuffers);
                ArrayUtil.Clear2D<bool>(this.tileIsValid);
                this.owner.NotifyTileCacheIsActiveChanged(this, false);
            }
        }

        public bool ProcessTileRenderedQueue()
        {
            base.VerifyAccess();
            using (this.processTileRenderedQueueRegion.UseEnterScope())
            {
                if (this.IsDisposed)
                {
                    bool flag = false;
                    do
                    {
                        KeyValuePair<PointInt32, RenderedTileInfo> pair;
                        while (this.tilesRenderedQueue.TryDequeue(out pair))
                        {
                            DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref pair.Value.Buffer);
                            flag = true;
                        }
                    }
                    while (Interlocked.Exchange(ref this.isProcessTileRenderedQueueQueued, 0) == 1);
                    return flag;
                }
                if (Interlocked.Exchange(ref this.isProcessTileRenderedQueueQueued, 0) == 0)
                {
                    return false;
                }
                List<PointInt32> collection = new List<PointInt32>();
                List<PointInt32> cancelledTileOffsets = null;
                bool flag3 = false;
                int count = this.tilesRenderedQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    KeyValuePair<PointInt32, RenderedTileInfo> pair2;
                    if (this.tilesRenderedQueue.TryDequeue(out pair2))
                    {
                        PointInt32 key = pair2.Key;
                        RenderedTileInfo info2 = pair2.Value;
                        IBitmap<ColorPbgra32> buffer = info2.Buffer;
                        if (this.tilesRenderingCancelledSet.Remove(key) || !info2.Completed)
                        {
                            DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref buffer);
                            if (this.tilesRenderQueue.Contains(key))
                            {
                                flag3 = true;
                            }
                            else if (info2.Error is OperationCanceledException)
                            {
                                if (cancelledTileOffsets == null)
                                {
                                    cancelledTileOffsets = new List<PointInt32>();
                                }
                                cancelledTileOffsets.Add(key);
                            }
                        }
                        else
                        {
                            if ((buffer.IsNullReference<IBitmap<ColorPbgra32>>() || buffer.IsDisposed) || !pair2.Value.Completed)
                            {
                                throw new PaintDotNet.InternalErrorException();
                            }
                            IBitmap<ColorPbgra32> disposeMe = this.tileBuffers[key.Y][key.X];
                            DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref disposeMe);
                            this.tileBuffers[key.Y][key.X] = buffer;
                            this.tileIsValid[key.Y][key.X] = true;
                            collection.Add(key);
                        }
                        if (!this.tilesRenderingSet.Remove(key))
                        {
                            throw new PaintDotNet.InternalErrorException();
                        }
                    }
                }
                if (collection.Any<PointInt32>())
                {
                    this.owner.NotifyTileCacheFinishedRendering<List<PointInt32>>(this, collection);
                }
                if (flag3)
                {
                    this.ProcessTileRenderQueue();
                }
                if (!this.tilesRenderingSet.Any<PointInt32>())
                {
                    this.owner.NotifyTileCacheIsIdle(this);
                }
                if (cancelledTileOffsets != null)
                {
                    this.syncContext.Post(delegate (object _) {
                        this.InvalidateCancelledTiles(cancelledTileOffsets);
                    });
                }
                return collection.Any<PointInt32>();
            }
        }

        private void ProcessTileRenderedQueueCallback(object ignored)
        {
            this.ProcessTileRenderedQueue();
        }

        public bool ProcessTileRenderQueue()
        {
            base.VerifyAccess();
            using (this.processTileRenderQueueRegion.UseEnterScope())
            {
                if (this.IsDisposed)
                {
                    this.tilesRenderQueue.Clear();
                    return false;
                }
                int count = this.tilesRenderQueue.Count;
                IList<PointInt32> workItemQueueAccumulator = null;
                for (int i = 0; i < count; i++)
                {
                    PointInt32 tileOffset = this.tilesRenderQueue.Dequeue();
                    if (this.IsTileRenderingCancelled(tileOffset))
                    {
                        if (!this.tilesRenderingSet.Contains(tileOffset))
                        {
                            ExceptionUtil.ThrowInternalErrorException();
                        }
                        this.tilesRenderQueue.TryEnqueue(tileOffset);
                    }
                    else
                    {
                        if (!this.tilesRenderingSet.Add(tileOffset))
                        {
                            ExceptionUtil.ThrowInternalErrorException();
                        }
                        if (this.tilesRenderingSet.Count == 1)
                        {
                            this.owner.NotifyTileCacheIsRendering(this);
                        }
                        if (workItemQueueAccumulator == null)
                        {
                            workItemQueueAccumulator = new List<PointInt32>();
                        }
                        workItemQueueAccumulator.Add(tileOffset);
                    }
                }
                if (workItemQueueAccumulator != null)
                {
                    PointInt32 sourcePt = PointDouble.Round(this.owner.MouseLocation, MidpointRounding.AwayFromZero);
                    PointInt32 comparand = this.tileMathHelper.ConvertSourcePointToTileOffset(sourcePt);
                    CompareTileOffsetsByDistance comparer = new CompareTileOffsetsByDistance(comparand);
                    this.workItemQueue.Edit(delegate {
                        this.workItemQueue.EnqueueRange<IList<PointInt32>>(workItemQueueAccumulator);
                        this.workItemQueue.Sort<CompareTileOffsetsByDistance>(comparer);
                    });
                    return true;
                }
                return false;
            }
        }

        private void ProcessTileRenderQueueCallback(object ignored)
        {
            if (this.isProcessTileRenderQueueQueued)
            {
                this.isProcessTileRenderQueueQueued = false;
                this.ProcessTileRenderQueue();
            }
        }

        public void PushActive()
        {
            base.VerifyAccess();
            this.isActiveCount++;
            if (this.isActiveCount == 1)
            {
                this.owner.NotifyTileCacheIsActiveChanged(this, true);
            }
        }

        public void QueueInvalidTilesForRendering(bool async = true)
        {
            this.QueueInvalidTilesForRendering(this.sourceBounds, async);
        }

        public void QueueInvalidTilesForRendering(RectInt32 sourceRect, bool async = true)
        {
            base.VerifyAccess();
            using (this.queueInvalidTilesForRenderingRegion.UseEnterScope())
            {
                PointInt32 num;
                bool flag = false;
                List<PointInt32> items = new List<PointInt32>();
                while (this.invalidTileOffsets.TryDequeue(out num))
                {
                    if (this.tileIsValid[num.Y][num.X])
                    {
                        ExceptionUtil.ThrowInternalErrorException();
                    }
                    if (!this.tilesRenderQueue.Contains(num) && (!this.tilesRenderingSet.Contains(num) || this.IsTileRenderingCancelled(num)))
                    {
                        if (!this.tileMathHelper.GetTileSourceRect(num).IntersectsWith(sourceRect))
                        {
                            items.Add(num);
                        }
                        else
                        {
                            this.tilesRenderQueue.TryEnqueue(num);
                            if (!this.IsTileRenderingCancelled(num))
                            {
                                flag = true;
                            }
                        }
                    }
                }
                this.invalidTileOffsets.EnqueueRange(items);
                if (flag)
                {
                    if (async)
                    {
                        this.QueueProcessTileRenderQueue();
                    }
                    else
                    {
                        this.ProcessTileRenderQueue();
                    }
                }
            }
        }

        private void QueueProcessTileRenderQueue()
        {
            base.VerifyAccess();
            using (this.queueProcessTileRenderQueueRegion.UseEnterScope())
            {
                if (!this.isProcessTileRenderQueueQueued)
                {
                    this.isProcessTileRenderQueueQueued = true;
                    this.syncContext.Post(this.processTileRenderQueueCallback);
                }
            }
        }

        private unsafe void RenderTileWorkItem(PointInt32 tileOffset)
        {
            IBitmap<ColorPbgra32> bitmap;
            bool isCancelled = false;
            bool flag = false;
            Exception error = null;
            isCancelled |= this.IsTileRenderingCancelled(tileOffset);
            if (isCancelled)
            {
                bitmap = null;
            }
            else
            {
                RectInt32 tileSourceRect = this.tileMathHelper.GetTileSourceRect(tileOffset);
                SizeInt32 tileBufferSize = this.GetTileBufferSize(tileOffset);
                bitmap = RetryManager.Eval<IBitmap<ColorPbgra32>>(3, () => BitmapAllocator.Pbgra32.Allocate(tileBufferSize, AllocationOptions.Default), delegate (Exception _) {
                    CleanupManager.RequestCleanup();
                    Thread.Sleep(200);
                    CleanupManager.WaitForPendingCleanup();
                }, delegate (AggregateException ex) {
                    throw new AggregateException($"could not allocate a bitmap of size {tileBufferSize.Width} x {tileBufferSize.Height}", ex).Flatten();
                });
                if (this.source != null)
                {
                    try
                    {
                        isCancelled |= this.IsTileRenderingCancelled(tileOffset);
                        if (!isCancelled)
                        {
                            using (IBitmapLock<ColorPbgra32> @lock = bitmap.Lock<ColorPbgra32>(BitmapLockOptions.ReadWrite))
                            {
                                if (this.mipLevel == 0)
                                {
                                    this.source.CopyPixels(new RectInt32?(tileSourceRect), @lock);
                                    RenderingKernels.ConvertBgra32ToPbgra32((uint*) @lock.Scan0, tileBufferSize.Width, tileBufferSize.Height, @lock.Stride);
                                    flag = true;
                                }
                                else
                                {
                                    BitmapInterpolationMode linear;
                                    if (!this.isHighQuality)
                                    {
                                        linear = BitmapInterpolationMode.Linear;
                                    }
                                    else if (this.mipLevel == 1)
                                    {
                                        linear = BitmapInterpolationMode.Linear;
                                    }
                                    else
                                    {
                                        linear = BitmapInterpolationMode.Fant;
                                    }
                                    IImagingFactory instance = ImagingFactory.Instance;
                                    Func<bool> pollIsCancelledCallback = () => isCancelled | this.IsTileRenderingCancelled(tileOffset);
                                    int copyHeightLog2 = Math.Max(3, 7 - this.mipLevel);
                                    using (ClippedBitmapSource<ColorBgra32> source2 = new ClippedBitmapSource<ColorBgra32>(this.source, tileSourceRect))
                                    {
                                        using (CancellableBitmapSource<ColorBgra32> source3 = new CancellableBitmapSource<ColorBgra32>(source2, r => this.tileMathHelper.EnumerateTilesClippedToSourceRect(r), null, pollIsCancelledCallback))
                                        {
                                            using (IBitmapSource<ColorPbgra32> source4 = CreateBufferedTileScaler(instance, source3, tileBufferSize.Width, tileBufferSize.Height, linear))
                                            {
                                                using (CancellableBitmapSource<ColorPbgra32> source5 = new CancellableBitmapSource<ColorPbgra32>(source4, r => TileRectSplitter(r, ((int) 1) << copyHeightLog2), null, pollIsCancelledCallback))
                                                {
                                                    try
                                                    {
                                                        source5.CopyPixels<ColorPbgra32>(@lock);
                                                        flag = true;
                                                    }
                                                    catch (OperationCanceledException exception2)
                                                    {
                                                        error = exception2;
                                                        isCancelled = true;
                                                    }
                                                    catch (Exception exception3)
                                                    {
                                                        error = exception3;
                                                        throw;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                isCancelled |= this.IsTileRenderingCancelled(tileOffset);
                                if (isCancelled)
                                {
                                    flag = false;
                                }
                            }
                            if (!flag)
                            {
                                DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref bitmap);
                            }
                        }
                    }
                    catch (OperationCanceledException exception4)
                    {
                        error = exception4;
                        isCancelled = true;
                    }
                    catch (Exception exception5)
                    {
                        error = exception5;
                        isCancelled |= this.IsTileRenderingCancelled(tileOffset);
                        if (!isCancelled)
                        {
                            using (IDrawingContext context = DrawingContext.FromBitmap(bitmap, FactorySource.PerThread))
                            {
                                context.Clear(new ColorRgba128Float?((ColorRgba128Float) Colors.White));
                                string text = exception5.ToString();
                                using (ISystemFonts fonts = new SystemFonts(true))
                                {
                                    TextLayout textLayout = UIText.CreateLayout(context, text, fonts.Caption, null, HotkeyRenderMode.Ignore, (double) bitmap.Size.Width, 65535.0);
                                    textLayout.FontSize *= 0.6;
                                    textLayout.WordWrapping = WordWrapping.Wrap;
                                    context.DrawTextLayout(PointDouble.Zero, textLayout, SolidColorBrushCache.Get((ColorRgba128Float) Colors.Black), DrawTextOptions.None);
                                }
                            }
                            flag = true;
                        }
                    }
                }
            }
            isCancelled |= this.IsTileRenderingCancelled(tileOffset);
            if (isCancelled)
            {
                DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref bitmap);
            }
            RenderedTileInfo info = new RenderedTileInfo(bitmap, !isCancelled && (bitmap > null), error);
            if (!this.tilesRenderedQueue.TryEnqueue(tileOffset, info))
            {
                ExceptionUtil.ThrowInternalErrorException("Could not enqueue to this.tilesRenderedQueue");
            }
            if (Interlocked.Exchange(ref this.isProcessTileRenderedQueueQueued, 1) == 0)
            {
                this.syncContext.Post(this.processTileRenderedQueueCallback);
            }
        }

        [IteratorStateMachine(typeof(<TileRectSplitter>d__66))]
        private static IEnumerable<RectInt32> TileRectSplitter(RectInt32 rect, int maxHeight) => 
            new <TileRectSplitter>d__66(-2) { 
                <>3__rect = rect,
                <>3__maxHeight = maxHeight
            };

        public IBitmap<ColorPbgra32> TryGetTileBufferRef(PointInt32 offset) => 
            this.TryGetTileBufferRef(offset.X, offset.Y);

        public IBitmap<ColorPbgra32> TryGetTileBufferRef(int tileColumn, int tileRow)
        {
            base.VerifyAccess();
            this.tileMathHelper.VerifyTileOffset(tileColumn, tileRow);
            IBitmap<ColorPbgra32> bitmap = this.tileBuffers[tileRow][tileColumn];
            if (bitmap.IsNullReference<IBitmap<ColorPbgra32>>())
            {
                return null;
            }
            return bitmap.CreateRef<ColorPbgra32>();
        }

        private void VerifyIsActive()
        {
            if (!this.IsActive)
            {
                ExceptionUtil.ThrowInvalidOperationException();
            }
        }

        public bool IsActive =>
            (this.isActiveCount > 0);

        public bool IsDisposed =>
            (this.tileBuffers == null);

        public bool IsHighQuality =>
            this.isHighQuality;

        public int MipLevel =>
            this.mipLevel;

        public WorkItemQueuePriority Priority
        {
            get => 
                this.workItemQueue.Priority;
            set
            {
                base.VerifyAccess();
                this.workItemQueue.Priority = value;
            }
        }

        public PaintDotNet.Rendering.TileMathHelper TileMathHelper =>
            this.tileMathHelper;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DocumentCanvasTileCache.<>c <>9 = new DocumentCanvasTileCache.<>c();
            public static Action<Exception> <>9__62_1;

            internal void <RenderTileWorkItem>b__62_1(Exception _)
            {
                CleanupManager.RequestCleanup();
                Thread.Sleep(200);
                CleanupManager.WaitForPendingCleanup();
            }
        }

        [CompilerGenerated]
        private sealed class <TileRectSplitter>d__66 : IEnumerable<RectInt32>, IEnumerable, IEnumerator<RectInt32>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private RectInt32 <>2__current;
            public int <>3__maxHeight;
            public RectInt32 <>3__rect;
            private int <>l__initialThreadId;
            private int <y>5__1;
            private int maxHeight;
            private RectInt32 rect;

            [DebuggerHidden]
            public <TileRectSplitter>d__66(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<y>5__1 = this.rect.Top;
                    while (this.<y>5__1 < this.rect.Bottom)
                    {
                        int num2 = this.<y>5__1 + this.maxHeight;
                        int num3 = Math.Min(this.rect.Bottom, num2);
                        this.<>2__current = new RectInt32(this.rect.X, this.<y>5__1, this.rect.Width, num3 - this.<y>5__1);
                        this.<>1__state = 1;
                        return true;
                    Label_0082:
                        this.<>1__state = -1;
                        this.<y>5__1 += this.maxHeight;
                    }
                    return false;
                }
                if (num != 1)
                {
                    return false;
                }
                goto Label_0082;
            }

            [DebuggerHidden]
            IEnumerator<RectInt32> IEnumerable<RectInt32>.GetEnumerator()
            {
                DocumentCanvasTileCache.<TileRectSplitter>d__66 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new DocumentCanvasTileCache.<TileRectSplitter>d__66(0);
                }
                d__.rect = this.<>3__rect;
                d__.maxHeight = this.<>3__maxHeight;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.RectInt32>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            RectInt32 IEnumerator<RectInt32>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

