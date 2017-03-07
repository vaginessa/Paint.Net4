namespace PaintDotNet.Shapes.Callouts
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class RectangularCalloutShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public RectangularCalloutShape() : base(PdnResources.GetString("RectangularCalloutShape.Name"), ShapeCategory.Callouts)
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
            segment.Points.Add(new PointDouble(100.0, 0.0));
            segment.Points.Add(new PointDouble(100.0, 75.0));
            segment.Points.Add(new PointDouble(25.0, 75.0));
            segment.Points.Add(new PointDouble(0.0, 100.0));
            segment.Points.Add(new PointDouble(12.5, 75.0));
            segment.Points.Add(new PointDouble(0.0, 75.0));
            segment.Points.Add(new PointDouble(0.0, 75.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

