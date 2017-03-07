namespace PaintDotNet.Shapes.Symbols
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class MultiplyShape : PdnGeometryShapeBase
    {
        private const double shapeModifier = 0.15;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public MultiplyShape() : base(PdnResources.GetString("MultiplyShape.Name"), ShapeCategory.Symbols)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            double x = 0.15;
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(x, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(0.5, 0.5 - x));
            segment.Points.Add(new PointDouble(1.0 - x, 0.0));
            segment.Points.Add(new PointDouble(1.0, x));
            segment.Points.Add(new PointDouble(0.5 + x, 0.5));
            segment.Points.Add(new PointDouble(1.0, 1.0 - x));
            segment.Points.Add(new PointDouble(1.0 - x, 1.0));
            segment.Points.Add(new PointDouble(0.5, 0.5 + x));
            segment.Points.Add(new PointDouble(x, 1.0));
            segment.Points.Add(new PointDouble(0.0, 1.0 - x));
            segment.Points.Add(new PointDouble(0.5 - x, 0.5));
            segment.Points.Add(new PointDouble(0.0, x));
            segment.Points.Add(new PointDouble(x, 0.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

