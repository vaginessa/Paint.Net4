namespace PaintDotNet.Tools
{
    using PaintDotNet.Controls;
    using System;

    internal sealed class LassoSelectTool : SelectionTool
    {
        public LassoSelectTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.LassoSelectToolIcon.png"), PdnResources.GetString("LassoSelectTool.Name"), PdnResources.GetString("LassoSelectTool.HelpText"), 's', ToolBarConfigItems.None)
        {
        }

        protected override void OnActivate()
        {
            base.SetCursors("Cursors.LassoSelectToolCursor.cur", "Cursors.LassoSelectToolCursorMinus.cur", "Cursors.LassoSelectToolCursorPlus.cur", "Cursors.LassoSelectToolCursorMouseDown.cur");
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
    }
}

