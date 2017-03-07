namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class SelectionInterimTransformHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private Matrix3x2Double interimTx;

        public SelectionInterimTransformHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.interimTx = historyWorkspace.Selection.GetInterimTransform();
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            SelectionInterimTransformHistoryMemento memento = new SelectionInterimTransformHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            using (this.historyWorkspace.Selection.UseChangeScope())
            {
                this.historyWorkspace.Selection.SetInterimTransform(this.interimTx);
            }
            return memento;
        }
    }
}

