namespace PaintDotNet.Shapes.Symbols
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class HeartShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public HeartShape() : base(PdnResources.GetString("HeartShape.Name"), ShapeCategory.Symbols)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(11.5, 20.5834)
            };
            item.Segments.Add(new BezierSegment(new PointDouble(10.3333, 16.25), new PointDouble(13.6667, 12.9167), new PointDouble(16.5, 12.4167)));
            item.Segments.Add(new BezierSegment(new PointDouble(19.3334, 11.9167), new PointDouble(21.6667, 14.25), new PointDouble(24.0, 16.5833)));
            item.Segments.Add(new BezierSegment(new PointDouble(26.3334, 14.25), new PointDouble(28.6667, 11.9167), new PointDouble(31.5, 12.4167)));
            item.Segments.Add(new BezierSegment(new PointDouble(34.3333, 12.9167), new PointDouble(37.6666, 16.25), new PointDouble(36.5, 20.5833)));
            item.Segments.Add(new BezierSegment(new PointDouble(35.3333, 24.9167), new PointDouble(29.6667, 30.25), new PointDouble(24.0, 35.5833)));
            item.Segments.Add(new BezierSegment(new PointDouble(18.3334, 30.25), new PointDouble(12.6667, 24.9167), new PointDouble(11.5, 20.5834)));
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

