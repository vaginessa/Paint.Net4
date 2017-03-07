namespace PaintDotNet.UI.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Input;
    using System;
    using System.Windows;

    internal static class ClickDragBehavior
    {
        public static readonly DependencyProperty AllowClickProperty = FrameworkProperty.RegisterAttached("AllowClick", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly DependencyProperty AllowDoubleClickProperty = FrameworkProperty.RegisterAttached("AllowDoubleClick", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        public static readonly DependencyProperty AllowDragProperty = FrameworkProperty.RegisterAttached("AllowDrag", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(true)));
        public static readonly RoutedEvent ClickedEvent = RoutedEvent.Register("Clicked", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(ClickDragBehavior));
        public static readonly RoutedEvent DragBeginEvent = RoutedEvent.Register("DragBegin", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(ClickDragBehavior));
        public static readonly RoutedEvent DragEndEvent = RoutedEvent.Register("DragEnd", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(ClickDragBehavior));
        public static readonly RoutedEvent DragMoveEvent = RoutedEvent.Register("DragMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(ClickDragBehavior));
        public static readonly RoutedEvent GestureBeginEvent = RoutedEvent.Register("GestureBegin", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(ClickDragBehavior));
        public static readonly RoutedEvent GestureEndEvent = RoutedEvent.Register("GestureEnd", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClickDragBehavior));
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsDraggingPropertyKey = FrameworkProperty.RegisterAttachedReadOnly("IsDragging", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        public static readonly DependencyProperty IsEnabledProperty = FrameworkProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(ClickDragBehavior.IsEnabledPropertyChanged)));
        public static readonly RoutedEvent IsPressedChangedEvent = RoutedEvent.Register("IsPressedChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ClickDragBehavior));
        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsPressedPropertyKey = FrameworkProperty.RegisterAttachedReadOnly("IsPressed", typeof(bool), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false), new PropertyChangedCallback(ClickDragBehavior.IsPressedPropertyChanged)));
        private static readonly DependencyProperty MouseCapturePointProperty = MouseCapturePointPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey MouseCapturePointPropertyKey = FrameworkProperty.RegisterAttachedReadOnly("MouseCapturePoint", typeof(PointDouble), typeof(PaintDotNet.UI.FrameworkElement), typeof(ClickDragBehavior), new PaintDotNet.UI.FrameworkPropertyMetadata(PointDouble.BoxedZero));

        public static void AddIsPressedChangedHandler(PaintDotNet.UI.FrameworkElement target, RoutedEventHandler handler)
        {
            target.AddHandler(IsPressedChangedEvent, handler);
        }

        public static bool GetAllowClick(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(AllowClickProperty));

        public static bool GetAllowDoubleClick(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(AllowDoubleClickProperty));

        public static bool GetAllowDrag(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(AllowDragProperty));

        public static bool GetIsDragging(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(IsDraggingProperty));

        public static bool GetIsEnabled(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(IsEnabledProperty));

        public static bool GetIsPressed(PaintDotNet.UI.FrameworkElement target) => 
            ((bool) target.GetValue(IsPressedProperty));

        private static PointDouble GetMouseCapturePoint(PaintDotNet.UI.FrameworkElement target) => 
            ((PointDouble) target.GetValue(MouseCapturePointProperty));

        private static void IsEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement target = (PaintDotNet.UI.FrameworkElement) sender;
            if (!((bool) e.NewValue))
            {
                target.RemoveHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(ClickDragBehavior.OnMouseDown));
                target.RemoveHandler(UIElement.MouseMoveEvent, new MouseEventHandler(ClickDragBehavior.OnMouseMove));
                target.RemoveHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(ClickDragBehavior.OnMouseUp));
                target.RemoveHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(ClickDragBehavior.OnLostMouseCapture));
                target.RemoveHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(ClickDragBehavior.OnMouseLeave));
                SetIsDragging(target, false);
            }
            else
            {
                target.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(ClickDragBehavior.OnMouseDown));
                target.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler(ClickDragBehavior.OnMouseMove));
                target.AddHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(ClickDragBehavior.OnMouseUp));
                target.AddHandler(UIElement.LostMouseCaptureEvent, new MouseEventHandler(ClickDragBehavior.OnLostMouseCapture));
                target.AddHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(ClickDragBehavior.OnMouseLeave));
            }
        }

        private static void IsPressedPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement source = (PaintDotNet.UI.FrameworkElement) sender;
            source.RaiseEvent(new RoutedEventArgs(IsPressedChangedEvent, source));
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement element = (PaintDotNet.UI.FrameworkElement) sender;
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement visual = (PaintDotNet.UI.FrameworkElement) sender;
            if (!visual.IsMouseCaptured)
            {
                PresentationSource source = PresentationSource.FromVisual(visual);
                if ((source != null) && (source.PrimaryMouseDevice.Captured == null))
                {
                    PointDouble position = e.GetPosition(visual);
                    visual.CaptureMouse();
                    visual.RaiseEvent(new MouseEventArgs(e.InputDevice, GestureBeginEvent, visual));
                    if (visual.IsMouseCaptured)
                    {
                        SetIsPressed(visual, true);
                        SetMouseCapturePoint(visual, position);
                        if (!GetAllowClick(visual))
                        {
                            SetIsDragging(visual, true);
                            visual.RaiseEvent(new MouseEventArgs(e.InputDevice, DragBeginEvent, visual));
                        }
                    }
                    else
                    {
                        visual.RaiseEvent(new MouseEventArgs(e.InputDevice, GestureEndEvent, visual));
                    }
                    e.Handled = true;
                }
            }
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement element = (PaintDotNet.UI.FrameworkElement) sender;
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement visual = (PaintDotNet.UI.FrameworkElement) sender;
            if (visual.IsMouseCaptured)
            {
                CompositionTarget compositionTarget = PresentationSource.FromVisual(visual).CompositionTarget;
                PointDouble position = e.GetPosition(visual);
                bool flag = visual.HitTestLocal(position);
                SetIsPressed(visual, flag);
                if (GetIsDragging(visual))
                {
                    visual.RaiseEvent(new MouseEventArgs(e.InputDevice, DragMoveEvent, visual));
                }
                else if (GetAllowDrag(visual))
                {
                    VectorDouble num8;
                    PointDouble mouseCapturePoint = GetMouseCapturePoint(visual);
                    PointDouble pt = e.GetPosition(visual);
                    Matrix3x2Double matrixToDevice = compositionTarget.MatrixToDevice;
                    PointDouble num5 = matrixToDevice.Transform(mouseCapturePoint);
                    VectorDouble num7 = (VectorDouble) (matrixToDevice.Transform(pt) - num5);
                    if (GetAllowClick(visual))
                    {
                        num8 = new VectorDouble(PaintDotNet.UI.SystemParameters.MinimumHorizontalDragDistance, PaintDotNet.UI.SystemParameters.MinimumVerticalDragDistance);
                    }
                    else
                    {
                        num8 = new VectorDouble(0.0, 0.0);
                    }
                    if ((Math.Abs(num7.X) >= num8.X) || (Math.Abs(num7.Y) >= num8.Y))
                    {
                        SetIsDragging(visual, true);
                        visual.RaiseEvent(new MouseEventArgs(e.InputDevice, DragBeginEvent, visual));
                    }
                }
                e.Handled = true;
            }
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PaintDotNet.UI.FrameworkElement target = (PaintDotNet.UI.FrameworkElement) sender;
            if (target.IsMouseCaptured)
            {
                target.ReleaseMouseCapture();
                if (GetIsDragging(target))
                {
                    target.RaiseEvent(new MouseEventArgs(e.InputDevice, DragEndEvent, target));
                }
                else if (GetAllowClick(target))
                {
                    PointDouble position = e.GetPosition(target);
                    if (target.HitTestLocal(position))
                    {
                        target.RaiseEvent(new MouseEventArgs(e.InputDevice, ClickedEvent, target));
                    }
                }
                SetIsDragging(target, false);
                SetIsPressed(target, false);
                target.RaiseEvent(new RoutedEventArgs(GestureEndEvent, target));
                e.Handled = true;
            }
        }

        public static void RemoveIsPressedChangedHandler(PaintDotNet.UI.FrameworkElement target, RoutedEventHandler handler)
        {
            target.RemoveHandler(IsPressedChangedEvent, handler);
        }

        public static void SetAllowClick(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(AllowClickProperty, BooleanUtil.GetBoxed(value));
        }

        public static void SetAllowDoubleClick(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(AllowDoubleClickProperty, BooleanUtil.GetBoxed(value));
        }

        public static void SetAllowDrag(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(AllowDragProperty, BooleanUtil.GetBoxed(value));
        }

        private static void SetIsDragging(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(IsDraggingPropertyKey, BooleanUtil.GetBoxed(value));
        }

        public static void SetIsEnabled(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(IsEnabledProperty, BooleanUtil.GetBoxed(value));
        }

        private static void SetIsPressed(PaintDotNet.UI.FrameworkElement target, bool value)
        {
            target.SetValue(IsPressedPropertyKey, BooleanUtil.GetBoxed(value));
        }

        private static void SetMouseCapturePoint(PaintDotNet.UI.FrameworkElement target, PointDouble value)
        {
            target.SetValue(MouseCapturePointPropertyKey, value);
        }
    }
}

