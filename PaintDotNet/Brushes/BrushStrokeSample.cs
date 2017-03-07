namespace PaintDotNet.Brushes
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct BrushStrokeSample
    {
        private PointDouble center;
        private double stampScale;
        public PointDouble Center =>
            this.center;
        public double StampScale =>
            this.stampScale;
        public RectDouble GetBounds(SizeDouble stampSize)
        {
            SizeDouble size = new SizeDouble(stampSize.Width * this.stampScale, stampSize.Height * this.stampScale);
            return RectDouble.FromCenter(this.center, size);
        }

        public BrushStrokeSample(PointDouble center, double stampScale)
        {
            this.center = center;
            this.stampScale = stampScale;
        }
    }
}

