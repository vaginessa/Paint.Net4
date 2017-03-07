namespace PaintDotNet.Shapes.Stars
{
    using System;

    internal sealed class Star5PointShape : StarShapeBase
    {
        public Star5PointShape() : base(PdnResources.GetString("Star5PointShape.Name"), 5, 0.381966)
        {
        }
    }
}

