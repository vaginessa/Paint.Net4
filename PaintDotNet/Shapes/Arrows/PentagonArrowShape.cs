namespace PaintDotNet.Shapes.Arrows
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class PentagonArrowShape : PdnGeometryShapeBase
    {
        private const double shapeModifier1 = 0.5;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public PentagonArrowShape() : base(PdnResources.GetString("PentagonArrowShape.Name"), ShapeCategory.Arrows)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            double x = 0.5;
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(1.0, 0.5)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(x, 0.0));
            segment.Points.Add(new PointDouble(0.0, 0.0));
            segment.Points.Add(new PointDouble(0.0, 1.0));
            segment.Points.Add(new PointDouble(x, 1.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

