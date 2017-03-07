namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Tools;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal abstract class ToolUICanvas : PaintDotNet.UI.Controls.Canvas
    {
        public static readonly DependencyProperty CanvasViewProperty = CanvasViewPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CanvasViewPropertyKey = FrameworkProperty.RegisterReadOnly("CanvasView", typeof(PaintDotNet.Canvas.CanvasView), typeof(ToolUICanvas), new PaintDotNet.UI.FrameworkPropertyMetadata(null, new PropertyChangedCallback(<>c.<>9.<.cctor>b__28_2), null, new CoerceValueCallback(<>c.<>9.<.cctor>b__28_3)));
        public static readonly RoutedEvent ClickedEvent = ClickDragBehavior.ClickedEvent;
        public static readonly RoutedEvent DragBeginEvent = ClickDragBehavior.DragBeginEvent;
        public static readonly RoutedEvent DragEndEvent = ClickDragBehavior.DragEndEvent;
        public static readonly RoutedEvent DragMoveEvent = ClickDragBehavior.DragMoveEvent;
        public static readonly RoutedEvent GestureBeginEvent = ClickDragBehavior.GestureBeginEvent;
        public static readonly RoutedEvent GestureEndEvent = ClickDragBehavior.GestureEndEvent;
        public const double HitTestPadding = 8.0;
        public static readonly DependencyProperty ToolProperty = FrameworkProperty.Register("Tool", typeof(PaintDotNet.Tools.Tool), typeof(ToolUICanvas), new PaintDotNet.UI.FrameworkPropertyMetadata(null, new PropertyChangedCallback(<>c.<>9.<.cctor>b__28_0), null, new CoerceValueCallback(<>c.<>9.<.cctor>b__28_1)));

        protected ToolUICanvas()
        {
            base.ClipToBounds = false;
            base.Focusable = true;
            base.Loaded += new EventHandler(this.OnLoaded);
            base.Unloaded += new EventHandler(this.OnUnloaded);
        }

        private void CanvasViewPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            PaintDotNet.Canvas.CanvasView oldValue = (PaintDotNet.Canvas.CanvasView) e.OldValue;
            PaintDotNet.Canvas.CanvasView newValue = (PaintDotNet.Canvas.CanvasView) e.NewValue;
            this.OnCanvasViewChanged(oldValue, newValue);
        }

        private object CoerceCanvasViewProperty(object baseValue)
        {
            PaintDotNet.Tools.Tool tool = this.Tool;
            return tool?.CanvasView;
        }

        private object CoerceToolProperty(object baseValue)
        {
            if (!base.IsLoaded)
            {
                return null;
            }
            return baseValue;
        }

        protected virtual void OnCanvasViewChanged(PaintDotNet.Canvas.CanvasView oldValue, PaintDotNet.Canvas.CanvasView newValue)
        {
        }

        protected override void OnInitialized()
        {
            this.SetInitialFocus();
            base.OnInitialized();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            base.CoerceValue(ToolProperty);
        }

        protected virtual void OnSetInitialFocus()
        {
            if (base.Focusable)
            {
                base.Focus();
            }
        }

        protected virtual void OnToolChanged(PaintDotNet.Tools.Tool oldValue, PaintDotNet.Tools.Tool newValue)
        {
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            base.CoerceValue(ToolProperty);
        }

        protected void ReRaiseEvent(RoutedEventArgs e)
        {
            if (e.GetType() != typeof(RoutedEventArgs))
            {
                ExceptionUtil.ThrowInvalidOperationException("can only use RoutedEventArgs, not any derived class");
            }
            RoutedEventArgs args = new RoutedEventArgs(e.RoutedEvent, this) {
                Handled = e.Handled
            };
            base.RaiseEvent(args);
            e.Handled = args.Handled;
        }

        public void SetInitialFocus()
        {
            this.OnSetInitialFocus();
        }

        private void ToolPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.CoerceValue(CanvasViewProperty);
            PaintDotNet.Tools.Tool oldValue = (PaintDotNet.Tools.Tool) e.OldValue;
            PaintDotNet.Tools.Tool newValue = (PaintDotNet.Tools.Tool) e.NewValue;
            this.OnToolChanged(oldValue, newValue);
        }

        public PaintDotNet.Canvas.CanvasView CanvasView =>
            ((PaintDotNet.Canvas.CanvasView) base.GetValue(CanvasViewProperty));

        public PaintDotNet.Tools.Tool Tool
        {
            get => 
                ((PaintDotNet.Tools.Tool) base.GetValue(ToolProperty));
            set
            {
                base.SetValue(ToolProperty, value);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ToolUICanvas.<>c <>9 = new ToolUICanvas.<>c();

            internal void <.cctor>b__28_0(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((ToolUICanvas) s).ToolPropertyChanged(e);
            }

            internal object <.cctor>b__28_1(DependencyObject dO, object bV) => 
                ((ToolUICanvas) dO).CoerceToolProperty(bV);

            internal void <.cctor>b__28_2(DependencyObject s, DependencyPropertyChangedEventArgs e)
            {
                ((ToolUICanvas) s).CanvasViewPropertyChanged(e);
            }

            internal object <.cctor>b__28_3(DependencyObject dO, object bV) => 
                ((ToolUICanvas) dO).CoerceCanvasViewProperty(bV);
        }
    }
}

