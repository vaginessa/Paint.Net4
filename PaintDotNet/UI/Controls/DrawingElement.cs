namespace PaintDotNet.UI.Controls
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal class DrawingElement : PaintDotNet.UI.FrameworkElement
    {
        public static readonly DependencyProperty DrawingProperty = FrameworkProperty.Register("Drawing", typeof(PaintDotNet.UI.Media.Drawing), typeof(DrawingElement), new PaintDotNet.UI.FrameworkPropertyMetadata(null, PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(<>c.<>9.<.cctor>b__27_0)));
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = FrameworkProperty.Register("HorizontalContentAlignment", typeof(PaintDotNet.UI.HorizontalAlignment), typeof(DrawingElement), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<PaintDotNet.UI.HorizontalAlignment>(PaintDotNet.UI.HorizontalAlignment.Center), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty PaddingProperty = FrameworkProperty.Register("Padding", typeof(PaintDotNet.UI.Thickness), typeof(DrawingElement), new PaintDotNet.UI.FrameworkPropertyMetadata(new PaintDotNet.UI.Thickness(0.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ScaleProperty = FrameworkProperty.Register("Scale", typeof(double), typeof(DrawingElement), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(1.0), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsMeasure | PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty VerticalContentAlignmentProperty = FrameworkProperty.Register("VerticalContentAlignment", typeof(PaintDotNet.UI.VerticalAlignment), typeof(DrawingElement), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<PaintDotNet.UI.VerticalAlignment>(PaintDotNet.UI.VerticalAlignment.Center), PaintDotNet.UI.FrameworkPropertyMetadataOptions.AffectsArrange));

        public DrawingElement()
        {
        }

        public DrawingElement(PaintDotNet.UI.Media.Drawing drawing)
        {
            this.Drawing = drawing;
        }

        protected override SizeDouble ArrangeOverride(SizeDouble finalSize) => 
            base.ArrangeOverride(finalSize);

        private void DrawingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnDrawingChanged((PaintDotNet.UI.Media.Drawing) e.OldValue, (PaintDotNet.UI.Media.Drawing) e.NewValue);
        }

        protected override SizeDouble MeasureOverride(SizeDouble availableSize)
        {
            PaintDotNet.UI.Media.Drawing drawing = this.Drawing;
            if (drawing == null)
            {
                return SizeDouble.Zero;
            }
            RectDouble num2 = RectDouble.Scale(drawing.Bounds, this.Scale);
            PaintDotNet.UI.Thickness padding = this.Padding;
            return new SizeDouble(num2.Width + padding.Size.Width, num2.Height + padding.Size.Height);
        }

        protected virtual void OnDrawingChanged(PaintDotNet.UI.Media.Drawing oldValue, PaintDotNet.UI.Media.Drawing newValue)
        {
        }

        protected override void OnRender(IDrawingContext dc)
        {
            double scale = this.Scale;
            PaintDotNet.UI.Media.Drawing drawing = this.Drawing;
            if (drawing != null)
            {
                RectDouble bounds = drawing.Bounds;
                if ((bounds.HasPositiveArea && scale.IsFinite()) && (scale > 0.0))
                {
                    double left;
                    double top;
                    RectDouble num3 = RectDouble.Scale(bounds, scale);
                    double actualWidth = base.ActualWidth;
                    double actualHeight = base.ActualHeight;
                    RectDouble num6 = new RectDouble(0.0, 0.0, actualWidth, actualHeight);
                    PaintDotNet.UI.Thickness padding = this.Padding;
                    switch (this.HorizontalContentAlignment)
                    {
                        case PaintDotNet.UI.HorizontalAlignment.Left:
                            left = padding.Left;
                            break;

                        case PaintDotNet.UI.HorizontalAlignment.Center:
                            left = (actualWidth - num3.Width) / 2.0;
                            break;

                        case PaintDotNet.UI.HorizontalAlignment.Right:
                            left = (actualWidth - num3.Width) - padding.Right;
                            break;

                        case PaintDotNet.UI.HorizontalAlignment.Stretch:
                            left = 0.0;
                            break;

                        default:
                            ExceptionUtil.ThrowInvalidEnumArgumentException<PaintDotNet.UI.HorizontalAlignment>(this.HorizontalContentAlignment, "HorizontalContentAlignment");
                            left = double.NaN;
                            break;
                    }
                    switch (this.VerticalContentAlignment)
                    {
                        case PaintDotNet.UI.VerticalAlignment.Top:
                            top = padding.Top;
                            break;

                        case PaintDotNet.UI.VerticalAlignment.Center:
                            top = (actualHeight - num3.Height) / 2.0;
                            break;

                        case PaintDotNet.UI.VerticalAlignment.Bottom:
                            top = (actualHeight - num3.Height) - padding.Bottom;
                            break;

                        case PaintDotNet.UI.VerticalAlignment.Stretch:
                            top = 0.0;
                            break;

                        default:
                            ExceptionUtil.ThrowInvalidEnumArgumentException<PaintDotNet.UI.HorizontalAlignment>(this.HorizontalContentAlignment, "HorizontalContentAlignment");
                            top = double.NaN;
                            break;
                    }
                    float sx = (float) scale;
                    using (dc.UseTranslateTransform((float) left, (float) top, MatrixMultiplyOrder.Prepend))
                    {
                        using (dc.UseScaleTransform(sx, sx, MatrixMultiplyOrder.Prepend))
                        {
                            using (dc.UseTranslateTransform(-((float) bounds.X), -((float) bounds.Y), MatrixMultiplyOrder.Prepend))
                            {
                                drawing.Render(dc);
                            }
                        }
                    }
                }
            }
            base.OnRender(dc);
        }

        public PaintDotNet.UI.Media.Drawing Drawing
        {
            get => 
                ((PaintDotNet.UI.Media.Drawing) base.GetValue(DrawingProperty));
            set
            {
                base.SetValue(DrawingProperty, value);
            }
        }

        public PaintDotNet.UI.HorizontalAlignment HorizontalContentAlignment
        {
            get => 
                ((PaintDotNet.UI.HorizontalAlignment) base.GetValue(HorizontalContentAlignmentProperty));
            set
            {
                base.SetValue(HorizontalContentAlignmentProperty, EnumUtil.GetBoxed<PaintDotNet.UI.HorizontalAlignment>(value));
            }
        }

        public PaintDotNet.UI.Thickness Padding
        {
            get => 
                ((PaintDotNet.UI.Thickness) base.GetValue(PaddingProperty));
            set
            {
                base.SetValue(PaddingProperty, value);
            }
        }

        public double Scale
        {
            get => 
                ((double) base.GetValue(ScaleProperty));
            set
            {
                base.SetValue(ScaleProperty, value);
            }
        }

        public PaintDotNet.UI.VerticalAlignment VerticalContentAlignment
        {
            get => 
                ((PaintDotNet.UI.VerticalAlignment) base.GetValue(VerticalContentAlignmentProperty));
            set
            {
                base.SetValue(VerticalContentAlignmentProperty, EnumUtil.GetBoxed<PaintDotNet.UI.VerticalAlignment>(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DrawingElement.<>c <>9 = new DrawingElement.<>c();

            internal void <.cctor>b__27_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((DrawingElement) s).DrawingPropertyChanged(e);
            }
        }
    }
}

