namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Tools;
    using System;

    internal sealed class TransactedToolEditHistoryMemento<TTool, TChanges> : TransactedToolHistoryMementoBase<TTool, TChanges> where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        private TChanges previousChanges;

        public TransactedToolEditHistoryMemento(DocumentWorkspace docWorkspace, string name, ImageResource image, TChanges previousChanges) : base(docWorkspace, name, image)
        {
            this.previousChanges = previousChanges;
        }

        protected override HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback)
        {
            if (tool.State != TransactedToolState.Dirty)
            {
                throw new InvalidOperationException($"Tool's state is not Dirty (Tool={tool.GetType().Name}, State={tool.State})");
            }
            TChanges changes = tool.Changes;
            TransactedToolEditHistoryMemento<TTool, TChanges> memento = new TransactedToolEditHistoryMemento<TTool, TChanges>(base.DocumentWorkspace, base.Name, base.Image, changes);
            tool.RestoreChanges(this.previousChanges);
            tool.VerifyState(TransactedToolState.Dirty);
            return memento;
        }
    }
}

