namespace PaintDotNet.Tools.Media
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class SquareHandleDrawing : HandleDrawing
    {
        private GeometryDrawing backgroundDrawing = new GeometryDrawing();
        private RectangleGeometry backgroundGeometry;
        private Pen backgroundPen = new Pen();
        private GeometryDrawing foregroundDrawing;
        private RectangleGeometry foregroundGeometry;
        private Pen foregroundPen;

        public SquareHandleDrawing()
        {
            this.backgroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay, t => ((double) t) + 2.5);
            this.backgroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.BackgroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.backgroundDrawing.Pen = this.backgroundPen;
            this.backgroundGeometry = new RectangleGeometry();
            this.backgroundGeometry.SetBinding<double, RectDouble>(RectangleGeometry.RectProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, new Func<double, RectDouble>(this.GetGeometryRect));
            this.backgroundDrawing.Geometry = this.backgroundGeometry;
            base.DrawingGroup.Children.Add(this.backgroundDrawing);
            this.foregroundDrawing = new GeometryDrawing();
            this.foregroundPen = new Pen();
            this.foregroundPen.SetBinding(Pen.ThicknessProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ThicknessProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundPen.SetBinding(Pen.BrushProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.ForegroundProperty.Name, Array.Empty<object>()), BindingMode.OneWay);
            this.foregroundDrawing.Pen = this.foregroundPen;
            this.foregroundGeometry = new RectangleGeometry();
            this.foregroundGeometry.SetBinding<double, RectDouble>(RectangleGeometry.RectProperty, this, new PaintDotNet.ObjectModel.PropertyPath(HandleDrawing.EffectiveRadiusProperty.Name, Array.Empty<object>()), BindingMode.OneWay, new Func<double, RectDouble>(this.GetGeometryRect));
            this.foregroundDrawing.Geometry = this.foregroundGeometry;
            base.DrawingGroup.Children.Add(this.foregroundDrawing);
        }

        protected override Freezable CreateInstanceCore() => 
            new SquareHandleDrawing();

        private RectDouble GetGeometryRect(double radius) => 
            RectDouble.FromCenter(PointDouble.Zero, (double) (radius * 2.0));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SquareHandleDrawing.<>c <>9 = new SquareHandleDrawing.<>c();
            public static Func<object, object> <>9__6_0;

            internal object <.ctor>b__6_0(object t) => 
                (((double) t) + 2.5);
        }
    }
}

