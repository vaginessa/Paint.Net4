namespace PaintDotNet.Effects
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    internal sealed class EffectRendererWorkItemQueue : WorkItemQueue
    {
        private int activeThreadCount;
        private ManualResetEvent idleEvent;
        private readonly int maxThreadCount;
        private readonly ConcurrentQueue<Action> queue;
        private readonly object sync;
        private IDisposable threadCountToken;
        private long totalEnqueueCount;
        private long totalNotifyCount;

        public EffectRendererWorkItemQueue(WorkItemDispatcher dispatcher, WorkItemQueuePriority priority, int maxThreadCount) : base(dispatcher, priority)
        {
            this.sync = new object();
            this.maxThreadCount = maxThreadCount;
            this.queue = new ConcurrentQueue<Action>();
            this.idleEvent = new ManualResetEvent(true);
            MultithreadedWorkItemDispatcher dispatcher2 = dispatcher as MultithreadedWorkItemDispatcher;
            if (dispatcher2 != null)
            {
                this.threadCountToken = dispatcher2.UseThreadCount(maxThreadCount);
            }
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<IDisposable>(ref this.threadCountToken, disposing);
            base.Dispose(disposing);
        }

        public void Enqueue(Action workItem)
        {
            Validate.IsNotNull<Action>(workItem, "workItem");
            object sync = this.sync;
            lock (sync)
            {
                this.queue.Enqueue(workItem);
                this.totalEnqueueCount += 1L;
                this.idleEvent.Reset();
            }
            this.UpdateNotifyWorkItemsQueued();
        }

        public void Join()
        {
            this.idleEvent.WaitOne();
        }

        protected override void OnExecuteNextWorkItem()
        {
            Action action;
            object sync = this.sync;
            lock (sync)
            {
                if (!this.queue.TryDequeue(out action))
                {
                    throw new InternalErrorException();
                }
            }
            try
            {
                action();
            }
            catch (Exception exception)
            {
                if (!base.TryReportException(new WorkItemExceptionInfo<Action>(this, exception, action)))
                {
                    throw;
                }
            }
            finally
            {
                object obj3 = this.sync;
                lock (obj3)
                {
                    this.activeThreadCount--;
                }
                this.UpdateNotifyWorkItemsQueued();
            }
        }

        private void UpdateNotifyWorkItemsQueued()
        {
            int num;
            object sync = this.sync;
            lock (sync)
            {
                long num2 = this.totalEnqueueCount - this.totalNotifyCount;
                int num3 = this.maxThreadCount - this.activeThreadCount;
                num = (int) Math.Min((long) num3, num2);
                this.totalNotifyCount += num;
                this.activeThreadCount += num;
                if ((this.queue.Count == 0) && (this.activeThreadCount == 0))
                {
                    this.idleEvent.Set();
                }
            }
            if (num > 0)
            {
                base.NotifyWorkItemsQueued(num);
            }
        }

        public override int WorkItemCount
        {
            get
            {
                object sync = this.sync;
                lock (sync)
                {
                    return ((this.queue == null) ? 0 : Math.Min(this.maxThreadCount, this.queue.Count));
                }
            }
        }
    }
}

