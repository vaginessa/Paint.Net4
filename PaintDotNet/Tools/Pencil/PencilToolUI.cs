namespace PaintDotNet.Tools.Pencil
{
    using PaintDotNet.Tools.Controls;
    using PaintDotNet.UI.Controls;
    using PaintDotNet.UI.Input;
    using System;

    internal sealed class PencilToolUI : ToolUICanvas<PencilTool, PencilToolChanges>
    {
        public PencilToolUI()
        {
            ClickDragBehavior.SetIsEnabled(this, true);
            ClickDragBehavior.SetAllowClick(this, false);
            base.Cursor = CursorUtil.LoadResource("Cursors.PencilToolCursor.cur");
        }
    }
}

