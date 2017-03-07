namespace PaintDotNet.Imaging
{
    using PaintDotNet;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class ClippedBitmapSource<TPixel> : RefTrackedObject, IBitmapSource<TPixel>, IBitmapSource, IImagingObject, IObjectRef, IDisposable, IIsDisposed where TPixel: struct, INaturalPixelInfo
    {
        private IBitmapSource<TPixel> source;
        private RectInt32 sourceRect;

        public ClippedBitmapSource(IBitmapSource<TPixel> source, RectInt32 sourceRect)
        {
            Validate.Begin().IsNotNull<IBitmapSource<TPixel>>(source, "source").Check().IsTrue(source.Bounds().Contains(sourceRect), "source.Bounds().Contains(sourceRect)").Check();
            this.source = source.CreateRef<TPixel>();
            this.sourceRect = sourceRect;
        }

        public void CopyPixels(RectInt32? srcRect, int bufferStride, int bufferSize, IntPtr buffer)
        {
            RectInt32? nullable = srcRect;
            RectInt32 rect = nullable.HasValue ? nullable.GetValueOrDefault() : this.sourceRect;
            RectInt32 num2 = RectInt32.Offset(rect, this.sourceRect.Location);
            if (!this.source.Bounds().Contains(num2))
            {
                ExceptionUtil.ThrowArgumentException();
            }
            this.source.CopyPixels(new RectInt32?(num2), bufferStride, bufferSize, buffer);
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<IBitmapSource<TPixel>>(ref this.source, disposing);
            base.Dispose(disposing);
        }

        public IPalette Palette =>
            this.source.Palette;

        public PaintDotNet.Imaging.PixelFormat PixelFormat
        {
            get
            {
                TPixel local = default(TPixel);
                return local.PixelFormat;
            }
        }

        public VectorDouble Resolution =>
            this.source.Resolution;

        public SizeInt32 Size =>
            this.sourceRect.Size;
    }
}

