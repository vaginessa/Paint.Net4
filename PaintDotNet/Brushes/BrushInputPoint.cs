namespace PaintDotNet.Brushes
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct BrushInputPoint
    {
        private PointDouble location;
        public PointDouble Location =>
            this.location;
        public BrushInputPoint(PointDouble location)
        {
            this.location = location;
        }
    }
}

