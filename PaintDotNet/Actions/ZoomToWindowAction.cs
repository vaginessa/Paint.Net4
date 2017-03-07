namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class ZoomToWindowAction : DocumentWorkspaceAction
    {
        public ZoomToWindowAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (documentWorkspace.ZoomBasis == ZoomBasis.FitToWindow)
            {
                documentWorkspace.ZoomBasis = ZoomBasis.ScaleFactor;
            }
            else
            {
                documentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
            }
            return null;
        }
    }
}

