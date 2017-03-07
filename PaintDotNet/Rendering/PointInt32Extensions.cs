namespace PaintDotNet.Rendering
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class PointInt32Extensions
    {
        public static PointInt32 OffsetCopy(this PointInt32 pt, int dx, int dy) => 
            new PointInt32(pt.X + dx, pt.Y + dy);
    }
}

