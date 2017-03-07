namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;

    internal abstract class TransactedToolHistoryMementoBase<TTool, TChanges> : ToolHistoryMemento where TTool: TransactedTool<TTool, TChanges> where TChanges: TransactedToolChanges<TChanges, TTool>
    {
        public TransactedToolHistoryMementoBase(DocumentWorkspace docWorkspace, string name, ImageResource image) : base(docWorkspace, name, image)
        {
        }

        protected override HistoryMemento OnToolUndo(ProgressEventHandler progressCallback)
        {
            TTool tool = base.DocumentWorkspace.Tool as TTool;
            if (tool == null)
            {
                throw new InvalidOperationException($"Current Tool, {(base.DocumentWorkspace.Tool == null) ? "<null>" : base.DocumentWorkspace.Tool.GetType().Name}, is not {typeof(TTool).Name}");
            }
            return this.OnTransactedToolUndo(tool, progressCallback);
        }

        protected abstract HistoryMemento OnTransactedToolUndo(TTool tool, ProgressEventHandler progressCallback);
    }
}

