namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Tools;
    using System;

    internal sealed class TransactedToolRedoCommitHistoryMemento<TTool, TChanges> : TransactedToolHistoryMementoBase<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        public TransactedToolRedoCommitHistoryMemento(DocumentWorkspace docWorkspace, string name, ImageResource image) : base(docWorkspace, name, image)
        {
        }

        protected override HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback)
        {
            tool.VerifyState(TransactedToolState.Dirty);
            TChanges changes = tool.Changes;
            return new TransactedToolUndoCommitHistoryMemento<TTool, TChanges>(base.DocumentWorkspace, changes, tool.CommitChangesInner(changes));
        }
    }
}

