namespace PaintDotNet.Tools.Gradient
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class GradientToolContentRenderer : CancellableMaskedRendererBgraBase
    {
        private IRenderer<ColorBgra> renderer;

        public GradientToolContentRenderer(IRenderer<ColorBgra> layerSource, GradientToolChanges changes, uint ditherSeed) : base(layerSource.Width, layerSource.Height, false)
        {
            ColorBgra startColor = changes.ReverseColors ? changes.SecondaryColor : changes.PrimaryColor;
            ColorBgra endColor = changes.ReverseColors ? changes.PrimaryColor : changes.SecondaryColor;
            GradientShape gradientShape = CreateGradientShape(changes.GradientType, changes.RepeatType, changes.GradientStartPoint, changes.GradientEndPoint);
            GradientRepeater gradientRepeater = CreateGradientRepeater(changes.RepeatType);
            bool antialiasing = changes.Antialiasing;
            bool isAntiAliased = changes.Antialiasing && this.IsAntialiasingRequiredForShape(changes.GradientType, changes.RepeatType, changes.GradientStartPoint, changes.GradientEndPoint);
            if (changes.IsAlphaOnly)
            {
                IRenderer<ColorAlpha8> renderer2;
                byte startAlpha = changes.ReverseColors ? ((byte) (0xff - startColor.A)) : startColor.A;
                byte endAlpha = changes.ReverseColors ? endColor.A : ((byte) (0xff - endColor.A));
                IRenderer<ColorAlpha8> second = CreateAlphaRenderer(layerSource.Width, layerSource.Height, gradientShape, gradientRepeater, startAlpha, endAlpha, isAntiAliased, antialiasing, ditherSeed);
                if (changes.BlendMode.IsCompositionOp())
                {
                    IRenderer<ColorAlpha8> first = new ExtractAlpha8FromBgra32Renderer(layerSource);
                    renderer2 = new MultiplyRendererAlpha8(first, second);
                }
                else
                {
                    renderer2 = second;
                }
                this.renderer = new ReplaceAlphaChannelRendererBgra32(layerSource, renderer2);
            }
            else
            {
                this.renderer = CreateColorRenderer(layerSource.Width, layerSource.Height, gradientShape, gradientRepeater, startColor, endColor, isAntiAliased, antialiasing, ditherSeed);
            }
        }

        private static IRenderer<ColorAlpha8> CreateAlphaRenderer(int width, int height, GradientShape gradientShape, GradientRepeater gradientRepeater, int startAlpha, int endAlpha, bool isAntiAliased, bool isDithered, uint ditherSeed)
        {
            if ((gradientShape == null) || (startAlpha == endAlpha))
            {
                return new FillRendererAlpha8(width, height, new ColorAlpha8((byte) endAlpha));
            }
            return new GradientAlphaRenderer(width, height, gradientShape, gradientRepeater, new GradientBlenders.Alpha(startAlpha, endAlpha, ditherSeed, isDithered), isAntiAliased);
        }

        private static IRenderer<ColorBgra> CreateColorRenderer(int width, int height, GradientShape gradientShape, GradientRepeater gradientRepeater, ColorBgra startColor, ColorBgra endColor, bool isAntiAliased, bool isDithered, uint ditherSeed)
        {
            GradientBlender<ColorBgra> blender;
            if ((gradientShape == null) || (startColor == endColor))
            {
                return new SolidColorRendererBgra(width, height, endColor);
            }
            if (isDithered)
            {
                if ((startColor.A == 0xff) && (endColor.A == 0xff))
                {
                    blender = new GradientBlenders.RgbSolidColor(startColor, endColor, ditherSeed);
                }
                else
                {
                    blender = new GradientBlenders.RgbColor(startColor, endColor, ditherSeed);
                }
            }
            else
            {
                blender = new GradientBlenders.FastColor(startColor, endColor);
            }
            return new GradientColorRenderer(width, height, gradientShape, gradientRepeater, blender, isAntiAliased);
        }

        private static GradientRepeater CreateGradientRepeater(GradientRepeatType repeatType)
        {
            switch (repeatType)
            {
                case GradientRepeatType.NoRepeat:
                    return new GradientRepeaters.NoRepeat();

                case GradientRepeatType.RepeatWrapped:
                    return new GradientRepeaters.RepeatWrapped();

                case GradientRepeatType.RepeatReflected:
                    return new GradientRepeaters.RepeatReflected();
            }
            throw ExceptionUtil.InvalidEnumArgumentException<GradientRepeatType>(repeatType, "repeatType");
        }

        private static GradientShape CreateGradientShape(GradientType gradientType, GradientRepeatType repeatType, PointDouble startPoint, PointDouble endPoint)
        {
            if (startPoint == endPoint)
            {
                return null;
            }
            switch (gradientType)
            {
                case GradientType.LinearClamped:
                    return new GradientShapes.LinearStraight(startPoint, endPoint);

                case GradientType.LinearReflected:
                    return new GradientShapes.LinearReflected(startPoint, endPoint);

                case GradientType.LinearDiamond:
                    return new GradientShapes.LinearDiamond(startPoint, endPoint);

                case GradientType.Radial:
                    return new GradientShapes.Radial(startPoint, endPoint);

                case GradientType.Conical:
                    if (repeatType != GradientRepeatType.NoRepeat)
                    {
                        return new GradientShapes.Conical(startPoint, endPoint);
                    }
                    return new GradientShapes.ConicalNoRepeat(startPoint, endPoint);

                case GradientType.Spiral:
                    if (repeatType != GradientRepeatType.RepeatReflected)
                    {
                        return new GradientShapes.Spiral(startPoint, endPoint);
                    }
                    return new GradientShapes.SpiralReflected(startPoint, endPoint);

                case GradientType.SpiralCounterClockwise:
                    if (repeatType != GradientRepeatType.RepeatReflected)
                    {
                        return new GradientShapes.SpiralCCW(startPoint, endPoint);
                    }
                    return new GradientShapes.SpiralReflectedCCW(startPoint, endPoint);
            }
            throw ExceptionUtil.InvalidEnumArgumentException<GradientType>(gradientType, "gradientType");
        }

        private bool IsAntialiasingRequiredForShape(GradientType gradientType, GradientRepeatType gradientRepeatType, PointDouble startPoint, PointDouble endPoint)
        {
            if (gradientRepeatType == GradientRepeatType.RepeatReflected)
            {
                VectorDouble num2 = (VectorDouble) (endPoint - startPoint);
                return (num2.Length < 8.0);
            }
            switch (gradientType)
            {
                case GradientType.LinearClamped:
                case GradientType.LinearReflected:
                case GradientType.LinearDiamond:
                case GradientType.Radial:
                    return (gradientRepeatType == GradientRepeatType.RepeatWrapped);

                case GradientType.Conical:
                case GradientType.Spiral:
                case GradientType.SpiralCounterClockwise:
                    return true;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<GradientType>(gradientType, "gradientType");
        }

        protected override void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            this.renderer.Render(dstContent, renderOffset);
        }
    }
}

