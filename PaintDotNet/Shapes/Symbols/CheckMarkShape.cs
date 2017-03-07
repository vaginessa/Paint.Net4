namespace PaintDotNet.Shapes.Symbols
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class CheckMarkShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public CheckMarkShape() : base(PdnResources.GetString("CheckMarkShape.Name"), ShapeCategory.Symbols)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(0.85, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(1.0, 0.15));
            segment.Points.Add(new PointDouble(0.375, 1.0));
            segment.Points.Add(new PointDouble(0.0, 0.625));
            segment.Points.Add(new PointDouble(0.15, 0.475));
            segment.Points.Add(new PointDouble(0.352, 0.677));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

