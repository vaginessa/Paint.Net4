namespace PaintDotNet.Tools.Recolor
{
    using PaintDotNet.Tools.BrushBase;
    using System;

    internal sealed class RecolorToolUI : BrushToolUIBase<RecolorToolUI, RecolorTool, RecolorToolChanges>
    {
        public RecolorToolUI() : base("Cursors.RecoloringToolCursor.cur")
        {
        }
    }
}

