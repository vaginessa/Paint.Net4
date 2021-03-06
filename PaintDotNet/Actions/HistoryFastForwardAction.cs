﻿namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Runtime;
    using System;

    internal sealed class HistoryFastForwardAction : DocumentWorkspaceAction
    {
        public HistoryFastForwardAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            DateTime utcNow = DateTime.UtcNow;
            documentWorkspace.History.BeginStepGroup();
            using (new WaitCursorChanger(documentWorkspace))
            {
                documentWorkspace.SuspendToolCursorChanges();
                while (documentWorkspace.History.RedoStack.Count > 0)
                {
                    documentWorkspace.History.StepForward(documentWorkspace);
                    CleanupManager.RequestCleanup();
                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - utcNow);
                    if (span.TotalMilliseconds >= 500.0)
                    {
                        documentWorkspace.History.EndStepGroup();
                        documentWorkspace.QueueUpdate();
                        utcNow = DateTime.UtcNow;
                        documentWorkspace.History.BeginStepGroup();
                    }
                }
                documentWorkspace.ResumeToolCursorChanges();
            }
            documentWorkspace.History.EndStepGroup();
            CleanupManager.RequestCleanup();
            documentWorkspace.Document.Invalidate();
            documentWorkspace.QueueUpdate();
            return null;
        }
    }
}

