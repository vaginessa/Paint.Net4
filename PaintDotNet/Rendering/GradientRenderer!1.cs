namespace PaintDotNet.Rendering
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class GradientRenderer<TPixel> : IRenderer<TPixel> where TPixel: struct, INaturalPixelInfo
    {
        private GradientBlender<TPixel> gradientBlender;
        private PaintDotNet.Rendering.GradientRepeater gradientRepeater;
        private PaintDotNet.Rendering.GradientShape gradientShape;
        private int height;
        private bool isAntialiased;
        private int width;

        public GradientRenderer(int width, int height, PaintDotNet.Rendering.GradientShape gradientShape, PaintDotNet.Rendering.GradientRepeater gradientRepeater, GradientBlender<TPixel> gradientBlender, bool isAntiAliased)
        {
            this.width = width;
            this.height = height;
            this.gradientShape = gradientShape;
            this.gradientRepeater = gradientRepeater;
            this.gradientBlender = gradientBlender;
            this.isAntialiased = isAntiAliased;
        }

        protected double GetLerp(double x, double y)
        {
            double t = this.gradientShape.ComputeLerp(x, y);
            return this.gradientRepeater.BoundLerp(t);
        }

        public void Render(ISurface<TPixel> dst, PointInt32 renderOffset)
        {
            if (this.isAntialiased)
            {
                this.RenderAntiAliased(dst, renderOffset);
            }
            else
            {
                this.RenderNormal(dst, renderOffset);
            }
        }

        protected abstract void RenderAntiAliased(ISurface<TPixel> dst, PointInt32 renderOffset);
        protected abstract void RenderNormal(ISurface<TPixel> dst, PointInt32 renderOffset);
        protected double SuperSamplePixel(double x, double y, double offset, int level = 0)
        {
            double lerp = this.GetLerp(x, y);
            if (++level > 3)
            {
                return lerp;
            }
            double num2 = this.GetLerp(x - offset, y - offset);
            double num3 = this.GetLerp(x + offset, y - offset);
            double num4 = this.GetLerp(x - offset, y + offset);
            double num5 = this.GetLerp(x + offset, y + offset);
            double num6 = 0.0;
            double num7 = offset * 0.5;
            if (Math.Abs((double) (lerp - num2)) < 0.05)
            {
                num6 += lerp;
            }
            else
            {
                num6 += this.SuperSamplePixel(x - num7, y - num7, num7, level);
            }
            if (Math.Abs((double) (lerp - num3)) < 0.05)
            {
                num6 += lerp;
            }
            else
            {
                num6 += this.SuperSamplePixel(x + num7, y - num7, num7, level);
            }
            if (Math.Abs((double) (lerp - num4)) < 0.05)
            {
                num6 += lerp;
            }
            else
            {
                num6 += this.SuperSamplePixel(x - num7, y + num7, num7, level);
            }
            if (Math.Abs((double) (lerp - num5)) < 0.05)
            {
                num6 += lerp;
            }
            else
            {
                num6 += this.SuperSamplePixel(x + num7, y + num7, num7, level);
            }
            return (num6 * 0.25);
        }

        protected GradientBlender<TPixel> GradientBlender =>
            this.gradientBlender;

        protected PaintDotNet.Rendering.GradientRepeater GradientRepeater =>
            this.gradientRepeater;

        protected PaintDotNet.Rendering.GradientShape GradientShape =>
            this.gradientShape;

        public int Height =>
            this.height;

        protected bool IsAntialiased =>
            this.isAntialiased;

        public int Width =>
            this.width;
    }
}

