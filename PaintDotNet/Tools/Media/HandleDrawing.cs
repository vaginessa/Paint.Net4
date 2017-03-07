namespace PaintDotNet.Tools.Media
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal abstract class HandleDrawing : Drawing
    {
        public static readonly DependencyProperty AutoScaleWithDpiProperty = DependencyProperty.Register("AutoScaleWithDpi", typeof(bool), typeof(HandleDrawing), new PropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__40_0)));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(HandleDrawing), new PropertyMetadata(SolidColorBrushCache.Get((ColorRgba128Float) Colors.White)));
        private PaintDotNet.UI.Media.DrawingGroup drawingGroup = new PaintDotNet.UI.Media.DrawingGroup();
        private ScaleTransform drawingGroupTransform = new ScaleTransform();
        public static readonly DependencyProperty EffectiveRadiusProperty = EffectiveRadiusPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey EffectiveRadiusPropertyKey = DependencyProperty.RegisterReadOnly("EffectiveRadius", typeof(double), typeof(HandleDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(0.0), null, new CoerceValueCallback(<>c.<>9.<.cctor>b__40_3)));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(HandleDrawing), new PropertyMetadata(SolidColorBrushCache.Get((ColorRgba128Float) Colors.Black)));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(HandleDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(3.0), new PropertyChangedCallback(<>c.<>9.<.cctor>b__40_1)));
        public static readonly DependencyProperty RadiusScaleProperty = DependencyProperty.Register("RadiusScale", typeof(double), typeof(HandleDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(1.0), new PropertyChangedCallback(<>c.<>9.<.cctor>b__40_2)));
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register("Thickness", typeof(double), typeof(HandleDrawing), new PropertyMetadata(DoubleUtil.GetBoxed(1.0)));

        protected HandleDrawing()
        {
            this.UpdateDrawingGroupTransform();
            this.drawingGroup.Transform = this.drawingGroupTransform;
            base.OnFreezablePropertyChanged(null, this.drawingGroup);
            base.CoerceValue(EffectiveRadiusProperty);
        }

        private void AutoScaleWithDpiPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateDrawingGroupTransform();
        }

        private object CoerceEffectiveRadiusProperty(object baseValue) => 
            DoubleUtil.GetBoxed(this.Radius * this.RadiusScale);

        protected override Geometry CreateClip(bool isStroked) => 
            this.drawingGroup?.GetClip(isStroked);

        protected override void OnRender(IDrawingContext dc)
        {
            if (this.drawingGroup != null)
            {
                this.drawingGroup.Render(dc);
            }
        }

        private void RadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(EffectiveRadiusProperty);
        }

        private void RadiusScalePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(EffectiveRadiusProperty);
        }

        private void UpdateDrawingGroupTransform()
        {
            if (this.AutoScaleWithDpi)
            {
                this.drawingGroupTransform.ScaleX = UIUtil.ScaleWidth((double) 1.0);
                this.drawingGroupTransform.ScaleY = UIUtil.ScaleHeight((double) 1.0);
            }
            else
            {
                this.drawingGroupTransform.ScaleX = 1.0;
                this.drawingGroupTransform.ScaleY = 1.0;
            }
        }

        public bool AutoScaleWithDpi
        {
            get => 
                ((bool) base.GetValue(AutoScaleWithDpiProperty));
            set
            {
                base.SetValue(AutoScaleWithDpiProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public Brush Background
        {
            get => 
                ((Brush) base.GetValue(BackgroundProperty));
            set
            {
                base.SetValue(BackgroundProperty, value);
            }
        }

        protected PaintDotNet.UI.Media.DrawingGroup DrawingGroup =>
            this.drawingGroup;

        public double EffectiveRadius =>
            ((double) base.GetValue(EffectiveRadiusProperty));

        public Brush Foreground
        {
            get => 
                ((Brush) base.GetValue(ForegroundProperty));
            set
            {
                base.SetValue(ForegroundProperty, value);
            }
        }

        public double Radius
        {
            get => 
                ((double) base.GetValue(RadiusProperty));
            set
            {
                base.SetValue(RadiusProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public double RadiusScale
        {
            get => 
                ((double) base.GetValue(RadiusScaleProperty));
            set
            {
                base.SetValue(RadiusScaleProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public double Thickness
        {
            get => 
                ((double) base.GetValue(ThicknessProperty));
            set
            {
                base.SetValue(ThicknessProperty, DoubleUtil.GetBoxed(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly HandleDrawing.<>c <>9 = new HandleDrawing.<>c();

            internal void <.cctor>b__40_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((HandleDrawing) s).AutoScaleWithDpiPropertyChanged(e);
            }

            internal void <.cctor>b__40_1(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((HandleDrawing) s).RadiusPropertyChanged(e);
            }

            internal void <.cctor>b__40_2(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((HandleDrawing) s).RadiusScalePropertyChanged(e);
            }

            internal object <.cctor>b__40_3(DependencyObject dO, object bV) => 
                ((HandleDrawing) dO).CoerceEffectiveRadiusProperty(bV);
        }
    }
}

