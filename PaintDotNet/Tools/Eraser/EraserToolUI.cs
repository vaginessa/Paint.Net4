namespace PaintDotNet.Tools.Eraser
{
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class EraserToolUI : BrushToolUIBase<EraserToolUI, EraserTool, EraserToolChanges>
    {
        public EraserToolUI() : base("Cursors.EraserToolCursor.cur")
        {
        }
    }
}

