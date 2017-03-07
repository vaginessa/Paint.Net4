namespace PaintDotNet.Effects
{
    using PaintDotNet;
    using System;

    internal sealed class RenderedTileEventArgs : EventArgs
    {
        private PdnRegion renderedRegion;
        private int tileCount;
        private int tileNumber;

        public RenderedTileEventArgs(PdnRegion renderedRegion, int tileCount, int tileNumber)
        {
            this.renderedRegion = renderedRegion;
            this.tileCount = tileCount;
            this.tileNumber = tileNumber;
        }

        public PdnRegion RenderedRegion =>
            this.renderedRegion;

        public int TileCount =>
            this.tileCount;

        public int TileNumber =>
            this.tileNumber;
    }
}

