namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class CompassHandleDrawing : HandleDrawing
    {
        private GeometryDrawing backFillDrawing;
        private RectangleGeometry backFillGeometry;
        private DependencyFunc<double, RectDouble> bounds = new DependencyFunc<double, RectDouble>(r => RectDouble.FromCenter(PointDouble.Zero, (double) (r * 2.0)));
        private GeometryDrawing compassDrawing;
        private PathGeometry compassGeometry;
        private Pen outlinePen;
        private DependencyValue<double> s;
        private DependencyFunc<double, double> sOver2;
        private DependencyFunc<double, double> st;
        private DependencyFunc<double, double> st3Over2;
        private DependencyFunc<double, double> stSqrt27Over2;

        static CompassHandleDrawing()
        {
            HandleDrawing.RadiusProperty.OverrideMetadata(typeof(CompassHandleDrawing), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(6.5)));
        }

        public CompassHandleDrawing()
        {
            this.bounds.SetBinding(DependencyFuncBase<double, RectDouble>.Arg1Property, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.outlinePen = new Pen();
            this.outlinePen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty), BindingMode.OneWay);
            this.outlinePen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty), BindingMode.OneWay);
            this.backFillDrawing = new GeometryDrawing();
            this.backFillDrawing.Pen = this.outlinePen;
            this.backFillDrawing.SetBinding(GeometryDrawing.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.BackgroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backFillGeometry = new RectangleGeometry();
            this.backFillGeometry.SetBinding(RectangleGeometry.RectProperty, this.bounds, new PaintDotNet.ObjectModel.PropertyPath(DependencyValue<RectDouble>.ValueProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backFillDrawing.Geometry = this.backFillGeometry;
            base.DrawingGroup.Children.Add(this.backFillDrawing);
            this.compassDrawing = new GeometryDrawing();
            this.compassDrawing.SetBinding(GeometryDrawing.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.compassGeometry = new PathGeometry();
            this.s = new DependencyValue<double>();
            this.s.SetBinding(this.s.GetValueProperty(), this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.RadiusScaleProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.st = new DependencyFunc<double, double>(s => s * 1.35, this.s);
            this.st3Over2 = new DependencyFunc<double, double>(st => (st * 3.0) / 2.0, this.st);
            this.sOver2 = new DependencyFunc<double, double>(s => s / 2.0, this.s);
            this.stSqrt27Over2 = new DependencyFunc<double, double>(st => (st * Math.Sqrt(27.0)) / 2.0, this.st);
            PathFigure targetObject = new PathFigure {
                IsFilled = true,
                IsClosed = true
            };
            targetObject.SetBinding<RectDouble, PointDouble>(PathFigure.StartPointProperty, this.bounds, new PaintDotNet.ObjectModel.PropertyPath(DependencyValue<RectDouble>.ValueProperty), BindingMode.OneWay, b => b.LeftCenter());
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.st3Over2, (bounds, stSqrt27Over2, st3over2) => new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y + st3over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.sOver2, (bounds, stSqrt27Over2, sOver2) => new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y + sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double>(this.bounds, this.sOver2, (bounds, sOver2) => new PointDouble(bounds.Center.X - sOver2, bounds.Center.Y + sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.sOver2, this.stSqrt27Over2, (bounds, sOver2, stSqrt27Over2) => new PointDouble(bounds.BottomCenter().X - sOver2, bounds.BottomCenter().Y - stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.st3Over2, this.stSqrt27Over2, (bounds, st3Over2, stSqrt27Over2) => new PointDouble(bounds.BottomCenter().X - st3Over2, bounds.BottomCenter().Y - stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble>(this.bounds, bounds => bounds.BottomCenter()));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.st3Over2, this.stSqrt27Over2, (bounds, st3Over2, stSqrt27Over2) => new PointDouble(bounds.BottomCenter().X + st3Over2, bounds.BottomCenter().Y - stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.sOver2, this.stSqrt27Over2, (bounds, sOver2, stSqrt27Over2) => new PointDouble(bounds.BottomCenter().X + sOver2, bounds.BottomCenter().Y - stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double>(this.bounds, this.sOver2, (bounds, sOver2) => new PointDouble(bounds.Center.X + sOver2, bounds.Center.Y + sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.sOver2, (bounds, stSqrt27Over2, sOver2) => new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y + sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.st3Over2, (bounds, stSqrt27Over2, st3Over2) => new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y + st3Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble>(this.bounds, bounds => bounds.RightCenter()));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.st3Over2, (bounds, stSqrt27Over2, st3Over2) => new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y - st3Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.sOver2, (bounds, stSqrt27Over2, sOver2) => new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y - sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double>(this.bounds, this.sOver2, (bounds, sOver2) => new PointDouble(bounds.Center.X + sOver2, bounds.Center.Y - sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.sOver2, this.stSqrt27Over2, (bounds, sOver2, stSqrt27Over2) => new PointDouble(bounds.TopCenter().X + sOver2, bounds.TopCenter().Y + stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.st3Over2, this.stSqrt27Over2, (bounds, st3Over2, stSqrt27Over2) => new PointDouble(bounds.TopCenter().X + st3Over2, bounds.TopCenter().Y + stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble>(this.bounds, bounds => bounds.TopCenter()));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.st3Over2, this.stSqrt27Over2, (bounds, st3Over2, stSqrt27Over2) => new PointDouble(bounds.TopCenter().X - st3Over2, bounds.TopCenter().Y + stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.sOver2, this.stSqrt27Over2, (bounds, sOver2, stSqrt27Over2) => new PointDouble(bounds.TopCenter().X - sOver2, bounds.TopCenter().Y + stSqrt27Over2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double>(this.bounds, this.sOver2, (bounds, sOver2) => new PointDouble(bounds.Center.X - sOver2, bounds.Center.Y - sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.sOver2, (bounds, stSqrt27Over2, sOver2) => new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y - sOver2)));
            targetObject.Segments.Add(CreateLineSegment<RectDouble, double, double>(this.bounds, this.stSqrt27Over2, this.st3Over2, (bounds, stSqrt27Over2, st3Over2) => new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y - st3Over2)));
            this.compassGeometry.Figures.Add(targetObject);
            this.compassDrawing.Geometry = this.compassGeometry;
            base.DrawingGroup.Children.Add(this.compassDrawing);
        }

        protected override Freezable CreateInstanceCore() => 
            new CompassHandleDrawing();

        private static LineSegment CreateLineSegment<TArg1>(DependencyValue<TArg1> arg1, Func<TArg1, PointDouble> convertToPointFn)
        {
            LineSegment targetObject = new LineSegment();
            targetObject.SetBinding<TArg1, PointDouble>(LineSegment.PointProperty, arg1, new PaintDotNet.ObjectModel.PropertyPath(arg1.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay, convertToPointFn);
            return targetObject;
        }

        private static LineSegment CreateLineSegment<TArg1, TArg2>(DependencyValue<TArg1> arg1, DependencyValue<TArg2> arg2, Func<TArg1, TArg2, PointDouble> convertToPointFn)
        {
            LineSegment targetObject = new LineSegment();
            object[] sourceObjects = new object[] { arg1, arg2 };
            PaintDotNet.ObjectModel.PropertyPath[] sourcePaths = new PaintDotNet.ObjectModel.PropertyPath[] { new PaintDotNet.ObjectModel.PropertyPath(arg1.GetValueProperty().Name, Array.Empty<object>()), new PaintDotNet.ObjectModel.PropertyPath(arg2.GetValueProperty().Name, Array.Empty<object>()) };
            targetObject.SetMultiBinding(LineSegment.PointProperty, sourceObjects, sourcePaths, BindingMode.OneWay, values => convertToPointFn((TArg1) values[0], (TArg2) values[1]), null);
            return targetObject;
        }

        private static LineSegment CreateLineSegment<TArg1, TArg2, TArg3>(DependencyValue<TArg1> arg1, DependencyValue<TArg2> arg2, DependencyValue<TArg3> arg3, Func<TArg1, TArg2, TArg3, PointDouble> convertToPointFn)
        {
            LineSegment targetObject = new LineSegment();
            object[] sourceObjects = new object[] { arg1, arg2, arg3 };
            PaintDotNet.ObjectModel.PropertyPath[] sourcePaths = new PaintDotNet.ObjectModel.PropertyPath[] { new PaintDotNet.ObjectModel.PropertyPath(arg1.GetValueProperty().Name, Array.Empty<object>()), new PaintDotNet.ObjectModel.PropertyPath(arg2.GetValueProperty().Name, Array.Empty<object>()), new PaintDotNet.ObjectModel.PropertyPath(arg3.GetValueProperty().Name, Array.Empty<object>()) };
            targetObject.SetMultiBinding(LineSegment.PointProperty, sourceObjects, sourcePaths, BindingMode.OneWay, values => convertToPointFn((TArg1) values[0], (TArg2) values[1], (TArg3) values[2]), null);
            return targetObject;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CompassHandleDrawing.<>c <>9 = new CompassHandleDrawing.<>c();
            public static Func<double, RectDouble> <>9__12_0;
            public static Func<double, double> <>9__12_1;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_10;
            public static Func<RectDouble, PointDouble> <>9__12_11;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_12;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_13;
            public static Func<RectDouble, double, PointDouble> <>9__12_14;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_15;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_16;
            public static Func<RectDouble, PointDouble> <>9__12_17;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_18;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_19;
            public static Func<double, double> <>9__12_2;
            public static Func<RectDouble, double, PointDouble> <>9__12_20;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_21;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_22;
            public static Func<RectDouble, PointDouble> <>9__12_23;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_24;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_25;
            public static Func<RectDouble, double, PointDouble> <>9__12_26;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_27;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_28;
            public static Func<double, double> <>9__12_3;
            public static Func<double, double> <>9__12_4;
            public static Func<RectDouble, PointDouble> <>9__12_5;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_6;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_7;
            public static Func<RectDouble, double, PointDouble> <>9__12_8;
            public static Func<RectDouble, double, double, PointDouble> <>9__12_9;

            internal RectDouble <.ctor>b__12_0(double r) => 
                RectDouble.FromCenter(PointDouble.Zero, (double) (r * 2.0));

            internal double <.ctor>b__12_1(double s) => 
                (s * 1.35);

            internal PointDouble <.ctor>b__12_10(RectDouble bounds, double st3Over2, double stSqrt27Over2) => 
                new PointDouble(bounds.BottomCenter().X - st3Over2, bounds.BottomCenter().Y - stSqrt27Over2);

            internal PointDouble <.ctor>b__12_11(RectDouble bounds) => 
                bounds.BottomCenter();

            internal PointDouble <.ctor>b__12_12(RectDouble bounds, double st3Over2, double stSqrt27Over2) => 
                new PointDouble(bounds.BottomCenter().X + st3Over2, bounds.BottomCenter().Y - stSqrt27Over2);

            internal PointDouble <.ctor>b__12_13(RectDouble bounds, double sOver2, double stSqrt27Over2) => 
                new PointDouble(bounds.BottomCenter().X + sOver2, bounds.BottomCenter().Y - stSqrt27Over2);

            internal PointDouble <.ctor>b__12_14(RectDouble bounds, double sOver2) => 
                new PointDouble(bounds.Center.X + sOver2, bounds.Center.Y + sOver2);

            internal PointDouble <.ctor>b__12_15(RectDouble bounds, double stSqrt27Over2, double sOver2) => 
                new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y + sOver2);

            internal PointDouble <.ctor>b__12_16(RectDouble bounds, double stSqrt27Over2, double st3Over2) => 
                new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y + st3Over2);

            internal PointDouble <.ctor>b__12_17(RectDouble bounds) => 
                bounds.RightCenter();

            internal PointDouble <.ctor>b__12_18(RectDouble bounds, double stSqrt27Over2, double st3Over2) => 
                new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y - st3Over2);

            internal PointDouble <.ctor>b__12_19(RectDouble bounds, double stSqrt27Over2, double sOver2) => 
                new PointDouble(bounds.RightCenter().X - stSqrt27Over2, bounds.RightCenter().Y - sOver2);

            internal double <.ctor>b__12_2(double st) => 
                ((st * 3.0) / 2.0);

            internal PointDouble <.ctor>b__12_20(RectDouble bounds, double sOver2) => 
                new PointDouble(bounds.Center.X + sOver2, bounds.Center.Y - sOver2);

            internal PointDouble <.ctor>b__12_21(RectDouble bounds, double sOver2, double stSqrt27Over2) => 
                new PointDouble(bounds.TopCenter().X + sOver2, bounds.TopCenter().Y + stSqrt27Over2);

            internal PointDouble <.ctor>b__12_22(RectDouble bounds, double st3Over2, double stSqrt27Over2) => 
                new PointDouble(bounds.TopCenter().X + st3Over2, bounds.TopCenter().Y + stSqrt27Over2);

            internal PointDouble <.ctor>b__12_23(RectDouble bounds) => 
                bounds.TopCenter();

            internal PointDouble <.ctor>b__12_24(RectDouble bounds, double st3Over2, double stSqrt27Over2) => 
                new PointDouble(bounds.TopCenter().X - st3Over2, bounds.TopCenter().Y + stSqrt27Over2);

            internal PointDouble <.ctor>b__12_25(RectDouble bounds, double sOver2, double stSqrt27Over2) => 
                new PointDouble(bounds.TopCenter().X - sOver2, bounds.TopCenter().Y + stSqrt27Over2);

            internal PointDouble <.ctor>b__12_26(RectDouble bounds, double sOver2) => 
                new PointDouble(bounds.Center.X - sOver2, bounds.Center.Y - sOver2);

            internal PointDouble <.ctor>b__12_27(RectDouble bounds, double stSqrt27Over2, double sOver2) => 
                new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y - sOver2);

            internal PointDouble <.ctor>b__12_28(RectDouble bounds, double stSqrt27Over2, double st3Over2) => 
                new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y - st3Over2);

            internal double <.ctor>b__12_3(double s) => 
                (s / 2.0);

            internal double <.ctor>b__12_4(double st) => 
                ((st * Math.Sqrt(27.0)) / 2.0);

            internal PointDouble <.ctor>b__12_5(RectDouble b) => 
                b.LeftCenter();

            internal PointDouble <.ctor>b__12_6(RectDouble bounds, double stSqrt27Over2, double st3over2) => 
                new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y + st3over2);

            internal PointDouble <.ctor>b__12_7(RectDouble bounds, double stSqrt27Over2, double sOver2) => 
                new PointDouble(bounds.LeftCenter().X + stSqrt27Over2, bounds.LeftCenter().Y + sOver2);

            internal PointDouble <.ctor>b__12_8(RectDouble bounds, double sOver2) => 
                new PointDouble(bounds.Center.X - sOver2, bounds.Center.Y + sOver2);

            internal PointDouble <.ctor>b__12_9(RectDouble bounds, double sOver2, double stSqrt27Over2) => 
                new PointDouble(bounds.BottomCenter().X - sOver2, bounds.BottomCenter().Y - stSqrt27Over2);
        }
    }
}

