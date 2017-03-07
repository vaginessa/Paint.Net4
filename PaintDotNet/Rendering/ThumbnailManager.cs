namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class ThumbnailManager : IDisposable, IIsDisposed
    {
        private bool disposed;
        private volatile bool quitRenderThread;
        private ManualResetEvent renderingInactive;
        private Deque<Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int>> renderQueue;
        private Thread renderThread;
        private ISynchronizationContext syncContext;
        private List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>> thumbnailReadyInvokeList = new List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>>();
        private int updateLatency;
        private object updateLock;

        public ThumbnailManager(ISynchronizationContext syncContext)
        {
            int logicalCpuCount = Processor.LogicalCpuCount;
            int num2 = 50;
            int num3 = 320;
            while ((logicalCpuCount > 0) && (num3 > 0))
            {
                num3 = num3 >> 1;
                logicalCpuCount--;
            }
            this.updateLatency = num2 + num3;
            this.syncContext = syncContext.CreateRef();
            this.updateLock = new object();
            this.quitRenderThread = false;
            this.renderQueue = new Deque<Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int>>();
            this.renderingInactive = new ManualResetEvent(true);
            this.renderThread = new Thread(new ThreadStart(this.RenderThread));
            this.renderThread.IsBackground = true;
            this.renderThread.Name = "ThumbnailManager";
            this.renderThread.Start();
        }

        public void ClearQueue()
        {
            object updateLock = this.updateLock;
            lock (updateLock)
            {
                this.renderQueue.Clear();
            }
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
                this.quitRenderThread = true;
                object updateLock = this.updateLock;
                lock (updateLock)
                {
                    Monitor.Pulse(this.updateLock);
                }
                if (this.renderThread != null)
                {
                    this.renderThread.Join();
                    this.renderThread = null;
                }
            }
            DisposableUtil.Free<ManualResetEvent>(ref this.renderingInactive, disposing);
            DisposableUtil.Free<ISynchronizationContext>(ref this.syncContext, disposing);
        }

        private void DrainThumbnailReadyInvokeList()
        {
            List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>> list = null;
            List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>> thumbnailReadyInvokeList = this.thumbnailReadyInvokeList;
            lock (thumbnailReadyInvokeList)
            {
                list = this.thumbnailReadyInvokeList;
                this.thumbnailReadyInvokeList = new List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>>();
            }
            foreach (Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>> tuple in list)
            {
                tuple.Item1(tuple.Item2, tuple.Item3);
                tuple.Item3.Return();
            }
        }

        ~ThumbnailManager()
        {
            this.Dispose(false);
        }

        private bool OnThumbnailReady(IThumbnailProvider dw, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>> callback, ISurface<ColorBgra> thumb)
        {
            ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>> args = ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>.Get(Tuple.Create<IThumbnailProvider, ISurface<ColorBgra>>(dw, thumb));
            List<Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>> thumbnailReadyInvokeList = this.thumbnailReadyInvokeList;
            lock (thumbnailReadyInvokeList)
            {
                this.thumbnailReadyInvokeList.Add(new Tuple<ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, object, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>>(callback, this, args));
            }
            try
            {
                this.syncContext.Post(delegate (object _) {
                    this.DrainThumbnailReadyInvokeList();
                }, null);
                return true;
            }
            catch (Exception exception)
            {
                if (!(exception is ObjectDisposedException) && !(exception is InvalidOperationException))
                {
                    throw;
                }
                return false;
            }
        }

        public void QueueThumbnailUpdate(IThumbnailProvider updateMe, int thumbSideLength, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>> callback)
        {
            if (thumbSideLength < 1)
            {
                throw new ArgumentOutOfRangeException("thumbSideLength", "must be greater than or equal to 1");
            }
            object updateLock = this.updateLock;
            lock (updateLock)
            {
                this.RemoveFromQueue(updateMe);
                Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int> item = new Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int>(updateMe, callback, thumbSideLength);
                if (this.renderQueue.Any<Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int>>())
                {
                    Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int> tuple2 = this.renderQueue.PeekBack();
                    if (item.Equals(tuple2))
                    {
                        this.renderQueue.DequeueBack();
                    }
                }
                this.renderQueue.Enqueue(item);
                Monitor.Pulse(this.updateLock);
            }
        }

        public bool RemoveFromQueue(IThumbnailProvider nukeMe)
        {
            if (nukeMe == null)
            {
                return false;
            }
            object updateLock = this.updateLock;
            lock (updateLock)
            {
                bool flag2 = false;
                for (int i = 0; i < this.renderQueue.Count; i++)
                {
                    if ((this.renderQueue[i] != null) && nukeMe.Equals(this.renderQueue[i].Item1))
                    {
                        this.renderQueue[i] = null;
                        flag2 = true;
                    }
                }
                return flag2;
            }
        }

        private void RenderThread()
        {
            try
            {
                while (this.RenderThreadLoop())
                {
                }
            }
            finally
            {
                this.renderingInactive.Set();
            }
        }

        private bool RenderThreadLoop()
        {
            Tuple<IThumbnailProvider, ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>, int> tuple = null;
            object updateLock = this.updateLock;
            lock (updateLock)
            {
                if (!this.quitRenderThread)
                {
                    goto Label_0043;
                }
                return false;
            Label_0025:
                Monitor.Wait(this.updateLock);
                if (this.quitRenderThread)
                {
                    return false;
                }
            Label_0043:
                if (this.renderQueue.Count == 0)
                {
                    goto Label_0025;
                }
                this.renderingInactive.Reset();
                while ((this.renderQueue.Count > 0) && (this.renderQueue.Peek() == null))
                {
                    this.renderQueue.Dequeue();
                }
                if (this.renderQueue.Count > 0)
                {
                    tuple = this.renderQueue.Dequeue();
                }
            }
            if (tuple != null)
            {
                Thread.Sleep(this.updateLatency);
            }
            bool flag = true;
            object obj3 = this.updateLock;
            lock (obj3)
            {
                if (this.quitRenderThread)
                {
                    return false;
                }
                if (((this.renderQueue.Count > 0) && (tuple != null)) && tuple.Equals(this.renderQueue.Peek()))
                {
                    flag = false;
                }
            }
            if (tuple == null)
            {
                flag = false;
            }
            if (flag)
            {
                try
                {
                    ISurface<ColorBgra> surface;
                    Thread.Sleep(this.updateLatency);
                    using (IRenderer<ColorBgra> renderer = tuple.Item1.CreateThumbnailRenderer(tuple.Item3))
                    {
                        surface = renderer.ToCancellable<ColorBgra>(() => this.quitRenderThread).Parallelize<ColorBgra>(TilingStrategy.HorizontalSlices, 0, WorkItemQueuePriority.Lowest).ToSurface();
                    }
                    bool flag5 = false;
                    object obj4 = this.updateLock;
                    lock (obj4)
                    {
                        if (this.quitRenderThread)
                        {
                            surface.Dispose();
                            surface = null;
                            return false;
                        }
                        if ((this.renderQueue.Count > 0) && tuple.Equals(this.renderQueue.Peek()))
                        {
                            flag5 = true;
                        }
                    }
                    if (!flag5)
                    {
                        flag5 = !this.OnThumbnailReady(tuple.Item1, tuple.Item2, surface);
                    }
                    if (flag5)
                    {
                        surface.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    if (!(exception is OperationCanceledException))
                    {
                        AggregateException exception2 = exception as AggregateException;
                    }
                }
            }
            this.renderingInactive.Set();
            return true;
        }

        public bool IsDisposed =>
            this.disposed;

        public int UpdateLatency
        {
            get => 
                this.updateLatency;
            set
            {
                this.updateLatency = value;
            }
        }
    }
}

