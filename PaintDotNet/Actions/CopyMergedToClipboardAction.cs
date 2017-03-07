namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class CopyMergedToClipboardAction : CopyToClipboardActionBase
    {
        public CopyMergedToClipboardAction(DocumentWorkspace documentWorkspace) : base(documentWorkspace)
        {
        }

        protected override IRenderer<ColorBgra> GetSource() => 
            base.DocumentWorkspace.DocumentCanvas.DocumentCanvasLayer.DocumentRenderer.CreateRenderer();

        protected override bool QueryCanPerformAction() => 
            (base.QueryCanPerformAction() && (base.DocumentWorkspace.Document > null));
    }
}

