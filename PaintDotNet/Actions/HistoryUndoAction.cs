namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Runtime;
    using System;

    internal sealed class HistoryUndoAction : DocumentWorkspaceAction
    {
        public HistoryUndoAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (documentWorkspace.History.UndoStack.Count > 0)
            {
                if (!(documentWorkspace.History.UndoStack[documentWorkspace.History.UndoStack.Count - 1] is NullHistoryMemento))
                {
                    using (new WaitCursorChanger(documentWorkspace.FindForm()))
                    {
                        documentWorkspace.History.StepBackward(documentWorkspace);
                        documentWorkspace.QueueUpdate();
                    }
                }
                CleanupManager.RequestCleanup();
            }
            return null;
        }
    }
}

