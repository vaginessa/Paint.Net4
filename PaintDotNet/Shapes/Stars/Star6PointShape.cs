namespace PaintDotNet.Shapes.Stars
{
    using System;

    internal sealed class Star6PointShape : StarShapeBase
    {
        public Star6PointShape() : base(PdnResources.GetString("Star6PointShape.Name"), 6, 0.57735)
        {
        }
    }
}

