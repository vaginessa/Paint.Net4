namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class RotateCursorDrawing : HandleDrawing
    {
        private const double angleDelta = 32.0;
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(RotateCursorDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(0.0)));
        private PathGeometry arrowGeometry;
        private const double arrowHeight = 3.0;
        private CombinedGeometry arrowsGeometry;
        private const double arrowWidth = 3.0;
        private GeometryDrawing backgroundDrawing;
        public static readonly DependencyProperty BigRadiusProperty = DependencyProperty.Register("BigRadius", typeof(double), typeof(RotateCursorDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(50.0)));
        private GeometryGroup ccwArrowGeometry;
        private GeometryGroup cwArrowGeometry;
        private DependencyFunc<double, double, PointDouble> ellipseCenter = new DependencyFunc<double, double, PointDouble>(new Func<double, double, PointDouble>(RotateCursorDrawing.GetEllipseCenter));
        private EllipseGeometry ellipseGeometry = new EllipseGeometry();
        private GeometryDrawing foregroundDrawing;
        private static readonly double thetaDelta = MathUtil.DegreesToRadians(32.0);
        private PathGeometry wedgeClipGeometry;

        public RotateCursorDrawing()
        {
            this.ellipseCenter.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(AngleProperty.Name, Array.Empty<object>()));
            this.ellipseCenter.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(BigRadiusProperty.Name, Array.Empty<object>()));
            this.ellipseGeometry.SetBinding(EllipseGeometry.CenterProperty, this.ellipseCenter, new PaintDotNet.ObjectModel.PropertyPath(this.ellipseCenter.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay);
            this.ellipseGeometry.SetBinding(EllipseGeometry.RadiusXProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BigRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.ellipseGeometry.SetBinding(EllipseGeometry.RadiusYProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BigRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.arrowGeometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = true
            };
            LineSegment targetObject = new LineSegment();
            targetObject.SetBinding<double, PointDouble>(LineSegment.PointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => new PointDouble(3.0 * t, -1.5 * t));
            item.Segments.Add(targetObject);
            LineSegment segment2 = new LineSegment();
            segment2.SetBinding<double, PointDouble>(LineSegment.PointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => new PointDouble(3.0 * t, 1.5 * t));
            item.Segments.Add(segment2);
            this.arrowGeometry.Figures.Add(item);
            this.cwArrowGeometry = new GeometryGroup();
            this.cwArrowGeometry.Children.Add(this.arrowGeometry);
            TransformGroup group = new TransformGroup();
            RotateTransform transform = new RotateTransform {
                Angle = 90.0
            };
            group.Children.Add(transform);
            RotateTransform transform2 = new RotateTransform();
            transform2.SetBinding(RotateTransform.AngleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AngleProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            group.Children.Add(transform2);
            RotateTransform transform3 = new RotateTransform();
            transform3.SetBinding<double, double>(RotateTransform.AngleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BigRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, br => -GetArrowAngleDelta(br));
            group.Children.Add(transform3);
            RotateTransform transform4 = new RotateTransform {
                Angle = 16.0
            };
            transform4.SetBinding<PointDouble, double>(RotateTransform.CenterXProperty, this.ellipseCenter, new PaintDotNet.ObjectModel.PropertyPath(this.ellipseCenter.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay, c => c.X);
            transform4.SetBinding<PointDouble, double>(RotateTransform.CenterYProperty, this.ellipseCenter, new PaintDotNet.ObjectModel.PropertyPath(this.ellipseCenter.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay, c => c.Y);
            group.Children.Add(transform4);
            this.cwArrowGeometry.Transform = group;
            this.ccwArrowGeometry = new GeometryGroup();
            this.ccwArrowGeometry.Children.Add(this.arrowGeometry);
            TransformGroup group2 = new TransformGroup();
            RotateTransform transform5 = new RotateTransform {
                Angle = -90.0
            };
            group2.Children.Add(transform5);
            RotateTransform transform6 = new RotateTransform();
            transform6.SetBinding(RotateTransform.AngleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AngleProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            group2.Children.Add(transform6);
            RotateTransform transform7 = new RotateTransform();
            transform7.SetBinding<double, double>(RotateTransform.AngleProperty, this, new PaintDotNet.ObjectModel.PropertyPath(BigRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, br => GetArrowAngleDelta(br));
            group2.Children.Add(transform7);
            RotateTransform transform8 = new RotateTransform {
                Angle = -16.0
            };
            transform8.SetBinding<PointDouble, double>(RotateTransform.CenterXProperty, this.ellipseCenter, new PaintDotNet.ObjectModel.PropertyPath(this.ellipseCenter.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay, c => c.X);
            transform8.SetBinding<PointDouble, double>(RotateTransform.CenterYProperty, this.ellipseCenter, new PaintDotNet.ObjectModel.PropertyPath(this.ellipseCenter.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay, c => c.Y);
            group2.Children.Add(transform8);
            this.ccwArrowGeometry.Transform = group2;
            this.arrowsGeometry = new CombinedGeometry(GeometryCombineMode.Xor, this.cwArrowGeometry, this.ccwArrowGeometry);
            this.wedgeClipGeometry = new PathGeometry();
            PathFigure figure2 = new PathFigure {
                IsClosed = true
            };
            figure2.SetBinding(PathFigure.StartPointProperty, this.ellipseGeometry, new PaintDotNet.ObjectModel.PropertyPath(EllipseGeometry.CenterProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            LineSegment segment3 = new LineSegment();
            segment3.SetBinding<PointDouble, PointDouble>(LineSegment.PointProperty, this.ellipseGeometry, new PaintDotNet.ObjectModel.PropertyPath(EllipseGeometry.CenterProperty.Name, Array.Empty<object>()), BindingMode.OneWay, c => GetWedgePathPoint(c, thetaDelta));
            figure2.Segments.Add(segment3);
            LineSegment segment4 = new LineSegment();
            segment4.SetBinding<PointDouble, PointDouble>(LineSegment.PointProperty, this.ellipseGeometry, new PaintDotNet.ObjectModel.PropertyPath(EllipseGeometry.CenterProperty.Name, Array.Empty<object>()), BindingMode.OneWay, c => GetWedgePathPoint(c, -thetaDelta));
            figure2.Segments.Add(segment4);
            this.wedgeClipGeometry.Figures.Add(figure2);
            this.backgroundDrawing = new GeometryDrawing();
            this.backgroundDrawing.SetBinding(GeometryDrawing.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.BackgroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            WidenedGeometry geometry = new WidenedGeometry(this.ellipseGeometry);
            geometry.SetBinding<double, double>(WidenedGeometry.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => t + 2.5);
            CombinedGeometry geometry2 = new CombinedGeometry(GeometryCombineMode.Intersect, geometry, this.wedgeClipGeometry);
            WidenedGeometry geometry3 = new WidenedGeometry(this.arrowsGeometry);
            geometry3.SetBinding<double, double>(WidenedGeometry.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => t + 2.5);
            CombinedGeometry geometry4 = new CombinedGeometry(GeometryCombineMode.Union, this.arrowsGeometry, geometry3);
            CombinedGeometry geometry5 = new CombinedGeometry(GeometryCombineMode.Union, geometry2, geometry4);
            this.backgroundDrawing.Geometry = geometry5;
            base.DrawingGroup.Children.Add(this.backgroundDrawing);
            this.foregroundDrawing = new GeometryDrawing();
            this.foregroundDrawing.SetBinding(GeometryDrawing.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            WidenedGeometry geometry6 = new WidenedGeometry(this.ellipseGeometry);
            geometry6.SetBinding(WidenedGeometry.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty), BindingMode.OneWay);
            CombinedGeometry geometry7 = new CombinedGeometry(GeometryCombineMode.Intersect, geometry6, this.wedgeClipGeometry);
            WidenedGeometry geometry8 = new WidenedGeometry(this.arrowsGeometry);
            geometry8.SetBinding(WidenedGeometry.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty), BindingMode.OneWay);
            CombinedGeometry geometry9 = new CombinedGeometry(GeometryCombineMode.Union, this.arrowsGeometry, geometry8);
            CombinedGeometry geometry10 = new CombinedGeometry(GeometryCombineMode.Union, geometry7, geometry9);
            this.foregroundDrawing.Geometry = geometry10;
            base.DrawingGroup.Children.Add(this.foregroundDrawing);
        }

        protected override Freezable CreateInstanceCore() => 
            new RotateCursorDrawing();

        private static double GetArcLength(double r, double theta) => 
            (r * theta);

        private static double GetArcLengthFromRadiusAndEuclideanDistance(double r, double distance)
        {
            double arcRadiansFromRadiusAndEuclideanDistance = GetArcRadiansFromRadiusAndEuclideanDistance(r, distance);
            return (r * arcRadiansFromRadiusAndEuclideanDistance);
        }

        private static double GetArcRadiansFromRadiusAndEuclideanDistance(double r, double distance)
        {
            double y = 0.5 * distance;
            double x = Math.Sqrt((r * r) - (y * y));
            return Math.Atan2(y, x);
        }

        private static double GetArrowAngleDelta(double bigRadius) => 
            MathUtil.RadiansToDegrees(GetArcRadiansFromRadiusAndEuclideanDistance(bigRadius, 3.0));

        private static RectDouble GetClipGeometryRect(double radius)
        {
            double num = 2.0 * (radius - 3.0);
            double edgeLength = Math.Max(0.0, num);
            return RectDouble.FromCenter(PointDouble.Zero, edgeLength);
        }

        private static PointDouble GetEllipseCenter(double angle, double bigRadius)
        {
            double d = MathUtil.DegreesToRadians(angle);
            double x = Math.Cos(d) * bigRadius;
            return new PointDouble(x, Math.Sin(d) * bigRadius);
        }

        private static PointDouble GetWedgePathPoint(PointDouble ellipseCenter, double thetaDelta)
        {
            VectorDouble num6 = (VectorDouble) ellipseCenter;
            double num2 = num6.Length + 2.0;
            double d = Math.Atan2(-ellipseCenter.Y, -ellipseCenter.X) + thetaDelta;
            return new PointDouble(Math.Cos(d) * num2, Math.Sin(d) * num2);
        }

        public double Angle
        {
            get => 
                ((double) base.GetValue(AngleProperty));
            set
            {
                base.SetValue(AngleProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public double BigRadius
        {
            get => 
                ((double) base.GetValue(BigRadiusProperty));
            set
            {
                base.SetValue(BigRadiusProperty, DoubleUtil.GetBoxed(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly RotateCursorDrawing.<>c <>9 = new RotateCursorDrawing.<>c();
            public static Func<double, PointDouble> <>9__13_0;
            public static Func<double, PointDouble> <>9__13_1;
            public static Func<double, double> <>9__13_10;
            public static Func<double, double> <>9__13_11;
            public static Func<double, double> <>9__13_2;
            public static Func<PointDouble, double> <>9__13_3;
            public static Func<PointDouble, double> <>9__13_4;
            public static Func<double, double> <>9__13_5;
            public static Func<PointDouble, double> <>9__13_6;
            public static Func<PointDouble, double> <>9__13_7;
            public static Func<PointDouble, PointDouble> <>9__13_8;
            public static Func<PointDouble, PointDouble> <>9__13_9;

            internal PointDouble <.ctor>b__13_0(double t) => 
                new PointDouble(3.0 * t, -1.5 * t);

            internal PointDouble <.ctor>b__13_1(double t) => 
                new PointDouble(3.0 * t, 1.5 * t);

            internal double <.ctor>b__13_10(double t) => 
                (t + 2.5);

            internal double <.ctor>b__13_11(double t) => 
                (t + 2.5);

            internal double <.ctor>b__13_2(double br) => 
                -RotateCursorDrawing.GetArrowAngleDelta(br);

            internal double <.ctor>b__13_3(PointDouble c) => 
                c.X;

            internal double <.ctor>b__13_4(PointDouble c) => 
                c.Y;

            internal double <.ctor>b__13_5(double br) => 
                RotateCursorDrawing.GetArrowAngleDelta(br);

            internal double <.ctor>b__13_6(PointDouble c) => 
                c.X;

            internal double <.ctor>b__13_7(PointDouble c) => 
                c.Y;

            internal PointDouble <.ctor>b__13_8(PointDouble c) => 
                RotateCursorDrawing.GetWedgePathPoint(c, RotateCursorDrawing.thetaDelta);

            internal PointDouble <.ctor>b__13_9(PointDouble c) => 
                RotateCursorDrawing.GetWedgePathPoint(c, -RotateCursorDrawing.thetaDelta);
        }
    }
}

