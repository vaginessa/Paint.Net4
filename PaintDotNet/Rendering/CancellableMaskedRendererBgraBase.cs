namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Threading;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class CancellableMaskedRendererBgraBase : MaskedRendererBgraBase, ICancellable
    {
        private int isCancellationRequested;

        protected CancellableMaskedRendererBgraBase(int width, int height, bool hasContentMask) : base(width, height, hasContentMask)
        {
        }

        public void Cancel()
        {
            if (Interlocked.Exchange(ref this.isCancellationRequested, 1) == 0)
            {
                this.OnCancelled();
            }
        }

        protected virtual void OnCancelled()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfCancellationRequested()
        {
            if (this.IsCancellationRequested)
            {
                ExceptionUtil.ThrowOperationCanceledException();
            }
        }

        public bool IsCancellationRequested =>
            (this.isCancellationRequested == 1);
    }
}

