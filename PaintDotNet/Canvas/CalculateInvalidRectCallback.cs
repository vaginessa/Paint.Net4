namespace PaintDotNet.Canvas
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate RectDouble CalculateInvalidRectCallback(CanvasView canvasView, RectDouble canvasRect);
}

