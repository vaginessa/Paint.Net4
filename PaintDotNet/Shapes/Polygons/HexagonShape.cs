namespace PaintDotNet.Shapes.Polygons
{
    using System;

    internal sealed class HexagonShape : RegularConvexNGonShapeBase
    {
        public HexagonShape() : base(PdnResources.GetString("HexagonShape.Name"), 6)
        {
        }
    }
}

