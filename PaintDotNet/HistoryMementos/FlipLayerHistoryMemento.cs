namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;
    using System.Threading.Tasks;

    internal sealed class FlipLayerHistoryMemento : HistoryMemento
    {
        private FlipType flipType;
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;

        public FlipLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, FlipType flipType) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.flipType = flipType;
        }

        private static unsafe void FlipInPlace(Surface surface, FlipType flipType)
        {
            int rightX;
            int bottomY;
            int stride;
            if (flipType != FlipType.Horizontal)
            {
                if (flipType == FlipType.Vertical)
                {
                    stride = surface.Stride;
                    bottomY = surface.Height - 1;
                    Parallel.For(0, surface.Width, delegate (int x) {
                        ColorBgra* pointAddress = surface.GetPointAddress(x, 0);
                        for (ColorBgra* bgraPtr2 = surface.GetPointAddress(x, bottomY); pointAddress < bgraPtr2; bgraPtr2 -= stride)
                        {
                            ColorBgra bgra = pointAddress[0];
                            pointAddress[0] = bgraPtr2[0];
                            bgraPtr2[0] = bgra;
                            pointAddress += stride;
                        }
                    });
                }
                else
                {
                    ExceptionUtil.ThrowInvalidEnumArgumentException<FlipType>(flipType, "flipType");
                }
            }
            else
            {
                rightX = surface.Width - 1;
                Parallel.For(0, surface.Height, delegate (int y) {
                    ColorBgra* pointAddress = surface.GetPointAddress(0, y);
                    for (ColorBgra* bgraPtr2 = surface.GetPointAddress(rightX, y); pointAddress < bgraPtr2; bgraPtr2--)
                    {
                        ColorBgra bgra = pointAddress[0];
                        pointAddress[0] = bgraPtr2[0];
                        bgraPtr2[0] = bgra;
                        pointAddress++;
                    }
                });
            }
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            FlipLayerHistoryMemento memento = new FlipLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex, this.flipType);
            BitmapLayer layer = (BitmapLayer) this.historyWorkspace.Document.Layers[this.layerIndex];
            FlipInPlace(layer.Surface, this.flipType);
            layer.Invalidate();
            return memento;
        }
    }
}

