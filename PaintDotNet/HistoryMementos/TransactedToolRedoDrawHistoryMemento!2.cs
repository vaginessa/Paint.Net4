namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Tools;
    using System;

    internal sealed class TransactedToolRedoDrawHistoryMemento<TTool, TChanges> : TransactedToolHistoryMementoBase<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private HistoryMemento beforeDrawingRedoMemento;
        private TChanges changes;

        public TransactedToolRedoDrawHistoryMemento(DocumentWorkspace docWorkspace, string name, ImageResource image, TChanges changes, HistoryMemento beforeDrawingRedoMemento) : base(docWorkspace, name, image)
        {
            Validate.IsNotNull<HistoryMemento>(beforeDrawingRedoMemento, "beforeDrawingRedoMemento");
            this.changes = changes;
            this.beforeDrawingRedoMemento = beforeDrawingRedoMemento;
        }

        protected override HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback)
        {
            if (tool.State != TransactedToolState.Idle)
            {
                throw new InvalidOperationException($"Tool's state is not Idle (Tool={tool.GetType().Name}, State={tool.State})");
            }
            HistoryMemento beforeDrawingUndoMemento = this.beforeDrawingRedoMemento.PerformUndo(progressCallback);
            TransactedToolUndoDrawHistoryMemento<TTool, TChanges> memento2 = new TransactedToolUndoDrawHistoryMemento<TTool, TChanges>(base.DocumentWorkspace, base.Name, base.Image, beforeDrawingUndoMemento);
            tool.RestoreChanges(this.changes);
            tool.VerifyState(TransactedToolState.Dirty);
            return memento2;
        }
    }
}

