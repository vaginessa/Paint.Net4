namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class DeleteLayerHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int index;

        public DeleteLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, Layer deleteMe) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.index = historyWorkspace.Document.Layers.IndexOf(deleteMe);
            base.Data = new DeleteLayerHistoryMementoData(deleteMe);
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            DeleteLayerHistoryMementoData data = (DeleteLayerHistoryMementoData) base.Data;
            HistoryMemento memento = new NewLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.index);
            this.historyWorkspace.Document.Layers.Insert(this.index, data.Layer);
            ((Layer) this.historyWorkspace.Document.Layers[this.index]).Invalidate();
            return memento;
        }

        [Serializable]
        private sealed class DeleteLayerHistoryMementoData : HistoryMementoData
        {
            private PaintDotNet.Layer layer;

            public DeleteLayerHistoryMementoData(PaintDotNet.Layer layer)
            {
                this.layer = layer;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.layer != null))
                {
                    this.layer.Dispose();
                    this.layer = null;
                }
            }

            public PaintDotNet.Layer Layer =>
                this.layer;
        }
    }
}

