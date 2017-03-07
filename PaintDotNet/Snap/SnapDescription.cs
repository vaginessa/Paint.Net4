namespace PaintDotNet.Snap
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class SnapDescription
    {
        private HorizontalSnapEdge horizontalEdge;
        private SnapObstacle snappedTo;
        private VerticalSnapEdge verticalEdge;
        private int xOffset;
        private int yOffset;

        public SnapDescription(SnapObstacle snappedTo, HorizontalSnapEdge horizontalEdge, VerticalSnapEdge verticalEdge, PointInt32 offset) : this(snappedTo, horizontalEdge, verticalEdge, offset.X, offset.Y)
        {
        }

        public SnapDescription(SnapObstacle snappedTo, HorizontalSnapEdge horizontalEdge, VerticalSnapEdge verticalEdge, int xOffset, int yOffset)
        {
            Validate.IsNotNull<SnapObstacle>(snappedTo, "snappedTo");
            this.snappedTo = snappedTo;
            this.horizontalEdge = horizontalEdge;
            this.verticalEdge = verticalEdge;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }

        public HorizontalSnapEdge HorizontalEdge
        {
            get => 
                this.horizontalEdge;
            set
            {
                this.horizontalEdge = value;
            }
        }

        public SnapObstacle SnappedTo =>
            this.snappedTo;

        public VerticalSnapEdge VerticalEdge
        {
            get => 
                this.verticalEdge;
            set
            {
                this.verticalEdge = value;
            }
        }

        public int XOffset
        {
            get => 
                this.xOffset;
            set
            {
                this.xOffset = value;
            }
        }

        public int YOffset
        {
            get => 
                this.yOffset;
            set
            {
                this.yOffset = value;
            }
        }
    }
}

