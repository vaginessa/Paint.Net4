namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class ZoomOutAction : DocumentWorkspaceAction
    {
        public ZoomOutAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            documentWorkspace.ZoomOut();
            return null;
        }
    }
}

