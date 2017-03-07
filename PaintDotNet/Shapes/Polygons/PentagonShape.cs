namespace PaintDotNet.Shapes.Polygons
{
    using System;

    internal sealed class PentagonShape : RegularConvexNGonShapeBase
    {
        public PentagonShape() : base(PdnResources.GetString("PentagonShape.Name"), 5)
        {
        }
    }
}

