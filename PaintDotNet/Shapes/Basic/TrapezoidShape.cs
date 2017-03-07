namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class TrapezoidShape : PdnGeometryShapeBase
    {
        private const double shapeModifier = 0.25;
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public TrapezoidShape() : base(PdnResources.GetString("TrapezoidShape.Name"), ShapeCategory.Basic)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(0.25, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(0.75, 0.0));
            segment.Points.Add(new PointDouble(1.0, 1.0));
            segment.Points.Add(new PointDouble(0.0, 1.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

