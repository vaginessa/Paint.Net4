namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;

    internal sealed class GradientColorRenderer : GradientRenderer<ColorBgra>
    {
        public GradientColorRenderer(int width, int height, GradientShape gradientShape, GradientRepeater gradientRepeater, GradientBlender<ColorBgra> gradientBlender, bool isAntialiased) : base(width, height, gradientShape, gradientRepeater, gradientBlender, isAntialiased)
        {
        }

        protected override unsafe void RenderAntiAliased(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int x = renderOffset.X;
            int num4 = renderOffset.X + width;
            for (int i = 0; i < height; i++)
            {
                int num6 = i + renderOffset.Y;
                ColorBgra* rowAddressUnchecked = dst.GetRowAddressUnchecked(i);
                uint pixelId = (uint) (((num6 * base.Width) + x) + 1L);
                for (int j = x; j < num4; j++)
                {
                    double lerp = base.SuperSamplePixel((double) j, (double) num6, 0.5, 0);
                    ColorBgra gradientValue = base.GradientBlender.GetGradientValue(lerp, pixelId);
                    rowAddressUnchecked->Bgra = gradientValue.Bgra;
                    rowAddressUnchecked++;
                    pixelId++;
                }
            }
        }

        protected override unsafe void RenderNormal(ISurface<ColorBgra> dst, PointInt32 renderOffset)
        {
            int width = dst.Width;
            int height = dst.Height;
            int x = renderOffset.X;
            int num4 = renderOffset.X + width;
            for (int i = 0; i < height; i++)
            {
                int num6 = i + renderOffset.Y;
                ColorBgra* rowAddressUnchecked = dst.GetRowAddressUnchecked(i);
                uint pixelId = (uint) (((num6 * base.Width) + x) + 1L);
                for (int j = x; j < num4; j++)
                {
                    double t = base.GradientShape.ComputeLerp((double) j, (double) num6);
                    double lerp = base.GradientRepeater.BoundLerp(t);
                    ColorBgra gradientValue = base.GradientBlender.GetGradientValue(lerp, pixelId);
                    rowAddressUnchecked->Bgra = gradientValue.Bgra;
                    rowAddressUnchecked++;
                    pixelId++;
                }
            }
        }
    }
}

