namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class TextCursorCanvasLayer : CanvasLayer
    {
        private CalculateInvalidRectCallback calculateInvalidRect;
        private RectDouble cursorBounds;
        private const double cursorFadeAnimationDuration = 0.1;
        private AnimatedDouble opacityAnimation;
        private const int outlineWidthPx = 1;

        public TextCursorCanvasLayer()
        {
            this.calculateInvalidRect = new CalculateInvalidRectCallback(this.CalculateInvalidRect);
            this.opacityAnimation = new AnimatedDouble(1.0);
        }

        public void AnimationCursorOpacityTo(double value)
        {
            if (base.IsVisible && (this.opacityAnimation.FinalValue != value))
            {
                this.opacityAnimation.AnimateValueTo(value, 0.1, AnimationTransitionType.SmoothStop);
            }
        }

        private RectDouble CalculateInvalidRect(CanvasView canvasView, RectDouble canvasRect) => 
            RectDouble.Inflate(this.GetCursorOutlineRect(canvasView, canvasRect), 1.0, 1.0);

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free<AnimatedDouble>(ref this.opacityAnimation, disposing);
            base.Dispose(disposing);
        }

        private RectDouble GetCursorOutlineRect(CanvasView canvasView, RectDouble cursorBounds)
        {
            double canvasHairWidth = canvasView.CanvasHairWidth;
            double num2 = (1.0 * canvasHairWidth) / 2.0;
            return RectDouble.Inflate(cursorBounds, canvasHairWidth, canvasHairWidth);
        }

        protected override void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                this.opacityAnimation.ValueChanged += new ValueChangedEventHandler<double>(this.OnOpacityAnimationValueChanged);
            }
            else
            {
                this.opacityAnimation.ValueChanged -= new ValueChangedEventHandler<double>(this.OnOpacityAnimationValueChanged);
                this.opacityAnimation.StopAnimation();
            }
            base.OnIsVisibleChanged(oldValue, newValue);
        }

        private void OnOpacityAnimationValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            base.Invalidate(this.calculateInvalidRect, this.CursorBounds);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            RectDouble cursorBounds = this.CursorBounds;
            RectDouble cursorOutlineRect = this.GetCursorOutlineRect(canvasView, cursorBounds);
            double opacity = this.opacityAnimation.Value;
            dc.FillRectangle(cursorBounds, SolidColorBrushCache.Get((ColorRgba128Float) Colors.Black, opacity));
            double canvasHairWidth = canvasView.CanvasHairWidth;
            double thickness = 1.0 * canvasHairWidth;
            dc.DrawRectangle(cursorOutlineRect, SolidColorBrushCache.Get((ColorRgba128Float) Colors.White, opacity), thickness);
            base.OnRender(dc, clipRect, canvasView);
        }

        public RectDouble CursorBounds
        {
            get => 
                this.cursorBounds;
            set
            {
                base.VerifyAccess();
                if (value != this.cursorBounds)
                {
                    RectDouble canvasRect = RectDouble.Union(this.cursorBounds, value);
                    this.cursorBounds = value;
                    base.Invalidate(this.calculateInvalidRect, canvasRect);
                }
            }
        }
    }
}

