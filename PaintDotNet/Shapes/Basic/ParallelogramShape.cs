namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class ParallelogramShape : PdnGeometryShapeBase
    {
        private const double shapeModifier = 0.25;

        public ParallelogramShape() : base(PdnResources.GetString("ParallelogramShape.Name"), ShapeCategory.Basic)
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            PathGeometry freezable = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(bounds.Width, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(bounds.Width * 0.75, bounds.Height));
            segment.Points.Add(new PointDouble(0.0, bounds.Height));
            segment.Points.Add(new PointDouble(bounds.Width * 0.25, 0.0));
            item.Segments.Add(segment);
            freezable.Figures.Add(item);
            return freezable.EnsureFrozen<PathGeometry>();
        }
    }
}

