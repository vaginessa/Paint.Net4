namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal abstract class DocumentWorkspaceAction
    {
        private PaintDotNet.ActionFlags actionFlags;

        public DocumentWorkspaceAction(PaintDotNet.ActionFlags actionFlags)
        {
            this.actionFlags = actionFlags;
        }

        public abstract HistoryMemento PerformAction(DocumentWorkspace documentWorkspace);

        public PaintDotNet.ActionFlags ActionFlags =>
            this.actionFlags;
    }
}

