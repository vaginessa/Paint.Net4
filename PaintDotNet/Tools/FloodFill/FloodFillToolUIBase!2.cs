namespace PaintDotNet.Tools.FloodFill
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Canvas;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal abstract class FloodFillToolUIBase<TTool, TChanges> : ToolUICanvas<TTool, TChanges> where TTool: PresentationBasedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        public static readonly DependencyProperty CanvasCursorProperty;
        private HandleElement canvasHandle;
        private Cursor handCursor;
        public static readonly DependencyProperty HandleTypeProperty;
        private Cursor handMouseDownCursor;
        private HandleElement moveHandle;
        private DependencyFunc<PointInt32, double, SizeDouble, PointDouble> moveHandleCanvasOffset;
        private AnimatedDouble moveHandleOpacity;
        private AnimationStateHelper moveHandleOpacityHelper;
        private SquareHandleDrawing originDrawing;
        private HandleElement originHandle;
        private DependencyFunc<PointInt32, SizeDouble, PointDouble> originHandleCanvasOffset;
        private DependencyValue<TransactedToolState> toolState;

        static FloodFillToolUIBase()
        {
            FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty = FrameworkProperty.Register("CanvasCursor", typeof(Cursor), typeof(FloodFillToolUIBase<TTool, TChanges>), new PaintDotNet.UI.FrameworkPropertyMetadata(null, PaintDotNet.UI.FrameworkPropertyMetadataOptions.None, null, null, new CoerceValueCallback(<>c<TTool, TChanges>.<>9.<.cctor>b__30_0)));
            FloodFillToolUIBase<TTool, TChanges>.HandleTypeProperty = FrameworkProperty.RegisterAttached("HandleType", typeof(FloodFillToolHandleType), typeof(UIElement), typeof(FloodFillToolUIBase<TTool, TChanges>), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<FloodFillToolHandleType>(FloodFillToolHandleType.None)));
        }

        public FloodFillToolUIBase()
        {
            this.handCursor = CursorUtil.LoadResource("Cursors.PanToolCursor.cur");
            this.handMouseDownCursor = CursorUtil.LoadResource("Cursors.PanToolCursorMouseDown.cur");
            this.toolState = new DependencyValue<TransactedToolState>();
            this.toolState.SetBinding(this.toolState.GetValueProperty(), this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()), BindingMode.OneWay);
            this.toolState.ValueChanged += new ValueChangedEventHandler<TransactedToolState>(this.OnToolStateChanged);
            this.canvasHandle = new HandleElement();
            this.canvasHandle.Focusable = true;
            FloodFillToolUIBase<TTool, TChanges>.SetHandleType(this.canvasHandle, FloodFillToolHandleType.Canvas);
            this.canvasHandle.ClipToBounds = false;
            ClickDragBehavior.SetAllowClick(this.canvasHandle, false);
            ClickDragBehavior.SetAllowDrag(this.canvasHandle, true);
            ClickDragBehavior.SetIsEnabled(this.canvasHandle, true);
            this.canvasHandle.SetBinding(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty), BindingMode.OneWay);
            base.Children.Add(this.canvasHandle);
            this.originHandle = new HandleElement();
            FloodFillToolUIBase<TTool, TChanges>.SetHandleType(this.originHandle, FloodFillToolHandleType.Origin);
            ClickDragBehavior.SetAllowClick(this.originHandle, true);
            ClickDragBehavior.SetAllowDrag(this.originHandle, true);
            ClickDragBehavior.SetIsEnabled(this.originHandle, true);
            this.originHandle.SetBinding<TransactedToolState, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Visibility>(FloodFillToolUIBase<TTool, TChanges>.GetHandleVisibility));
            this.originDrawing = new SquareHandleDrawing();
            this.originDrawing.SetBinding<double, double>(HandleDrawing.RadiusProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, hw => (1.0 / hw) / 2.0);
            this.originHandle.Drawing = this.originDrawing;
            this.originHandleCanvasOffset = new DependencyFunc<PointInt32, SizeDouble, PointDouble>(new Func<PointInt32, SizeDouble, PointDouble>(FloodFillToolUIBase<TTool, TChanges>.GetHandleCanvasOffset));
            this.originHandleCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.OriginPointInt32", Array.Empty<object>()));
            this.originHandleCanvasOffset.SetArgInput(2, this.originHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty.Name, Array.Empty<object>()));
            this.originHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.originHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.originHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.originHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.originHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.originHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(FloodFillToolUIBase<TTool, TChanges>.GetHandlePadding));
            this.originHandle.SetBinding<TransactedToolState, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Cursor>(this.GetHandleCursor));
            base.Children.Add(this.originHandle);
            this.moveHandle = new HandleElement(new CompassHandleDrawing());
            FloodFillToolUIBase<TTool, TChanges>.SetHandleType(this.moveHandle, FloodFillToolHandleType.Move);
            ClickDragBehavior.SetAllowClick(this.moveHandle, true);
            ClickDragBehavior.SetAllowDrag(this.moveHandle, true);
            ClickDragBehavior.SetIsEnabled(this.moveHandle, true);
            this.moveHandle.SetBinding<TransactedToolState, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Visibility>(FloodFillToolUIBase<TTool, TChanges>.GetHandleVisibility));
            this.moveHandle.RenderTransformOrigin = new PointDouble(0.5, 0.5);
            this.moveHandleCanvasOffset = new DependencyFunc<PointInt32, double, SizeDouble, PointDouble>(new Func<PointInt32, double, SizeDouble, PointDouble>(FloodFillToolUIBase<TTool, TChanges>.GetMoveHandleCanvasOffset));
            this.moveHandleCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.OriginPointInt32", Array.Empty<object>()));
            this.moveHandleCanvasOffset.SetArgInput(2, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty));
            this.moveHandleCanvasOffset.SetArgInput(3, this.moveHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty.Name, Array.Empty<object>()));
            this.moveHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.moveHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.moveHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.moveHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.moveHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.moveHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(FloodFillToolUIBase<TTool, TChanges>.GetHandlePadding));
            this.moveHandle.SetBinding<TransactedToolState, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Cursor>(this.GetHandleCursor));
            this.moveHandleOpacityHelper = new AnimationStateHelper();
            this.moveHandleOpacityHelper.Element = this.moveHandle;
            this.moveHandleOpacityHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                base.moveHandleOpacity = new AnimatedDouble(1.0);
                base.moveHandle.SetBinding(UIElement.OpacityProperty, base.moveHandleOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                base.moveHandleOpacity.AnimateRawValue((s, v) => FloodFillToolUIBase<TTool, TChanges>.InitializeHandleOpacityStoryboard(s, v, 0.33333333333333331), null);
            };
            this.moveHandleOpacityHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                base.moveHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.moveHandleOpacity);
            };
            base.Children.Add(this.moveHandle);
            base.Loaded += new EventHandler(this.OnLoaded);
        }

        private object CoerceCanvasCursorProperty(object baseValue)
        {
            if (((TransactedToolState) this.toolState.Value) == TransactedToolState.Drawing)
            {
                return this.handMouseDownCursor;
            }
            return this.OnCoerceCanvasCursorProperty(baseValue);
        }

        private static PointDouble GetHandleCanvasOffset(PointInt32 point, SizeDouble actualSize) => 
            new PointDouble((point.X + 0.5) - (actualSize.Width / 2.0), (point.Y + 0.5) - (actualSize.Height / 2.0));

        private Cursor GetHandleCursor(TransactedToolState state)
        {
            switch (state)
            {
                case TransactedToolState.Inactive:
                    return null;

                case TransactedToolState.Idle:
                case TransactedToolState.Dirty:
                    return this.handCursor;

                case TransactedToolState.Drawing:
                case TransactedToolState.Editing:
                    return this.handMouseDownCursor;
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<TransactedToolState>(state, "state");
            return null;
        }

        private static PaintDotNet.UI.Thickness GetHandlePadding(double canvasHairWidth) => 
            new PaintDotNet.UI.Thickness(canvasHairWidth * 8.0);

        public static FloodFillToolHandleType GetHandleType(UIElement target) => 
            ((FloodFillToolHandleType) target.GetValue(FloodFillToolUIBase<TTool, TChanges>.HandleTypeProperty));

        private static Visibility GetHandleVisibility(TransactedToolState state)
        {
            switch (state)
            {
                case TransactedToolState.Inactive:
                case TransactedToolState.Idle:
                    return Visibility.Hidden;

                case TransactedToolState.Drawing:
                case TransactedToolState.Dirty:
                case TransactedToolState.Editing:
                    return Visibility.Visible;
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<TransactedToolState>(state, "state");
            return Visibility.Hidden;
        }

        private static PointDouble GetMoveHandleCanvasOffset(PointInt32 originPoint, double hairWidth, SizeDouble moveHandleActualSize)
        {
            VectorDouble vec = new VectorDouble(1.0, 1.0);
            VectorDouble num3 = (VectorDouble) (VectorDouble.Normalize(vec) * (hairWidth * 25.0));
            double x = UIUtil.ScaleWidth(num3.X);
            VectorDouble num4 = new VectorDouble(x, UIUtil.ScaleHeight(num3.Y));
            PointDouble num5 = new PointDouble((double) (originPoint.X + 1), (double) (originPoint.Y + 1));
            PointDouble pt = num5 + num4;
            return PointDouble.Offset(pt, -moveHandleActualSize.Width / 2.0, -moveHandleActualSize.Height / 2.0);
        }

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

        protected virtual object OnCoerceCanvasCursorProperty(object baseValue) => 
            baseValue;

        private void OnLoaded(object sender, EventArgs e)
        {
            base.CoerceValue(FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty);
        }

        protected override void OnSetInitialFocus()
        {
            this.canvasHandle.Focus();
            base.OnSetInitialFocus();
        }

        private void OnToolStateChanged(object sender, ValueChangedEventArgs<TransactedToolState> e)
        {
            base.CoerceValue(FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty);
        }

        public static void SetHandleType(UIElement target, FloodFillToolHandleType value)
        {
            target.SetValue(FloodFillToolUIBase<TTool, TChanges>.HandleTypeProperty, EnumUtil.GetBoxed<FloodFillToolHandleType>(value));
        }

        public Cursor CanvasCursor
        {
            get => 
                ((Cursor) base.GetValue(FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty));
            set
            {
                base.SetValue(FloodFillToolUIBase<TTool, TChanges>.CanvasCursorProperty, value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly FloodFillToolUIBase<TTool, TChanges>.<>c <>9;
            public static Func<double, double> <>9__11_0;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__11_2;

            static <>c()
            {
                FloodFillToolUIBase<TTool, TChanges>.<>c.<>9 = new FloodFillToolUIBase<TTool, TChanges>.<>c();
            }

            internal object <.cctor>b__30_0(DependencyObject dO, object bV) => 
                ((FloodFillToolUIBase<TTool, TChanges>) dO).CoerceCanvasCursorProperty(bV);

            internal double <.ctor>b__11_0(double hw) => 
                ((1.0 / hw) / 2.0);

            internal void <.ctor>b__11_2(IAnimationStoryboard s, IAnimationVariable v)
            {
                FloodFillToolUIBase<TTool, TChanges>.InitializeHandleOpacityStoryboard(s, v, 0.33333333333333331);
            }
        }
    }
}

