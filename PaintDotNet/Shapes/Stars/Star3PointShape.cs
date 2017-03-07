namespace PaintDotNet.Shapes.Stars
{
    using System;

    internal sealed class Star3PointShape : StarShapeBase
    {
        public Star3PointShape() : base(PdnResources.GetString("Star3PointShape.Name"), 3, 0.2)
        {
        }
    }
}

