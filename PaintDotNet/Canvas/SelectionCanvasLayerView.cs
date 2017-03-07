namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Functional;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SelectionCanvasLayerView : CanvasLayerView<SelectionCanvasLayerView, SelectionCanvasLayer>, ITrimmable
    {
        private static readonly SolidColorBrush blackBrush = SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.Black);
        private ConcurrentQueue<IDeviceBitmap> deviceBitmapPool;
        private static readonly object isPooledBitmapAttachedKey = new object();
        private bool isRedrawing;
        private bool isRedrawNeeded;
        private static readonly BitmapProperties maskBitmapProperties = new BitmapProperties(new Direct2DPixelFormat(DxgiFormat.A8_UNorm, AlphaMode.Premultiplied), 96f, 96f);
        private const float outlineStrokeWidthDip = 1f;
        private readonly float outlineStrokeWidthPx;
        private Action queuedBeginRedrawCallback;
        private WaitCallback redrawOnBackgroundThread;
        private SelectionRenderParameters redrawRenderParams;
        private IDirect2DFactory redrawThreadFactory;
        private SelectionGeometryCache redrawThreadGeometryCache;
        private RectDouble renderedCanvasBounds;
        private IDeviceBitmap[] renderedDashedOutlineDeviceBitmaps;
        private IBitmap<ColorAlpha8>[] renderedDashedOutlineMasks;
        private IDeviceBitmap renderedInteriorDeviceBitmap;
        private IBitmap<ColorAlpha8> renderedInteriorMask;
        private RectFloat renderedMaskSourceRect;
        private SelectionRenderParameters renderedRenderParams;
        private static readonly SolidColorBrush whiteBrush = SolidColorBrushCache.Get((ColorRgba128Float) ColorBgra.White);

        public SelectionCanvasLayerView(SelectionCanvasLayer owner, CanvasView canvasView) : base(owner, canvasView)
        {
            this.deviceBitmapPool = new ConcurrentQueue<IDeviceBitmap>();
            this.outlineStrokeWidthPx = 1f;
            this.queuedBeginRedrawCallback = new Action(this.QueuedBeginRedrawCallback);
            this.redrawOnBackgroundThread = new WaitCallback(this.RedrawOnBackgroundThread);
            CleanupService.RegisterTrimmableObject(this);
        }

        private void BeginRedraw()
        {
            base.VerifyAccess();
            if ((this.isRedrawing || (base.CanvasView == null)) || (base.CanvasView.RenderTarget == null))
            {
                this.isRedrawNeeded = true;
            }
            else
            {
                this.isRedrawNeeded = false;
                this.isRedrawing = true;
                this.redrawRenderParams = base.Owner.GetRenderParameters(base.CanvasView);
                if (!this.redrawRenderParams.IsOutlineEnabled && !this.redrawRenderParams.IsInteriorFilled)
                {
                    this.EndRedraw(RectDouble.Zero, RectFloat.Zero, null, null);
                }
                else
                {
                    WorkItemDispatcher.Default.Enqueue(new Action(this.RedrawOnBackgroundThread), WorkItemQueuePriority.High);
                }
            }
        }

        private bool CanContinueRedrawing(IBitmap<ColorAlpha8> interiorMask = null, IBitmap<ColorAlpha8>[] dashedOutlineMasks = null)
        {
            if (base.IsDisposed)
            {
                this.redrawThreadFactory = null;
                DisposableUtil.Free<SelectionGeometryCache>(ref this.redrawThreadGeometryCache);
                DisposableUtil.Free<IBitmap<ColorAlpha8>>(ref interiorMask);
                DisposableUtil.FreeContents<IBitmap<ColorAlpha8>>(dashedOutlineMasks);
                base.SyncContext.Post(delegate (object _) {
                    this.EndRedraw(RectDouble.Zero, RectFloat.Zero, null, null);
                });
                return false;
            }
            return true;
        }

        private void ClearDeviceBitmapPool()
        {
            IDeviceBitmap bitmap;
            while (this.deviceBitmapPool.TryDequeue(out bitmap))
            {
                bitmap.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<IBitmap<ColorAlpha8>>(ref this.renderedInteriorMask);
                DisposableUtil.FreeContents<IBitmap<ColorAlpha8>>(this.renderedDashedOutlineMasks);
                if (!this.isRedrawing)
                {
                    this.redrawThreadFactory = null;
                    DisposableUtil.Free<SelectionGeometryCache>(ref this.redrawThreadGeometryCache);
                }
                this.ClearDeviceBitmapPool();
            }
            base.Dispose(disposing);
        }

        private void EndRedraw(RectDouble canvasBounds, RectFloat maskSourceRect, IBitmap<ColorAlpha8> interiorMask, IBitmap<ColorAlpha8>[] dashedOutlineMasks)
        {
            base.VerifyAccess();
            if (base.IsDisposed)
            {
                DisposableUtil.Free<IBitmap<ColorAlpha8>>(ref interiorMask);
                DisposableUtil.FreeContents<IBitmap<ColorAlpha8>>(dashedOutlineMasks);
            }
            else
            {
                this.isRedrawing = false;
                base.CanvasView.Invalidate(this.renderedCanvasBounds);
                this.ReturnOrFreeDeviceBitmap(ref this.renderedInteriorDeviceBitmap);
                this.ReturnOrFreeDeviceBitmaps(this.renderedDashedOutlineDeviceBitmaps);
                this.renderedDashedOutlineDeviceBitmaps = null;
                DisposableUtil.Free<IBitmap<ColorAlpha8>>(ref this.renderedInteriorMask);
                this.renderedInteriorMask = interiorMask;
                DisposableUtil.FreeContents<IBitmap<ColorAlpha8>>(this.renderedDashedOutlineMasks);
                this.renderedDashedOutlineMasks = dashedOutlineMasks;
                this.renderedCanvasBounds = canvasBounds;
                this.renderedMaskSourceRect = maskSourceRect;
                this.renderedRenderParams = this.redrawRenderParams;
                this.redrawRenderParams = null;
                base.CanvasView.Invalidate(this.renderedCanvasBounds);
                if (this.isRedrawNeeded)
                {
                    this.isRedrawNeeded = false;
                    this.QueueBeginRedraw();
                }
            }
        }

        private void GetInitializedRenderBitmaps(IDeviceResourceFactory factory, out IDeviceBitmap interiorDeviceBitmap, out IDeviceBitmap blackDashedOutlineDeviceBitmap, out IDeviceBitmap whiteDashedOutlineDeviceBitmap)
        {
            interiorDeviceBitmap = this.InitializeDeviceBitmap(factory, this.renderedInteriorMask, ref this.renderedInteriorDeviceBitmap);
            blackDashedOutlineDeviceBitmap = null;
            whiteDashedOutlineDeviceBitmap = null;
            if (this.renderedDashedOutlineMasks != null)
            {
                int num = this.renderedRenderParams.IsOutlineAnimated ? base.Owner.GetOutlineDashOffset(base.CanvasView) : 0;
                int index = num % SelectionCanvasLayer.DashLength;
                int num3 = (num + (SelectionCanvasLayer.DashLength / 2)) % SelectionCanvasLayer.DashLength;
                if (this.renderedDashedOutlineDeviceBitmaps == null)
                {
                    this.renderedDashedOutlineDeviceBitmaps = new IDeviceBitmap[this.renderedDashedOutlineMasks.Length];
                }
                blackDashedOutlineDeviceBitmap = this.InitializeDeviceBitmap(factory, this.renderedDashedOutlineMasks[index], ref this.renderedDashedOutlineDeviceBitmaps[index]);
                whiteDashedOutlineDeviceBitmap = this.InitializeDeviceBitmap(factory, this.renderedDashedOutlineMasks[num3], ref this.renderedDashedOutlineDeviceBitmaps[num3]);
            }
        }

        private IDeviceBitmap GetOrCreateDeviceBitmap(IDeviceResourceFactory factory, SizeInt32 desiredPixelSize)
        {
            base.VerifyAccess();
            IDeviceBitmap result = null;
            int num = Int32Util.Pow2RoundUp(desiredPixelSize.Width);
            int num2 = Int32Util.Pow2RoundUp(desiredPixelSize.Height);
            int width = Math.Max(Math.Max(num, num2), 0x100);
            SizeInt32 num5 = new SizeInt32(width, width);
            while (this.deviceBitmapPool.TryDequeue(out result))
            {
                SizeInt32 pixelSize = result.PixelSize;
                if (((pixelSize.Width >= num5.Width) && (pixelSize.Width <= (num5.Width * 2))) && ((pixelSize.Height >= num5.Height) && (pixelSize.Height <= (num5.Height * 2))))
                {
                    break;
                }
                DisposableUtil.Free<IDeviceBitmap>(ref result);
            }
            if (result == null)
            {
                result = factory.CreateDeviceBitmap(desiredPixelSize, maskBitmapProperties);
                AttachedData.SetValue(result, isPooledBitmapAttachedKey, BooleanUtil.GetBoxed(true));
            }
            return result;
        }

        private IDeviceBitmap InitializeDeviceBitmap(IDeviceResourceFactory factory, IBitmap bitmap, ref IDeviceBitmap deviceBitmap)
        {
            if (bitmap == null)
            {
                this.ReturnOrFreeDeviceBitmap(ref deviceBitmap);
                return deviceBitmap;
            }
            if ((bitmap != null) && (deviceBitmap == null))
            {
                if (factory.IsSupported(RenderTargetType.Software, null, null, null))
                {
                    SizeInt32 num = bitmap.Size;
                    IBitmapLock bitmapLock = bitmap.Lock(BitmapLockOptions.Read);
                    deviceBitmap = factory.CreateSharedBitmap(bitmapLock, new BitmapProperties?(maskBitmapProperties));
                    return deviceBitmap;
                }
                SizeInt32 size = bitmap.Size;
                deviceBitmap = this.GetOrCreateDeviceBitmap(factory, size);
                deviceBitmap.CopyFromBitmap(new PointInt32?(PointInt32.Zero), bitmap, new RectInt32(PointInt32.Zero, size));
            }
            return deviceBitmap;
        }

        internal void InvalidateSelectionArea()
        {
            base.VerifyAccess();
            SelectionSnapshot selectionSnapshot = base.Owner.SelectionSnapshot;
            RectDouble fastMaxBounds = selectionSnapshot.FastMaxBounds;
            if (!selectionSnapshot.IsEmpty && !fastMaxBounds.IsEmpty)
            {
                double canvasHairWidth = base.CanvasView.CanvasHairWidth;
                double dx = this.outlineStrokeWidthPx * canvasHairWidth;
                RectDouble canvasRect = RectDouble.Inflate(fastMaxBounds, dx, dx);
                base.CanvasView.Invalidate(canvasRect);
            }
        }

        internal void NotifySelectionSnapshotInvalidated()
        {
            base.VerifyAccess();
            this.QueueBeginRedraw();
        }

        protected override void OnBeforeRender(RectFloat clipRect)
        {
            if (this.ShouldRender)
            {
                IRenderTarget renderTarget = base.CanvasView.RenderTarget;
                if (renderTarget != null)
                {
                    try
                    {
                        IDeviceBitmap bitmap;
                        IDeviceBitmap bitmap2;
                        IDeviceBitmap bitmap3;
                        this.GetInitializedRenderBitmaps(renderTarget, out bitmap, out bitmap2, out bitmap3);
                    }
                    catch (RecreateTargetException)
                    {
                    }
                }
            }
            base.OnBeforeRender(clipRect);
        }

        protected override void OnInvalidateDeviceResources()
        {
            DisposableUtil.Free<IDeviceBitmap>(ref this.renderedInteriorDeviceBitmap);
            DisposableUtil.FreeContents<IDeviceBitmap>(this.renderedDashedOutlineDeviceBitmaps);
            this.ClearDeviceBitmapPool();
            if ((!base.IsDisposed && !this.isRedrawing) && this.isRedrawNeeded)
            {
                this.QueueBeginRedraw();
            }
            base.OnInvalidateDeviceResources();
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (this.ShouldRender)
            {
                this.OnRenderSelection(dc, clipRect);
            }
            base.OnRender(dc, clipRect);
        }

        private void OnRenderSelection(IDrawingContext dc, RectFloat clipRect)
        {
            IDeviceBitmap bitmap;
            IDeviceBitmap bitmap2;
            IDeviceBitmap bitmap3;
            bool flag = false;
            try
            {
                this.GetInitializedRenderBitmaps(dc, out bitmap, out bitmap2, out bitmap3);
            }
            catch (RecreateTargetException)
            {
                bitmap = null;
                bitmap2 = null;
                bitmap3 = null;
            }
            if (((bitmap == null) || (bitmap2 == null)) || (bitmap3 == null))
            {
                flag = true;
            }
            else
            {
                using (dc.UseAntialiasMode(AntialiasMode.Aliased))
                {
                    Matrix3x2Float identity;
                    RectFloat renderedMaskSourceRect = this.renderedMaskSourceRect;
                    RectFloat renderedCanvasBounds = (RectFloat) this.renderedCanvasBounds;
                    Matrix3x2Double interimTransform = this.renderedRenderParams.SelectionSnapshot.InterimTransform;
                    VectorDouble scale = interimTransform.GetScale();
                    if (((interimTransform.HasInverse && (Math.Abs(scale.X) > 0.001)) && (Math.Abs(scale.Y) > 0.001)) && (base.Owner.SelectionSnapshot.GeometryVersion == this.renderedRenderParams.SelectionSnapshot.GeometryVersion))
                    {
                        Matrix3x2Double inverse = interimTransform.Inverse;
                        Matrix3x2Double num9 = base.Owner.Selection.GetInterimTransform();
                        Matrix3x2Double num10 = inverse * num9;
                        identity = (Matrix3x2Float) num10;
                    }
                    else
                    {
                        identity = Matrix3x2Float.Identity;
                    }
                    RectFloat num6 = (RectFloat) RectDouble.Inflate(renderedCanvasBounds, 1.0, 1.0);
                    RectFloat num7 = new RectFloat(PointFloat.Zero, (SizeFloat) this.renderedRenderParams.CanvasSize);
                    using (dc.UseAxisAlignedClip(num7, AntialiasMode.Aliased))
                    {
                        using (dc.UseTransformMultiply(identity, MatrixMultiplyOrder.Prepend))
                        {
                            IBrush cachedOrCreateResource = dc.GetCachedOrCreateResource<IBrush>(this.renderedRenderParams.InteriorBrush);
                            dc.FillOpacityMask(bitmap, cachedOrCreateResource, OpacityMaskContent.Graphics, new RectFloat?(renderedCanvasBounds), new RectFloat?(renderedMaskSourceRect));
                        }
                    }
                    using (dc.UseTransformMultiply(identity, MatrixMultiplyOrder.Prepend))
                    {
                        using (dc.UseAxisAlignedClip(num6, AntialiasMode.Aliased))
                        {
                            IBrush brush = dc.GetCachedOrCreateResource<IBrush>(blackBrush);
                            dc.FillOpacityMask(bitmap2, brush, OpacityMaskContent.Graphics, new RectFloat?(renderedCanvasBounds), new RectFloat?(renderedMaskSourceRect));
                            IBrush brush3 = dc.GetCachedOrCreateResource<IBrush>(whiteBrush);
                            dc.FillOpacityMask(bitmap3, brush3, OpacityMaskContent.Graphics, new RectFloat?(renderedCanvasBounds), new RectFloat?(renderedMaskSourceRect));
                        }
                    }
                }
                if (this.renderedRenderParams.ViewportCanvasBounds != base.CanvasView.ViewportCanvasBounds)
                {
                    flag = true;
                }
            }
            if (flag && !this.isRedrawing)
            {
                this.QueueBeginRedraw();
            }
        }

        void ITrimmable.Trim()
        {
            this.ClearDeviceBitmapPool();
        }

        public void QueueBeginRedraw()
        {
            PdnSynchronizationContext.Instance.EnsurePosted(this.queuedBeginRedrawCallback);
        }

        private void QueuedBeginRedrawCallback()
        {
            base.VerifyAccess();
            if (!base.IsDisposed)
            {
                this.BeginRedraw();
            }
        }

        private void RedrawOnBackgroundThread()
        {
            if (base.CheckAccess())
            {
                ExceptionUtil.ThrowInvalidOperationException();
            }
            if (this.redrawThreadFactory == null)
            {
                this.redrawThreadFactory = new Direct2DFactory(Direct2DFactoryType.MultiThreaded, DebugLevel.None);
            }
            if ((this.redrawThreadGeometryCache == null) || (this.redrawThreadGeometryCache.SelectionSnapshot != this.redrawRenderParams.SelectionSnapshot))
            {
                DisposableUtil.Free<SelectionGeometryCache>(ref this.redrawThreadGeometryCache);
                this.redrawThreadGeometryCache = new SelectionGeometryCache(this.redrawThreadFactory, this.redrawRenderParams.SelectionSnapshot);
            }
            double scaleRatio = this.redrawRenderParams.ScaleRatio;
            PointDouble location = this.redrawRenderParams.ViewportCanvasBounds.Location;
            SizeDouble viewportSize = this.redrawRenderParams.ViewportSize;
            SizeInt32 num5 = SizeDouble.Ceiling(viewportSize);
            RectDouble b = RectDouble.Inflate(CanvasCoordinateConversions.ConvertExtentToViewport(CanvasCoordinateConversions.ConvertCanvasToExtent(this.redrawRenderParams.SelectionSnapshot.GeometryList.Value.Bounds, scaleRatio), scaleRatio, location), (double) this.outlineStrokeWidthPx, (double) this.outlineStrokeWidthPx);
            RectDouble a = new RectDouble(PointDouble.Zero, viewportSize);
            RectDouble renderViewportBounds = RectDouble.Intersect(a, b);
            RectInt32 viewportRect = renderViewportBounds.Int32Bound;
            RectDouble extentRect = CanvasCoordinateConversions.ConvertViewportToExtent(viewportRect, scaleRatio, location);
            RectDouble renderCanvasBounds = CanvasCoordinateConversions.ConvertExtentToCanvas(extentRect, scaleRatio);
            if (!viewportRect.HasPositiveArea)
            {
                base.SyncContext.Post((SendOrPostCallback) (_ => this.EndRedraw(renderViewportBounds, RectFloat.Zero, null, null)));
            }
            else
            {
                IBitmap<ColorAlpha8> interiorMask;
                float scale;
                PointFloat offset;
                Result<IGeometry> lazyGeometry;
                IBitmap<ColorAlpha8>[] dashedOutlineMasks;
                RectFloat maskSourceRect;
                SelectionSnapshot selectionSnapshot = this.redrawRenderParams.SelectionSnapshot;
                SelectionGeometryCache redrawThreadGeometryCache = this.redrawThreadGeometryCache;
                bool flag = this.redrawRenderParams.SelectionRenderingQuality == SelectionRenderingQuality.Aliased;
                if (!flag)
                {
                    lazyGeometry = redrawThreadGeometryCache.Geometry;
                }
                else
                {
                    bool flag3 = selectionSnapshot.IsRectilinear.Value;
                    bool flag4 = selectionSnapshot.IsPixelated.Value;
                    if (flag3 & flag4)
                    {
                        lazyGeometry = redrawThreadGeometryCache.Geometry;
                    }
                    else
                    {
                        lazyGeometry = redrawThreadGeometryCache.PixelatedGeometry;
                    }
                }
                bool flag2 = !flag && this.redrawRenderParams.IsOutlineAntialiased;
                AntialiasMode antialiasMode = ((flag || !flag2) || selectionSnapshot.IsRectilinear.Value) ? AntialiasMode.Aliased : AntialiasMode.PerPrimitive;
                if (this.CanContinueRedrawing(null, null))
                {
                    scale = (float) this.redrawRenderParams.ScaleRatio;
                    float x = -((float) renderCanvasBounds.X);
                    float y = -((float) renderCanvasBounds.Y);
                    offset = new PointFloat(x, y);
                    interiorMask = BitmapAllocator.Alpha8.Allocate(viewportRect.Size, AllocationOptions.ZeroFillNotRequired);
                    try
                    {
                        RetryManager.RunMemorySensitiveOperation(delegate {
                            using (IDrawingContext context = DrawingContext.FromBitmap(this.redrawThreadFactory, interiorMask))
                            {
                                context.Clear(null);
                                IBrush interiorBrush = context.GetCachedOrCreateResource<IBrush>(whiteBrush);
                                this.RenderSelection(context, scale, offset, this.redrawRenderParams, lazyGeometry, interiorBrush, null, null, antialiasMode);
                            }
                        });
                    }
                    catch (OutOfMemoryException)
                    {
                    }
                    if (this.CanContinueRedrawing(interiorMask, null))
                    {
                        int num15;
                        dashedOutlineMasks = new IBitmap<ColorAlpha8>[SelectionCanvasLayer.DashLength];
                        for (int i = 0; i < dashedOutlineMasks.Length; i = num15)
                        {
                            if ((!this.redrawRenderParams.IsOutlineAnimated && (i != 0)) && (i != (SelectionCanvasLayer.DashLength / 2)))
                            {
                                dashedOutlineMasks[i] = null;
                            }
                            else
                            {
                                dashedOutlineMasks[i] = BitmapAllocator.Alpha8.Allocate(viewportRect.Size, AllocationOptions.ZeroFillNotRequired);
                                try
                                {
                                    RetryManager.RunMemorySensitiveOperation(delegate {
                                        using (IDrawingContext context = DrawingContext.FromBitmap(this.redrawThreadFactory, dashedOutlineMasks[i]))
                                        {
                                            context.Clear(null);
                                            StrokeStyle resourceSource = SelectionCanvasLayer.GetDashedStrokeStyle(i);
                                            IStrokeStyle outlineStrokeStyle = context.GetCachedOrCreateResource<IStrokeStyle>(resourceSource);
                                            IBrush cachedOrCreateResource = context.GetCachedOrCreateResource<IBrush>(whiteBrush);
                                            this.RenderSelection(context, scale, offset, this.redrawRenderParams, lazyGeometry, null, cachedOrCreateResource, outlineStrokeStyle, antialiasMode);
                                        }
                                    });
                                }
                                catch (OutOfMemoryException)
                                {
                                }
                                if (!this.CanContinueRedrawing(interiorMask, dashedOutlineMasks))
                                {
                                    return;
                                }
                            }
                            num15 = i + 1;
                        }
                        maskSourceRect = new RectFloat(PointFloat.Zero, interiorMask.Size);
                        base.SyncContext.Post(_ => this.EndRedraw(renderCanvasBounds, maskSourceRect, interiorMask, dashedOutlineMasks), null);
                    }
                }
            }
        }

        private void RedrawOnBackgroundThread(object ignored)
        {
            this.RedrawOnBackgroundThread();
        }

        private void RenderSelection(IDrawingContext dc, float scale, PointFloat offset, SelectionRenderParameters renderParams, Result<IGeometry> lazyGeometry, IBrush interiorBrush, IBrush outlineBrush, IStrokeStyle outlineStrokeStyle, AntialiasMode antialiasMode)
        {
            using (dc.UseScaleTransform(scale, scale, MatrixMultiplyOrder.Prepend))
            {
                using (dc.UseTranslateTransform(offset.X, offset.Y, MatrixMultiplyOrder.Prepend))
                {
                    if (renderParams.IsInteriorFilled && (interiorBrush != null))
                    {
                        using (dc.UseAntialiasMode(AntialiasMode.Aliased))
                        {
                            dc.FillGeometry(lazyGeometry.Value, interiorBrush, null);
                        }
                    }
                    if (renderParams.IsOutlineEnabled && (outlineBrush != null))
                    {
                        float strokeWidth = this.outlineStrokeWidthPx / scale;
                        using (dc.UseAntialiasMode(antialiasMode))
                        {
                            using (dc.UseTranslateTransform(0.5f, 0.5f, MatrixMultiplyOrder.Append))
                            {
                                dc.DrawGeometry(lazyGeometry.Value, outlineBrush, strokeWidth, outlineStrokeStyle);
                            }
                        }
                    }
                }
            }
        }

        private void ReturnOrFreeDeviceBitmap(ref IDeviceBitmap deviceBitmap)
        {
            object obj2;
            if (((deviceBitmap != null) && AttachedData.TryGetValue(deviceBitmap, isPooledBitmapAttachedKey, out obj2)) && ((bool) obj2))
            {
                this.deviceBitmapPool.Enqueue(deviceBitmap);
                deviceBitmap = null;
            }
            DisposableUtil.Free<IDeviceBitmap>(ref deviceBitmap);
        }

        private void ReturnOrFreeDeviceBitmaps(IDeviceBitmap[] deviceBitmaps)
        {
            if (deviceBitmaps != null)
            {
                for (int i = 0; i < deviceBitmaps.Length; i++)
                {
                    this.ReturnOrFreeDeviceBitmap(ref deviceBitmaps[i]);
                }
            }
        }

        private bool ShouldRender =>
            ((((this.renderedRenderParams != null) && (this.renderedRenderParams.IsOutlineEnabled || this.renderedRenderParams.IsInteriorFilled)) && ((base.Owner != null) && (base.Owner.SelectionSnapshot != null))) && !base.Owner.SelectionSnapshot.IsEmpty);
    }
}

