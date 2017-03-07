namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal class HandleElement : DrawingElement
    {
        private const double hotRadiusAnimationDuration = 0.2;
        private const double hotRadiusScale = 1.4;
        public static readonly DependencyProperty IsHotOnMouseOverProperty = FrameworkProperty.Register("IsHotOnMouseOver", typeof(bool), typeof(HandleElement), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__3_0)));
        public static readonly DependencyProperty IsHotProperty = FrameworkProperty.Register("IsHot", typeof(bool), typeof(HandleElement), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(<>c.<>9.<.cctor>b__3_1), null, new CoerceValueCallback(<>c.<>9.<.cctor>b__3_2)));
        private AnimatedDouble radiusScale;

        static HandleElement()
        {
            Visual.VisualOpacityGranularityProperty.OverrideMetadata(typeof(HandleElement), new VisualPropertyMetadata(DoubleUtil.GetBoxed(15.0)));
        }

        public HandleElement() : this(null)
        {
        }

        public HandleElement(HandleDrawing drawing)
        {
            this.radiusScale = new AnimatedDouble(1.0);
            this.Drawing = drawing;
        }

        private object CoerceIsHotProperty(object baseValue)
        {
            if (this.IsHotOnMouseOver && base.IsMouseOver)
            {
                return BooleanUtil.GetBoxed(true);
            }
            return baseValue;
        }

        private void IsHotOnMouseOverPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(IsHotProperty);
        }

        private void IsHotPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            double finalValue = ((bool) e.NewValue) ? 1.4 : 1.0;
            if (this.radiusScale.FinalValue != finalValue)
            {
                this.radiusScale.AnimateValueTo(finalValue, 0.2, AnimationTransitionType.SmoothStop);
            }
        }

        protected virtual void OnDrawingChanged(HandleDrawing oldValue, HandleDrawing newValue)
        {
        }

        protected sealed override void OnDrawingChanged(PaintDotNet.UI.Media.Drawing oldValue, PaintDotNet.UI.Media.Drawing newValue)
        {
            if (oldValue != newValue)
            {
                if (oldValue != null)
                {
                    oldValue.ClearBinding(HandleDrawing.RadiusScaleProperty);
                }
                this.OnDrawingChanged((HandleDrawing) oldValue, (HandleDrawing) newValue);
                if (newValue != null)
                {
                    newValue.SetBinding(HandleDrawing.RadiusScaleProperty, this.radiusScale, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                }
            }
            base.OnDrawingChanged(oldValue, newValue);
        }

        protected override void OnIsMouseOverChanged(bool oldValue, bool newValue)
        {
            base.CoerceValue(IsHotProperty);
            base.OnIsMouseOverChanged(oldValue, newValue);
        }

        public HandleDrawing Drawing
        {
            get => 
                ((HandleDrawing) base.Drawing);
            set
            {
                base.Drawing = value;
            }
        }

        public bool IsHot
        {
            get => 
                ((bool) base.GetValue(IsHotProperty));
            set
            {
                base.SetValue(IsHotProperty, value);
            }
        }

        public bool IsHotOnMouseOver
        {
            get => 
                ((bool) base.GetValue(IsHotOnMouseOverProperty));
            set
            {
                base.SetValue(IsHotOnMouseOverProperty, BooleanUtil.GetBoxed(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly HandleElement.<>c <>9 = new HandleElement.<>c();

            internal void <.cctor>b__3_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((HandleElement) s).IsHotOnMouseOverPropertyChanged(e);
            }

            internal void <.cctor>b__3_1(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((HandleElement) s).IsHotPropertyChanged(e);
            }

            internal object <.cctor>b__3_2(DependencyObject dO, object bV) => 
                ((HandleElement) dO).CoerceIsHotProperty(bV);
        }
    }
}

