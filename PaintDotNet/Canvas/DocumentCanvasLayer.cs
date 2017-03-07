namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    internal sealed class DocumentCanvasLayer : CanvasLayer
    {
        private PaintDotNet.Document document;
        private Dictionary<CanvasView, DocumentCanvasLayerView> documentCanvasLayerViews = new Dictionary<CanvasView, DocumentCanvasLayerView>(1);
        private PaintDotNet.Canvas.DocumentRenderer documentRenderer;
        private ConcurrentQueue<RectInt32> invalidationQueue = new ConcurrentQueue<RectInt32>();
        public static readonly DependencyProperty IsHighQualityScalingEnabledProperty = DependencyProperty.RegisterAttached("IsHighQualityScalingEnabled", typeof(bool), typeof(DocumentCanvasLayer), new PropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(DocumentCanvasLayer.OnIsHighQualityScalingEnabledPropertyChanged)));
        private PointDouble mouseLocation;
        private Dictionary<Layer, DocumentLayerOverlay> overlays = new Dictionary<Layer, DocumentLayerOverlay>();
        private Action processInvalidationQueue;
        private long tileCacheHQIsRenderingMask;
        private static int tileCacheIsRenderingCount;
        private long tileCacheIsRenderingMask;
        private DocumentCanvasTileCache[] tileCaches;
        private DocumentCanvasTileCache[] tileCachesHQ;
        private ReadOnlyCollection<DocumentCanvasTileCache> tileCachesHQRO;
        private ReadOnlyCollection<DocumentCanvasTileCache> tileCachesRO;
        private const int tileEdgeLog2 = 7;

        [field: CompilerGenerated]
        public event EventHandler CompositionIdle;

        [field: CompilerGenerated]
        public static  event ValueChangedEventHandler<bool> IsHighQualityScalingEnabledChanged;

        public DocumentCanvasLayer()
        {
            IsHighQualityScalingEnabledChanged += new ValueChangedEventHandler<bool>(this.OnCanvasViewIsHighQualityScalingEnabledChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsHighQualityScalingEnabledChanged -= new ValueChangedEventHandler<bool>(this.OnCanvasViewIsHighQualityScalingEnabledChanged);
                this.Document = null;
            }
            base.Dispose(disposing);
        }

        public static bool GetIsHighQualityScalingEnabled(CanvasView canvasView) => 
            ((bool) canvasView.GetValue(IsHighQualityScalingEnabledProperty));

        private void InvalidateCore(RectInt32 invalidRect)
        {
            base.VerifyAccess();
            if (!base.IsDisposed)
            {
                for (int i = 0; i < this.tileCaches.Length; i++)
                {
                    this.tileCaches[i].Invalidate(invalidRect);
                }
                for (int j = 1; j < this.tileCachesHQ.Length; j++)
                {
                    this.tileCachesHQ[j].Invalidate(invalidRect);
                }
            }
        }

        internal void NotifyTileCacheFinishedRendering<TEnumerable>(DocumentCanvasTileCache tileCache, TEnumerable tileOffsets) where TEnumerable: IEnumerable<PointInt32>
        {
            base.VerifyAccess();
            int mipLevel = tileCache.MipLevel;
            foreach (DocumentCanvasLayerView view in this.documentCanvasLayerViews.Values)
            {
                if ((mipLevel == 0) || (tileCache.IsHighQuality == GetIsHighQualityScalingEnabled(view.CanvasView)))
                {
                    view.NotifyTileCacheFinishedRendering<TEnumerable>(tileCache, tileOffsets);
                }
            }
        }

        internal void NotifyTileCacheInvalidated(DocumentCanvasTileCache tileCache, RectInt32 sourceRect)
        {
            base.VerifyAccess();
            int mipLevel = tileCache.MipLevel;
            foreach (DocumentCanvasLayerView view in this.documentCanvasLayerViews.Values)
            {
                if ((mipLevel == 0) || (tileCache.IsHighQuality == GetIsHighQualityScalingEnabled(view.CanvasView)))
                {
                    view.NotifyTileCacheInvalidated(tileCache, sourceRect);
                }
            }
            if (tileCache.IsActive)
            {
                tileCache.ProcessTileRenderQueue();
            }
        }

        internal void NotifyTileCacheIsActiveChanged(DocumentCanvasTileCache tileCache, bool isActive)
        {
            int mipLevel = tileCache.MipLevel;
            foreach (DocumentCanvasLayerView view in this.documentCanvasLayerViews.Values)
            {
                if ((mipLevel == 0) || (tileCache.IsHighQuality == GetIsHighQualityScalingEnabled(view.CanvasView)))
                {
                    view.NotifyTileCacheIsActiveChanged(tileCache, isActive);
                }
            }
        }

        internal void NotifyTileCacheIsIdle(DocumentCanvasTileCache tileCache)
        {
            long num2;
            long num3;
            base.VerifyAccess();
            int mipLevel = tileCache.MipLevel;
            if ((mipLevel < 0) || (mipLevel > 0x3f))
            {
                ExceptionUtil.ThrowInternalErrorException();
            }
            if (tileCache.IsHighQuality)
            {
                long tileCacheHQIsRenderingMask = this.tileCacheHQIsRenderingMask;
                this.tileCacheHQIsRenderingMask &= ~(((long) 1L) << mipLevel);
                num2 = tileCacheHQIsRenderingMask;
                num3 = this.tileCacheHQIsRenderingMask;
            }
            else
            {
                long tileCacheIsRenderingMask = this.tileCacheIsRenderingMask;
                this.tileCacheIsRenderingMask &= ~(((long) 1L) << mipLevel);
                num2 = tileCacheIsRenderingMask;
                num3 = this.tileCacheIsRenderingMask;
            }
            if ((num2 != 0) && (num3 == 0))
            {
                Interlocked.Decrement(ref tileCacheIsRenderingCount);
                this.CompositionIdle.Raise(this);
            }
        }

        internal void NotifyTileCacheIsRendering(DocumentCanvasTileCache tileCache)
        {
            long num2;
            long num3;
            base.VerifyAccess();
            int mipLevel = tileCache.MipLevel;
            if ((mipLevel < 0) || (mipLevel > 0x3f))
            {
                ExceptionUtil.ThrowInternalErrorException();
            }
            if (tileCache.IsHighQuality)
            {
                long tileCacheHQIsRenderingMask = this.tileCacheHQIsRenderingMask;
                this.tileCacheHQIsRenderingMask |= ((long) 1L) << mipLevel;
                num2 = tileCacheHQIsRenderingMask;
                num3 = this.tileCacheHQIsRenderingMask;
            }
            else
            {
                long tileCacheIsRenderingMask = this.tileCacheIsRenderingMask;
                this.tileCacheIsRenderingMask |= ((long) 1L) << mipLevel;
                num2 = tileCacheIsRenderingMask;
                num3 = this.tileCacheIsRenderingMask;
            }
            if ((num2 == 0) && (num3 != 0))
            {
                Interlocked.Increment(ref tileCacheIsRenderingCount);
            }
        }

        protected override void OnAfterRender(RectFloat clipRect, CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if (this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.AfterRender(clipRect);
            }
            base.OnBeforeRender(clipRect, canvasView);
        }

        protected override void OnBeforeRender(RectFloat clipRect, CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if ((this.document != null) && this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.BeforeRender(clipRect);
            }
            base.OnBeforeRender(clipRect, canvasView);
        }

        private void OnCanvasViewIsHighQualityScalingEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (!base.CheckAccess())
            {
                PaintDotNet.Canvas.Canvas owner = base.Owner;
                if ((owner != null) && (owner.Dispatcher != null))
                {
                    try
                    {
                        object[] args = new object[] { sender, e };
                        base.Owner.Dispatcher.BeginInvoke(new Action<object, ValueChangedEventArgs<bool>>(this.OnCanvasViewIsHighQualityScalingEnabledChanged), args);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (!base.IsDisposed)
            {
                CanvasView key = (CanvasView) sender;
                if (this.documentCanvasLayerViews.ContainsKey(key))
                {
                    this.RecreateDocumentCanvasLayerView(key);
                    key.Invalidate(key.GetCanvasBounds());
                }
            }
        }

        private void OnDocumentInvalidated(object sender, RectInt32InvalidatedEventArgs e)
        {
            this.invalidationQueue.Enqueue(e.InvalidRect);
            if (this.processInvalidationQueue == null)
            {
                this.processInvalidationQueue = new Action(this.ProcessInvalidationQueue);
            }
            PdnSynchronizationContext.Instance.EnsurePosted(this.processInvalidationQueue);
        }

        private void OnDocumentLayersChanged(object sender, EventArgs e)
        {
            base.VerifyAccess();
            foreach (KeyValuePair<Layer, DocumentLayerOverlay> pair in this.overlays)
            {
                if (!this.document.Layers.Contains(pair.Key))
                {
                    throw new PaintDotNet.InternalErrorException("A layer overlay is still registered for a layer that was removed");
                }
            }
        }

        private void OnDocumentLayersChanging(object sender, EventArgs e)
        {
            base.VerifyAccess();
            for (int i = 0; i < this.tileCaches.Length; i++)
            {
                this.tileCaches[i].CancelAllRendering();
            }
            for (int j = 1; j < this.tileCachesHQ.Length; j++)
            {
                this.tileCachesHQ[j].CancelAllRendering();
            }
        }

        protected override void OnInvalidateDeviceResources(CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if (this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.InvalidateDeviceResources();
            }
            base.OnInvalidateDeviceResources(canvasView);
        }

        private static void OnIsHighQualityScalingEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            IsHighQualityScalingEnabledChanged.Raise<bool>(target, e);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if ((this.document != null) && this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.Render(dc, clipRect);
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        protected override void OnViewRegistered(CanvasView canvasView)
        {
            if (this.Document != null)
            {
                this.RecreateDocumentCanvasLayerView(canvasView);
            }
            base.OnViewRegistered(canvasView);
        }

        protected override void OnViewUnregistered(CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if (this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.Dispose();
                this.documentCanvasLayerViews.Remove(canvasView);
            }
            base.OnViewUnregistered(canvasView);
        }

        public void PreRenderSync(CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            base.VerifyAccess();
            if (this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.PreRenderSync();
            }
        }

        private void ProcessInvalidationQueue()
        {
            if (!base.IsDisposed)
            {
                RectInt32 num2;
                base.VerifyAccess();
                int count = this.invalidationQueue.Count;
                RectInt32? nullable = null;
                while ((count > 0) && this.invalidationQueue.TryDequeue(out num2))
                {
                    nullable = new RectInt32?(RectInt32Util.Union(nullable, num2));
                    count--;
                }
                if (nullable.HasValue)
                {
                    RectInt32 invalidRect = RectInt32.Intersect(this.Document.Bounds(), nullable.Value);
                    this.InvalidateCore(invalidRect);
                }
            }
        }

        private void RecreateDocumentCanvasLayerView(CanvasView canvasView)
        {
            DocumentCanvasLayerView view;
            if (this.documentCanvasLayerViews.TryGetValue(canvasView, out view))
            {
                view.Dispose();
                this.documentCanvasLayerViews.Remove(canvasView);
            }
            bool isHighQualityScalingEnabled = GetIsHighQualityScalingEnabled(canvasView);
            DocumentCanvasLayerView view2 = new DocumentCanvasLayerView(this, canvasView, isHighQualityScalingEnabled ? this.tileCachesHQRO : this.tileCachesRO);
            this.documentCanvasLayerViews.Add(canvasView, view2);
        }

        public void RemoveLayerOverlay(Layer layer, DocumentLayerOverlay overlay, RectInt32? invalidateRect)
        {
            DocumentLayerOverlay overlay2;
            Validate.Begin().IsNotNull<Layer>(layer, "layer").IsNotNull<DocumentLayerOverlay>(overlay, "overlay").Check();
            base.VerifyAccess();
            if (!this.overlays.TryGetValue(layer, out overlay2))
            {
                throw new KeyNotFoundException();
            }
            if (overlay2 != overlay)
            {
                throw new InvalidOperationException();
            }
            this.overlays.Remove(layer);
            this.documentRenderer.RemoveLayerOverlay(layer);
            overlay.Cancel();
            if (invalidateRect.HasValue)
            {
                if (invalidateRect.Value.HasPositiveArea)
                {
                    layer.Invalidate(invalidateRect.Value);
                }
            }
            else
            {
                layer.Invalidate(overlay.AffectedBounds);
            }
        }

        public void ReplaceLayerOverlay(Layer layer, DocumentLayerOverlay oldOverlay, DocumentLayerOverlay newOverlay, RectInt32? invalidateRect)
        {
            DocumentLayerOverlay overlay;
            base.VerifyAccess();
            if ((oldOverlay == null) && (newOverlay == null))
            {
                throw new ArgumentNullException();
            }
            this.overlays.TryGetValue(layer, out overlay);
            if (overlay != oldOverlay)
            {
                throw new InvalidOperationException();
            }
            if ((oldOverlay == null) && (newOverlay != null))
            {
                this.SetLayerOverlay(layer, newOverlay, invalidateRect);
            }
            else if (newOverlay == null)
            {
                this.RemoveLayerOverlay(layer, overlay, invalidateRect);
            }
            else
            {
                if (newOverlay.IsCancellationRequested)
                {
                    throw new InvalidOperationException("Cannot set an overlay which is already cancelled");
                }
                this.overlays[layer] = newOverlay;
                this.documentRenderer.ReplaceLayerOverlay(layer, newOverlay);
                oldOverlay.Cancel();
                if (invalidateRect.HasValue)
                {
                    if (invalidateRect.Value.HasPositiveArea)
                    {
                        layer.Invalidate(invalidateRect.Value);
                    }
                }
                else
                {
                    RectInt32? nullable = (oldOverlay == null) ? null : new RectInt32?(oldOverlay.AffectedBounds);
                    RectInt32? nullable2 = (newOverlay == null) ? null : new RectInt32?(newOverlay.AffectedBounds);
                    RectInt32? nullable3 = RectInt32Util.Union(nullable, nullable2);
                    if (nullable3.HasValue && nullable3.Value.HasPositiveArea)
                    {
                        layer.Invalidate(nullable3.Value);
                    }
                }
            }
        }

        public static void SetIsHighQualityScalingEnabled(CanvasView canvasView, bool value)
        {
            canvasView.SetValue(IsHighQualityScalingEnabledProperty, BooleanUtil.GetBoxed(value));
        }

        public void SetLayerOverlay(Layer layer, DocumentLayerOverlay overlay, RectInt32? invalidateRect)
        {
            Validate.Begin().IsNotNull<Layer>(layer, "layer").IsNotNull<DocumentLayerOverlay>(overlay, "overlay").Check();
            base.VerifyAccess();
            if (overlay.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot set a layer overlay if it's already been cancelled");
            }
            if (this.document == null)
            {
                throw new InvalidOperationException("Cannot set a layer if there's no document");
            }
            if (!this.document.Layers.Contains(layer))
            {
                throw new InvalidOperationException("Cannot set a layer overlay for a layer that doesn't exist in this Document");
            }
            if (this.overlays.ContainsKey(layer))
            {
                throw new InvalidOperationException("that layer already has an overlay");
            }
            this.overlays.Add(layer, overlay);
            this.documentRenderer.SetLayerOverlay(layer, overlay);
            if (invalidateRect.HasValue)
            {
                if (invalidateRect.Value.HasPositiveArea)
                {
                    layer.Invalidate(invalidateRect.Value);
                }
            }
            else
            {
                layer.Invalidate(overlay.AffectedBounds);
            }
        }

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                base.VerifyAccess();
                if (value != this.document)
                {
                    if (this.document != null)
                    {
                        if (this.overlays.Any<KeyValuePair<Layer, DocumentLayerOverlay>>())
                        {
                            throw new InvalidOperationException("There are still registered layer overlays");
                        }
                        this.documentRenderer = null;
                        this.document.Invalidated -= new EventHandler<RectInt32InvalidatedEventArgs>(this.OnDocumentInvalidated);
                        this.document.Layers.Changing -= new EventHandler(this.OnDocumentLayersChanging);
                        this.document.Layers.Changed -= new EventHandler(this.OnDocumentLayersChanged);
                        foreach (CanvasView view in base.Owner.RegisteredViews)
                        {
                            this.OnViewUnregistered(view);
                        }
                        DisposableUtil.FreeContents<DocumentCanvasTileCache>(this.tileCaches);
                        this.tileCaches = null;
                        this.tileCachesRO = null;
                    }
                    this.document = value;
                    if (this.document != null)
                    {
                        this.documentRenderer = new PaintDotNet.Canvas.DocumentRenderer(this);
                        foreach (KeyValuePair<Layer, DocumentLayerOverlay> pair in this.overlays)
                        {
                            this.documentRenderer.SetLayerOverlay(pair.Key, pair.Value);
                        }
                        DocumentCanvasTileCache cache = new DocumentCanvasTileCache(this, this.documentRenderer, 7, 0, false);
                        int maxMipLevels = cache.TileMathHelper.MaxMipLevels;
                        this.tileCaches = new DocumentCanvasTileCache[maxMipLevels];
                        this.tileCachesHQ = new DocumentCanvasTileCache[maxMipLevels];
                        this.tileCaches[0] = cache;
                        this.tileCachesHQ[0] = cache;
                        for (int i = 1; i < maxMipLevels; i++)
                        {
                            this.tileCaches[i] = new DocumentCanvasTileCache(this, this.documentRenderer, 7 + i, i, false);
                            this.tileCachesHQ[i] = new DocumentCanvasTileCache(this, this.documentRenderer, 7 + i, i, true);
                        }
                        this.tileCachesRO = new ReadOnlyCollection<DocumentCanvasTileCache>(this.tileCaches);
                        this.tileCachesHQRO = new ReadOnlyCollection<DocumentCanvasTileCache>(this.tileCachesHQ);
                        foreach (CanvasView view2 in base.Owner.RegisteredViews)
                        {
                            this.OnViewRegistered(view2);
                        }
                        this.document.Invalidated += new EventHandler<RectInt32InvalidatedEventArgs>(this.OnDocumentInvalidated);
                        this.document.Layers.Changing += new EventHandler(this.OnDocumentLayersChanging);
                        this.document.Layers.Changed += new EventHandler(this.OnDocumentLayersChanged);
                    }
                    base.Invalidate();
                }
            }
        }

        internal PaintDotNet.Canvas.DocumentRenderer DocumentRenderer =>
            this.documentRenderer;

        internal PointDouble MouseLocation
        {
            get => 
                this.mouseLocation;
            set
            {
                base.VerifyAccess();
                this.mouseLocation = value;
            }
        }
    }
}

