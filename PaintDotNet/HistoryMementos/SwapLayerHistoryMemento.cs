namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class SwapLayerHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex1;
        private int layerIndex2;

        public SwapLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex1, int layerIndex2) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex1 = layerIndex1;
            this.layerIndex2 = layerIndex2;
            if (((this.layerIndex1 < 0) || (this.layerIndex2 < 0)) || ((this.layerIndex1 >= this.historyWorkspace.Document.Layers.Count) || (this.layerIndex2 >= this.historyWorkspace.Document.Layers.Count)))
            {
                throw new ArgumentOutOfRangeException("layerIndex[1|2]", "out of range");
            }
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            SwapLayerHistoryMemento memento = new SwapLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex2, this.layerIndex1);
            using (this.historyWorkspace.Document.Layers.UseChangeScope())
            {
                Layer layer = (Layer) this.historyWorkspace.Document.Layers[this.layerIndex1];
                Layer layer2 = (Layer) this.historyWorkspace.Document.Layers[this.layerIndex2];
                int num = Math.Min(this.layerIndex1, this.layerIndex2);
                if ((Math.Max(this.layerIndex1, this.layerIndex2) - num) == 1)
                {
                    this.historyWorkspace.Document.Layers.RemoveAt(this.layerIndex1);
                    this.historyWorkspace.Document.Layers.Insert(this.layerIndex2, layer);
                }
                else
                {
                    this.historyWorkspace.Document.Layers[this.layerIndex1] = layer2;
                    this.historyWorkspace.Document.Layers[this.layerIndex2] = layer;
                }
                this.historyWorkspace.Document.Invalidate();
            }
            return memento;
        }
    }
}

