namespace PaintDotNet.Tools.Media
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class CircleHandleDrawing : HandleDrawing
    {
        private GeometryDrawing backgroundDrawing = new GeometryDrawing();
        private EllipseGeometry backgroundGeometry;
        private Pen backgroundPen = new Pen();
        private GeometryDrawing foregroundDrawing;
        private EllipseGeometry foregroundGeometry;
        private Pen foregroundPen;

        public CircleHandleDrawing()
        {
            this.backgroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => ((double) t) * 2.5);
            this.backgroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.BackgroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backgroundDrawing.Pen = this.backgroundPen;
            this.backgroundGeometry = new EllipseGeometry();
            this.backgroundGeometry.SetBinding(EllipseGeometry.RadiusXProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backgroundGeometry.SetBinding(EllipseGeometry.RadiusYProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backgroundDrawing.Geometry = this.backgroundGeometry;
            base.DrawingGroup.Children.Add(this.backgroundDrawing);
            this.foregroundDrawing = new GeometryDrawing();
            this.foregroundPen = new Pen();
            this.foregroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundDrawing.Pen = this.foregroundPen;
            this.foregroundGeometry = new EllipseGeometry();
            this.foregroundGeometry.SetBinding(EllipseGeometry.RadiusXProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundGeometry.SetBinding(EllipseGeometry.RadiusYProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundDrawing.Geometry = this.foregroundGeometry;
            base.DrawingGroup.Children.Add(this.foregroundDrawing);
        }

        protected override Freezable CreateInstanceCore() => 
            new CircleHandleDrawing();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CircleHandleDrawing.<>c <>9 = new CircleHandleDrawing.<>c();
            public static Func<object, object> <>9__6_0;

            internal object <.ctor>b__6_0(object t) => 
                (((double) t) * 2.5);
        }
    }
}

