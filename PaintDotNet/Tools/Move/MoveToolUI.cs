namespace PaintDotNet.Tools.Move
{
    using PaintDotNet.Canvas;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using PaintDotNet.UI.Media;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class MoveToolUI : ToolUICanvas<MoveTool, MoveToolChanges>
    {
        public static readonly RoutedEvent EditChangedEvent = TransformControl.EditChangedEvent;
        public static readonly RoutedEvent EditingBeginEvent = TransformControl.EditingBeginEvent;
        public static readonly RoutedEvent EditingCancelledEvent = TransformControl.EditingCancelledEvent;
        public static readonly RoutedEvent EditingFinishedEvent = TransformControl.EditingFinishedEvent;
        private TransformControl transformControl;
        private DependencyFunc<TransactedToolState, RectDouble, Visibility> transformControlVisibility;

        public event RoutedEventHandler EditChanged
        {
            add
            {
                base.AddHandler(EditChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditChangedEvent, value);
            }
        }

        public event TransformEditingBeginEventHandler EditingBegin
        {
            add
            {
                base.AddHandler(EditingBeginEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingBeginEvent, value);
            }
        }

        public event RoutedEventHandler EditingCancelled
        {
            add
            {
                base.AddHandler(EditingCancelledEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingCancelledEvent, value);
            }
        }

        public event RoutedEventHandler EditingFinished
        {
            add
            {
                base.AddHandler(EditingFinishedEvent, value);
            }
            remove
            {
                base.RemoveHandler(EditingFinishedEvent, value);
            }
        }

        public MoveToolUI()
        {
            ClickDragBehavior.SetIsEnabled(this, true);
            ClickDragBehavior.SetAllowClick(this, false);
            this.transformControl = new TransformControl();
            this.transformControl.HitTestPadding = 8.0;
            this.transformControl.BackgroundCursor = Cursors.SizeAll;
            this.transformControl.SetBinding<SizeDouble, double>(FrameworkElement.WidthProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasSizeProperty), BindingMode.OneWay, s => s.Width);
            this.transformControl.SetBinding<SizeDouble, double>(FrameworkElement.HeightProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasSizeProperty), BindingMode.OneWay, s => s.Height);
            PropertyPath sourcePath = PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty);
            this.transformControl.SetBinding(TransformControl.HairWidthProperty, this, sourcePath, BindingMode.OneWay);
            this.transformControlVisibility = new DependencyFunc<TransactedToolState, RectDouble, Visibility>(new Func<TransactedToolState, RectDouble, Visibility>(MoveToolUI.GetTransformControlVisibility));
            this.transformControlVisibility.SetArgInput(1, this, new PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()));
            this.transformControlVisibility.SetArgInput(2, this.transformControl, new PropertyPath(TransformControl.BaseBoundsProperty));
            this.transformControl.SetBinding(UIElement.VisibilityProperty, this.transformControlVisibility, new PropertyPath(this.transformControlVisibility.GetValueProperty().Name, Array.Empty<object>()), BindingMode.OneWay);
            base.Children.Add(this.transformControl);
            this.transformControl.EditingBegin += new TransformEditingBeginEventHandler(this.OnTransformControlEditingBegin);
            this.transformControl.EditingCancelled += new RoutedEventHandler(this.OnTransformControlEditingCancelled);
            this.transformControl.EditChanged += new RoutedEventHandler(this.OnTransformControlEditChanged);
            this.transformControl.EditingFinished += new RoutedEventHandler(this.OnTransformControlEditingFinished);
            base.Loaded += new EventHandler(this.OnLoaded);
        }

        public void BeginEditChanges()
        {
            this.transformControl.BeginEditChanges();
        }

        public void EndEditChanges()
        {
            this.transformControl.EndEditChanges();
        }

        private static Visibility GetTransformControlVisibility(TransactedToolState state, RectDouble baseBounds)
        {
            if (((state != TransactedToolState.Idle) || baseBounds.IsEmpty) && (((state != TransactedToolState.Drawing) && (state != TransactedToolState.Dirty)) && (state != TransactedToolState.Editing)))
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            this.transformControl.Focus();
        }

        private void OnTransformControlEditChanged(object sender, RoutedEventArgs e)
        {
            base.ReRaiseEvent(e);
        }

        private void OnTransformControlEditingBegin(object sender, TransformEditingBeginEventArgs e)
        {
            TransformEditingBeginEventArgs args = new TransformEditingBeginEventArgs(EditingBeginEvent, this, e.EditingMode, e.TriggerHandle) {
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

        public EditTransformToken ActiveEditToken =>
            this.transformControl.ActiveEditToken;

        public Cursor BackgroundCursor
        {
            get => 
                this.transformControl.BackgroundCursor;
            set
            {
                this.transformControl.BackgroundCursor = value;
            }
        }

        public RectDouble BaseBounds
        {
            get => 
                this.transformControl.BaseBounds;
            set
            {
                this.transformControl.BaseBounds = value;
            }
        }

        public Transform BaseTransform
        {
            get => 
                this.transformControl.BaseTransform;
            set
            {
                this.transformControl.BaseTransform = value;
            }
        }

        public Transform DeltaTransform
        {
            get => 
                this.transformControl.DeltaTransform;
            set
            {
                this.transformControl.DeltaTransform = value;
            }
        }

        public Transform EditTransform =>
            this.transformControl.EditTransform;

        public Transform FinalTransform =>
            this.transformControl.FinalTransform;

        public bool IsEditing =>
            this.transformControl.IsEditing;

        public PointDouble? RotationAnchorOffset
        {
            get => 
                this.transformControl.RotationAnchorOffset;
            set
            {
                this.transformControl.RotationAnchorOffset = value;
            }
        }

        public Cursor TranslateCursor
        {
            get => 
                this.transformControl.TranslateCursor;
            set
            {
                this.transformControl.TranslateCursor = value;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MoveToolUI.<>c <>9 = new MoveToolUI.<>c();
            public static Func<SizeDouble, double> <>9__2_0;
            public static Func<SizeDouble, double> <>9__2_1;

            internal double <.ctor>b__2_0(SizeDouble s) => 
                s.Width;

            internal double <.ctor>b__2_1(SizeDouble s) => 
                s.Height;
        }
    }
}

