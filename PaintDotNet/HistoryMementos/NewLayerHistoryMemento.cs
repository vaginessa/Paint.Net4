namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class NewLayerHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;

        public NewLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            DeleteLayerHistoryMemento memento = new DeleteLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, (Layer) this.historyWorkspace.Document.Layers[this.layerIndex]) {
                ID = base.ID
            };
            this.historyWorkspace.Document.Layers.RemoveAt(this.layerIndex);
            this.historyWorkspace.Document.Invalidate();
            return memento;
        }
    }
}

