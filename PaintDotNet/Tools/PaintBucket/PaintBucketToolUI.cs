namespace PaintDotNet.Tools.PaintBucket
{
    using PaintDotNet.Tools.FloodFill;
    using PaintDotNet.UI.Input;
    using System;

    internal sealed class PaintBucketToolUI : FloodFillToolUIBase<PaintBucketTool, PaintBucketToolChanges>
    {
        public PaintBucketToolUI()
        {
            base.CanvasCursor = CursorUtil.LoadResource("Cursors.PaintBucketToolCursor.cur");
        }
    }
}

