namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using System;

    internal sealed class MergeLayerDownFunction : HistoryFunction
    {
        private int layerIndex;

        public MergeLayerDownFunction(int layerIndex) : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if ((this.layerIndex < 1) || (this.layerIndex >= historyWorkspace.Document.Layers.Count))
            {
                object[] objArray1 = new object[] { "layerIndex must be greater than or equal to 1, and a valid layer index. layerIndex=", this.layerIndex, ", allowableRange=[0,", historyWorkspace.Document.Layers.Count, ")" };
                throw new ArgumentException(string.Concat(objArray1));
            }
            int layerIndex = this.layerIndex - 1;
            RectInt32 rect = historyWorkspace.Document.Bounds();
            GeometryList list = new GeometryList();
            list.AddRect(rect);
            RectInt32[] changedRegion = list.EnumerateInteriorScans().ToArrayEx<RectInt32>();
            BitmapHistoryMemento memento = new BitmapHistoryMemento(null, null, historyWorkspace, layerIndex, changedRegion);
            BitmapLayer layer = (BitmapLayer) historyWorkspace.Document.Layers[this.layerIndex];
            BitmapLayer layer2 = (BitmapLayer) historyWorkspace.Document.Layers[layerIndex];
            RenderArgs args = new RenderArgs(layer2.Surface);
            base.EnterCriticalRegion();
            foreach (RectInt32 num4 in changedRegion)
            {
                layer.Render(args, num4.ToGdipRectangle());
            }
            layer2.Invalidate();
            args.Dispose();
            args = null;
            list = null;
            HistoryMemento memento2 = new DeleteLayerFunction(this.layerIndex).Execute(historyWorkspace);
            return new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { memento, memento2 });
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersMergeLayerDownIcon.png");

        public static string StaticName =>
            PdnResources.GetString("MergeLayerDown.HistoryMementoName");
    }
}

