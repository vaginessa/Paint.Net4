namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Tools;
    using System;

    internal sealed class TransactedToolUndoDrawHistoryMemento<TTool, TChanges> : TransactedToolHistoryMementoBase<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private HistoryMemento beforeDrawingUndoMemento;

        public TransactedToolUndoDrawHistoryMemento(DocumentWorkspace docWorkspace, string name, ImageResource image, HistoryMemento beforeDrawingUndoMemento) : base(docWorkspace, name, image)
        {
            Validate.IsNotNull<HistoryMemento>(beforeDrawingUndoMemento, "beforeDrawingUndoMemento");
            this.beforeDrawingUndoMemento = beforeDrawingUndoMemento;
        }

        protected override HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback)
        {
            if (tool.State == TransactedToolState.Editing)
            {
                throw new InvalidOperationException("Cannot undo while the tool is in the Editing state");
            }
            if (tool.State != TransactedToolState.Dirty)
            {
                throw new InvalidOperationException($"Tool's state is not Dirty (Tool={tool.GetType().Name}, State={tool.State})");
            }
            TChanges changes = tool.Changes;
            tool.CancelChanges();
            tool.VerifyState(TransactedToolState.Idle);
            return new TransactedToolRedoDrawHistoryMemento<TTool, TChanges>(base.DocumentWorkspace, base.Name, base.Image, changes, this.beforeDrawingUndoMemento.PerformUndo(progressCallback));
        }
    }
}

