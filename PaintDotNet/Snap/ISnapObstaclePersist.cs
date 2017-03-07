namespace PaintDotNet.Snap
{
    using PaintDotNet.Rendering;
    using System;

    internal interface ISnapObstaclePersist
    {
        RectInt32 Bounds { get; set; }

        HorizontalSnapEdge HorizontalEdge { get; set; }

        bool IsDataAvailable { get; }

        bool IsSnapped { get; set; }

        PointInt32 Offset { get; set; }

        string SnappedToName { get; set; }

        VerticalSnapEdge VerticalEdge { get; set; }
    }
}

