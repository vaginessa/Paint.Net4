namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class TriangleShape : PdnGeometryShapeBase
    {
        private static readonly double aspectRatio = (1.0 / height);
        private static readonly double height = Math.Sqrt(0.75);
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public TriangleShape() : base(PdnResources.GetString("IsoscelesTriangleShape.Name"), ShapeCategory.Basic)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(0.5, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(0.0, height));
            segment.Points.Add(new PointDouble(1.0, height));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;

        protected override double OnGetAspectRatio() => 
            aspectRatio;
    }
}

