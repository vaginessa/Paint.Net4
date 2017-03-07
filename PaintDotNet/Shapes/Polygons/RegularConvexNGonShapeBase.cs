namespace PaintDotNet.Shapes.Polygons
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal abstract class RegularConvexNGonShapeBase : PdnGeometryShapeBase
    {
        private readonly double aspectRatio;
        private readonly string imageStringOverlay;
        private readonly PathGeometry unitGeometry;

        public RegularConvexNGonShapeBase(string name, int sides) : base(name, ShapeCategory.PolygonsAndStars)
        {
            this.imageStringOverlay = (sides < 5) ? string.Empty : sides.ToString(PdnResources.Culture);
            if ((sides % 2) == 0)
            {
                this.aspectRatio = Math.Sin((6.2831853071795862 * (sides / 4)) / ((double) sides));
            }
            else
            {
                double num = 0.0;
                for (int j = 1; j < (sides / 2); j++)
                {
                    double num6 = Math.Sin(((j * 2) * 3.1415926535897931) / ((double) sides));
                    if (num6 > num)
                    {
                        num = num6;
                    }
                }
                double num2 = -Math.Cos((6.2831853071795862 * (Math.Ceiling((double) (((double) sides) / 2.0)) - 1.0)) / ((double) sides));
                double num3 = num * 2.0;
                double num4 = 1.0 + num2;
                this.aspectRatio = num3 / num4;
            }
            this.unitGeometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true,
                StartPoint = new PointDouble(1.0, 0.0)
            };
            PolyLineSegment segment = new PolyLineSegment {
                IsSmoothJoin = false
            };
            for (int i = 1; i < sides; i++)
            {
                double num8 = ((i * 2) * 3.1415926535897931) / ((double) sides);
                double x = 1.0 + Math.Cos(num8 - 1.5707963267948966);
                double y = 1.0 + Math.Sin(num8 - 1.5707963267948966);
                segment.Points.Add(new PointDouble(x, y));
            }
            item.Segments.Add(segment);
            this.unitGeometry.Figures.Add(item);
            this.unitGeometry.Freeze();
        }

        protected sealed override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.unitGeometry;

        protected override double OnGetAspectRatio() => 
            this.aspectRatio;

        protected override string ImageStringOverlay =>
            this.imageStringOverlay;
    }
}

