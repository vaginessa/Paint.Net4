namespace PaintDotNet.Tools.CloneStamp
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Canvas;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.BrushBase;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class CloneStampToolUI : BrushToolUIBase<CloneStampToolUI, CloneStampTool, CloneStampToolChanges>
    {
        public static readonly DependencyProperty AnchorCenterProperty = FrameworkProperty.Register("AnchorCenter", typeof(PointDouble), typeof(CloneStampToolUI), new PaintDotNet.UI.FrameworkPropertyMetadata(PointDouble.BoxedZero));
        private CircleHandleDrawing anchorDrawing;
        private HandleElement anchorElement;
        private DependencyFunc<PointDouble, SizeDouble, PointDouble> anchorElementCanvasOffset;
        private AnimatedDouble anchorElementOpacity;
        private AnimationStateHelper anchorElementOpacityAnimationHelper;
        public static readonly DependencyProperty AnchorRadiusProperty = FrameworkProperty.Register("AnchorRadius", typeof(double), typeof(CloneStampToolUI), new PaintDotNet.UI.FrameworkPropertyMetadata(DoubleUtil.GetBoxed(0.0)));
        public static readonly DependencyProperty IsAnchorVisibleProperty = FrameworkProperty.Register("IsAnchorVisible", typeof(bool), typeof(CloneStampToolUI), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        public static readonly DependencyProperty IsSettingAnchorProperty = FrameworkProperty.Register("IsSettingAnchor", typeof(bool), typeof(CloneStampToolUI), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(<>c.<>9.<.cctor>b__29_0)));
        private Cursor setAnchorMouseDownCursor;
        private Cursor setAnchorMouseUpCursor;

        public CloneStampToolUI() : base("Cursors.CloneStampToolCursor.cur")
        {
            this.setAnchorMouseUpCursor = CursorUtil.LoadResource("Cursors.CloneStampToolCursorSetSource.cur");
            this.setAnchorMouseDownCursor = CursorUtil.LoadResource("Cursors.GenericToolCursorMouseDown.cur");
            this.anchorDrawing = new CircleHandleDrawing();
            this.anchorDrawing.SetBinding(HandleDrawing.RadiusProperty, this, new PaintDotNet.ObjectModel.PropertyPath(AnchorRadiusProperty), BindingMode.OneWay);
            this.anchorDrawing.SetBinding(HandleDrawing.ThicknessProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.anchorElement = new HandleElement(this.anchorDrawing);
            this.anchorElement.IsHitTestVisible = false;
            this.anchorElement.IsHotOnMouseOver = false;
            this.anchorElement.SetBinding<bool, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath(IsAnchorVisibleProperty), BindingMode.OneWay, delegate (bool x) {
                if (!x)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            });
            this.anchorElementCanvasOffset = new DependencyFunc<PointDouble, SizeDouble, PointDouble>(new Func<PointDouble, SizeDouble, PointDouble>(CloneStampToolUI.GetHandleCanvasOffset));
            this.anchorElementCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(AnchorCenterProperty));
            this.anchorElementCanvasOffset.SetArgInput(2, this.anchorElement, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty));
            this.anchorElement.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.anchorElementCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.anchorElement.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.anchorElementCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.anchorElementOpacityAnimationHelper = new AnimationStateHelper();
            this.anchorElementOpacityAnimationHelper.Element = this.anchorElement;
            this.anchorElementOpacityAnimationHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.anchorElementOpacity = new AnimatedDouble(1.0);
                this.anchorElement.SetBinding(UIElement.OpacityProperty, this.anchorElementOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.anchorElementOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.0), null);
            };
            this.anchorElementOpacityAnimationHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.anchorElement.ClearBinding(UIElement.OpacityProperty);
                this.anchorElement.ClearValue(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.anchorElementOpacity);
            };
            base.Children.Insert(0, this.anchorElement);
        }

        private static PointDouble GetHandleCanvasOffset(PointDouble point, SizeDouble actualSize) => 
            new PointDouble(point.X - (actualSize.Width / 2.0), point.Y - (actualSize.Height / 2.0));

        private static void InitializeHandleOpacityStoryboard(IAnimationStoryboard storyboard, IAnimationVariable variable, double startOffsetPeriodFraction)
        {
            AnimationSeconds duration = 2.0;
            AnimationSeconds seconds2 = duration * startOffsetPeriodFraction;
            if (seconds2 != 0.0)
            {
                AnimationTransition transition2 = new ConstantAnimationTransition(seconds2);
                storyboard.AddTransition(variable, transition2);
            }
            AnimationKeyFrame startKeyFrame = storyboard.AddKeyFrameAtOffset(AnimationKeyFrame.StoryboardStart, seconds2);
            AnimationTransition transition = new SinusoidalFromRangeAnimationTransition(duration, 0.25, 1.5, duration, AnimationSlope.Decreasing);
            storyboard.AddTransition(variable, transition);
            AnimationKeyFrame endKeyFrame = storyboard.AddKeyFrameAfterTransition(transition);
            storyboard.RepeatBetweenKeyFrames(startKeyFrame, endKeyFrame, -1);
        }

        private void IsSettingAnchorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.UpdateCursor();
        }

        protected override Cursor OnQueryMouseDownCursor()
        {
            if (!this.IsSettingAnchor)
            {
                return base.OnQueryMouseDownCursor();
            }
            return this.setAnchorMouseDownCursor;
        }

        protected override Cursor OnQueryMouseUpCursor()
        {
            if (!this.IsSettingAnchor)
            {
                return base.OnQueryMouseUpCursor();
            }
            return this.setAnchorMouseUpCursor;
        }

        public PointDouble AnchorCenter
        {
            get => 
                ((PointDouble) base.GetValue(AnchorCenterProperty));
            set
            {
                base.SetValue(AnchorCenterProperty, value);
            }
        }

        public double AnchorRadius
        {
            get => 
                ((double) base.GetValue(AnchorRadiusProperty));
            set
            {
                base.SetValue(AnchorRadiusProperty, DoubleUtil.GetBoxed(value));
            }
        }

        public bool IsAnchorVisible
        {
            get => 
                ((bool) base.GetValue(IsAnchorVisibleProperty));
            set
            {
                base.SetValue(IsAnchorVisibleProperty, BooleanUtil.GetBoxed(value));
            }
        }

        public bool IsSettingAnchor
        {
            get => 
                ((bool) base.GetValue(IsSettingAnchorProperty));
            set
            {
                base.SetValue(IsSettingAnchorProperty, BooleanUtil.GetBoxed(value));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CloneStampToolUI.<>c <>9 = new CloneStampToolUI.<>c();
            public static Func<bool, Visibility> <>9__7_0;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__7_2;

            internal void <.cctor>b__29_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((CloneStampToolUI) s).IsSettingAnchorPropertyChanged(e);
            }

            internal Visibility <.ctor>b__7_0(bool x)
            {
                if (!x)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }

            internal void <.ctor>b__7_2(IAnimationStoryboard s, IAnimationVariable v)
            {
                CloneStampToolUI.InitializeHandleOpacityStoryboard(s, v, 0.0);
            }
        }
    }
}

