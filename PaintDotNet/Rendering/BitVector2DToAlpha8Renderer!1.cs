namespace PaintDotNet.Rendering
{
    using System;

    internal sealed class BitVector2DToAlpha8Renderer<TBitVector2D> : IRenderer<ColorAlpha8> where TBitVector2D: IBitVector2D
    {
        private TBitVector2D stencil;

        public BitVector2DToAlpha8Renderer(TBitVector2D stencil)
        {
            this.stencil = stencil;
        }

        public unsafe void Render(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            for (int i = 0; i < height; i++)
            {
                byte* rowPointer = (byte*) dst.GetRowPointer<ColorAlpha8>(i);
                for (int j = 0; j < width; j++)
                {
                    rowPointer[0] = this.stencil.GetUnchecked(j + renderOffset.X, i + renderOffset.Y) ? ((byte) 0xff) : ((byte) 0);
                    rowPointer++;
                }
            }
        }

        public int Height =>
            this.stencil.Height;

        public int Width =>
            this.stencil.Width;
    }
}

