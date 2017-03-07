namespace PaintDotNet.Shapes.Arrows
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class ChevronArrowShape : PdnGeometryShapeBase
    {
        private const double shapeModifier = 0.33333333333333331;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public ChevronArrowShape() : base(PdnResources.GetString("ChevronArrowShape.Name"), ShapeCategory.Arrows)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(0.0, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(0.33333333333333331, 0.5));
            segment.Points.Add(new PointDouble(0.0, 1.0));
            segment.Points.Add(new PointDouble(0.66666666666666674, 1.0));
            segment.Points.Add(new PointDouble(1.0, 0.5));
            segment.Points.Add(new PointDouble(0.66666666666666674, 0.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

