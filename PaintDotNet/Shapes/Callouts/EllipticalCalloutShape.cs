namespace PaintDotNet.Shapes.Callouts
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class EllipticalCalloutShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry;

        static EllipticalCalloutShape()
        {
            PathGeometry freezable = new PathGeometry();
            PathFigureCollection collection1 = new PathFigureCollection();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(0.195, 44.743)
            };
            PathSegmentCollection collection2 = new PathSegmentCollection();
            BezierSegment segment1 = new BezierSegment(new PointDouble(0.195, 20.032), new PointDouble(22.342, 0.0), new PointDouble(49.902, 0.0)) {
                IsSmoothJoin = true
            };
            collection2.Add(segment1);
            collection2.Add(new BezierSegment(new PointDouble(77.658, 0.0), new PointDouble(99.805, 20.032), new PointDouble(99.805, 44.743)));
            collection2.Add(new BezierSegment(new PointDouble(100.0, 69.453), new PointDouble(77.463, 89.485), new PointDouble(49.902, 89.485)));
            collection2.Add(new BezierSegment(new PointDouble(42.201, 89.485), new PointDouble(34.538, 87.841), new PointDouble(27.901, 84.913)));
            collection2.Add(new LineSegment(0.0, 100.0));
            collection2.Add(new LineSegment(14.441, 76.045));
            collection2.Add(new BezierSegment(new PointDouble(5.628, 67.976), new PointDouble(0.0, 56.928), new PointDouble(0.195, 44.743)));
            item.Segments = collection2;
            collection1.Add(item);
            freezable.Figures = collection1;
            unitGeometry = freezable.EnsureFrozen<PathGeometry>();
        }

        public EllipticalCalloutShape() : base(PdnResources.GetString("EllipticalCalloutShape.Name"), ShapeCategory.Callouts)
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

