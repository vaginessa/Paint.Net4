namespace PaintDotNet.Shapes.Symbols
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class LightningBoltShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = CreateUnitGeometry().EnsureFrozen<Geometry>();

        public LightningBoltShape() : base(PdnResources.GetString("LightningBoltShape.Name"), ShapeCategory.Symbols)
        {
        }

        private static Geometry CreateUnitGeometry()
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(29.965, 0.5047)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            segment.Points.Add(new PointDouble(0.5047, 18.514));
            segment.Points.Add(new PointDouble(26.9396, 39.3102));
            segment.Points.Add(new PointDouble(17.968, 45.4352));
            segment.Points.Add(new PointDouble(43.0051, 64.8427));
            segment.Points.Add(new PointDouble(35.3201, 69.5556));
            segment.Points.Add(new PointDouble(75.6158, 100.505));
            segment.Points.Add(new PointDouble(51.855, 60.1204));
            segment.Points.Add(new PointDouble(58.149, 56.0927));
            segment.Points.Add(new PointDouble(39.2442, 32.3176));
            segment.Points.Add(new PointDouble(44.0417, 28.4428));
            segment.Points.Add(new PointDouble(29.965, 0.5047));
            item.Segments.Add(segment);
            geometry.Figures.Add(item);
            return geometry;
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

