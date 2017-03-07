namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    internal class CanvasView : DependencyObject
    {
        public static readonly DependencyProperty CanvasExtentPaddingProperty = DependencyProperty.Register("CanvasExtentPadding", typeof(SizeDouble), typeof(CanvasView), new PropertyMetadata(SizeDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_3)));
        public static readonly DependencyProperty CanvasHairWidthProperty = CanvasHairWidthPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanvasHairWidthPropertyKey = DependencyProperty.RegisterReadOnly("CanvasHairWidth", typeof(double), typeof(CanvasView), new PropertyMetadata(DoubleUtil.GetBoxed(1.0), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_10), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_11)));
        public static readonly DependencyProperty CanvasProperty = DependencyProperty.Register("Canvas", typeof(PaintDotNet.Canvas.Canvas), typeof(CanvasView), new PropertyMetadata(null, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_0)));
        public static readonly DependencyProperty CanvasSizeProperty = DependencyProperty.Register("CanvasSize", typeof(SizeDouble), typeof(CanvasView), new PropertyMetadata(SizeDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_2)));
        public static readonly DependencyProperty FrameCanvasPaddingProperty = FrameCanvasPaddingPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey FrameCanvasPaddingPropertyKey = DependencyProperty.RegisterReadOnly("FrameCanvasPadding", typeof(ThicknessDouble), typeof(CanvasView), new PropertyMetadata(ThicknessDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_23), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_24)), new ValidateValueCallback(CanvasView.ValidateFrameCanvasPaddingProperty));
        public static readonly DependencyProperty FramedCanvasBoundsProperty = FramedCanvasBoundsPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey FramedCanvasBoundsPropertyKey = DependencyProperty.RegisterReadOnly("FramedCanvasBounds", typeof(RectDouble), typeof(CanvasView), new PropertyMetadata(RectDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_25), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_26)));
        public static readonly DependencyProperty IsCanvasFrameEnabledProperty = DependencyProperty.Register("IsCanvasFrameEnabled", typeof(bool), typeof(CanvasView), new PropertyMetadata(BooleanUtil.GetBoxed(true), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_22)));
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register("IsVisible", typeof(bool), typeof(CanvasView), new PropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_1)));
        private IRenderTarget renderTarget;
        public static readonly DependencyProperty ScaleBasisProperty = DependencyProperty.Register("ScaleBasis", typeof(PaintDotNet.Canvas.ScaleBasis), typeof(CanvasView), new PropertyMetadata(EnumUtil.GetBoxed<PaintDotNet.Canvas.ScaleBasis>(PaintDotNet.Canvas.ScaleBasis.Ratio), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_8), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_9)));
        public const double ScaleRatioMax = 32.0;
        public const double ScaleRatioMin = 0.01;
        public static readonly DependencyProperty ScaleRatioProperty = DependencyProperty.Register("ScaleRatio", typeof(double), typeof(CanvasView), new PropertyMetadata(DoubleUtil.GetBoxed(1.0), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_6), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_7)));
        public static readonly DependencyProperty SnapViewportOriginToDevicePixelsProperty = SnapViewportOriginToDevicePixelsPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey SnapViewportOriginToDevicePixelsPropertyKey = DependencyProperty.RegisterReadOnly("SnapViewportOriginToDevicePixels", typeof(bool), typeof(CanvasView), new PropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_4), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_5)));
        public static readonly DependencyProperty ViewportCanvasBoundsProperty = ViewportCanvasBoundsPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ViewportCanvasBoundsPropertyKey = DependencyProperty.RegisterReadOnly("ViewportCanvasBounds", typeof(RectDouble), typeof(CanvasView), new PropertyMetadata(RectDouble.Zero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_20), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_21)));
        public static readonly DependencyProperty ViewportCanvasOffsetMaxProperty = ViewportCanvasOffsetMaxPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ViewportCanvasOffsetMaxPropertyKey = DependencyProperty.RegisterReadOnly("ViewportCanvasOffsetMax", typeof(PointDouble), typeof(CanvasView), new PropertyMetadata(PointDouble.Zero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_18), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_19)));
        public static readonly DependencyProperty ViewportCanvasOffsetMinProperty = ViewportCanvasOffsetMinPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ViewportCanvasOffsetMinPropertyKey = DependencyProperty.RegisterReadOnly("ViewportCanvasOffsetMin", typeof(PointDouble), typeof(CanvasView), new PropertyMetadata(PointDouble.Zero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_16), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_17)));
        public static readonly DependencyProperty ViewportCanvasOffsetProperty = DependencyProperty.Register("ViewportCanvasOffset", typeof(PointDouble), typeof(CanvasView), new PropertyMetadata(PointDouble.Zero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_14), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_15)));
        public static readonly DependencyProperty ViewportSizeProperty = DependencyProperty.Register("ViewportSize", typeof(SizeDouble), typeof(CanvasView), new PropertyMetadata(SizeDouble.BoxedZero, new PropertyChangedCallback(<>c.<>9.<.cctor>b__150_12), new CoerceValueCallback(<>c.<>9.<.cctor>b__150_13)));

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PaintDotNet.Canvas.Canvas> CanvasChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<SizeDouble> CanvasExtentPaddingChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<double> CanvasHairWidthChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<SizeDouble> CanvasSizeChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<ThicknessDouble> FrameCanvasPaddingChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<RectDouble> FramedCanvasBoundsChanged;

        [field: CompilerGenerated]
        public event EventHandler<RectDoubleInvalidatedEventArgs> Invalidated;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<bool> IsVisibleChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PaintDotNet.Canvas.ScaleBasis> ScaleBasisChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<double> ScaleRatioChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<RectDouble> ViewportCanvasBoundsChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PointDouble> ViewportCanvasOffsetChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PointDouble> ViewportCanvasOffsetMaxChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<PointDouble> ViewportCanvasOffsetMinChanged;

        [field: CompilerGenerated]
        public event ValueChangedEventHandler<SizeDouble> ViewportSizeChanged;

        public CanvasView()
        {
            this.SetBinding(CanvasSizeProperty, this, PropertyPathUtil.Combine(CanvasProperty, PaintDotNet.Canvas.Canvas.CanvasSizeProperty), BindingMode.TwoWay);
        }

        private void CanvasExtentPaddingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ScaleRatioProperty);
            this.CanvasExtentPaddingChanged.Raise<SizeDouble>(this, e);
        }

        private void CanvasHairWidthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.CanvasHairWidthChanged.Raise<double>(this, e);
        }

        protected virtual void CanvasPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            PaintDotNet.Canvas.Canvas oldValue = (PaintDotNet.Canvas.Canvas) e.OldValue;
            if (oldValue != null)
            {
                oldValue.UnregisterView(this);
            }
            PaintDotNet.Canvas.Canvas newValue = (PaintDotNet.Canvas.Canvas) e.NewValue;
            if (newValue != null)
            {
                newValue.RegisterView(this);
            }
            this.CanvasChanged.Raise<PaintDotNet.Canvas.Canvas>(this, e);
        }

        private void CanvasSizePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ScaleRatioProperty);
            this.UpdateProperty(FrameCanvasPaddingProperty);
            this.UpdateProperty(FramedCanvasBoundsProperty);
            this.CanvasSizeChanged.Raise<SizeDouble>(this, e);
        }

        public double ClampScaleRatio(double newScaleRatio) => 
            DoubleUtil.Clamp(newScaleRatio, 0.01, 32.0);

        private object CoerceCanvasHairWidthProperty(object baseValue)
        {
            double scaleRatio = this.ScaleRatio;
            double num2 = 1.0 / scaleRatio;
            return DoubleUtil.GetBoxed(num2);
        }

        private object CoerceFrameCanvasPaddingProperty(object baseValue)
        {
            if (!this.IsCanvasFrameEnabled)
            {
                return ThicknessDouble.BoxedZero;
            }
            if (this.ScaleBasis == PaintDotNet.Canvas.ScaleBasis.FitToViewport)
            {
                return ThicknessDouble.BoxedZero;
            }
            SizeDouble canvasSize = this.CanvasSize;
            SizeDouble num2 = this.ConvertExtentToCanvas(this.ViewportSize);
            double num3 = num2.Width / 2.0;
            double num4 = num2.Width - (canvasSize.Width / 2.0);
            double num5 = Math.Max(num3, num4);
            double num6 = num2.Height / 2.0;
            double num7 = num2.Height - (canvasSize.Height / 2.0);
            double bottom = Math.Max(num6, num7);
            double left = num5;
            double top = bottom;
            double right = num5;
            return new ThicknessDouble(left, top, right, bottom);
        }

        private object CoerceFramedCanvasBoundsProperty(object baseValue)
        {
            RectDouble canvasBounds = this.GetCanvasBounds();
            ThicknessDouble frameCanvasPadding = this.FrameCanvasPadding;
            return RectDouble.Inflate(canvasBounds, frameCanvasPadding);
        }

        private object CoerceSnapViewportOriginToDevicePixelsProperty(object baseValue)
        {
            if (this.ScaleBasis == PaintDotNet.Canvas.ScaleBasis.FitToViewport)
            {
                return BooleanUtil.GetBoxed(true);
            }
            if (this.ScaleRatio.IsInteger())
            {
                return BooleanUtil.GetBoxed(true);
            }
            return BooleanUtil.GetBoxed(false);
        }

        private object CoerceViewportCanvasBoundsProperty(object baseValue)
        {
            PointDouble viewportCanvasOffset = this.ViewportCanvasOffset;
            return new RectDouble(viewportCanvasOffset, this.ConvertExtentToCanvas(this.ViewportSize));
        }

        private object CoerceViewportCanvasOffsetMaxProperty(object baseValue)
        {
            PointDouble viewportCanvasOffsetMin = this.ViewportCanvasOffsetMin;
            SizeDouble canvasSize = this.CanvasSize;
            SizeDouble viewportSize = this.ViewportSize;
            SizeDouble num4 = this.ConvertExtentToCanvas(viewportSize);
            ThicknessDouble frameCanvasPadding = this.FrameCanvasPadding;
            double num6 = (canvasSize.Width - num4.Width) + frameCanvasPadding.Right;
            double x = Math.Max(viewportCanvasOffsetMin.X, num6);
            double num8 = (canvasSize.Height - num4.Height) + frameCanvasPadding.Bottom;
            return new PointDouble(x, Math.Max(viewportCanvasOffsetMin.Y, num8));
        }

        private object CoerceViewportCanvasOffsetMinProperty(object baseValue)
        {
            SizeDouble viewportSize = this.ViewportSize;
            SizeDouble num2 = this.ConvertExtentToCanvas(viewportSize);
            SizeDouble canvasSize = this.CanvasSize;
            ThicknessDouble frameCanvasPadding = this.FrameCanvasPadding;
            double num5 = num2.Width - canvasSize.Width;
            double num6 = num2.Height - canvasSize.Height;
            double x = Math.Min(-frameCanvasPadding.Left, -num5 / 2.0);
            return new PointDouble(x, Math.Min(-frameCanvasPadding.Top, -num6 / 2.0));
        }

        private void FrameCanvasPaddingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(FramedCanvasBoundsProperty);
            this.UpdateProperty(ViewportCanvasOffsetMinProperty);
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.FrameCanvasPaddingChanged.Raise<ThicknessDouble>(this, e);
        }

        private void FramedCanvasBoundsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasOffsetMinProperty);
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.FramedCanvasBoundsChanged.Raise<RectDouble>(this, e);
        }

        public void Invalidate()
        {
            this.Invalidate(PaintDotNet.Canvas.Canvas.CanvasMaxBounds);
        }

        public void Invalidate(RectDouble canvasRect)
        {
            base.VerifyAccess();
            this.Invalidated.RaisePooled<RectDoubleInvalidatedEventArgs, RectDouble>(this, canvasRect);
        }

        public void Invalidate(RectInt32 canvasRect)
        {
            this.Invalidate((RectDouble) canvasRect);
        }

        private void IsCanvasFrameEnabledPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(FrameCanvasPaddingProperty);
        }

        private void IsVisiblePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.IsVisibleChanged.Raise<bool>(this, e);
        }

        protected virtual object OnCoerceScaleBasisProperty(object baseValue) => 
            baseValue;

        protected virtual object OnCoerceScaleRatioProperty(object baseValue)
        {
            double num2;
            double num = (double) baseValue;
            PaintDotNet.Canvas.ScaleBasis scaleBasis = this.ScaleBasis;
            if (scaleBasis != PaintDotNet.Canvas.ScaleBasis.Ratio)
            {
                if (scaleBasis != PaintDotNet.Canvas.ScaleBasis.FitToViewport)
                {
                    ExceptionUtil.ThrowInvalidEnumArgumentException<PaintDotNet.Canvas.ScaleBasis>(this.ScaleBasis, "this.ScaleBasis");
                    return null;
                }
                SizeDouble canvasSize = this.CanvasSize;
                SizeDouble viewportSize = this.ViewportSize;
                SizeDouble canvasExtentPadding = this.CanvasExtentPadding;
                if (canvasSize.HasPositiveArea && viewportSize.HasPositiveArea)
                {
                    double num7 = (viewportSize.Width - canvasExtentPadding.Width) / canvasSize.Width;
                    double num8 = (viewportSize.Height - canvasExtentPadding.Height) / canvasSize.Height;
                    num2 = Math.Min(Math.Min(num7, num8), 1.0);
                }
                else
                {
                    num2 = 1.0;
                }
            }
            else
            {
                num2 = num;
            }
            return DoubleUtil.Clamp(num2, 0.01, 32.0);
        }

        protected virtual object OnCoerceViewportCanvasOffsetProperty(object baseValue)
        {
            PointDouble num = (PointDouble) baseValue;
            PointDouble viewportCanvasOffsetMin = this.ViewportCanvasOffsetMin;
            PointDouble viewportCanvasOffsetMax = this.ViewportCanvasOffsetMax;
            double x = DoubleUtil.Clamp(num.X, viewportCanvasOffsetMin.X, viewportCanvasOffsetMax.X);
            PointDouble viewportCanvasOffset = new PointDouble(x, DoubleUtil.Clamp(num.Y, viewportCanvasOffsetMin.Y, viewportCanvasOffsetMax.Y));
            if (this.SnapViewportOriginToDevicePixels)
            {
                return this.SnapViewportCanvasOffset(viewportCanvasOffset);
            }
            return viewportCanvasOffset;
        }

        protected virtual object OnCoerceViewportSizeProperty(object baseValue) => 
            baseValue;

        protected virtual void OnScaleBasisPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ScaleRatioProperty);
            this.UpdateProperty(SnapViewportOriginToDevicePixelsProperty);
            this.UpdateProperty(FrameCanvasPaddingProperty);
            this.ScaleBasisChanged.Raise<PaintDotNet.Canvas.ScaleBasis>(this, e);
        }

        protected virtual void OnScaleRatioPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(CanvasHairWidthProperty);
            this.UpdateProperty(FrameCanvasPaddingProperty);
            this.UpdateProperty(SnapViewportOriginToDevicePixelsProperty);
            this.UpdateProperty(ViewportCanvasBoundsProperty);
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.UpdateProperty(ViewportCanvasOffsetProperty);
            this.ScaleRatioChanged.Raise<double>(this, e);
        }

        protected virtual void OnViewportCanvasOffsetPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasBoundsProperty);
            this.ViewportCanvasOffsetChanged.Raise<PointDouble>(this, e);
        }

        protected virtual void OnViewportSizePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ScaleRatioProperty);
            this.UpdateProperty(FrameCanvasPaddingProperty);
            this.UpdateProperty(ViewportCanvasOffsetMinProperty);
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.UpdateProperty(ViewportCanvasBoundsProperty);
            this.ViewportSizeChanged.Raise<SizeDouble>(this, e);
        }

        private PointDouble SnapViewportCanvasOffset(PointDouble viewportCanvasOffset)
        {
            double scaleRatio = this.ScaleRatio;
            PointDouble num2 = new PointDouble(viewportCanvasOffset.X * scaleRatio, viewportCanvasOffset.Y * scaleRatio);
            double introduced5 = Math.Floor(num2.X);
            double introduced6 = Math.Floor(num2.Y);
            PointDouble num3 = new PointDouble(introduced5 - num2.X, introduced6 - num2.Y);
            PointDouble num4 = new PointDouble(num3.X / scaleRatio, num3.Y / scaleRatio);
            return new PointDouble(viewportCanvasOffset.X + num4.X, viewportCanvasOffset.Y + num4.Y);
        }

        private void SnapViewportOriginToDevicePixelsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasOffsetProperty);
        }

        private void UpdateProperty(DependencyProperty property)
        {
            base.InvalidateProperty(property);
        }

        private static bool ValidateFrameCanvasPaddingProperty(object value) => 
            true;

        private void ViewportCanvasBoundsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasOffsetMinProperty);
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.ViewportCanvasBoundsChanged.Raise<RectDouble>(this, e);
        }

        private void ViewportCanvasOffsetMaxPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasOffsetProperty);
            this.ViewportCanvasOffsetMaxChanged.Raise<PointDouble>(this, e);
        }

        private void ViewportCanvasOffsetMinPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateProperty(ViewportCanvasOffsetMaxProperty);
            this.UpdateProperty(ViewportCanvasOffsetProperty);
            this.ViewportCanvasOffsetMinChanged.Raise<PointDouble>(this, e);
        }

        public PaintDotNet.Canvas.Canvas Canvas
        {
            get => 
                ((PaintDotNet.Canvas.Canvas) base.GetValue(CanvasProperty));
            set
            {
                base.SetValue(CanvasProperty, value);
            }
        }

        public SizeDouble CanvasExtentPadding
        {
            get => 
                ((SizeDouble) base.GetValue(CanvasExtentPaddingProperty));
            set
            {
                base.SetValue(CanvasExtentPaddingProperty, value);
            }
        }

        public double CanvasHairWidth =>
            ((double) base.GetValue(CanvasHairWidthProperty));

        public SizeDouble CanvasSize
        {
            get => 
                ((SizeDouble) base.GetValue(CanvasSizeProperty));
            set
            {
                base.SetValue(CanvasSizeProperty, value);
            }
        }

        public ThicknessDouble FrameCanvasPadding =>
            ((ThicknessDouble) base.GetValue(FrameCanvasPaddingProperty));

        public RectDouble FramedCanvasBounds =>
            ((RectDouble) base.GetValue(FramedCanvasBoundsProperty));

        public bool IsCanvasFrameEnabled
        {
            get => 
                ((bool) base.GetValue(IsCanvasFrameEnabledProperty));
            set
            {
                base.SetValue(IsCanvasFrameEnabledProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool IsVisible
        {
            get => 
                ((bool) base.GetValue(IsVisibleProperty));
            set
            {
                base.SetValue(IsVisibleProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public IRenderTarget RenderTarget
        {
            get
            {
                base.VerifyAccess();
                return this.renderTarget;
            }
            set
            {
                base.VerifyAccess();
                this.renderTarget = value;
            }
        }

        public PaintDotNet.Canvas.ScaleBasis ScaleBasis
        {
            get => 
                ((PaintDotNet.Canvas.ScaleBasis) base.GetValue(ScaleBasisProperty));
            set
            {
                base.SetValue(ScaleBasisProperty, EnumUtil.GetBoxed<PaintDotNet.Canvas.ScaleBasis>(value));
            }
        }

        public double ScaleRatio
        {
            get => 
                ((double) base.GetValue(ScaleRatioProperty));
            set
            {
                base.SetValue(ScaleRatioProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public bool SnapViewportOriginToDevicePixels =>
            ((bool) base.GetValue(SnapViewportOriginToDevicePixelsProperty));

        public RectDouble ViewportCanvasBounds =>
            ((RectDouble) base.GetValue(ViewportCanvasBoundsProperty));

        public PointDouble ViewportCanvasOffset
        {
            get => 
                ((PointDouble) base.GetValue(ViewportCanvasOffsetProperty));
            set
            {
                base.SetValue(ViewportCanvasOffsetProperty, value);
            }
        }

        public PointDouble ViewportCanvasOffsetMax =>
            ((PointDouble) base.GetValue(ViewportCanvasOffsetMaxProperty));

        public PointDouble ViewportCanvasOffsetMin =>
            ((PointDouble) base.GetValue(ViewportCanvasOffsetMinProperty));

        public SizeDouble ViewportSize
        {
            get => 
                ((SizeDouble) base.GetValue(ViewportSizeProperty));
            set
            {
                base.SetValue(ViewportSizeProperty, value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CanvasView.<>c <>9 = new CanvasView.<>c();

            internal void <.cctor>b__150_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).CanvasPropertyChanged(e);
            }

            internal void <.cctor>b__150_1(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).IsVisiblePropertyChanged(e);
            }

            internal void <.cctor>b__150_10(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).CanvasHairWidthPropertyChanged(e);
            }

            internal object <.cctor>b__150_11(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceCanvasHairWidthProperty(bV);

            internal void <.cctor>b__150_12(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).OnViewportSizePropertyChanged(e);
            }

            internal object <.cctor>b__150_13(DependencyObject dO, object bV) => 
                ((CanvasView) dO).OnCoerceViewportSizeProperty(bV);

            internal void <.cctor>b__150_14(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).OnViewportCanvasOffsetPropertyChanged(e);
            }

            internal object <.cctor>b__150_15(DependencyObject dO, object bV) => 
                ((CanvasView) dO).OnCoerceViewportCanvasOffsetProperty(bV);

            internal void <.cctor>b__150_16(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).ViewportCanvasOffsetMinPropertyChanged(e);
            }

            internal object <.cctor>b__150_17(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceViewportCanvasOffsetMinProperty(bV);

            internal void <.cctor>b__150_18(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).ViewportCanvasOffsetMaxPropertyChanged(e);
            }

            internal object <.cctor>b__150_19(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceViewportCanvasOffsetMaxProperty(bV);

            internal void <.cctor>b__150_2(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).CanvasSizePropertyChanged(e);
            }

            internal void <.cctor>b__150_20(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).ViewportCanvasBoundsPropertyChanged(e);
            }

            internal object <.cctor>b__150_21(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceViewportCanvasBoundsProperty(bV);

            internal void <.cctor>b__150_22(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).IsCanvasFrameEnabledPropertyChanged(e);
            }

            internal void <.cctor>b__150_23(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).FrameCanvasPaddingPropertyChanged(e);
            }

            internal object <.cctor>b__150_24(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceFrameCanvasPaddingProperty(bV);

            internal void <.cctor>b__150_25(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).FramedCanvasBoundsPropertyChanged(e);
            }

            internal object <.cctor>b__150_26(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceFramedCanvasBoundsProperty(bV);

            internal void <.cctor>b__150_3(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).CanvasExtentPaddingPropertyChanged(e);
            }

            internal void <.cctor>b__150_4(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).SnapViewportOriginToDevicePixelsPropertyChanged(e);
            }

            internal object <.cctor>b__150_5(DependencyObject dO, object bV) => 
                ((CanvasView) dO).CoerceSnapViewportOriginToDevicePixelsProperty(bV);

            internal void <.cctor>b__150_6(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).OnScaleRatioPropertyChanged(e);
            }

            internal object <.cctor>b__150_7(DependencyObject dO, object bV) => 
                ((CanvasView) dO).OnCoerceScaleRatioProperty(bV);

            internal void <.cctor>b__150_8(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CanvasView) s).OnScaleBasisPropertyChanged(e);
            }

            internal object <.cctor>b__150_9(DependencyObject dO, object bV) => 
                ((CanvasView) dO).OnCoerceScaleBasisProperty(bV);
        }
    }
}

