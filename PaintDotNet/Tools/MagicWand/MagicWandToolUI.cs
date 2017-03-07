namespace PaintDotNet.Tools.MagicWand
{
    using PaintDotNet.Tools.FloodFill;
    using PaintDotNet.UI.Input;
    using System;

    internal sealed class MagicWandToolUI : FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>
    {
        private Cursor crosshairCursor = CursorUtil.LoadResource("Cursors.MagicWandToolCursor.cur");
        private Cursor crosshairMinusCursor = CursorUtil.LoadResource("Cursors.MagicWandToolCursorMinus.cur");
        private Cursor crosshairPlusCursor = CursorUtil.LoadResource("Cursors.MagicWandToolCursorPlus.cur");

        protected override object OnCoerceCanvasCursorProperty(object baseValue)
        {
            if (base.IsLoaded)
            {
                KeyboardDevice keyboardDevice = base.GetKeyboardDevice();
                if ((keyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    return this.crosshairPlusCursor;
                }
                if ((keyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    return this.crosshairMinusCursor;
                }
            }
            return this.crosshairCursor;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.CoerceValue(FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.CanvasCursorProperty);
            base.OnKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.CoerceValue(FloodFillToolUIBase<MagicWandTool, MagicWandToolChanges>.CanvasCursorProperty);
            base.OnKeyUp(e);
        }
    }
}

