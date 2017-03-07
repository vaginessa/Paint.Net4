namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class DocumentCanvasLayerViewMipLayer : ThreadAffinitizedObjectBase, IDisposable, IIsDisposed
    {
        private bool afterRenderCalled = true;
        private bool beforeRenderCalled;
        private CanvasView canvasView;
        private IDeviceBitmap[][] deviceBitmaps;
        private ObjectPoolTicket<IDeviceBitmap>[][] deviceBitmapTickets;
        private bool isActive;
        private bool[][] isDeviceBitmapCurrent;
        private bool isRendering;
        private bool isVisible;
        private const long maxUpdateMs = 0x7fffffffffffffffL;
        private int mipLevel;
        private DequeSet<PointInt32> nonCurrentTileOffsets = new DequeSet<PointInt32>();
        private DocumentCanvasLayerView owner;
        private bool popTileCacheActiveAfterRender;
        private bool renderCalled;
        private Stopwatch renderStopwatch = new Stopwatch();
        private IBitmap<ColorPbgra32>[][] tileBuffers;
        private DocumentCanvasTileCache tileCache;
        private TileMathHelper tileMathHelper;

        public DocumentCanvasLayerViewMipLayer(DocumentCanvasLayerView owner, int canvasTileEdgeLog2, int mipLevel, DocumentCanvasTileCache tileCache)
        {
            Validate.Begin().IsNotNull<DocumentCanvasLayerView>(owner, "owner").IsNotNull<DocumentCanvasTileCache>(tileCache, "tileCache").Check().IsGreaterThanOrEqualTo(canvasTileEdgeLog2, 1, "canvasTileEdgeLog2").Check();
            owner.VerifyAccess();
            this.owner = owner;
            this.tileCache = tileCache;
            this.canvasView = this.owner.CanvasView;
            this.canvasView.ViewportCanvasBoundsChanged += new ValueChangedEventHandler<RectDouble>(this.OnCanvasViewViewportCanvasBoundsChanged);
            this.mipLevel = mipLevel;
            this.tileMathHelper = this.tileCache.TileMathHelper;
            this.deviceBitmapTickets = ArrayUtil.Create2D<ObjectPoolTicket<IDeviceBitmap>>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
            this.deviceBitmaps = ArrayUtil.Create2D<IDeviceBitmap>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
            this.tileBuffers = ArrayUtil.Create2D<IBitmap<ColorPbgra32>>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
            this.isDeviceBitmapCurrent = ArrayUtil.Create2D<bool>(this.tileMathHelper.TileRows, this.tileMathHelper.TileColumns);
        }

        public void AfterRender(RectFloat clipRect)
        {
            if ((!this.beforeRenderCalled || !this.renderCalled) || this.afterRenderCalled)
            {
                throw new PaintDotNet.InternalErrorException();
            }
            this.afterRenderCalled = true;
            this.beforeRenderCalled = false;
            this.renderCalled = false;
            if (this.nonCurrentTileOffsets.Any())
            {
                RectInt32 canvasRect = ((IEnumerable<RectInt32>) (from to in this.nonCurrentTileOffsets select this.tileMathHelper.GetTileSourceRect(to))).Bounds();
                if (canvasRect.HasPositiveArea)
                {
                    this.canvasView.Invalidate(canvasRect);
                }
            }
            if (this.popTileCacheActiveAfterRender)
            {
                this.popTileCacheActiveAfterRender = false;
                this.PopTileCacheActive();
            }
        }

        public void BeforeRender(RectFloat clipRect)
        {
            if ((!this.afterRenderCalled || this.beforeRenderCalled) || this.renderCalled)
            {
                throw new PaintDotNet.InternalErrorException();
            }
            this.afterRenderCalled = false;
            this.beforeRenderCalled = true;
            bool flag = this.canvasView.RenderTarget.IsSupported(RenderTargetType.Software, null, null, null);
            if (flag || this.IsActive)
            {
                this.renderStopwatch.Restart();
                RectInt32 num2 = RectInt32.Intersect(clipRect.Int32Bound, this.owner.Document.Bounds());
                bool flag2 = false;
                if (num2.HasPositiveArea)
                {
                    PointInt32 num7;
                    PointInt32[] array = this.nonCurrentTileOffsets.ToArrayEx<PointInt32>();
                    this.nonCurrentTileOffsets.Clear();
                    PointInt32 sourcePt = PointDouble.Round(this.owner.Owner.MouseLocation, MidpointRounding.AwayFromZero);
                    PointInt32 comparand = this.tileMathHelper.ConvertSourcePointToTileOffset(sourcePt);
                    CompareTileOffsetsByDistance comparer = new CompareTileOffsetsByDistance(comparand);
                    ListUtil.Sort<PointInt32, ArrayStruct<PointInt32>, CompareTileOffsetsByDistance>(array.AsStruct<PointInt32>(), comparer);
                    DequeSet<PointInt32> set = new DequeSet<PointInt32>(array);
                    int count = set.Count;
                    while (set.TryDequeue(out num7))
                    {
                        if (!this.isDeviceBitmapCurrent[num7.Y][num7.X])
                        {
                            if (flag2)
                            {
                                this.nonCurrentTileOffsets.TryEnqueue(num7);
                            }
                            else
                            {
                                if (!this.TryUpdateDeviceBitmap(num7))
                                {
                                    this.nonCurrentTileOffsets.TryEnqueue(num7);
                                    flag2 = true;
                                }
                                if ((!flag2 && !flag) && (this.renderStopwatch.ElapsedMilliseconds > 0x7fffffffffffffffL))
                                {
                                    flag2 = true;
                                }
                            }
                        }
                    }
                }
            }
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
                if (this.tileCache != null)
                {
                    this.IsActive = false;
                    this.IsVisible = false;
                    this.tileCache = null;
                }
                if (((this.deviceBitmaps != null) && (this.deviceBitmapTickets != null)) && (this.tileBuffers != null))
                {
                    Work.QueueFreeStream<IDisposable[]>(ref (from to in this.tileMathHelper.EnumerateTileOffsets() select this.EnumerateTileResources(to)).SelectMany<IDisposable>().ToArrayEx<IDisposable>());
                    this.deviceBitmaps = null;
                    this.deviceBitmapTickets = null;
                    this.tileBuffers = null;
                }
                if (this.canvasView != null)
                {
                    this.canvasView.ViewportCanvasBoundsChanged -= new ValueChangedEventHandler<RectDouble>(this.OnCanvasViewViewportCanvasBoundsChanged);
                    this.canvasView = null;
                }
            }
        }

        private IEnumerable<IDisposable> EnumerateTileResources(PointInt32 tileOffset) => 
            this.EnumerateTileResources(tileOffset.X, tileOffset.Y);

        [IteratorStateMachine(typeof(<EnumerateTileResources>d__39))]
        private IEnumerable<IDisposable> EnumerateTileResources(int tileColumn, int tileRow)
        {
            if (this.deviceBitmapTickets[tileRow][tileColumn] == null)
            {
                yield return this.deviceBitmaps[tileRow][tileColumn];
                yield return this.tileBuffers[tileRow][tileColumn];
            }
            else
            {
                yield return this.deviceBitmapTickets[tileRow][tileColumn];
            }
        }

        public void InvalidateDeviceResources()
        {
            for (int i = 0; i < this.deviceBitmaps.Length; i++)
            {
                for (int j = 0; j < this.deviceBitmaps[i].Length; j++)
                {
                    this.ReleaseDeviceBitmap(j, i);
                }
            }
        }

        public bool IsRegionCurrent(RectInt32 canvasRect)
        {
            base.VerifyAccess();
            foreach (PointInt32 num in this.tileMathHelper.EnumerateTileOffsets(canvasRect))
            {
                if ((!this.isDeviceBitmapCurrent[num.Y][num.X] || (this.deviceBitmaps[num.Y][num.X] == null)) || !this.tileCache.IsTileValid(num))
                {
                    return false;
                }
            }
            return true;
        }

        internal void NotifyTileCacheFinishedRendering<TEnumerable>(TEnumerable finishedTileOffsets) where TEnumerable: IEnumerable<PointInt32>
        {
            base.VerifyAccess();
            if (this.IsActive)
            {
                RectInt32 num = this.canvasView.GetVisibleCanvasBounds().Int32Bound;
                RectInt32? nullable = null;
                foreach (PointInt32 num3 in finishedTileOffsets)
                {
                    this.isDeviceBitmapCurrent[num3.Y][num3.X] = false;
                    RectInt32 tileSourceRect = this.tileMathHelper.GetTileSourceRect(num3);
                    if (num.IntersectsWith(tileSourceRect))
                    {
                        nullable = new RectInt32?(RectInt32Util.Union(nullable, tileSourceRect));
                    }
                    else
                    {
                        this.ReleaseDeviceBitmap(num3);
                    }
                    this.nonCurrentTileOffsets.TryEnqueue(num3, QueueSide.Back);
                }
                if (nullable.HasValue)
                {
                    this.canvasView.Invalidate(nullable.Value);
                }
            }
            else
            {
                foreach (PointInt32 num5 in finishedTileOffsets)
                {
                    this.ReleaseDeviceBitmap(num5);
                }
            }
        }

        internal void NotifyTileCacheInvalidated(RectInt32 canvasRect)
        {
        }

        internal void NotifyTileCacheIsActiveChanged(bool isActive)
        {
        }

        private void OnCanvasViewViewportCanvasBoundsChanged(object sender, ValueChangedEventArgs<RectDouble> e)
        {
            if (this.IsVisible && this.IsActive)
            {
                RectInt32 sourceRect = e.NewValue.Int32Bound;
                this.tileCache.QueueInvalidTilesForRendering(sourceRect, true);
            }
        }

        private void PopTileCacheActive()
        {
            if (this.isActive)
            {
                throw new PaintDotNet.InternalErrorException("this.isActive is true");
            }
            if ((this.isRendering || this.beforeRenderCalled) || (this.renderCalled || !this.afterRenderCalled))
            {
                if (this.popTileCacheActiveAfterRender)
                {
                    throw new PaintDotNet.InternalErrorException("this.popTileCacheAfterRender is already true");
                }
                this.popTileCacheActiveAfterRender = true;
            }
            else
            {
                this.tileCache.PopActive();
            }
        }

        private void PushTileCacheActive()
        {
            if (!this.isActive)
            {
                throw new PaintDotNet.InternalErrorException("this.isActive is false");
            }
            if (this.popTileCacheActiveAfterRender)
            {
                throw new PaintDotNet.InternalErrorException("this.popTileCacheActiveAfterRender is true");
            }
            this.tileCache.PushActive();
            this.tileCache.QueueInvalidTilesForRendering(true);
        }

        private void ReleaseDeviceBitmap(PointInt32 tileOffset)
        {
            this.ReleaseDeviceBitmap(tileOffset.X, tileOffset.Y);
        }

        private void ReleaseDeviceBitmap(int tileColumn, int tileRow)
        {
            if ((this.isRendering || this.beforeRenderCalled) || (this.renderCalled || !this.afterRenderCalled))
            {
                throw new InvalidOperationException("Cannot call ReleaseDeviceBitmap() while rendering");
            }
            if (this.deviceBitmapTickets[tileRow][tileColumn] != null)
            {
                DisposableUtil.Free<ObjectPoolTicket<IDeviceBitmap>>(ref this.deviceBitmapTickets[tileRow][tileColumn]);
                this.deviceBitmaps[tileRow][tileColumn] = null;
            }
            else
            {
                DisposableUtil.Free<IDeviceBitmap>(ref this.deviceBitmaps[tileRow][tileColumn]);
                DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref this.tileBuffers[tileRow][tileColumn]);
            }
            this.isDeviceBitmapCurrent[tileRow][tileColumn] = false;
            this.nonCurrentTileOffsets.TryEnqueue(new PointInt32(tileColumn, tileRow));
        }

        public void Render(IDrawingContext dc, RectFloat clipRect, float opacity, BitmapInterpolationMode interpolationMode)
        {
            if (!this.beforeRenderCalled || this.afterRenderCalled)
            {
                throw new PaintDotNet.InternalErrorException();
            }
            this.renderCalled = true;
            if (this.isRendering)
            {
                throw new InvalidOperationException("Render() is not reentrant");
            }
            RectInt32 sourceRect = RectInt32.Intersect(clipRect.Int32Bound, this.owner.Document.Bounds());
            if (sourceRect.HasPositiveArea)
            {
                this.isRendering = true;
                try
                {
                    bool flag = dc.IsSupported(RenderTargetType.Software, null, null, null);
                    foreach (PointInt32 num3 in this.tileMathHelper.EnumerateTileOffsets(sourceRect))
                    {
                        if (flag)
                        {
                            IBitmap<ColorPbgra32> bitmap2 = this.tileBuffers[num3.Y][num3.X];
                            if ((bitmap2 != null) && bitmap2.IsDisposed)
                            {
                                continue;
                            }
                        }
                        IDeviceBitmap bitmap = this.deviceBitmaps[num3.Y][num3.X];
                        if (bitmap != null)
                        {
                            RectInt32 tileSourceRect = this.tileMathHelper.GetTileSourceRect(num3.X, num3.Y);
                            RectFloat? srcRect = null;
                            dc.DrawBitmap(bitmap, new RectFloat?(tileSourceRect), opacity, interpolationMode, srcRect);
                        }
                    }
                }
                finally
                {
                    this.isRendering = false;
                }
            }
        }

        private bool TryUpdateDeviceBitmap(PointInt32 tileOffset) => 
            this.TryUpdateDeviceBitmap(tileOffset.X, tileOffset.Y);

        private bool TryUpdateDeviceBitmap(int tileColumn, int tileRow)
        {
            IRenderTarget renderTarget = this.canvasView.RenderTarget;
            if (renderTarget == null)
            {
                return false;
            }
            bool flag = renderTarget.IsSupported(RenderTargetType.Software, null, null, null);
            if (!flag && (!this.IsVisible || !this.IsActive))
            {
                throw new PaintDotNet.InternalErrorException();
            }
            if (!this.isDeviceBitmapCurrent[tileRow][tileColumn])
            {
                using (IBitmap<ColorPbgra32> bitmap = this.tileCache.TryGetTileBufferRef(tileColumn, tileRow))
                {
                    if (bitmap != null)
                    {
                        if (bitmap.IsDisposed)
                        {
                            throw new ObjectDisposedException("tileBufferRef");
                        }
                        if (flag)
                        {
                            DisposableUtil.Free<IDeviceBitmap>(ref this.deviceBitmaps[tileRow][tileColumn]);
                            DisposableUtil.Free<IBitmap<ColorPbgra32>>(ref this.tileBuffers[tileRow][tileColumn]);
                            IBitmapLock bitmapLock = bitmap.Lock<ColorPbgra32>(BitmapLockOptions.Read);
                            IDeviceBitmap bitmap2 = renderTarget.CreateSharedBitmap(bitmapLock, null);
                            this.deviceBitmaps[tileRow][tileColumn] = bitmap2;
                            this.tileBuffers[tileRow][tileColumn] = bitmap.CreateRef<ColorPbgra32>();
                        }
                        else
                        {
                            ObjectPoolTicket<IDeviceBitmap> ticket = this.deviceBitmapTickets[tileRow][tileColumn];
                            if (ticket == null)
                            {
                                try
                                {
                                    ticket = this.owner.DeviceBitmapPool.Get(bitmap.Size);
                                }
                                catch (RecreateTargetException)
                                {
                                    return false;
                                }
                                this.deviceBitmapTickets[tileRow][tileColumn] = ticket;
                                this.deviceBitmaps[tileRow][tileColumn] = ticket.Value;
                            }
                            ticket.Value.CopyFromBitmap(null, bitmap, null);
                        }
                        this.isDeviceBitmapCurrent[tileRow][tileColumn] = true;
                    }
                }
            }
            return true;
        }

        public bool IsActive
        {
            get => 
                this.isActive;
            set
            {
                base.VerifyAccess();
                if (value != this.isActive)
                {
                    this.isActive = value;
                    if (value)
                    {
                        this.PushTileCacheActive();
                    }
                    else
                    {
                        this.PopTileCacheActive();
                    }
                }
            }
        }

        public bool IsDisposed =>
            (this.deviceBitmapTickets == null);

        public bool IsVisible
        {
            get => 
                this.isVisible;
            set
            {
                base.VerifyAccess();
                if (value != this.isVisible)
                {
                    this.isVisible = value;
                    if (!value)
                    {
                        this.InvalidateDeviceResources();
                    }
                }
            }
        }

        public int MipLevel =>
            this.mipLevel;

        public DocumentCanvasLayerView Owner =>
            this.owner;

        public DocumentCanvasTileCache TileCache =>
            this.tileCache;

    }
}

