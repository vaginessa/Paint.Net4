namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Dxgi;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class DocumentCanvasLayerView : ThreadAffinitizedObjectBase, IDisposable, IIsDisposed
    {
        private bool afterRenderCalled = true;
        private bool beforeRenderCalled;
        private PaintDotNet.Canvas.CanvasView canvasView;
        private ObjectPool<SizeInt32, IDeviceBitmap> deviceBitmapPool;
        private PaintDotNet.Document document;
        private DocumentCanvasLayerViewMipLayer[] mipLayers;
        private List<DocumentCanvasLayerViewMipLayer> mipLayerZOrder;
        private DocumentCanvasLayer owner;
        private bool renderCalled;
        private IList<DocumentCanvasTileCache> tileCaches;

        public DocumentCanvasLayerView(DocumentCanvasLayer owner, PaintDotNet.Canvas.CanvasView canvasView, IList<DocumentCanvasTileCache> tileCaches)
        {
            Validate.Begin().IsNotNull<DocumentCanvasLayer>(owner, "owner").IsNotNull<PaintDotNet.Canvas.CanvasView>(canvasView, "canvasView").IsNotNull<IList<DocumentCanvasTileCache>>(tileCaches, "tileCaches").Check();
            this.owner = owner;
            this.document = this.owner.Document;
            this.canvasView = canvasView;
            this.canvasView.IsVisibleChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
            this.tileCaches = tileCaches;
            this.mipLayers = new DocumentCanvasLayerViewMipLayer[this.tileCaches.Count];
            for (int i = 0; i < this.mipLayers.Length; i++)
            {
                this.mipLayers[i] = new DocumentCanvasLayerViewMipLayer(this, this.tileCaches[i].TileMathHelper.TileEdgeLog2, i, this.tileCaches[i]);
            }
            this.deviceBitmapPool = new ObjectPool<SizeInt32, IDeviceBitmap>(new Func<SizeInt32, IDeviceBitmap>(this.CreateDeviceBitmap), new Action<SizeInt32, IDeviceBitmap>(this.DisposeDeviceBitmap));
            this.mipLayerZOrder = new List<DocumentCanvasLayerViewMipLayer>();
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
            for (int i = this.mipLayerZOrder.Count - 1; i >= 0; i--)
            {
                this.mipLayerZOrder[i].AfterRender(clipRect);
            }
            if ((this.mipLayerZOrder.Count > 1) && this.mipLayerZOrder[0].IsRegionCurrent(this.canvasView.GetVisibleCanvasBounds().Int32Bound))
            {
                for (int k = this.mipLayerZOrder.Count - 1; k >= 1; k--)
                {
                    this.mipLayerZOrder[k].IsVisible = false;
                    this.mipLayerZOrder.RemoveAt(k);
                }
            }
            bool flag = true;
            for (int j = 0; j < this.mipLayers.Length; j++)
            {
                if (this.mipLayers[j].IsActive)
                {
                    if (!flag)
                    {
                        this.mipLayers[j].TileCache.Priority = WorkItemQueuePriority.High;
                        return;
                    }
                    this.mipLayers[j].TileCache.Priority = WorkItemQueuePriority.AboveNormal;
                    flag = false;
                }
                else
                {
                    this.mipLayers[j].TileCache.Priority = WorkItemQueuePriority.Normal;
                }
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
            base.VerifyAccess();
            double scaleRatio = this.canvasView.ScaleRatio;
            bool flag = scaleRatio == ((int) scaleRatio);
            int index = (int) DoubleUtil.Clamp(this.ConvertScaleRatioToMipLevel(scaleRatio), 0.0, (double) (this.MipLevelCount - 1));
            DocumentCanvasLayerViewMipLayer item = this.mipLayers[index];
            if (this.mipLayerZOrder.IndexOf(item) != 0)
            {
                this.mipLayerZOrder.Remove(item);
                this.mipLayerZOrder.Insert(0, item);
                item.IsVisible = true;
                item.IsActive = true;
                this.canvasView.Invalidate();
            }
            this.tileCaches[this.mipLayerZOrder[0].MipLevel].ProcessTileRenderedQueue();
            this.mipLayerZOrder[0].BeforeRender(clipRect);
            for (int i = this.mipLayerZOrder.Count - 1; i >= 1; i--)
            {
                this.mipLayerZOrder[i].BeforeRender(clipRect);
                this.mipLayerZOrder[i].IsActive = false;
            }
        }

        private double ConvertScaleRatioToMipLevel(double scaleRatio) => 
            Math.Log(scaleRatio, 0.5);

        private IDeviceBitmap CreateDeviceBitmap(SizeInt32 size)
        {
            IRenderTarget renderTarget = this.canvasView.RenderTarget;
            if (renderTarget == null)
            {
                throw new PaintDotNet.InternalErrorException();
            }
            return RetryManager.Eval<IDeviceBitmap>(3, () => renderTarget.CreateDeviceBitmap(size, DxgiFormat.B8G8R8A8_UNorm, AlphaMode.Premultiplied, null, null), delegate (Exception _) {
                CleanupManager.RequestCleanup();
                Thread.Sleep(200);
                CleanupManager.WaitForPendingCleanup();
            }, delegate (AggregateException ex) {
                throw new AggregateException($"could not allocate a bitmap of size {size.Width} x {size.Height}", ex).Flatten();
            });
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
                DisposableUtil.FreeContents<DocumentCanvasLayerViewMipLayer>(this.mipLayers);
                this.tileCaches = null;
                if (this.canvasView != null)
                {
                    this.canvasView.IsVisibleChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsVisibleChanged);
                    this.canvasView = null;
                }
            }
        }

        private void DisposeDeviceBitmap(SizeInt32 size, IDeviceBitmap deviceBitmap)
        {
            deviceBitmap.Dispose();
        }

        internal DocumentCanvasLayerViewMipLayer GetMipLayer(int mipLevel) => 
            this.mipLayers[mipLevel];

        public void InvalidateDeviceResources()
        {
            base.VerifyAccess();
            for (int i = 0; i < this.mipLayers.Length; i++)
            {
                this.mipLayers[i].InvalidateDeviceResources();
            }
            this.deviceBitmapPool.Clear();
        }

        internal void NotifyTileCacheFinishedRendering<TEnumerable>(DocumentCanvasTileCache tileCache, TEnumerable finishedTileOffsets) where TEnumerable: IEnumerable<PointInt32>
        {
            int mipLevel = tileCache.MipLevel;
            this.mipLayers[mipLevel].NotifyTileCacheFinishedRendering<TEnumerable>(finishedTileOffsets);
        }

        internal void NotifyTileCacheInvalidated(DocumentCanvasTileCache tileCache, RectInt32 sourceRect)
        {
            int mipLevel = tileCache.MipLevel;
            this.mipLayers[mipLevel].NotifyTileCacheInvalidated(sourceRect);
            if (this.mipLayers[mipLevel].IsActive)
            {
                RectInt32 num4 = RectInt32.Intersect(this.canvasView.GetVisibleCanvasBounds().Int32Bound, sourceRect);
                if (num4.HasPositiveArea)
                {
                    this.tileCaches[mipLevel].QueueInvalidTilesForRendering(num4, true);
                }
            }
        }

        internal void NotifyTileCacheIsActiveChanged(DocumentCanvasTileCache tileCache, bool isActive)
        {
            int mipLevel = tileCache.MipLevel;
            this.mipLayers[mipLevel].NotifyTileCacheIsActiveChanged(isActive);
        }

        private void OnCanvasViewIsVisibleChanged(object sender, ValueChangedEventArgs<bool> e)
        {
        }

        public void PreRenderSync()
        {
            base.VerifyAccess();
            foreach (DocumentCanvasTileCache cache in this.tileCaches)
            {
                cache.ProcessTileRenderQueue();
                cache.ProcessTileRenderedQueue();
            }
            foreach (DocumentCanvasLayerViewMipLayer layer in this.mipLayers)
            {
                DocumentCanvasTileCache tileCache = layer.TileCache;
                layer.NotifyTileCacheFinishedRendering<IEnumerable<PointInt32>>(tileCache.TileMathHelper.EnumerateTileOffsets());
            }
        }

        public void Render(IDrawingContext dc, RectFloat clipRect)
        {
            if ((!this.beforeRenderCalled || this.renderCalled) || this.afterRenderCalled)
            {
                throw new PaintDotNet.InternalErrorException();
            }
            this.renderCalled = true;
            using (dc.UseAntialiasMode(AntialiasMode.Aliased))
            {
                double scaleRatio = this.canvasView.ScaleRatio;
                double d = this.ConvertScaleRatioToMipLevel(scaleRatio);
                int num3 = (int) Math.Floor(d);
                bool flag = Math.Abs((double) (d - Math.Round(d, MidpointRounding.AwayFromZero))) < 0.01;
                bool isHighQualityScalingEnabled = DocumentCanvasLayer.GetIsHighQualityScalingEnabled(this.canvasView);
                for (int i = this.mipLayerZOrder.Count - 1; i >= 0; i--)
                {
                    DocumentCanvasLayerViewMipLayer layer = this.mipLayerZOrder[i];
                    if (layer.MipLevel == 0)
                    {
                        if (flag)
                        {
                            layer.Render(dc, clipRect, 1f, BitmapInterpolationMode.NearestNeighbor);
                        }
                        else if (scaleRatio < 1.0)
                        {
                            layer.Render(dc, clipRect, 1f, isHighQualityScalingEnabled ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
                        }
                        else
                        {
                            layer.Render(dc, clipRect, 1f, BitmapInterpolationMode.NearestNeighbor);
                            if ((scaleRatio < 2.0) & isHighQualityScalingEnabled)
                            {
                                double num5 = d - num3;
                                double num6 = 1.0 - (((1.0 - num5) * (1.0 - num5)) * (1.0 - num5));
                                double num7 = DoubleUtil.Clamp(num6, 0.0, 1.0);
                                layer.Render(dc, clipRect, (float) num7, BitmapInterpolationMode.Linear);
                            }
                        }
                    }
                    else
                    {
                        layer.Render(dc, clipRect, 1f, isHighQualityScalingEnabled ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
                    }
                }
            }
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            this.canvasView;

        internal ObjectPool<SizeInt32, IDeviceBitmap> DeviceBitmapPool
        {
            get
            {
                base.VerifyAccess();
                return this.deviceBitmapPool;
            }
        }

        public PaintDotNet.Document Document =>
            this.document;

        public bool IsDisposed =>
            (this.canvasView == null);

        public int MipLevelCount =>
            this.mipLayers.Length;

        public DocumentCanvasLayer Owner =>
            this.owner;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DocumentCanvasLayerView.<>c <>9 = new DocumentCanvasLayerView.<>c();
            public static Action<Exception> <>9__25_1;

            internal void <CreateDeviceBitmap>b__25_1(Exception _)
            {
                CleanupManager.RequestCleanup();
                Thread.Sleep(200);
                CleanupManager.WaitForPendingCleanup();
            }
        }
    }
}

