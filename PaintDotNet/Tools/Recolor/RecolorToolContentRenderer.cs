namespace PaintDotNet.Tools.Recolor
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Tools.BrushBase;
    using PaintDotNet.Tools.FloodFill;
    using System;
    using System.Linq;

    internal sealed class RecolorToolContentRenderer : BrushToolContentRendererBase<RecolorTool, RecolorToolChanges, RecolorToolUI>
    {
        private ColorBgra basisColor;
        private IRenderer<ColorAlpha8> maskRenderer;
        private IRenderer<ColorBgra> sampleSource;

        public RecolorToolContentRenderer(BitmapLayer activeLayer, RecolorToolChanges changes) : base(activeLayer.Width, activeLayer.Height, changes)
        {
            IRenderer<ColorAlpha8> renderer2;
            this.sampleSource = activeLayer.Surface;
            byte x = (byte) Math.Round((double) (changes.Tolerance * 255.0), MidpointRounding.AwayFromZero);
            byte tolerance = ByteUtil.FastScale(x, x);
            if (changes.SamplingMode == RecolorToolSamplingMode.SecondaryColor)
            {
                this.basisColor = changes.BasisColor;
            }
            else
            {
                this.basisColor = GetBasisColor(changes, this.sampleSource);
            }
            IRenderer<ColorAlpha8> stencilSource = new FillStencilByColorRenderer(this.sampleSource, this.basisColor, tolerance, () => base.IsCancellationRequested);
            if (changes.Antialiasing)
            {
                renderer2 = new FeatheredMaskRenderer(this.sampleSource, changes.BasisColor, stencilSource, tolerance, () => base.IsCancellationRequested);
            }
            else
            {
                renderer2 = stencilSource;
            }
            IRenderer<ColorAlpha8> first = changes.RenderCache.CreateMaskRenderer(activeLayer.Size());
            this.maskRenderer = new MultiplyRendererAlpha8(first, renderer2);
        }

        private static ColorBgra GetBasisColor(RecolorToolChanges changes, IRenderer<ColorBgra> sampleSource)
        {
            PointInt32 pt = PointDouble.Truncate(changes.InputPoints.First<BrushInputPoint>().Location);
            if (sampleSource.Bounds<ColorBgra>().Contains(pt))
            {
                return sampleSource.GetPointSlow(pt);
            }
            return ColorBgra.TransparentBlack;
        }

        protected override unsafe void OnRenderContent(ISurface<ColorBgra> dstContent, PointInt32 renderOffset)
        {
            SizeInt32 num = dstContent.Size<ColorBgra>();
            ColorBgra fillColor = base.Changes.FillColor;
            this.sampleSource.Render(dstContent, renderOffset);
            for (int i = 0; i < num.Height; i++)
            {
                ColorBgra* rowPointer = (ColorBgra*) dstContent.GetRowPointer<ColorBgra>(i);
                for (int j = 0; j < num.Width; j++)
                {
                    ColorBgra bgra2 = rowPointer[j];
                    byte b = Int32Util.ClampToByte(bgra2.B + (fillColor.B - this.basisColor.B));
                    byte g = Int32Util.ClampToByte(bgra2.G + (fillColor.G - this.basisColor.G));
                    byte r = Int32Util.ClampToByte(bgra2.R + (fillColor.R - this.basisColor.R));
                    byte a = Int32Util.ClampToByte(bgra2.A + (fillColor.A - this.basisColor.A));
                    rowPointer[j] = ColorBgra.FromBgra(b, g, r, a);
                }
            }
        }

        protected override void OnRenderMask(ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            this.maskRenderer.Render(dstMask, renderOffset);
        }
    }
}

