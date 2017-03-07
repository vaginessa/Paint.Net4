namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class SelectionRenderParameters
    {
        public SelectionRenderParameters(PaintDotNet.Canvas.SelectionSnapshot selectionSnapshot, SizeDouble canvasSize, SizeDouble viewportSize, RectDouble viewportCanvasBounds, double scaleRatio, bool isInteriorFilled, Brush interiorBrush, bool isOutlineEnabled, bool isOutlineAntialiased, bool isOutlineAnimated, PaintDotNet.SelectionRenderingQuality selectionRenderingQuality)
        {
            this.SelectionSnapshot = selectionSnapshot;
            this.CanvasSize = canvasSize;
            this.ViewportSize = viewportSize;
            this.ViewportCanvasBounds = viewportCanvasBounds;
            this.ScaleRatio = scaleRatio;
            this.IsInteriorFilled = isInteriorFilled;
            this.InteriorBrush = interiorBrush;
            this.IsOutlineEnabled = isOutlineEnabled;
            this.IsOutlineAntialiased = isOutlineAntialiased;
            this.IsOutlineAnimated = isOutlineAnimated;
            this.SelectionRenderingQuality = selectionRenderingQuality;
        }

        public SizeDouble CanvasSize { get; private set; }

        public Brush InteriorBrush { get; private set; }

        public bool IsInteriorFilled { get; private set; }

        public bool IsOutlineAnimated { get; private set; }

        public bool IsOutlineAntialiased { get; private set; }

        public bool IsOutlineEnabled { get; private set; }

        public double ScaleRatio { get; private set; }

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality { get; private set; }

        public PaintDotNet.Canvas.SelectionSnapshot SelectionSnapshot { get; private set; }

        public RectDouble ViewportCanvasBounds { get; private set; }

        public SizeDouble ViewportSize { get; private set; }
    }
}

