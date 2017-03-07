namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Runtime;
    using System;

    internal sealed class HistoryRedoAction : DocumentWorkspaceAction
    {
        public HistoryRedoAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (documentWorkspace.History.RedoStack.Count > 0)
            {
                if (!(documentWorkspace.History.RedoStack[documentWorkspace.History.RedoStack.Count - 1] is NullHistoryMemento))
                {
                    using (new WaitCursorChanger(documentWorkspace.FindForm()))
                    {
                        documentWorkspace.History.StepForward(documentWorkspace);
                        documentWorkspace.QueueUpdate();
                    }
                }
                CleanupManager.RequestCleanup();
            }
            return null;
        }
    }
}

