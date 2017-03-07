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

    internal sealed class ScrewHandleDrawing : HandleDrawing
    {
        private GeometryDrawing backgroundDrawing;
        private Pen backgroundPen;
        private GeometryDrawing foregroundDrawing;
        private Pen foregroundPen;
        private GeometryGroup screwGeometry = new GeometryGroup();

        static ScrewHandleDrawing()
        {
            HandleDrawing.RadiusProperty.OverrideMetadata(typeof(ScrewHandleDrawing), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(4.0)));
        }

        public ScrewHandleDrawing()
        {
            EllipseGeometry targetObject = new EllipseGeometry();
            targetObject.SetBinding(EllipseGeometry.RadiusXProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            targetObject.SetBinding(EllipseGeometry.RadiusYProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.screwGeometry.Children.Add(targetObject);
            LineGeometry geometry2 = new LineGeometry();
            geometry2.SetBinding<double, PointDouble>(LineGeometry.StartPointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, r => new PointDouble(0.5 - r, 0.0));
            geometry2.SetBinding<double, PointDouble>(LineGeometry.EndPointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, r => new PointDouble(r - 0.5, 0.0));
            this.screwGeometry.Children.Add(geometry2);
            LineGeometry geometry3 = new LineGeometry();
            geometry3.SetBinding<double, PointDouble>(LineGeometry.StartPointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, r => new PointDouble(0.0, 0.5 - r));
            geometry3.SetBinding<double, PointDouble>(LineGeometry.EndPointProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, r => new PointDouble(0.0, r - 0.5));
            this.screwGeometry.Children.Add(geometry3);
            this.backgroundDrawing = new GeometryDrawing();
            this.backgroundPen = new Pen();
            this.backgroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => ((double) t) + 2.5);
            this.backgroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.BackgroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backgroundDrawing.Pen = this.backgroundPen;
            this.backgroundDrawing.Geometry = this.screwGeometry;
            base.DrawingGroup.Children.Add(this.backgroundDrawing);
            this.foregroundDrawing = new GeometryDrawing();
            this.foregroundPen = new Pen();
            this.foregroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundDrawing.Pen = this.foregroundPen;
            this.foregroundDrawing.Geometry = this.screwGeometry;
            base.DrawingGroup.Children.Add(this.foregroundDrawing);
        }

        protected override Geometry CreateClip(bool isStroked) => 
            base.CreateClip(isStroked);

        protected override Freezable CreateInstanceCore() => 
            new ScrewHandleDrawing();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ScrewHandleDrawing.<>c <>9 = new ScrewHandleDrawing.<>c();
            public static Func<double, PointDouble> <>9__6_0;
            public static Func<double, PointDouble> <>9__6_1;
            public static Func<double, PointDouble> <>9__6_2;
            public static Func<double, PointDouble> <>9__6_3;
            public static Func<object, object> <>9__6_4;

            internal PointDouble <.ctor>b__6_0(double r) => 
                new PointDouble(0.5 - r, 0.0);

            internal PointDouble <.ctor>b__6_1(double r) => 
                new PointDouble(r - 0.5, 0.0);

            internal PointDouble <.ctor>b__6_2(double r) => 
                new PointDouble(0.0, 0.5 - r);

            internal PointDouble <.ctor>b__6_3(double r) => 
                new PointDouble(0.0, r - 0.5);

            internal object <.ctor>b__6_4(object t) => 
                (((double) t) + 2.5);
        }
    }
}

