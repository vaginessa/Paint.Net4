namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class MoveLayerFunction : HistoryFunction
    {
        private int newIndex;
        private int oldIndex;

        public MoveLayerFunction(int oldIndex, int newIndex) : base(ActionFlags.None)
        {
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (((this.oldIndex < 0) || (this.oldIndex >= historyWorkspace.Document.Layers.Count)) || ((this.newIndex < 0) || (this.newIndex >= historyWorkspace.Document.Layers.Count)))
            {
                object[] objArray1 = new object[] { "oldIndex = ", this.oldIndex, ", newIndex = ", this.newIndex, ", expected [0,", historyWorkspace.Document.Layers.Count, ")" };
                throw new ArgumentOutOfRangeException(string.Concat(objArray1));
            }
            MoveLayerHistoryMemento memento = new MoveLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, this.oldIndex, this.newIndex);
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers.Move(this.oldIndex, this.newIndex);
            historyWorkspace.Document.Invalidate();
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MoveLayer.png");

        public static string StaticName =>
            PdnResources.GetString("MoveLayerFunction.Name");
    }
}

