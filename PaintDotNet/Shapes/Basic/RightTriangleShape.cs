namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class RightTriangleShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public RightTriangleShape() : base(PdnResources.GetString("RightTriangleShape.Name"), ShapeCategory.Basic)
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
            segment.Points.Add(new PointDouble(0.0, 1.0));
            segment.Points.Add(new PointDouble(1.0, 1.0));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

