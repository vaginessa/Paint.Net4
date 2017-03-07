namespace PaintDotNet.Effects
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class BackgroundEffectRenderer : IDisposable
    {
        private volatile bool aborted;
        private IRenderer<ColorAlpha8> clipMaskRenderer;
        private volatile bool disposed;
        private RenderArgs dstArgs;
        private Effect effect;
        private EffectConfigToken effectToken;
        private EffectConfigToken effectTokenCopy;
        private SynchronizedList<Exception> exceptions = new SynchronizedList<Exception>(new SegmentedList<Exception>());
        private PdnRegion renderRegion;
        private RenderArgs srcArgs;
        private Thread thread;
        private ManualResetEvent threadInitialized;
        private EffectRendererWorkItemQueue threadPool;
        private volatile bool threadShouldStop;
        private int tileCount;
        private PdnRegion[] tilePdnRegions;
        private Rectangle[][] tileRegions;
        private int workerThreads;

        [field: CompilerGenerated]
        public event EventHandler FinishedRendering;

        [field: CompilerGenerated]
        public event RenderedTileEventHandler RenderedTile;

        [field: CompilerGenerated]
        public event EventHandler StartingRendering;

        public BackgroundEffectRenderer(Effect effect, EffectConfigToken effectToken, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion renderRegion, IRenderer<ColorAlpha8> clipMaskRenderer, int tileCount, int workerThreads)
        {
            this.effect = effect;
            this.effectToken = effectToken;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;
            this.renderRegion = renderRegion;
            this.renderRegion.Intersect(dstArgs.Bounds);
            this.tileCount = tileCount;
            if (effect.CheckForEffectFlags(EffectFlags.None | EffectFlags.SingleRenderCall))
            {
                this.tileCount = 1;
            }
            this.tileRegions = this.SliceUpRegion(renderRegion, this.tileCount, dstArgs.Bounds);
            this.tilePdnRegions = new PdnRegion[this.tileRegions.Length];
            for (int i = 0; i < this.tileRegions.Length; i++)
            {
                this.tilePdnRegions[i] = PdnRegion.FromRectangles(this.tileRegions[i]);
            }
            this.workerThreads = workerThreads;
            if (effect.CheckForEffectFlags(EffectFlags.None | EffectFlags.SingleThreaded))
            {
                this.workerThreads = 1;
            }
            this.clipMaskRenderer = clipMaskRenderer;
            this.threadPool = new EffectRendererWorkItemQueue(WorkItemDispatcher.Default, WorkItemQueuePriority.Normal, workerThreads);
        }

        public void Abort()
        {
            if (this.thread != null)
            {
                this.threadShouldStop = true;
                if (this.effect != null)
                {
                    try
                    {
                        this.effect.SignalCancelRequest();
                    }
                    catch (Exception)
                    {
                    }
                }
                this.Join();
                this.threadPool.Join();
            }
        }

        public void AbortAsync()
        {
            this.threadShouldStop = true;
            Effect effect = this.effect;
            if (effect != null)
            {
                try
                {
                    effect.SignalCancelRequest();
                }
                catch (Exception)
                {
                }
            }
        }

        private Rectangle[] ConsolidateRects(Rectangle[] scans)
        {
            if (scans.Length == 0)
            {
                return Array.Empty<Rectangle>();
            }
            SegmentedList<Rectangle> items = new SegmentedList<Rectangle>();
            int num = 0;
            items.Add(scans[0]);
            for (int i = 1; i < scans.Length; i++)
            {
                Rectangle rectangle = items[num];
                if (scans[i].Left == rectangle.Left)
                {
                    rectangle = items[num];
                    if (scans[i].Right == rectangle.Right)
                    {
                        rectangle = items[num];
                        if (scans[i].Top == rectangle.Bottom)
                        {
                            Rectangle rectangle2 = items[num];
                            rectangle = items[num];
                            rectangle2.Height = scans[i].Bottom - rectangle.Top;
                            items[num] = rectangle2;
                            continue;
                        }
                    }
                }
                items.Add(scans[i]);
                num = items.Count - 1;
            }
            return items.ToArrayEx<Rectangle>();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.disposed = true;
            if (disposing)
            {
                DisposableUtil.Free<RenderArgs>(ref this.srcArgs);
                DisposableUtil.Free<RenderArgs>(ref this.dstArgs);
                DisposableUtil.Free<EffectRendererWorkItemQueue>(ref this.threadPool);
            }
        }

        private void DrainExceptions()
        {
            if (this.exceptions.Count > 0)
            {
                Exception innerException = this.exceptions[0];
                this.exceptions.Clear();
                throw new WorkerThreadException("Worker thread threw an exception", innerException);
            }
        }

        ~BackgroundEffectRenderer()
        {
            this.Dispose(false);
        }

        private static Scanline[] GetRegionScanlines(Rectangle[] region)
        {
            int num = 0;
            for (int i = 0; i < region.Length; i++)
            {
                num += region[i].Height;
            }
            if (num == 0)
            {
                return Array.Empty<Scanline>();
            }
            Scanline[] scanlineArray = new Scanline[num];
            int index = 0;
            foreach (Rectangle rectangle in region)
            {
                for (int j = 0; j < rectangle.Height; j++)
                {
                    scanlineArray[index] = new Scanline(rectangle.X, rectangle.Y + j, rectangle.Width);
                    index++;
                }
            }
            return scanlineArray;
        }

        public void Join()
        {
            this.thread.Join();
            this.DrainExceptions();
        }

        private void OnFinishedRendering()
        {
            this.FinishedRendering.Raise(this);
        }

        private void OnRenderedTile(RenderedTileEventArgs e)
        {
            if (this.RenderedTile != null)
            {
                this.RenderedTile(this, e);
            }
        }

        private void OnStartingRendering()
        {
            this.StartingRendering.Raise(this);
        }

        private static unsafe void RenderWithClipMask(Effect effect, EffectConfigToken token, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, IRenderer<ColorAlpha8> clipMaskRenderer)
        {
            effect.Render(token, dstArgs, srcArgs, rois);
            if (!effect.IsCancelRequested && (clipMaskRenderer != null))
            {
                RectInt32 bounds = RectangleUtil.Bounds(rois).ToRectInt32();
                if (bounds.HasPositiveArea)
                {
                    using (ISurface<ColorAlpha8> surface = clipMaskRenderer.UseTileOrToSurface(bounds))
                    {
                        int width = bounds.Width;
                        int height = bounds.Height;
                        int left = bounds.Left;
                        int top = bounds.Top;
                        int bottom = bounds.Bottom;
                        int stride = dstArgs.Surface.Stride;
                        int num8 = srcArgs.Surface.Stride;
                        int num9 = surface.Stride;
                        ColorBgra* pointAddress = dstArgs.Surface.GetPointAddress(left, top);
                        ColorBgra* bgraPtr2 = srcArgs.Surface.GetPointAddress(left, top);
                        byte* numPtr = (byte*) surface.Scan0;
                        for (int i = height; i > 0; i--)
                        {
                            ColorBgra.Underwrite(bgraPtr2, pointAddress, numPtr, width);
                            pointAddress += stride;
                            bgraPtr2 += num8;
                            numPtr += num9;
                        }
                    }
                }
            }
        }

        private static Rectangle[] ScanlinesToRectangles(Scanline[] scans, int startIndex, int length)
        {
            if (length == 0)
            {
                return Array.Empty<Rectangle>();
            }
            Rectangle[] rectangleArray = new Rectangle[length];
            for (int i = 0; i < length; i++)
            {
                Scanline scanline = scans[i + startIndex];
                rectangleArray[i] = new Rectangle(scanline.X, scanline.Y, scanline.Length, 1);
            }
            return rectangleArray;
        }

        private Rectangle[][] SliceUpRegion(PdnRegion region, int sliceCount, Rectangle layerBounds)
        {
            if (sliceCount <= 0)
            {
                throw new ArgumentOutOfRangeException("sliceCount");
            }
            Rectangle[][] rectangleArray = new Rectangle[sliceCount][];
            Scanline[] regionScanlines = GetRegionScanlines(region.GetRegionScansReadOnlyInt());
            for (int i = 0; i < sliceCount; i++)
            {
                int num2 = (regionScanlines.Length * i) / sliceCount;
                int num3 = Math.Min(regionScanlines.Length, (regionScanlines.Length * (i + 1)) / sliceCount);
                if (sliceCount > 1)
                {
                    switch (i)
                    {
                        case 0:
                            num3 = Math.Min(num3, num2 + 1);
                            break;

                        case 1:
                            num2 = Math.Min(num2, 1);
                            break;
                    }
                }
                Rectangle[] scans = ScanlinesToRectangles(regionScanlines, num2, num3 - num2);
                for (int j = 0; j < scans.Length; j++)
                {
                    scans[j].Intersect(layerBounds);
                }
                rectangleArray[i] = this.ConsolidateRects(scans);
            }
            return rectangleArray;
        }

        public void Start()
        {
            this.Abort();
            this.aborted = false;
            if (this.effectToken != null)
            {
                try
                {
                    this.effectTokenCopy = (EffectConfigToken) this.effectToken.Clone();
                }
                catch (Exception exception)
                {
                    this.exceptions.Add(exception);
                    this.effectTokenCopy = null;
                }
            }
            this.threadShouldStop = false;
            this.OnStartingRendering();
            this.thread = new Thread(new ThreadStart(this.ThreadFunction));
            this.thread.Name = "BackgroundEffectRenderer";
            this.threadInitialized = new ManualResetEvent(false);
            this.thread.Start();
            this.threadInitialized.WaitOne();
            this.threadInitialized.Close();
            this.threadInitialized = null;
        }

        private void ThreadFunction()
        {
            if (this.srcArgs.Surface.Scan0.MaySetAllowWrites)
            {
                this.srcArgs.Surface.Scan0.AllowWrites = false;
            }
            try
            {
                this.threadInitialized.Set();
                this.effect.SetRenderInfo(this.effectTokenCopy, this.dstArgs, this.srcArgs);
                RendererContext context = new RendererContext(this);
                if (this.threadShouldStop)
                {
                    this.effect.SignalCancelRequest();
                }
                else if (this.tileCount > 0)
                {
                    context.RenderNextTile(this.effectTokenCopy);
                }
                WaitCallback rcwc = new WaitCallback(context.RenderNextTileProc);
                for (int i = 0; i < this.tileCount; i++)
                {
                    if (this.threadShouldStop)
                    {
                        this.effect.SignalCancelRequest();
                        break;
                    }
                    EffectConfigToken token = (this.effectTokenCopy == null) ? null : this.effectTokenCopy.CloneT<EffectConfigToken>();
                    this.threadPool.Enqueue(() => rcwc(token));
                }
                this.threadPool.Join();
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
            finally
            {
                EffectRendererWorkItemQueue threadPool = this.threadPool;
                if (!this.disposed && (threadPool != null))
                {
                    try
                    {
                        threadPool.Join();
                    }
                    catch (Exception)
                    {
                    }
                }
                this.OnFinishedRendering();
                RenderArgs srcArgs = this.srcArgs;
                if (srcArgs != null)
                {
                    Surface surface = srcArgs.Surface;
                    if (surface != null)
                    {
                        MemoryBlock block = surface.Scan0;
                        if (((block != null) && !this.disposed) && block.MaySetAllowWrites)
                        {
                            try
                            {
                                block.AllowWrites = true;
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        }
                    }
                }
            }
        }

        public bool Aborted =>
            this.aborted;

        private sealed class RendererContext
        {
            private BackgroundEffectRenderer ber;
            private int nextTileIndex = -1;

            public RendererContext(BackgroundEffectRenderer ber)
            {
                this.ber = ber;
            }

            private void RendererLoop(EffectConfigToken token)
            {
                try
                {
                    while (this.RenderNextTile(token))
                    {
                    }
                }
                catch (Exception exception)
                {
                    this.ber.exceptions.Add(exception);
                }
            }

            public void RendererThreadProc(object token)
            {
                if (token == null)
                {
                    this.RendererLoop(null);
                }
                else
                {
                    this.RendererLoop((EffectConfigToken) token);
                }
            }

            public bool RenderNextTile(EffectConfigToken token)
            {
                if (this.ber.threadShouldStop)
                {
                    this.ber.effect.SignalCancelRequest();
                    this.ber.aborted = true;
                    return false;
                }
                int num = this.ber.tileCount - 1;
                int tileIndex = Interlocked.Increment(ref this.nextTileIndex);
                if (tileIndex > num)
                {
                    return false;
                }
                this.RenderTile(token, tileIndex);
                return true;
            }

            public void RenderNextTileProc(object token)
            {
                this.RenderNextTile((EffectConfigToken) token);
            }

            private void RenderTile(EffectConfigToken token, int tileIndex)
            {
                Rectangle[] rois = this.ber.tileRegions[tileIndex];
                BackgroundEffectRenderer.RenderWithClipMask(this.ber.effect, token, this.ber.dstArgs, this.ber.srcArgs, rois, this.ber.clipMaskRenderer);
                PdnRegion renderedRegion = this.ber.tilePdnRegions[tileIndex];
                if (!this.ber.threadShouldStop)
                {
                    this.ber.OnRenderedTile(new RenderedTileEventArgs(renderedRegion, this.ber.tileCount, tileIndex));
                }
            }
        }
    }
}

