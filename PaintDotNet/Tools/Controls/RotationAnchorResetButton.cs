namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Tools.Media;
    using PaintDotNet.UI;
    using PaintDotNet.UI.Controls;
    using System;

    internal sealed class RotationAnchorResetButton : HandleElement
    {
        public static readonly RoutedEvent ClickedEvent = ClickDragBehavior.ClickedEvent;

        static RotationAnchorResetButton()
        {
            HandleElement.IsHotOnMouseOverProperty.OverrideMetadata(typeof(RotationAnchorResetButton), new FrameworkPropertyMetadata(BooleanUtil.GetBoxed(false)));
        }

        public RotationAnchorResetButton()
        {
            base.Drawing = new ResetButtonDrawing();
            ClickDragBehavior.SetIsEnabled(this, true);
            ClickDragBehavior.SetAllowDrag(this, false);
            ClickDragBehavior.AddIsPressedChangedHandler(this, new RoutedEventHandler(this.OnClickDragBehaviorIsPressedChanged));
        }

        private void OnClickDragBehaviorIsPressedChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateState();
        }

        protected override void OnIsEnabledChanged(bool oldValue, bool newValue)
        {
            this.UpdateState();
            base.OnIsEnabledChanged(oldValue, newValue);
        }

        protected override void OnIsMouseOverChanged(bool oldValue, bool newValue)
        {
            this.UpdateState();
            base.OnIsMouseOverChanged(oldValue, newValue);
        }

        private void UpdateState()
        {
            PdnPushButtonState disabled;
            ResetButtonDrawing drawing = (ResetButtonDrawing) base.Drawing;
            bool isEnabled = base.IsEnabled;
            bool isPressed = ClickDragBehavior.GetIsPressed(this);
            bool isMouseOver = base.IsMouseOver;
            if (!isEnabled)
            {
                disabled = PdnPushButtonState.Disabled;
            }
            else if (isPressed)
            {
                disabled = PdnPushButtonState.Pressed;
            }
            else if (isMouseOver)
            {
                disabled = PdnPushButtonState.Hot;
            }
            else
            {
                disabled = PdnPushButtonState.Normal;
            }
            drawing.ButtonState = disabled;
        }
    }
}

