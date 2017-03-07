namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class DeleteLayerFunction : HistoryFunction
    {
        private int layerIndex;

        public DeleteLayerFunction(int layerIndex) : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if ((this.layerIndex < 0) || (this.layerIndex >= historyWorkspace.Document.Layers.Count))
            {
                object[] objArray1 = new object[] { "layerIndex = ", this.layerIndex, ", expected [0, ", historyWorkspace.Document.Layers.Count, ")" };
                throw new ArgumentOutOfRangeException(string.Concat(objArray1));
            }
            HistoryMemento memento = new DeleteLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, historyWorkspace.Document.Layers.GetAt(this.layerIndex));
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers.RemoveAt(this.layerIndex);
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersDeleteLayerIcon.png");

        public static string StaticName =>
            PdnResources.GetString("DeleteLayer.HistoryMementoName");
    }
}

