namespace PaintDotNet.Tools.Gradient
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

    internal sealed class GradientToolUI : ToolUICanvas<GradientTool, GradientToolChanges>
    {
        private HandleElement canvasHandle = new HandleElement();
        private Cursor crosshairCursor;
        private Cursor crosshairMouseDownCursor;
        private HandleElement endHandle;
        private DependencyFunc<PointDouble, SizeDouble, PointDouble> endHandleCanvasOffset;
        private AnimatedDouble endHandleOpacity;
        private AnimationStateHelper endHandleOpacityHelper;
        private Cursor handCursor;
        private HandleElement[] handles;
        public static readonly DependencyProperty HandleTypeProperty = FrameworkProperty.RegisterAttached("HandleType", typeof(GradientToolHandleType), typeof(UIElement), typeof(GradientToolUI), new PaintDotNet.UI.FrameworkPropertyMetadata(EnumUtil.GetBoxed<GradientToolHandleType>(GradientToolHandleType.None)));
        private Cursor handMouseDownCursor;
        private HandleElement moveHandle;
        private DependencyFunc<PointDouble, PointDouble, double, SizeDouble, PointDouble> moveHandleCanvasOffset;
        private AnimatedDouble moveHandleOpacity;
        private AnimationStateHelper moveHandleOpacityHelper;
        private HandleElement startHandle;
        private DependencyFunc<PointDouble, SizeDouble, PointDouble> startHandleCanvasOffset;
        private AnimatedDouble startHandleOpacity;
        private AnimationStateHelper startHandleOpacityHelper;

        public GradientToolUI()
        {
            SetHandleType(this.canvasHandle, GradientToolHandleType.Canvas);
            this.canvasHandle.ClipToBounds = false;
            ClickDragBehavior.SetAllowClick(this.canvasHandle, false);
            this.startHandle = new HandleElement(new CircleHandleDrawing());
            SetHandleType(this.startHandle, GradientToolHandleType.Start);
            this.startHandle.SetBinding<TransactedToolState, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Visibility>(GradientToolUI.GetHandleVisibility));
            this.startHandleCanvasOffset = new DependencyFunc<PointDouble, SizeDouble, PointDouble>(new Func<PointDouble, SizeDouble, PointDouble>(GradientToolUI.GetHandleCanvasOffset));
            this.startHandleCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.GradientStartPoint", Array.Empty<object>()));
            this.startHandleCanvasOffset.SetArgInput(2, this.startHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty.Name, Array.Empty<object>()));
            this.startHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.startHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.startHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.startHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.startHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.startHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(GradientToolUI.GetHandlePadding));
            this.startHandleOpacityHelper = new AnimationStateHelper();
            this.startHandleOpacityHelper.Element = this.startHandle;
            this.startHandleOpacityHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.startHandleOpacity = new AnimatedDouble(1.0);
                this.startHandle.SetBinding(UIElement.OpacityProperty, this.startHandleOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.startHandleOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.0), null);
            };
            this.startHandleOpacityHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.startHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.startHandleOpacity);
            };
            this.endHandle = new HandleElement(new CircleHandleDrawing());
            SetHandleType(this.endHandle, GradientToolHandleType.End);
            this.endHandle.SetBinding<TransactedToolState, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Visibility>(GradientToolUI.GetHandleVisibility));
            this.endHandleCanvasOffset = new DependencyFunc<PointDouble, SizeDouble, PointDouble>(new Func<PointDouble, SizeDouble, PointDouble>(GradientToolUI.GetHandleCanvasOffset));
            this.endHandleCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.GradientEndPoint", Array.Empty<object>()));
            this.endHandleCanvasOffset.SetArgInput(2, this.endHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty.Name, Array.Empty<object>()));
            this.endHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.endHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.endHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.endHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.endHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.endHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(GradientToolUI.GetHandlePadding));
            this.endHandleOpacityHelper = new AnimationStateHelper();
            this.endHandleOpacityHelper.Element = this.endHandle;
            this.endHandleOpacityHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.endHandleOpacity = new AnimatedDouble(1.0);
                this.endHandle.SetBinding(UIElement.OpacityProperty, this.endHandleOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.endHandleOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.33333333333333331), null);
            };
            this.endHandleOpacityHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.endHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.endHandleOpacity);
            };
            this.moveHandle = new HandleElement(new CompassHandleDrawing());
            SetHandleType(this.moveHandle, GradientToolHandleType.Move);
            this.moveHandle.SetBinding<TransactedToolState, Visibility>(UIElement.VisibilityProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Visibility>(GradientToolUI.GetHandleVisibility));
            this.moveHandle.RenderTransformOrigin = new PointDouble(0.5, 0.5);
            this.moveHandleCanvasOffset = new DependencyFunc<PointDouble, PointDouble, double, SizeDouble, PointDouble>(new Func<PointDouble, PointDouble, double, SizeDouble, PointDouble>(GradientToolUI.GetMoveHandleCanvasOffset));
            this.moveHandleCanvasOffset.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.GradientStartPoint", Array.Empty<object>()));
            this.moveHandleCanvasOffset.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.Changes.GradientEndPoint", Array.Empty<object>()));
            this.moveHandleCanvasOffset.SetArgInput(3, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty));
            this.moveHandleCanvasOffset.SetArgInput(4, this.moveHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty.Name, Array.Empty<object>()));
            this.moveHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.moveHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.moveHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.moveHandleCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.moveHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.moveHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(GradientToolUI.GetHandlePadding));
            this.moveHandleOpacityHelper = new AnimationStateHelper();
            this.moveHandleOpacityHelper.Element = this.moveHandle;
            this.moveHandleOpacityHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.moveHandleOpacity = new AnimatedDouble(1.0);
                this.moveHandle.SetBinding(UIElement.OpacityProperty, this.moveHandleOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.moveHandleOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.66666666666666663), null);
            };
            this.moveHandleOpacityHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.moveHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.moveHandleOpacity);
            };
            this.handles = new HandleElement[] { this.canvasHandle, this.startHandle, this.endHandle, this.moveHandle };
            foreach (HandleElement element in this.handles)
            {
                ClickDragBehavior.SetIsEnabled(element, true);
                base.Children.Add(element);
            }
            this.handCursor = CursorUtil.LoadResource("Cursors.PanToolCursor.cur");
            this.handMouseDownCursor = CursorUtil.LoadResource("Cursors.PanToolCursorMouseDown.cur");
            this.crosshairCursor = CursorUtil.LoadResource("Cursors.GenericToolCursor.cur");
            this.crosshairMouseDownCursor = CursorUtil.LoadResource("Cursors.GenericToolCursorMouseDown.cur");
            this.canvasHandle.Cursor = this.crosshairCursor;
            for (int i = 1; i < this.handles.Length; i++)
            {
                this.handles[i].SetBinding<TransactedToolState, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath("Tool.State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, Cursor>(this.GetHandleCursor));
            }
        }

        private Cursor GetCanvasCursor(TransactedToolState state)
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

        public HandleElement GetHandle(GradientToolHandleType handleType)
        {
            base.VerifyAccess();
            for (int i = 0; i < this.handles.Length; i++)
            {
                if (GetHandleType(this.handles[i]) == handleType)
                {
                    return this.handles[i];
                }
            }
            ExceptionUtil.ThrowKeyNotFoundException();
            return null;
        }

        private static PointDouble GetHandleCanvasOffset(PointDouble point, SizeDouble actualSize) => 
            new PointDouble(point.X - (actualSize.Width / 2.0), point.Y - (actualSize.Height / 2.0));

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

        public static GradientToolHandleType GetHandleType(UIElement target) => 
            ((GradientToolHandleType) target.GetValue(HandleTypeProperty));

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

        private static PointDouble GetMoveHandleCanvasOffset(PointDouble gradientStartPoint, PointDouble gradientEndPoint, double hairWidth, SizeDouble moveHandleActualSize)
        {
            VectorDouble num2;
            VectorDouble num = (VectorDouble) (gradientEndPoint - gradientStartPoint);
            if (num.LengthSquared <= double.Epsilon)
            {
                num2 = new VectorDouble(1.0, 1.0);
            }
            else
            {
                num2 = num;
            }
            VectorDouble num4 = (VectorDouble) (VectorDouble.Normalize(num2) * (hairWidth * 35.0));
            double x = UIUtil.ScaleWidth(num4.X);
            VectorDouble num5 = new VectorDouble(x, UIUtil.ScaleHeight(num4.Y));
            PointDouble pt = gradientEndPoint + num5;
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

        public static void SetHandleType(UIElement target, GradientToolHandleType value)
        {
            target.SetValue(HandleTypeProperty, EnumUtil.GetBoxed<GradientToolHandleType>(value));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly GradientToolUI.<>c <>9 = new GradientToolUI.<>c();
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__19_1;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__19_4;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__19_7;

            internal void <.ctor>b__19_1(IAnimationStoryboard s, IAnimationVariable v)
            {
                GradientToolUI.InitializeHandleOpacityStoryboard(s, v, 0.0);
            }

            internal void <.ctor>b__19_4(IAnimationStoryboard s, IAnimationVariable v)
            {
                GradientToolUI.InitializeHandleOpacityStoryboard(s, v, 0.33333333333333331);
            }

            internal void <.ctor>b__19_7(IAnimationStoryboard s, IAnimationVariable v)
            {
                GradientToolUI.InitializeHandleOpacityStoryboard(s, v, 0.66666666666666663);
            }
        }
    }
}

