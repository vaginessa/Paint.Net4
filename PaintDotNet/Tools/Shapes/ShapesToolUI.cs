namespace PaintDotNet.Tools.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.PropertySystem;
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal sealed class ShapesToolUI : ToolUICanvas<ShapesToolBase, ShapesToolChanges>
    {
        private HandleElement canvasHandle;
        private Cursor canvasMouseDownCursor = CursorUtil.LoadResource("Cursors.ShapeToolCursorMouseDown.cur");
        private Cursor canvasMouseUpCursor = CursorUtil.LoadResource("Cursors.ShapeToolCursor.cur");
        private AnimationStateHelper endPointAnimationHelper;
        private DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble> endPointCanvasOffset;
        private HandleElement endPointHandle;
        private AnimatedDouble endPointOpacity;
        private Cursor handCursor = CursorUtil.LoadResource("Cursors.PanToolCursor.cur");
        public static readonly DependencyProperty HandleTypeProperty = DependencyProperty.RegisterAttached("HandleType", typeof(ShapesToolHandleType), typeof(ShapesToolUI), new PropertyMetadata(EnumUtil.GetBoxed<ShapesToolHandleType>(ShapesToolHandleType.None)));
        private Cursor handMouseDownCursor = CursorUtil.LoadResource("Cursors.PanToolCursorMouseDown.cur");
        private Dictionary<object, PaintDotNet.UI.FrameworkElement> propertyNameToControlMap = new Dictionary<object, PaintDotNet.UI.FrameworkElement>();
        private static readonly DependencyObjectTagKey<DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>> shapePropertyControlCanvasOffsetTagKey = DependencyObjectTagKey.Create<DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>>();
        private static readonly DependencyObjectTagKey<AnimationStateHelper> shapePropertyControlOpacityAnimationStateHelperTagKey = DependencyObjectTagKey.Create<AnimationStateHelper>();
        private static readonly DependencyObjectTagKey<AnimatedDouble> shapePropertyControlOpacityAnimationTagKey = DependencyObjectTagKey.Create<AnimatedDouble>();
        public static readonly DependencyProperty ShapePropertyNameProperty = DependencyProperty.RegisterAttached("ShapePropertyName", typeof(object), typeof(ShapesToolUI), new PropertyMetadata(null));
        private DependencyFunc<TransactedToolState, ShapesToolChanges, Visibility> startEndPointHandleVisibility;
        private AnimationStateHelper startPointAnimationHelper;
        private DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble> startPointCanvasOffset;
        private HandleElement startPointHandle;
        private AnimatedDouble startPointOpacity;
        private DependencyValue<ShapesToolChanges> toolChanges = new DependencyValue<ShapesToolChanges>();
        private TransformControl transformControl;
        private DependencyFunc<TransactedToolState, ShapesToolChanges, bool> transformControlAreScaleHandlesVisible;
        private DependencyFunc<TransactedToolState, ShapesToolChanges, Visibility> transformControlVisibility;
        public static readonly RoutedEvent TransformEditChangedEvent = TransformControl.EditChangedEvent;
        public static readonly RoutedEvent TransformEditingBeginEvent = TransformControl.EditingBeginEvent;
        public static readonly RoutedEvent TransformEditingCancelledEvent = TransformControl.EditingCancelledEvent;
        public static readonly RoutedEvent TransformEditingFinishedEvent = TransformControl.EditingFinishedEvent;

        public event RoutedEventHandler TransformEditChanged
        {
            add
            {
                base.AddHandler(TransformEditChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TransformEditChangedEvent, value);
            }
        }

        public event TransformEditingBeginEventHandler TransformEditingBegin
        {
            add
            {
                base.AddHandler(TransformEditingBeginEvent, value);
            }
            remove
            {
                base.RemoveHandler(TransformEditingBeginEvent, value);
            }
        }

        public event RoutedEventHandler TransformEditingCancelled
        {
            add
            {
                base.AddHandler(TransformEditingCancelledEvent, value);
            }
            remove
            {
                base.RemoveHandler(TransformEditingCancelledEvent, value);
            }
        }

        public event RoutedEventHandler TransformEditingFinished
        {
            add
            {
                base.AddHandler(TransformEditingFinishedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TransformEditingFinishedEvent, value);
            }
        }

        public ShapesToolUI()
        {
            this.toolChanges.ValueChanged += new ValueChangedEventHandler<ShapesToolChanges>(this.OnToolChangesChanged);
            this.toolChanges.SetBinding(this.toolChanges.GetValueProperty(), this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".Changes", Array.Empty<object>()), BindingMode.OneWay);
            this.canvasHandle = new HandleElement();
            SetHandleType(this.canvasHandle, ShapesToolHandleType.Canvas);
            this.canvasHandle.Focusable = true;
            this.canvasHandle.ClipToBounds = false;
            ClickDragBehavior.SetAllowClick(this.canvasHandle, false);
            ClickDragBehavior.SetAllowDrag(this.canvasHandle, true);
            ClickDragBehavior.SetIsEnabled(this.canvasHandle, true);
            this.canvasHandle.SetBinding<bool, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseCaptureWithinProperty), BindingMode.OneWay, delegate (bool imcw) {
                if (!imcw)
                {
                    return this.canvasMouseUpCursor;
                }
                return this.canvasMouseDownCursor;
            });
            this.transformControl = new TransformControl();
            this.transformControl.HitTestPadding = 8.0;
            this.transformControl.SetBinding<TransactedToolState, bool>(TransformControl.AllowBackgroundClickProperty, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()), BindingMode.OneWay, new Func<TransactedToolState, bool>(ShapesToolUI.GetTransformControlAllowBackgroundClick));
            this.transformControl.SetBinding<bool, Cursor>(TransformControl.BackgroundCursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseCaptureWithinProperty), BindingMode.OneWay, delegate (bool imcw) {
                if (!imcw)
                {
                    return this.canvasMouseUpCursor;
                }
                return this.canvasMouseDownCursor;
            });
            this.transformControl.SetBinding<SizeDouble, double>(PaintDotNet.UI.FrameworkElement.WidthProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasSizeProperty), BindingMode.OneWay, s => s.Width);
            this.transformControl.SetBinding<SizeDouble, double>(PaintDotNet.UI.FrameworkElement.HeightProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasSizeProperty), BindingMode.OneWay, s => s.Height);
            this.transformControl.SetBinding(TransformControl.HairWidthProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.transformControlVisibility = new DependencyFunc<TransactedToolState, ShapesToolChanges, Visibility>(new Func<TransactedToolState, ShapesToolChanges, Visibility>(ShapesToolUI.GetTransformControlVisibility));
            this.transformControlVisibility.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()));
            this.transformControlVisibility.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".Changes", Array.Empty<object>()));
            this.transformControl.SetBinding(UIElement.VisibilityProperty, this.transformControlVisibility, new PaintDotNet.ObjectModel.PropertyPath(this.transformControlVisibility.GetValueProperty()), BindingMode.OneWay);
            this.transformControlAreScaleHandlesVisible = new DependencyFunc<TransactedToolState, ShapesToolChanges, bool>(new Func<TransactedToolState, ShapesToolChanges, bool>(ShapesToolUI.GetTransformControlAreScaleHandlesVisible));
            this.transformControlAreScaleHandlesVisible.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()));
            this.transformControlAreScaleHandlesVisible.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".Changes", Array.Empty<object>()));
            this.transformControl.SetBinding(TransformControl.AreScaleHandlesVisibleProperty, this.transformControlAreScaleHandlesVisible, new PaintDotNet.ObjectModel.PropertyPath(this.transformControlAreScaleHandlesVisible.GetValueProperty()), BindingMode.OneWay);
            this.transformControl.EditingBegin += new TransformEditingBeginEventHandler(this.OnTransformControlEditingBegin);
            this.transformControl.EditingCancelled += new RoutedEventHandler(this.OnTransformControlEditingCancelled);
            this.transformControl.EditChanged += new RoutedEventHandler(this.OnTransformControlEditChanged);
            this.transformControl.EditingFinished += new RoutedEventHandler(this.OnTransformControlEditingFinished);
            this.startEndPointHandleVisibility = new DependencyFunc<TransactedToolState, ShapesToolChanges, Visibility>(new Func<TransactedToolState, ShapesToolChanges, Visibility>(ShapesToolUI.GetStartEndPointHandleVisibility));
            this.startEndPointHandleVisibility.SetArgInput(1, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()));
            this.startEndPointHandleVisibility.SetArgInput(2, this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".Changes", Array.Empty<object>()));
            this.startPointHandle = new HandleElement(new CircleHandleDrawing());
            ClickDragBehavior.SetAllowClick(this.startPointHandle, false);
            ClickDragBehavior.SetAllowDrag(this.startPointHandle, true);
            ClickDragBehavior.SetIsEnabled(this.startPointHandle, true);
            SetHandleType(this.startPointHandle, ShapesToolHandleType.StartPoint);
            this.startPointHandle.SetBinding(UIElement.VisibilityProperty, this.startEndPointHandleVisibility, new PaintDotNet.ObjectModel.PropertyPath(this.startEndPointHandleVisibility.GetValueProperty()), BindingMode.OneWay);
            this.startPointHandle.SetBinding<bool, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseCaptureWithinProperty), BindingMode.OneWay, delegate (bool imcw) {
                if (!imcw)
                {
                    return this.handCursor;
                }
                return this.handMouseDownCursor;
            });
            this.startPointCanvasOffset = new DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>((ch, actsz) => GetStartEndPointHandleCanvasOffset(ShapesToolHandleType.StartPoint, ch, actsz));
            this.startPointCanvasOffset.SetArgInput<ShapesToolChanges>(1, this.toolChanges);
            this.startPointCanvasOffset.SetArgInput(2, this.startPointHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty));
            this.startPointHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.startPointCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath(this.startPointCanvasOffset.GetValueProperty().Name + ".X", Array.Empty<object>()), BindingMode.OneWay);
            this.startPointHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.startPointCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath(this.startPointCanvasOffset.GetValueProperty().Name + ".Y", Array.Empty<object>()), BindingMode.OneWay);
            this.startPointHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.startPointHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(ShapesToolUI.GetHandlePadding));
            this.startPointAnimationHelper = new AnimationStateHelper();
            this.startPointAnimationHelper.Element = this.startPointHandle;
            this.startPointAnimationHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.startPointOpacity = new AnimatedDouble(1.0);
                this.startPointHandle.SetBinding(UIElement.OpacityProperty, this.startPointOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.startPointOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.0), null);
            };
            this.startPointAnimationHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.startPointHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.startPointOpacity);
            };
            this.endPointHandle = new HandleElement(new CircleHandleDrawing());
            ClickDragBehavior.SetAllowClick(this.endPointHandle, false);
            ClickDragBehavior.SetAllowDrag(this.endPointHandle, true);
            ClickDragBehavior.SetIsEnabled(this.endPointHandle, true);
            SetHandleType(this.endPointHandle, ShapesToolHandleType.EndPoint);
            this.endPointHandle.SetBinding(UIElement.VisibilityProperty, this.startEndPointHandleVisibility, new PaintDotNet.ObjectModel.PropertyPath(this.startEndPointHandleVisibility.GetValueProperty()), BindingMode.OneWay);
            this.endPointHandle.SetBinding<bool, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseCaptureWithinProperty), BindingMode.OneWay, delegate (bool imcw) {
                if (!imcw)
                {
                    return this.handCursor;
                }
                return this.handMouseDownCursor;
            });
            this.endPointCanvasOffset = new DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>((ch, actsz) => GetStartEndPointHandleCanvasOffset(ShapesToolHandleType.EndPoint, ch, actsz));
            this.endPointCanvasOffset.SetArgInput<ShapesToolChanges>(1, this.toolChanges);
            this.endPointCanvasOffset.SetArgInput(2, this.endPointHandle, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty));
            this.endPointHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.endPointCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath(this.endPointCanvasOffset.GetValueProperty().Name + ".X", Array.Empty<object>()), BindingMode.OneWay);
            this.endPointHandle.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.endPointCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath(this.endPointCanvasOffset.GetValueProperty().Name + ".Y", Array.Empty<object>()), BindingMode.OneWay);
            this.endPointHandle.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            this.endPointHandle.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(ShapesToolUI.GetHandlePadding));
            this.endPointAnimationHelper = new AnimationStateHelper();
            this.endPointAnimationHelper.Element = this.endPointHandle;
            this.endPointAnimationHelper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.endPointOpacity = new AnimatedDouble(1.0);
                this.endPointHandle.SetBinding(UIElement.OpacityProperty, this.endPointOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                this.endPointOpacity.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.5), null);
            };
            this.endPointAnimationHelper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                this.endPointHandle.ClearBinding(UIElement.OpacityProperty);
                DisposableUtil.Free<AnimatedDouble>(ref this.endPointOpacity);
            };
            base.Children.Add(this.canvasHandle);
            base.Children.Add(this.transformControl);
            base.Children.Add(this.startPointHandle);
            base.Children.Add(this.endPointHandle);
            base.Loaded += new EventHandler(this.OnLoaded);
            base.Unloaded += new EventHandler(this.OnUnloaded);
        }

        private void EnsureShapePropertyControlRemoved(object propertyName)
        {
            PaintDotNet.UI.FrameworkElement element;
            if (this.propertyNameToControlMap.TryGetValue(propertyName, out element))
            {
                DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble> func;
                AnimationStateHelper helper;
                element.ClearValue(ShapePropertyNameProperty);
                element.ClearAllBindings();
                base.RemoveVisualChild(element);
                this.propertyNameToControlMap.Remove(propertyName);
                if (DependencyObjectTagger.TryGet<DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>>(element, shapePropertyControlCanvasOffsetTagKey, out func))
                {
                    func.ClearAllBindings();
                    DependencyObjectTagger.Remove(element, shapePropertyControlCanvasOffsetTagKey);
                }
                if (DependencyObjectTagger.TryGet<AnimationStateHelper>(element, shapePropertyControlOpacityAnimationStateHelperTagKey, out helper))
                {
                    DependencyObjectTagger.Remove(element, shapePropertyControlOpacityAnimationStateHelperTagKey);
                    helper.Dispose();
                }
            }
        }

        private void EnsureShapePropertyControlUpdated(object propertyName)
        {
            PaintDotNet.UI.FrameworkElement element;
            Property property = this.TryGetChanges().ShapePropertySchema[propertyName];
            if (!(property is DoubleVectorProperty))
            {
                throw new InternalErrorException();
            }
            if (!this.propertyNameToControlMap.TryGetValue(propertyName, out element))
            {
                CircleHandleDrawing drawing = new CircleHandleDrawing();
                HandleElement handleElement = new HandleElement(drawing);
                handleElement.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
                handleElement.SetBinding<double, PaintDotNet.UI.Thickness>(DrawingElement.PaddingProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay, new Func<double, PaintDotNet.UI.Thickness>(ShapesToolUI.GetHandlePadding));
                handleElement.SetBinding<bool, Cursor>(PaintDotNet.UI.FrameworkElement.CursorProperty, this, new PaintDotNet.ObjectModel.PropertyPath(UIElement.IsMouseCaptureWithinProperty), BindingMode.OneWay, delegate (bool imcw) {
                    if (!imcw)
                    {
                        return this.handCursor;
                    }
                    return this.handMouseDownCursor;
                });
                DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble> func = new DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>((stc, actsz) => GetShapePropertyControlCanvasOffset(propertyName, stc, actsz));
                func.SetArgInput<ShapesToolChanges>(1, this.toolChanges);
                func.SetArgInput(2, handleElement, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty));
                handleElement.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, func, new PaintDotNet.ObjectModel.PropertyPath(func.GetValueProperty().Name + ".X", Array.Empty<object>()), BindingMode.OneWay);
                handleElement.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, func, new PaintDotNet.ObjectModel.PropertyPath(func.GetValueProperty().Name + ".Y", Array.Empty<object>()), BindingMode.OneWay);
                DependencyObjectTagger.Set<DependencyFunc<ShapesToolChanges, SizeDouble, PointDouble>>(handleElement, shapePropertyControlCanvasOffsetTagKey, func);
                AnimationStateHelper helper = new AnimationStateHelper {
                    Element = handleElement
                };
                helper.EnableAnimations += delegate (object <sender>, EventArgs <e>) {
                    AnimatedDouble sourceObject = new AnimatedDouble(1.0);
                    handleElement.SetBinding(UIElement.OpacityProperty, sourceObject, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
                    sourceObject.AnimateRawValue((s, v) => InitializeHandleOpacityStoryboard(s, v, 0.5), null);
                    DependencyObjectTagger.Set<AnimatedDouble>(handleElement, shapePropertyControlOpacityAnimationTagKey, sourceObject);
                };
                helper.DisableAnimations += delegate (object <sender>, EventArgs <e>) {
                    AnimatedDouble num;
                    handleElement.ClearBinding(UIElement.OpacityProperty);
                    if (DependencyObjectTagger.TryGet<AnimatedDouble>(handleElement, shapePropertyControlOpacityAnimationTagKey, out num))
                    {
                        num.StopAnimation();
                        num.Dispose();
                        DependencyObjectTagger.Remove(handleElement, shapePropertyControlOpacityAnimationTagKey);
                    }
                };
                DependencyObjectTagger.Set<AnimationStateHelper>(handleElement, shapePropertyControlOpacityAnimationStateHelperTagKey, helper);
                ClickDragBehavior.SetAllowClick(handleElement, false);
                ClickDragBehavior.SetAllowDoubleClick(handleElement, false);
                ClickDragBehavior.SetAllowDrag(handleElement, true);
                ClickDragBehavior.SetIsEnabled(handleElement, true);
                SetShapePropertyName(handleElement, propertyName);
                SetHandleType(handleElement, ShapesToolHandleType.ShapeProperty);
                base.AddVisualChild(handleElement);
                element = handleElement;
                this.propertyNameToControlMap.Add(propertyName, element);
            }
        }

        [IteratorStateMachine(typeof(<EnumerateShapePropertyElements>d__35))]
        private IEnumerable<PaintDotNet.UI.FrameworkElement> EnumerateShapePropertyElements() => 
            new <EnumerateShapePropertyElements>d__35(-2) { <>4__this = this };

        private static PaintDotNet.UI.Thickness GetHandlePadding(double canvasHairWidth) => 
            new PaintDotNet.UI.Thickness(canvasHairWidth * 8.0);

        public static ShapesToolHandleType GetHandleType(UIElement target) => 
            ((ShapesToolHandleType) target.GetValue(HandleTypeProperty));

        private static PointDouble GetShapePropertyControlCanvasOffset(object propertyName, ShapesToolChanges changes, SizeDouble actualSize)
        {
            if (changes == null)
            {
                return PointDouble.Zero;
            }
            Pair<double, double> pair = (Pair<double, double>) changes.ShapePropertyValues[propertyName];
            PointDouble pt = new PointDouble(pair.First, pair.Second);
            PointDouble num3 = changes.Transform.Transform(pt);
            double x = num3.X - (actualSize.Width / 2.0);
            return new PointDouble(x, num3.Y - (actualSize.Height / 2.0));
        }

        public static object GetShapePropertyName(UIElement target) => 
            target.GetValue(ShapePropertyNameProperty);

        private static PointDouble GetStartEndPointHandleCanvasOffset(ShapesToolHandleType whichHandle, ShapesToolChanges changes, SizeDouble actualSize)
        {
            PointDouble endPoint;
            PointDouble num3;
            if (changes == null)
            {
                return PointDouble.Zero;
            }
            if (whichHandle != ShapesToolHandleType.StartPoint)
            {
                if (whichHandle != ShapesToolHandleType.EndPoint)
                {
                    throw new InternalErrorException();
                }
            }
            else
            {
                endPoint = changes.StartPoint;
                goto Label_0031;
            }
            endPoint = changes.EndPoint;
        Label_0031:
            num3 = changes.Transform.Transform(endPoint);
            double x = num3.X - (actualSize.Width / 2.0);
            return new PointDouble(x, num3.Y - (actualSize.Height / 2.0));
        }

        private static Visibility GetStartEndPointHandleVisibility(TransactedToolState state, ShapesToolChanges changes)
        {
            Visibility hidden;
            if ((state == TransactedToolState.Idle) || (state == TransactedToolState.Drawing))
            {
                hidden = Visibility.Hidden;
            }
            else if ((state == TransactedToolState.Dirty) || (state == TransactedToolState.Editing))
            {
                hidden = Visibility.Visible;
            }
            else
            {
                hidden = Visibility.Hidden;
            }
            if (hidden == Visibility.Visible)
            {
                if (changes == null)
                {
                    return Visibility.Hidden;
                }
                if (changes.Shape.Options.Transform != ShapeTransformOption.MoveStartAndEndPoints)
                {
                    hidden = Visibility.Hidden;
                }
            }
            return hidden;
        }

        private static bool GetTransformControlAllowBackgroundClick(TransactedToolState state)
        {
            switch (state)
            {
                case TransactedToolState.Dirty:
                    return true;
            }
            return false;
        }

        private static bool GetTransformControlAreScaleHandlesVisible(TransactedToolState state, ShapesToolChanges changes)
        {
            bool flag;
            if ((state == TransactedToolState.Idle) || (state == TransactedToolState.Drawing))
            {
                flag = false;
            }
            else if ((state == TransactedToolState.Dirty) || (state == TransactedToolState.Editing))
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            return ((!flag || ((changes != null) && (changes.Shape.Options.Transform == ShapeTransformOption.FullTransform))) && flag);
        }

        private static Visibility GetTransformControlVisibility(TransactedToolState state, ShapesToolChanges changes)
        {
            Visibility hidden;
            if ((state == TransactedToolState.Idle) || (state == TransactedToolState.Drawing))
            {
                hidden = Visibility.Hidden;
            }
            else if ((state == TransactedToolState.Dirty) || (state == TransactedToolState.Editing))
            {
                hidden = Visibility.Visible;
            }
            else
            {
                hidden = Visibility.Hidden;
            }
            if ((hidden == Visibility.Visible) && (changes == null))
            {
                hidden = Visibility.Hidden;
            }
            return hidden;
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

        private void OnLoaded(object sender, EventArgs e)
        {
            this.UpdateShapePropertyControls();
        }

        protected override void OnSetInitialFocus()
        {
            this.canvasHandle.Focus();
            base.OnSetInitialFocus();
        }

        private void OnToolChangesChanged(object sender, ValueChangedEventArgs<ShapesToolChanges> e)
        {
            this.UpdateShapePropertyControls();
            ShapesToolChanges newValue = e.NewValue;
            if ((newValue == null) || (newValue.Shape.Category != ShapeCategory.Lines))
            {
                this.transformControl.ClearValue(TransformControl.TranslateBoxClipProperty);
                this.transformControl.ClearValue(TransformControl.RotateBoxClipProperty);
                this.transformControl.ClearValue(TransformControl.IsRotationAnchorVisibleProperty);
            }
            else
            {
                this.transformControl.TranslateBoxClip = Geometry.Empty;
                this.transformControl.RotateBoxClip = Geometry.Empty;
                this.transformControl.IsRotationAnchorVisible = false;
            }
        }

        private void OnTransformControlEditChanged(object sender, RoutedEventArgs e)
        {
            base.ReRaiseEvent(e);
        }

        private void OnTransformControlEditingBegin(object sender, TransformEditingBeginEventArgs e)
        {
            TransformEditingBeginEventArgs args = new TransformEditingBeginEventArgs(TransformEditingBeginEvent, this, e.EditingMode, e.TriggerHandle) {
                Cancel = e.Cancel,
                Handled = e.Handled
            };
            base.RaiseEvent(args);
            e.Cancel = args.Cancel;
            e.Handled = args.Handled;
        }

        private void OnTransformControlEditingCancelled(object sender, RoutedEventArgs e)
        {
            base.ReRaiseEvent(e);
        }

        private void OnTransformControlEditingFinished(object sender, RoutedEventArgs e)
        {
            base.ReRaiseEvent(e);
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            this.UpdateShapePropertyControls();
        }

        public static void SetHandleType(UIElement target, ShapesToolHandleType value)
        {
            target.SetValue(HandleTypeProperty, EnumUtil.GetBoxed<ShapesToolHandleType>(value));
        }

        public static void SetShapePropertyName(UIElement target, object value)
        {
            target.SetValue(ShapePropertyNameProperty, value);
        }

        private ShapesToolChanges TryGetChanges()
        {
            base.VerifyAccess();
            if ((base.Tool != null) && (base.Tool.Changes != null))
            {
                return base.Tool.Changes;
            }
            return null;
        }

        private void UpdateShapePropertyControl(object propertyName)
        {
            Validate.IsNotNull<object>(propertyName, "propertyName");
            base.VerifyAccess();
            ShapesToolChanges changes = this.TryGetChanges();
            if ((!base.IsLoaded || (changes == null)) || (!changes.ShapePropertyValues.ContainsKey(propertyName) || (changes.ShapePropertySchema[propertyName] == null)))
            {
                this.EnsureShapePropertyControlRemoved(propertyName);
            }
            else
            {
                this.EnsureShapePropertyControlUpdated(propertyName);
            }
        }

        private void UpdateShapePropertyControls()
        {
            IEnumerable<object> keys;
            base.VerifyAccess();
            ShapesToolChanges changes = this.TryGetChanges();
            IEnumerable<object> first = from fxe in this.EnumerateShapePropertyElements() select GetShapePropertyName(fxe);
            if ((changes == null) || !changes.HasAllShapePropertyValues)
            {
                keys = Array.Empty<object>();
            }
            else
            {
                keys = changes.ShapePropertyValues.Keys;
            }
            foreach (object obj2 in first.Concat<object>(keys).Distinct<object>().ToArrayEx<object>())
            {
                this.UpdateShapePropertyControl(obj2);
            }
        }

        public RectDouble TransformBaseBounds
        {
            get => 
                this.transformControl.BaseBounds;
            set
            {
                this.transformControl.BaseBounds = value;
            }
        }

        public Transform TransformBaseTransform
        {
            get => 
                this.transformControl.BaseTransform;
            set
            {
                this.transformControl.BaseTransform = value;
            }
        }

        public Transform TransformDeltaTransform
        {
            get => 
                this.transformControl.DeltaTransform;
            set
            {
                this.transformControl.DeltaTransform = value;
            }
        }

        public Transform TransformEditTransform =>
            this.transformControl.EditTransform;

        public Transform TransformFinalTransform =>
            this.transformControl.FinalTransform;

        public bool TransformIsEditing =>
            this.transformControl.IsEditing;

        public PointDouble? TransformRotationAnchorOffset
        {
            get => 
                this.transformControl.RotationAnchorOffset;
            set
            {
                this.transformControl.RotationAnchorOffset = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ShapesToolUI.<>c <>9 = new ShapesToolUI.<>c();
            public static Func<ShapesToolChanges, SizeDouble, PointDouble> <>9__22_10;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__22_12;
            public static Func<SizeDouble, double> <>9__22_2;
            public static Func<SizeDouble, double> <>9__22_3;
            public static Func<ShapesToolChanges, SizeDouble, PointDouble> <>9__22_5;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__22_7;
            public static Func<PaintDotNet.UI.FrameworkElement, object> <>9__27_0;
            public static Action<IAnimationStoryboard, IAnimationVariable> <>9__30_3;

            internal PointDouble <.ctor>b__22_10(ShapesToolChanges ch, SizeDouble actsz) => 
                ShapesToolUI.GetStartEndPointHandleCanvasOffset(ShapesToolHandleType.EndPoint, ch, actsz);

            internal void <.ctor>b__22_12(IAnimationStoryboard s, IAnimationVariable v)
            {
                ShapesToolUI.InitializeHandleOpacityStoryboard(s, v, 0.5);
            }

            internal double <.ctor>b__22_2(SizeDouble s) => 
                s.Width;

            internal double <.ctor>b__22_3(SizeDouble s) => 
                s.Height;

            internal PointDouble <.ctor>b__22_5(ShapesToolChanges ch, SizeDouble actsz) => 
                ShapesToolUI.GetStartEndPointHandleCanvasOffset(ShapesToolHandleType.StartPoint, ch, actsz);

            internal void <.ctor>b__22_7(IAnimationStoryboard s, IAnimationVariable v)
            {
                ShapesToolUI.InitializeHandleOpacityStoryboard(s, v, 0.0);
            }

            internal void <EnsureShapePropertyControlUpdated>b__30_3(IAnimationStoryboard s, IAnimationVariable v)
            {
                ShapesToolUI.InitializeHandleOpacityStoryboard(s, v, 0.5);
            }

            internal object <UpdateShapePropertyControls>b__27_0(PaintDotNet.UI.FrameworkElement fxe) => 
                ShapesToolUI.GetShapePropertyName(fxe);
        }

        [CompilerGenerated]
        private sealed class <EnumerateShapePropertyElements>d__35 : IEnumerable<PaintDotNet.UI.FrameworkElement>, IEnumerable, IEnumerator<PaintDotNet.UI.FrameworkElement>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PaintDotNet.UI.FrameworkElement <>2__current;
            public ShapesToolUI <>4__this;
            private int <>l__initialThreadId;
            private IEnumerator<Visual> <visualChildren>5__1;

            [DebuggerHidden]
            public <EnumerateShapePropertyElements>d__35(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<visualChildren>5__1 = this.<>4__this.GetVisualChildrenEnumerator();
                    while (this.<visualChildren>5__1.MoveNext())
                    {
                        PaintDotNet.UI.FrameworkElement current = this.<visualChildren>5__1.Current as PaintDotNet.UI.FrameworkElement;
                        if ((current == null) || (ShapesToolUI.GetShapePropertyName(current) == null))
                        {
                            continue;
                        }
                        this.<>2__current = current;
                        this.<>1__state = 1;
                        return true;
                    Label_0058:
                        this.<>1__state = -1;
                    }
                    return false;
                }
                if (num != 1)
                {
                    return false;
                }
                goto Label_0058;
            }

            [DebuggerHidden]
            IEnumerator<PaintDotNet.UI.FrameworkElement> IEnumerable<PaintDotNet.UI.FrameworkElement>.GetEnumerator()
            {
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new ShapesToolUI.<EnumerateShapePropertyElements>d__35(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.UI.FrameworkElement>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            PaintDotNet.UI.FrameworkElement IEnumerator<PaintDotNet.UI.FrameworkElement>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

