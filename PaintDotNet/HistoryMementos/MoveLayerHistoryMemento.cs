namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class MoveLayerHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int newIndex;
        private int oldIndex;

        public MoveLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int oldIndex, int newIndex) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            if (((this.oldIndex < 0) || (this.newIndex < 0)) || ((this.oldIndex >= this.historyWorkspace.Document.Layers.Count) || (this.newIndex >= this.historyWorkspace.Document.Layers.Count)))
            {
                throw new ArgumentOutOfRangeException("[old|new]Index", "out of range");
            }
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            MoveLayerHistoryMemento memento = new MoveLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.newIndex, this.oldIndex);
            this.historyWorkspace.Document.Layers.Move(this.newIndex, this.oldIndex);
            this.historyWorkspace.Document.Invalidate();
            return memento;
        }
    }
}

