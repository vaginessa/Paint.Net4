namespace PaintDotNet.Shapes.Stars
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal abstract class StarShapeBase : PdnGeometryShapeBase
    {
        private readonly string imageStringOverlay;
        private readonly PathGeometry unitGeometry;

        public StarShapeBase(string name, int sides, double ratio) : base(name, ShapeCategory.PolygonsAndStars)
        {
            this.imageStringOverlay = (sides < 7) ? string.Empty : sides.ToString(PdnResources.Culture);
            this.unitGeometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(1.0, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            double num = 3.1415926535897931 / ((double) sides);
            for (int i = 1; i < sides; i++)
            {
                double d = (((i * 2) * 3.1415926535897931) / ((double) sides)) - 1.5707963267948966;
                double num6 = 1.0 + (ratio * Math.Cos(d - num));
                double num7 = 1.0 + (ratio * Math.Sin(d - num));
                segment.Points.Add(new PointDouble(num6, num7));
                double num8 = 1.0 + Math.Cos(d);
                double num9 = 1.0 + Math.Sin(d);
                segment.Points.Add(new PointDouble(num8, num9));
            }
            double x = 1.0 + (ratio * Math.Cos(4.71238898038469 - num));
            double y = 1.0 + (ratio * Math.Sin(4.71238898038469 - num));
            segment.Points.Add(new PointDouble(x, y));
            item.Segments.Add(segment);
            this.unitGeometry.Figures.Add(item);
            this.unitGeometry.Freeze();
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.unitGeometry;

        protected override double OnGetAspectRatio() => 
            (this.unitGeometry.Bounds.Width / this.unitGeometry.Bounds.Height);

        protected override string ImageStringOverlay =>
            this.imageStringOverlay;
    }
}

