namespace PaintDotNet.Canvas
{
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CompareTileOffsetsByDistance : IComparer<PointInt32>
    {
        private PointInt32 comparand;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDistance(PointInt32 x)
        {
            int num = x.X - this.comparand.X;
            int num2 = x.Y - this.comparand.Y;
            return ((num * num) + (num2 * num2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(PointInt32 x, PointInt32 y)
        {
            int distance = this.GetDistance(x);
            int num2 = this.GetDistance(y);
            return distance.CompareTo(num2);
        }

        public CompareTileOffsetsByDistance(PointInt32 comparand)
        {
            this.comparand = comparand;
        }
    }
}

