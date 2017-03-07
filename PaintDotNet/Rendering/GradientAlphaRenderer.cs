namespace PaintDotNet.Rendering
{
    using PaintDotNet.Imaging;
    using System;

    internal sealed class GradientAlphaRenderer : GradientRenderer<ColorAlpha8>
    {
        public GradientAlphaRenderer(int width, int height, GradientShape gradientShape, GradientRepeater gradientRepeater, GradientBlender<ColorAlpha8> gradientBlender, bool isAntialiased) : base(width, height, gradientShape, gradientRepeater, gradientBlender, isAntialiased)
        {
        }

        protected override unsafe void RenderAntiAliased(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int x = renderOffset.X;
            int num4 = renderOffset.X + width;
            for (int i = 0; i < height; i++)
            {
                int num6 = i + renderOffset.Y;
                ColorAlpha8* rowPointer = (ColorAlpha8*) dst.GetRowPointer<ColorAlpha8>(i);
                uint pixelId = (uint) (((num6 * base.Width) + x) + 1L);
                for (int j = x; j < num4; j++)
                {
                    double lerp = base.SuperSamplePixel((double) j, (double) num6, 0.5, 0);
                    rowPointer.A = base.GradientBlender.GetGradientValue(lerp, pixelId).A;
                    rowPointer++;
                    pixelId++;
                }
            }
        }

        protected override unsafe void RenderNormal(ISurface<ColorAlpha8> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int x = renderOffset.X;
            int num4 = renderOffset.X + width;
            for (int i = 0; i < height; i++)
            {
                int num6 = i + renderOffset.Y;
                ColorAlpha8* rowPointer = (ColorAlpha8*) dst.GetRowPointer<ColorAlpha8>(i);
                uint pixelId = (uint) (((num6 * base.Width) + x) + 1L);
                for (int j = x; j < num4; j++)
                {
                    double t = base.GradientShape.ComputeLerp((double) j, (double) num6);
                    double lerp = base.GradientRepeater.BoundLerp(t);
                    rowPointer.A = base.GradientBlender.GetGradientValue(lerp, pixelId).A;
                    rowPointer++;
                    pixelId++;
                }
            }
        }
    }
}

