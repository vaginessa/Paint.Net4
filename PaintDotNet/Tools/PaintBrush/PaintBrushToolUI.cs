namespace PaintDotNet.Tools.PaintBrush
{
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class PaintBrushToolUI : BrushToolUIBase<PaintBrushToolUI, PaintBrushTool, PaintBrushToolChanges>
    {
        public PaintBrushToolUI() : base("Cursors.PaintBrushToolCursor.cur")
        {
        }
    }
}

