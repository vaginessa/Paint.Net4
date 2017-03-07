namespace PaintDotNet.Brushes
{
    using PaintDotNet.Functional;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class BrushStamp
    {
        private LazyResult<IBitmap<ColorAlpha8>> lazyMaskBitmap;

        protected BrushStamp(SizeInt32 size, double opacity, bool antialiased)
        {
            this.Size = size;
            this.Opacity = opacity;
            this.Antialiased = antialiased;
            this.lazyMaskBitmap = new LazyResult<IBitmap<ColorAlpha8>, int>(_ => this.CreateMaskBitmap(), 0);
        }

        private IBitmap<ColorAlpha8> CreateMaskBitmap()
        {
            IBitmap<ColorAlpha8> dstMask = BitmapAllocator.Alpha8.Allocate(this.Size, AllocationOptions.Default);
            this.OnRender(dstMask);
            IBitmapLock<ColorAlpha8> keepAlive = dstMask.Lock<ColorAlpha8>(BitmapLockOptions.Read);
            return new SharedBitmap<ColorAlpha8>(keepAlive, keepAlive.Size, keepAlive.Scan0, keepAlive.Stride, 96.0, 96.0);
        }

        protected abstract void OnRender(IBitmap<ColorAlpha8> dstMask);

        public bool Antialiased { get; private set; }

        public IBitmap<ColorAlpha8> MaskBitmap =>
            this.lazyMaskBitmap.Value;

        public double Opacity { get; private set; }

        public SizeInt32 Size { get; private set; }
    }
}

