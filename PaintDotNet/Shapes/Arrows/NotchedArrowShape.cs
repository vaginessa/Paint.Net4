namespace PaintDotNet.Shapes.Arrows
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class NotchedArrowShape : PdnGeometryShapeBase
    {
        private const double shapeModifier1 = 0.5;
        private const double shapeModifier2 = 0.5;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public NotchedArrowShape() : base(PdnResources.GetString("NotchedArrowShape.Name"), ShapeCategory.Arrows)
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
            segment.Points.Add(new PointDouble(0.5, 0.0));
            segment.Points.Add(new PointDouble(0.5, 0.25));
            segment.Points.Add(new PointDouble(0.0, 0.25));
            segment.Points.Add(new PointDouble(0.25, 0.5));
            segment.Points.Add(new PointDouble(0.0, 0.75));
            segment.Points.Add(new PointDouble(0.5, 0.75));
            segment.Points.Add(new PointDouble(0.5, 1.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

