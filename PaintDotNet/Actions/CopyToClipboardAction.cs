namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class CopyToClipboardAction : CopyToClipboardActionBase
    {
        public CopyToClipboardAction(DocumentWorkspace documentWorkspace) : base(documentWorkspace)
        {
        }

        protected override IRenderer<ColorBgra> GetSource()
        {
            int activeLayerIndex = base.DocumentWorkspace.ActiveLayerIndex;
            return base.DocumentWorkspace.DocumentCanvas.DocumentCanvasLayer.DocumentRenderer.CreateLayerRenderer(activeLayerIndex);
        }

        protected override bool QueryCanPerformAction() => 
            base.QueryCanPerformAction();
    }
}

