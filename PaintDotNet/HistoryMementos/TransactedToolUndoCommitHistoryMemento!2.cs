namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Tools;
    using System;

    internal sealed class TransactedToolUndoCommitHistoryMemento<TTool, TChanges> : TransactedToolHistoryMementoBase<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private TChanges changes;
        private HistoryMemento innerCommitHM;

        public TransactedToolUndoCommitHistoryMemento(DocumentWorkspace docWorkspace, TChanges changes, HistoryMemento innerCommitHM) : base(docWorkspace, innerCommitHM.Name, innerCommitHM.Image)
        {
            Validate.Begin().IsNotNull<TChanges>(changes, "changes").IsNotNull<HistoryMemento>(innerCommitHM, "innerCommitHM").Check();
            if (innerCommitHM is TransactedToolHistoryMementoBase<TTool, TChanges>)
            {
                ExceptionUtil.ThrowArgumentException("inner HistoryMemento cannot derive from TransactedToolHistoryMementoBase", "innerCommitHM");
            }
            this.changes = changes;
            this.innerCommitHM = innerCommitHM;
        }

        protected override HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback)
        {
            tool.VerifyState(TransactedToolState.Idle);
            HistoryMemento memento = this.innerCommitHM.PerformUndo(progressCallback);
            tool.RestoreChanges(this.changes);
            return new TransactedToolRedoCommitHistoryMemento<TTool, TChanges>(base.DocumentWorkspace, base.Name, base.Image);
        }

        public HistoryMemento InnerMemento =>
            this.innerCommitHM;
    }
}

