namespace PaintDotNet
{
    using System;

    [Flags]
    public enum SelectionChangeFlags
    {
        All = 0x3f,
        BaseGeometry = 1,
        ClipRectangle = 0x20,
        ContinuationCombineMode = 4,
        ContinuationGeometry = 2,
        CumulativeTransform = 0x10,
        InterimTransform = 8,
        None = 0
    }
}

