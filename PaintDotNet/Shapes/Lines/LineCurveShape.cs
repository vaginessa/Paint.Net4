namespace PaintDotNet.Shapes.Lines
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class LineCurveShape : PdnShapeBase
    {
        private const double arrowHeight = 5.0;
        private const double arrowWidth = 5.0;
        private readonly int[] controlPointPropertyNames;
        private static readonly double flatteningTolerance = 0.1;
        private static readonly VectorDouble[] imageControlPointUnitOffsets = new VectorDouble[] { new VectorDouble(0.0, 0.0), new VectorDouble(0.25, 0.75), new VectorDouble(0.75, 0.25), new VectorDouble(1.0, 1.0) };
        private static readonly ShapeOptions shapeOptions = new ShapeOptions(ShapeConstraintOption.HypotenuseMultipleOf15Degrees, ShapeElideOption.ZeroWidthAndZeroHeight, ShapeTransformOption.None, ShapeSnappingOption.PixelCenters);

        public LineCurveShape() : this(4)
        {
        }

        private LineCurveShape(int controlPointCount) : base(PdnResources.GetString("LineTool.Name"), ShapeCategory.Lines, shapeOptions)
        {
            if ((controlPointCount < 0) || (controlPointCount > 10))
            {
                throw new ArgumentOutOfRangeException("controlPointCount");
            }
            this.controlPointPropertyNames = new int[controlPointCount];
            for (int i = 0; i < controlPointCount; i++)
            {
                this.controlPointPropertyNames[i] = i;
            }
        }

        private static Geometry CreateArrowGeometry(double width, double height, double strokeWidth, double strokeScale, bool isFilled, bool isClosed)
        {
            double num = (width * strokeWidth) * strokeScale;
            double num2 = (height * strokeWidth) * strokeScale;
            PathGeometry geometry = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = isClosed,
                IsFilled = isFilled,
                StartPoint = new PointDouble(-num2, num / 2.0)
            };
            item.Segments.Add(new LineSegment(0.0, 0.0));
            item.Segments.Add(new LineSegment(-num2, -num / 2.0));
            geometry.Figures.Add(item);
            if (isFilled)
            {
                return new OutlinedGeometry(geometry) { FlatteningTolerance = flatteningTolerance };
            }
            return new WidenedGeometry(geometry, strokeWidth) { FlatteningTolerance = flatteningTolerance };
        }

        private static LineGeometry CreateGuideGeometry(ShapeRenderParameters renderParams) => 
            new LineGeometry(renderParams.StartPoint, renderParams.EndPoint);

        private Geometry CreateLineCurveGeometry(ShapeRenderParameters renderParams)
        {
            Geometry geometry;
            CurveType type = (CurveType) renderParams.SettingValues[ToolSettings.Null.Shapes.CurveType.Path];
            if (renderParams.HasPropertyValues && renderParams.PropertyValues.Any<KeyValuePair<object, object>>())
            {
                RectDouble num = RectDouble.FromCorners(renderParams.StartPoint, renderParams.EndPoint);
                List<PointDouble> source = new List<PointDouble>();
                PointDouble[] numArray = this.GetInitialControlPointValues(renderParams).ToArrayEx<PointDouble>();
                bool flag = false;
                for (int i = 0; i < this.controlPointPropertyNames.Length; i++)
                {
                    object controlPointPropertyName = this.GetControlPointPropertyName(i);
                    Pair<double, double> pair = (Pair<double, double>) renderParams.PropertyValues[controlPointPropertyName];
                    if ((pair.First != numArray[i].X) || (pair.Second != numArray[i].Y))
                    {
                        flag = true;
                    }
                    PointDouble num3 = new PointDouble(pair.First, pair.Second);
                    PointDouble item = new PointDouble(((num3.X - num.X) * renderParams.TransformScale.X) + num.X, ((num3.Y - num.Y) * renderParams.TransformScale.Y) + num.Y);
                    source.Add(item);
                }
                if (!flag)
                {
                    geometry = new LineGeometry(renderParams.StartPoint, renderParams.EndPoint).EnsureFrozen<LineGeometry>();
                }
                else
                {
                    PathSegment segment;
                    if (type != CurveType.Spline)
                    {
                        if (type != CurveType.Bezier)
                        {
                            throw ExceptionUtil.InvalidEnumArgumentException<CurveType>(type, "curveType");
                        }
                    }
                    else
                    {
                        PathGeometry geometry1 = new PathGeometry();
                        PathFigureCollection collection1 = new PathFigureCollection(1);
                        PathFigure figure1 = new PathFigure {
                            StartPoint = source[0],
                            IsClosed = false
                        };
                        PathSegmentCollection collection2 = new PathSegmentCollection(1);
                        collection2.Add(new PolyCurveSegment(new ListSegment<PointDouble>(source, 1, source.Count - 1)));
                        figure1.Segments = collection2;
                        collection1.Add(figure1);
                        geometry1.Figures = collection1;
                        geometry = geometry1;
                        goto Label_0271;
                    }
                    if (source.Count == 4)
                    {
                        segment = new BezierSegment(source[1], source[2], source[3]);
                    }
                    else
                    {
                        segment = new PolyBezierSegment(new ListSegment<PointDouble>(source, 1, source.Count - 1));
                    }
                    PathGeometry geometry2 = new PathGeometry();
                    PathFigureCollection collection3 = new PathFigureCollection(1);
                    PathFigure figure2 = new PathFigure {
                        StartPoint = source[0],
                        IsClosed = false
                    };
                    PathSegmentCollection collection4 = new PathSegmentCollection(1);
                    collection4.Add(segment);
                    figure2.Segments = collection4;
                    collection3.Add(figure2);
                    geometry2.Figures = collection3;
                    geometry = geometry2;
                }
            }
            else
            {
                geometry = new LineGeometry(renderParams.StartPoint, renderParams.EndPoint);
            }
        Label_0271:
            return geometry.EnsureFrozen<Geometry>();
        }

        private static Geometry CreateStrokedLineGeometry(Geometry lineGeometry, double strokeWidth, LineCap2 startCap, LineCap2 endCap, PaintDotNet.UI.Media.DashStyle dashStyle)
        {
            Geometry geometry;
            Geometry geometry2;
            Geometry geometry3;
            Geometry geometry4;
            Geometry geometry5;
            double length = lineGeometry.GetLength(flatteningTolerance);
            StrokeStyle strokeStyle = new StrokeStyle {
                DashStyle = dashStyle,
                LineJoin = PenLineJoin.Round
            };
            double num2 = 0.0;
            switch (startCap)
            {
                case LineCap2.Flat:
                    geometry = null;
                    break;

                case LineCap2.Arrow:
                    geometry = CreateArrowGeometry(5.0, 5.0, strokeWidth, 1.0, false, false).EnsureFrozen<Geometry>();
                    num2 = 0.5;
                    break;

                case LineCap2.ArrowFilled:
                    geometry = CreateArrowGeometry(5.0, 5.0, strokeWidth, 1.0, true, true).EnsureFrozen<Geometry>();
                    num2 = 1.5;
                    break;

                case LineCap2.Rounded:
                    strokeStyle.StartLineCap = PenLineCap.Round;
                    geometry = null;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<LineCap2>(startCap, "startCap");
            }
            double num3 = 0.0;
            switch (endCap)
            {
                case LineCap2.Flat:
                    geometry2 = null;
                    break;

                case LineCap2.Arrow:
                    geometry2 = CreateArrowGeometry(5.0, 5.0, strokeWidth, 1.0, false, false).EnsureFrozen<Geometry>();
                    num3 = 0.5;
                    break;

                case LineCap2.ArrowFilled:
                    geometry2 = CreateArrowGeometry(5.0, 5.0, strokeWidth, 1.0, true, true).EnsureFrozen<Geometry>();
                    num3 = 1.5;
                    break;

                case LineCap2.Rounded:
                    strokeStyle.EndLineCap = PenLineCap.Round;
                    geometry2 = null;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<LineCap2>(endCap, "endCap");
            }
            strokeStyle.Freeze();
            if (geometry == null)
            {
                geometry3 = null;
            }
            else
            {
                PointAndTangentDouble pointAtLength = lineGeometry.GetPointAtLength(0.0, flatteningTolerance);
                double radians = Math.Atan2(pointAtLength.Tangent.Y, pointAtLength.Tangent.X) + 3.1415926535897931;
                Matrix3x2Double num9 = Matrix3x2Double.RotationByRadians(radians);
                Matrix3x2Double num10 = Matrix3x2Double.Translation(pointAtLength.Point.X, pointAtLength.Point.Y);
                Matrix3x2Double matrix = num9 * num10;
                geometry3 = geometry.GetTransformedGeometry(matrix).EnsureFrozen<Geometry>();
            }
            if (geometry2 == null)
            {
                geometry4 = null;
            }
            else
            {
                double num14 = lineGeometry.GetLength(flatteningTolerance);
                PointAndTangentDouble num15 = lineGeometry.GetPointAtLength(num14, flatteningTolerance);
                Matrix3x2Double num18 = Matrix3x2Double.RotationByRadians(Math.Atan2(num15.Tangent.Y, num15.Tangent.X));
                Matrix3x2Double num19 = Matrix3x2Double.Translation(num15.Point.X, num15.Point.Y);
                Matrix3x2Double num20 = num18 * num19;
                geometry4 = geometry2.GetTransformedGeometry(num20).EnsureFrozen<Geometry>();
            }
            double startLength = 0.0;
            double endLength = length;
            if (num2 != 0.0)
            {
                startLength = strokeWidth * num2;
            }
            if (num3 != 0.0)
            {
                endLength = length - (strokeWidth * num3);
            }
            if ((startLength != 0.0) || (endLength != length))
            {
                geometry5 = GetTrimmedGeometry(lineGeometry, startLength, endLength).EnsureFrozen<Geometry>();
            }
            else
            {
                geometry5 = lineGeometry;
            }
            Geometry item = new WidenedGeometry(geometry5, strokeWidth, strokeStyle) { FlatteningTolerance = flatteningTolerance }.EnsureFrozen<WidenedGeometry>();
            GeometryGroup group = new GeometryGroup {
                FillRule = FillRule.Nonzero
            };
            group.Children.Add(item);
            if (geometry3 != null)
            {
                group.Children.Add(geometry3);
            }
            if (geometry4 != null)
            {
                group.Children.Add(geometry4);
            }
            group.Freeze();
            return group;
        }

        private static Geometry ExtendGeometry(Geometry geometry, double exLength)
        {
            LineGeometry geometry2 = geometry as LineGeometry;
            if (geometry2 != null)
            {
                return ExtendLineGeometry(geometry2, exLength);
            }
            PathGeometry geometry3 = geometry as PathGeometry;
            if (geometry3 == null)
            {
                throw new ArgumentException();
            }
            return ExtendPathGeometry(geometry3, exLength);
        }

        private static LineGeometry ExtendLineGeometry(LineGeometry geometry, double exLength)
        {
            LineGeometry geometry2 = (LineGeometry) geometry.Clone();
            PointDouble startPoint = geometry.StartPoint;
            PointDouble endPoint = geometry.EndPoint;
            if (startPoint != endPoint)
            {
                VectorDouble vec = (VectorDouble) (endPoint - startPoint);
                VectorDouble num4 = VectorDouble.Normalize(vec);
                PointDouble num5 = startPoint - ((PointDouble) (num4 * exLength));
                PointDouble num6 = endPoint + ((PointDouble) (num4 * exLength));
                geometry2.StartPoint = num5;
                geometry2.EndPoint = num6;
            }
            return geometry2;
        }

        private static PathGeometry ExtendPathGeometry(PathGeometry geometry, double exLength)
        {
            if (geometry.Figures.Count != 1)
            {
                throw new ArgumentException();
            }
            PathGeometry geometry2 = (PathGeometry) geometry.Clone();
            PointAndTangentDouble pointAtLength = geometry.GetPointAtLength(0.0, flatteningTolerance);
            VectorDouble num2 = pointAtLength.Tangent.NormalizeOrZeroCopy();
            PointDouble num3 = pointAtLength.Point - ((PointDouble) (num2 * exLength));
            geometry2.Figures[0].Segments.Insert(0, new LineSegment(pointAtLength.Point));
            geometry2.Figures[0].StartPoint = num3;
            PointAndTangentDouble num4 = geometry.GetPointAtLength(double.PositiveInfinity, flatteningTolerance);
            VectorDouble num5 = num4.Tangent.NormalizeOrZeroCopy();
            PointDouble point = num4.Point + ((PointDouble) (num5 * exLength));
            geometry2.Figures[0].Segments.Add(new LineSegment(point));
            return geometry2;
        }

        private object GetControlPointPropertyName(int index) => 
            this.controlPointPropertyNames[index];

        [IteratorStateMachine(typeof(<GetControlPointUnitOffsets>d__11))]
        private IEnumerable<double> GetControlPointUnitOffsets(int count) => 
            new <GetControlPointUnitOffsets>d__11(-2) { <>3__count = count };

        [IteratorStateMachine(typeof(<GetInitialControlPointValues>d__12))]
        private IEnumerable<PointDouble> GetInitialControlPointValues(ShapeRenderParameters renderParams) => 
            new <GetInitialControlPointValues>d__12(-2) { 
                <>4__this = this,
                <>3__renderParams = renderParams
            };

        private static PointDouble GetPointAtLength(LineSegmentDouble lineSegment, double length)
        {
            if (!length.IsFinite())
            {
                throw new ArgumentException("length");
            }
            double num = lineSegment.Length;
            if (length == 0.0)
            {
                return lineSegment.StartPoint;
            }
            if (length == num)
            {
                return lineSegment.EndPoint;
            }
            double num2 = length / num;
            VectorDouble num3 = (VectorDouble) (lineSegment.EndPoint - lineSegment.StartPoint);
            VectorDouble num4 = (VectorDouble) (num3 * num2);
            return (lineSegment.StartPoint + num4);
        }

        private static Geometry GetTrimmedGeometry(Geometry geometry, double startLength, double endLength)
        {
            if ((endLength < startLength) || (startLength > endLength))
            {
                return Geometry.Empty;
            }
            if (geometry.GetLength(flatteningTolerance) <= (endLength - startLength))
            {
                return Geometry.Empty;
            }
            IEnumerable<PointDouble> source = PathGeometryUtil.EnumeratePathGeometryPolygons(geometry.GetFlattenedPathGeometry(flatteningTolerance)).SelectMany<PointDouble>();
            if (!source.Any<PointDouble>())
            {
                return Geometry.Empty;
            }
            IEnumerable<PointDouble> enumerable2 = TrimPolyLine(source, startLength, endLength);
            if (!enumerable2.Any<PointDouble>() || !enumerable2.Skip<PointDouble>(1).Any<PointDouble>())
            {
                return Geometry.Empty;
            }
            PathGeometry geometry3 = new PathGeometry();
            PathFigure item = new PathFigure {
                IsClosed = false,
                StartPoint = enumerable2.First<PointDouble>()
            };
            PolyLineSegment segment = new PolyLineSegment(enumerable2.Skip<PointDouble>(1));
            item.Segments.Add(segment);
            geometry3.Figures.Add(item);
            return geometry3;
        }

        protected override PropertyCollection OnCreatePropertyCollection(ShapeRenderParameters renderParams)
        {
            List<Property> properties = new List<Property>();
            int index = 0;
            foreach (PointDouble num2 in this.GetInitialControlPointValues(renderParams))
            {
                properties.Add(new DoubleVectorProperty(this.GetControlPointPropertyName(index), new Pair<double, double>(num2.X, num2.Y)));
                index++;
            }
            return new PropertyCollection(properties);
        }

        protected override ShapeRenderData OnCreateRenderData(ShapeRenderParameters renderParams)
        {
            Geometry guideGeometry = CreateGuideGeometry(renderParams).EnsureFrozen<LineGeometry>();
            Geometry lineGeometry = ExtendGeometry(this.CreateLineCurveGeometry(renderParams), 0.4999).EnsureFrozen<Geometry>();
            float num = (float) renderParams.SettingValues[ToolSettings.Null.Pen.Width.Path];
            System.Drawing.Drawing2D.DashStyle gdipDashStyle = (System.Drawing.Drawing2D.DashStyle) renderParams.SettingValues[ToolSettings.Null.Pen.DashStyle.Path];
            LineCap2 startCap = (LineCap2) renderParams.SettingValues[ToolSettings.Null.Pen.StartCap.Path];
            LineCap2 endCap = (LineCap2) renderParams.SettingValues[ToolSettings.Null.Pen.EndCap.Path];
            Geometry outlineFillGeometry = null;
            if (((startCap != LineCap2.Flat) || (endCap != LineCap2.Flat)) || (gdipDashStyle != System.Drawing.Drawing2D.DashStyle.Solid))
            {
                PaintDotNet.UI.Media.DashStyle dashStyle = DashStyleUtil.ToMedia(gdipDashStyle);
                outlineFillGeometry = CreateStrokedLineGeometry(lineGeometry, (double) num, startCap, endCap, dashStyle);
            }
            return new ShapeRenderData(guideGeometry, null, lineGeometry, outlineFillGeometry);
        }

        protected override IEnumerable<string> OnGetRenderSettingPaths()
        {
            string[] tails = new string[] { ToolSettings.Null.Pen.Width.Path, ToolSettings.Null.Pen.StartCap.Path, ToolSettings.Null.Pen.DashStyle.Path, ToolSettings.Null.Pen.EndCap.Path, ToolSettings.Null.Shapes.CurveType.Path };
            return base.OnGetRenderSettingPaths().Concat<string>(tails);
        }

        protected sealed override void OnSetImagePropertyCollectionValues(ShapeRenderParameters renderParams, PropertyCollection properties)
        {
            for (int i = 0; i < imageControlPointUnitOffsets.Length; i++)
            {
                VectorDouble num2 = imageControlPointUnitOffsets[i];
                VectorDouble num3 = new VectorDouble(1.0 - num2.X, 1.0 - num2.Y);
                PointDouble num4 = new PointDouble((renderParams.StartPoint.X * num2.X) + (renderParams.EndPoint.X * num3.X), (renderParams.StartPoint.Y * num2.Y) + (renderParams.EndPoint.Y * num3.Y));
                properties[this.GetControlPointPropertyName(i)].Value = new Pair<double, double>(num4.X, num4.Y);
            }
            base.OnSetImagePropertyCollectionValues(renderParams, properties);
        }

        [IteratorStateMachine(typeof(<TrimPolyLine>d__22))]
        private static IEnumerable<PointDouble> TrimPolyLine(IEnumerable<PointDouble> points, double startPointDistance, double endPointDistance) => 
            new <TrimPolyLine>d__22(-2) { 
                <>3__points = points,
                <>3__startPointDistance = startPointDistance,
                <>3__endPointDistance = endPointDistance
            };

        [CompilerGenerated]
        private sealed class <GetControlPointUnitOffsets>d__11 : IEnumerable<double>, IEnumerable, IEnumerator<double>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private double <>2__current;
            public int <>3__count;
            private int <>l__initialThreadId;
            private int <i>5__1;
            private int count;

            [DebuggerHidden]
            public <GetControlPointUnitOffsets>d__11(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<i>5__1 = 0;
                    while (this.<i>5__1 < this.count)
                    {
                        this.<>2__current = ((double) this.<i>5__1) / ((double) (this.count - 1));
                        this.<>1__state = 1;
                        return true;
                    Label_0040:
                        this.<>1__state = -1;
                        int num2 = this.<i>5__1 + 1;
                        this.<i>5__1 = num2;
                    }
                    return false;
                }
                if (num != 1)
                {
                    return false;
                }
                goto Label_0040;
            }

            [DebuggerHidden]
            IEnumerator<double> IEnumerable<double>.GetEnumerator()
            {
                LineCurveShape.<GetControlPointUnitOffsets>d__11 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new LineCurveShape.<GetControlPointUnitOffsets>d__11(0);
                }
                d__.count = this.<>3__count;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Double>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            double IEnumerator<double>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <GetInitialControlPointValues>d__12 : IEnumerable<PointDouble>, IEnumerable, IEnumerator<PointDouble>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PointDouble <>2__current;
            public ShapeRenderParameters <>3__renderParams;
            public LineCurveShape <>4__this;
            private IEnumerator<double> <>7__wrap1;
            private int <>l__initialThreadId;
            private Geometry <lineGeometry>5__1;
            private double <lineGeometryLength>5__2;
            private ShapeRenderParameters renderParams;

            [DebuggerHidden]
            public <GetInitialControlPointValues>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<lineGeometry>5__1 = LineCurveShape.CreateGuideGeometry(this.renderParams);
                        this.<lineGeometryLength>5__2 = this.<lineGeometry>5__1.GetLength(LineCurveShape.flatteningTolerance);
                        this.<>7__wrap1 = this.<>4__this.GetControlPointUnitOffsets(this.<>4__this.controlPointPropertyNames.Length).GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            double current = this.<>7__wrap1.Current;
                            PointDouble point = this.<lineGeometry>5__1.GetPointAtLength(this.<lineGeometryLength>5__2 * current, LineCurveShape.flatteningTolerance).Point;
                            this.<>2__current = point;
                            this.<>1__state = 1;
                            return true;
                        Label_00B4:
                            this.<>1__state = -3;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_00B4;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<PointDouble> IEnumerable<PointDouble>.GetEnumerator()
            {
                LineCurveShape.<GetInitialControlPointValues>d__12 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new LineCurveShape.<GetInitialControlPointValues>d__12(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.renderParams = this.<>3__renderParams;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.PointDouble>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case -3:
                    case 1:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            PointDouble IEnumerator<PointDouble>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <TrimPolyLine>d__22 : IEnumerable<PointDouble>, IEnumerable, IEnumerator<PointDouble>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PointDouble <>2__current;
            public double <>3__endPointDistance;
            public IEnumerable<PointDouble> <>3__points;
            public double <>3__startPointDistance;
            private IEnumerator<PointDouble> <>7__wrap1;
            private int <>l__initialThreadId;
            private PointDouble? <lastPoint>5__5;
            private PointDouble <point>5__6;
            private LineSegmentDouble <segment>5__4;
            private double <segmentEndDistance>5__3;
            private double <segmentStartDistance>5__2;
            private double <walkedDistance>5__1;
            private double endPointDistance;
            private IEnumerable<PointDouble> points;
            private double startPointDistance;

            [DebuggerHidden]
            public <TrimPolyLine>d__22(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                this.<lastPoint>5__5 = new PointDouble?(this.<point>5__6);
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<walkedDistance>5__1 = 0.0;
                            this.<lastPoint>5__5 = null;
                            this.<>7__wrap1 = this.points.GetEnumerator();
                            this.<>1__state = -3;
                            goto Label_022E;

                        case 1:
                            goto Label_013C;

                        case 2:
                            goto Label_0192;

                        case 3:
                            this.<>1__state = -4;
                            flag = false;
                            goto Label_021A;

                        case 4:
                            goto Label_027E;

                        default:
                            return false;
                    }
                Label_0068:
                    this.<point>5__6 = this.<>7__wrap1.Current;
                    this.<>1__state = -4;
                    if (!this.<lastPoint>5__5.HasValue)
                    {
                        goto Label_0202;
                    }
                    this.<segment>5__4 = new LineSegmentDouble(this.<lastPoint>5__5.Value, this.<point>5__6);
                    double length = this.<segment>5__4.Length;
                    this.<segmentStartDistance>5__2 = this.<walkedDistance>5__1;
                    this.<segmentEndDistance>5__3 = this.<walkedDistance>5__1 + length;
                    this.<walkedDistance>5__1 = this.<segmentEndDistance>5__3;
                    if (this.<segmentStartDistance>5__2 > this.endPointDistance)
                    {
                        goto Label_020A;
                    }
                    if (this.<segmentEndDistance>5__3 < this.startPointDistance)
                    {
                        goto Label_0212;
                    }
                    if ((this.<segmentStartDistance>5__2 < this.startPointDistance) || (this.<segmentStartDistance>5__2 > this.endPointDistance))
                    {
                        goto Label_0144;
                    }
                    this.<>2__current = this.<lastPoint>5__5.Value;
                    this.<>1__state = 1;
                    return true;
                Label_013C:
                    this.<>1__state = -4;
                Label_0144:
                    if ((this.startPointDistance <= this.<segmentStartDistance>5__2) || (this.startPointDistance >= this.<segmentEndDistance>5__3))
                    {
                        goto Label_019A;
                    }
                    double num3 = this.startPointDistance - this.<segmentStartDistance>5__2;
                    PointDouble pointAtLength = LineCurveShape.GetPointAtLength(this.<segment>5__4, num3);
                    this.<>2__current = pointAtLength;
                    this.<>1__state = 2;
                    return true;
                Label_0192:
                    this.<>1__state = -4;
                Label_019A:
                    if ((this.endPointDistance > this.<segmentStartDistance>5__2) && (this.endPointDistance < this.<segmentEndDistance>5__3))
                    {
                        double num5 = this.endPointDistance - this.<segmentStartDistance>5__2;
                        PointDouble num6 = LineCurveShape.GetPointAtLength(this.<segment>5__4, num5);
                        this.<>2__current = num6;
                        this.<>1__state = 3;
                        return true;
                    }
                    this.<segment>5__4 = new LineSegmentDouble();
                Label_0202:
                    this.<>m__Finally2();
                    goto Label_0222;
                Label_020A:
                    this.<>m__Finally2();
                    goto Label_023E;
                Label_0212:
                    this.<>m__Finally2();
                    goto Label_022E;
                Label_021A:
                    this.<>m__Finally2();
                    goto Label_0246;
                Label_0222:
                    this.<point>5__6 = new PointDouble();
                Label_022E:
                    if (this.<>7__wrap1.MoveNext())
                    {
                        goto Label_0068;
                    }
                Label_023E:
                    this.<>m__Finally1();
                    goto Label_024E;
                Label_0246:
                    this.<>m__Finally1();
                    return flag;
                Label_024E:
                    this.<>7__wrap1 = null;
                    if (!this.<lastPoint>5__5.HasValue)
                    {
                        goto Label_0285;
                    }
                    this.<>2__current = this.<lastPoint>5__5.Value;
                    this.<>1__state = 4;
                    return true;
                Label_027E:
                    this.<>1__state = -1;
                Label_0285:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<PointDouble> IEnumerable<PointDouble>.GetEnumerator()
            {
                LineCurveShape.<TrimPolyLine>d__22 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new LineCurveShape.<TrimPolyLine>d__22(0);
                }
                d__.points = this.<>3__points;
                d__.startPointDistance = this.<>3__startPointDistance;
                d__.endPointDistance = this.<>3__endPointDistance;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.PointDouble>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                switch (num)
                {
                    case -4:
                    case -3:
                    case 1:
                    case 2:
                    case 3:
                        try
                        {
                            switch (num)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case -4:
                                    break;

                                default:
                                    break;
                            }
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally2();
                            }
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;

                    case -2:
                    case -1:
                    case 0:
                        break;

                    default:
                        return;
                }
            }

            PointDouble IEnumerator<PointDouble>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

