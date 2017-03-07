namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class ZoomInAction : DocumentWorkspaceAction
    {
        public ZoomInAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            documentWorkspace.ZoomIn();
            return null;
        }
    }
}

