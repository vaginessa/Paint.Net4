namespace PaintDotNet.Tools.BrushBase
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Canvas;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using System;
    using System.Windows;

    internal abstract class BrushToolUIBase<TDerived, TTool, TChanges> : ToolUICanvas<TTool, TChanges> where TDerived: BrushToolUIBase<TDerived, TTool, TChanges> where TTool: PresentationBasedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private CircleHandleDrawing brushPreviewDrawing;
        private HandleElement brushPreviewElement;
        private DependencyFunc<PointDouble, SizeDouble, PointDouble> brushPreviewElementCanvasOffset;
        private AnimatedDouble brushPreviewElementOpacity;
        private Cursor canvasMouseUpCursor;
        private DependencyValue<PointDouble> mouseCenterPt;
        private DependencyValue<TransactedToolState> toolState;

        public BrushToolUIBase(string canvasMouseCursorUpResName)
        {
            ClickDragBehavior.SetIsEnabled(this, true);
            ClickDragBehavior.SetAllowClick(this, false);
            this.canvasMouseUpCursor = CursorUtil.LoadResource(canvasMouseCursorUpResName);
            this.mouseCenterPt = new DependencyValue<PointDouble>();
            this.toolState = new DependencyValue<TransactedToolState>();
            this.toolState.SetBinding(this.toolState.GetValueProperty(), this, new PaintDotNet.ObjectModel.PropertyPath(ToolUICanvas.ToolProperty.Name + ".State", Array.Empty<object>()), BindingMode.OneWay);
            this.toolState.ValueChanged += new ValueChangedEventHandler<TransactedToolState>(this.OnToolStateChanged);
            this.brushPreviewDrawing = new CircleHandleDrawing();
            this.brushPreviewDrawing.AutoScaleWithDpi = false;
            this.brushPreviewElement = new HandleElement(this.brushPreviewDrawing);
            this.brushPreviewElement.IsHitTestVisible = false;
            this.brushPreviewElement.IsHotOnMouseOver = false;
            this.brushPreviewElementCanvasOffset = new DependencyFunc<PointDouble, SizeDouble, PointDouble>(new Func<PointDouble, SizeDouble, PointDouble>(BrushToolUIBase<TDerived, TTool, TChanges>.GetHandleCanvasOffset));
            this.brushPreviewElementCanvasOffset.SetArgInput<PointDouble>(1, this.mouseCenterPt);
            this.brushPreviewElementCanvasOffset.SetArgInput(2, this.brushPreviewElement, new PaintDotNet.ObjectModel.PropertyPath(PaintDotNet.UI.FrameworkElement.ActualSizeProperty));
            this.brushPreviewElement.SetBinding(PaintDotNet.UI.Controls.Canvas.LeftProperty, this.brushPreviewElementCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.X", Array.Empty<object>()), BindingMode.OneWay);
            this.brushPreviewElement.SetBinding(PaintDotNet.UI.Controls.Canvas.TopProperty, this.brushPreviewElementCanvasOffset, new PaintDotNet.ObjectModel.PropertyPath("Value.Y", Array.Empty<object>()), BindingMode.OneWay);
            this.brushPreviewElement.SetBinding(DrawingElement.ScaleProperty, this, PropertyPathUtil.Combine(ToolUICanvas.CanvasViewProperty, CanvasView.CanvasHairWidthProperty), BindingMode.OneWay);
            base.Children.Add(this.brushPreviewElement);
            base.Loaded += new EventHandler(this.OnLoaded);
            base.Unloaded += new EventHandler(this.OnUnloaded);
        }

        private void AnimateBrushPreviewElementOpacity(double newOpacity, AnimationSeconds duration)
        {
            if (this.brushPreviewElementOpacity != null)
            {
                this.brushPreviewElementOpacity.AnimateValueTo(newOpacity, duration, AnimationTransitionType.SmoothStop);
            }
        }

        private static double GetBrushPreviewElementOpacity(TransactedToolState toolState)
        {
            switch (toolState)
            {
                case TransactedToolState.Drawing:
                    return 0.5;
            }
            return 1.0;
        }

        private static PointDouble GetHandleCanvasOffset(PointDouble point, SizeDouble actualSize) => 
            new PointDouble(point.X - (actualSize.Width / 2.0), point.Y - (actualSize.Height / 2.0));

        private void OnCanvasViewCanvasHairWidthChanged(object sender, ValueChangedEventArgs<double> e)
        {
            this.UpdateBrushPreview();
        }

        protected override void OnCanvasViewChanged(CanvasView oldValue, CanvasView newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanvasHairWidthChanged -= new ValueChangedEventHandler<double>(this.OnCanvasViewCanvasHairWidthChanged);
            }
            if (newValue != null)
            {
                this.UpdateBrushPreview();
                newValue.CanvasHairWidthChanged += new ValueChangedEventHandler<double>(this.OnCanvasViewCanvasHairWidthChanged);
            }
            base.OnCanvasViewChanged(oldValue, newValue);
        }

        protected override void OnIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateCursor();
            base.OnIsMouseCaptureWithinChanged(e);
        }

        protected virtual void OnLoaded(object sender, EventArgs e)
        {
            this.UpdateCursor();
            this.brushPreviewElementOpacity = new AnimatedDouble(1.0);
            this.brushPreviewElement.SetBinding(UIElement.OpacityProperty, this.brushPreviewElementOpacity, new PaintDotNet.ObjectModel.PropertyPath(AnimatedValue<double>.ValuePropertyName, Array.Empty<object>()), BindingMode.OneWay);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.brushPreviewElement.Visibility = Visibility.Visible;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.brushPreviewElement.Visibility = Visibility.Hidden;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.UpdateBrushPreview();
            base.OnMouseMove(e);
        }

        protected virtual Cursor OnQueryMouseDownCursor() => 
            Cursors.None;

        protected virtual Cursor OnQueryMouseUpCursor() => 
            this.canvasMouseUpCursor;

        protected override void OnToolChanged(Tool oldValue, Tool newValue)
        {
            if (oldValue != null)
            {
                oldValue.ToolSettings.Pen.Width.ValueChangedT -= new ValueChangedEventHandler<float>(this.OnToolSettingsPenWidthValueChanged);
            }
            if (newValue != null)
            {
                this.UpdateBrushPreview();
                newValue.ToolSettings.Pen.Width.ValueChangedT += new ValueChangedEventHandler<float>(this.OnToolSettingsPenWidthValueChanged);
            }
            base.OnToolChanged(oldValue, newValue);
        }

        private void OnToolSettingsPenWidthValueChanged(object sender, ValueChangedEventArgs<float> e)
        {
            this.UpdateBrushPreview();
        }

        private void OnToolStateChanged(object sender, ValueChangedEventArgs<TransactedToolState> e)
        {
            double num;
            double num2;
            if (((TransactedToolState) e.NewValue) != TransactedToolState.Drawing)
            {
                num = 1.0;
                num2 = 0.35;
            }
            else
            {
                num = 0.5;
                num2 = 0.15;
            }
            this.AnimateBrushPreviewElementOpacity(num, num2);
        }

        protected virtual void OnUnloaded(object sender, EventArgs e)
        {
            this.brushPreviewElement.ClearBinding(UIElement.OpacityProperty);
            this.brushPreviewElement.ClearValue(UIElement.OpacityProperty);
            DisposableUtil.Free<AnimatedDouble>(ref this.brushPreviewElementOpacity);
        }

        private void UpdateBrushPreview()
        {
            double num;
            double canvasHairWidth;
            PointDouble position;
            base.VerifyAccess();
            if (base.Tool == null)
            {
                num = 0.0;
            }
            else
            {
                num = (double) base.Tool.ToolSettings.Pen.Width.Value;
            }
            if (base.CanvasView == null)
            {
                canvasHairWidth = 1.0;
            }
            else
            {
                canvasHairWidth = base.CanvasView.CanvasHairWidth;
            }
            MouseDevice mouseDevice = base.GetMouseDevice();
            if ((mouseDevice == null) || !mouseDevice.CurrentTargetPosition.HasValue)
            {
                position = new PointDouble(-131072.0, -131072.0);
            }
            else
            {
                position = mouseDevice.GetPosition(this);
            }
            double num4 = (num / 2.0) / canvasHairWidth;
            this.brushPreviewDrawing.Radius = num4;
            RectDouble num5 = new RectDouble(position, new SizeDouble(canvasHairWidth, canvasHairWidth));
            this.mouseCenterPt.Value = num5.Center;
        }

        protected void UpdateCursor()
        {
            Cursor cursor;
            if (base.IsMouseCaptureWithin)
            {
                cursor = this.OnQueryMouseDownCursor();
            }
            else
            {
                cursor = this.OnQueryMouseUpCursor();
            }
            base.Cursor = cursor;
        }

        protected Cursor MouseUpCursor =>
            this.canvasMouseUpCursor;
    }
}

