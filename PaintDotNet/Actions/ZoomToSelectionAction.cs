namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class ZoomToSelectionAction : DocumentWorkspaceAction
    {
        public ZoomToSelectionAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            documentWorkspace.ZoomToSelection();
            return null;
        }
    }
}

