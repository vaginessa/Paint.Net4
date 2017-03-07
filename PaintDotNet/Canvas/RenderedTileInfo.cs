namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RenderedTileInfo
    {
        private IBitmap<ColorPbgra32> buffer;
        private bool completed;
        private Exception error;
        public IBitmap<ColorPbgra32> Buffer =>
            this.buffer;
        public bool Completed =>
            this.completed;
        public Exception Error =>
            this.error;
        public RenderedTileInfo(IBitmap<ColorPbgra32> buffer, bool completed, Exception error)
        {
            if ((buffer == null) & completed)
            {
                throw new PaintDotNet.InternalErrorException("buffer == null but completed = true");
            }
            this.buffer = buffer;
            this.completed = completed;
            this.error = error;
        }
    }
}

