namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;

    internal abstract class DocumentBitmapLayerOverlay : DocumentLayerOverlay
    {
        protected DocumentBitmapLayerOverlay(BitmapLayer layer, RectInt32 affectedBounds) : base(layer, affectedBounds)
        {
        }

        public BitmapLayer Layer =>
            ((BitmapLayer) base.Layer);
    }
}

