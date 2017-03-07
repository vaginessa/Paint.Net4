namespace PaintDotNet.Shapes.Arrows
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class BlockArrowShape : PdnGeometryShapeBase
    {
        private const double shapeModifier1 = 0.5;
        private const double shapeModifier2 = 0.5;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public BlockArrowShape() : base(PdnResources.GetString("BlockArrowShape.Name"), ShapeCategory.Arrows)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(1.0, 0.5)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            double x = 0.5;
            double y = 0.25;
            double num3 = 0.75;
            segment.Points.Add(new PointDouble(x, 0.0));
            segment.Points.Add(new PointDouble(x, y));
            segment.Points.Add(new PointDouble(0.0, y));
            segment.Points.Add(new PointDouble(0.0, num3));
            segment.Points.Add(new PointDouble(x, num3));
            segment.Points.Add(new PointDouble(x, 1.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

