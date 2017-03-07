namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using System;

    internal static class GradientBlenders
    {
        public sealed class Alpha : GradientBlender<ColorAlpha8>
        {
            private double diffAlpha;
            private bool ditherA;
            private uint ditherSeed;
            private double endAlpha;
            private double startAlpha;

            public Alpha(int startAlpha, int endAlpha, uint ditherSeed, bool isDithered)
            {
                this.startAlpha = startAlpha;
                this.endAlpha = endAlpha;
                this.diffAlpha = endAlpha - startAlpha;
                this.ditherSeed = ditherSeed;
                this.ditherA = isDithered && !(this.diffAlpha == 0.0);
            }

            public override ColorAlpha8 GetGradientValue(double lerp, uint pixelId)
            {
                double num;
                if (!this.ditherA)
                {
                    num = 0.0;
                }
                else
                {
                    uint h = pixelId * 0x5bd1e995;
                    h ^= h >> 0x18;
                    h *= 0x5bd1e995;
                    h ^= this.ditherSeed;
                    h ^= h >> 13;
                    h *= 0x5bd1e995;
                    h ^= h >> 15;
                    int num3 = ((int) h) & 0xfffffff;
                    int num4 = ((int) GradientBlender<ColorAlpha8>.XorShift(h)) & 0xfffffff;
                    num = (num3 - num4) * 3.7252903123397019E-09;
                }
                return new ColorAlpha8(DoubleUtil.ClampToByte(((this.startAlpha + (this.diffAlpha * lerp)) + num) + 0.5));
            }
        }

        public sealed class FastColor : GradientBlender<ColorBgra>
        {
            private readonly ColorBgra[] lerpColors = new ColorBgra[0x100];

            public FastColor(ColorBgra startColor, ColorBgra endColor)
            {
                for (int i = 0; i < 0x100; i++)
                {
                    byte index = (byte) i;
                    this.lerpColors[index] = ColorBgra.Blend(startColor, endColor, index);
                }
            }

            public override ColorBgra GetGradientValue(double lerp, uint pixelId)
            {
                byte index = (byte) (lerp * 255.0);
                return this.lerpColors[index];
            }
        }

        public sealed class RgbColor : GradientBlender<ColorBgra>
        {
            private double diffA;
            private double diffB;
            private double diffG;
            private double diffR;
            private bool ditherA;
            private bool ditherB;
            private bool ditherG;
            private bool ditherR;
            private uint ditherSeed;
            private ColorBgra endColor;
            private double startA;
            private double startB;
            private ColorBgra startColor;
            private double startG;
            private double startR;

            public RgbColor(ColorBgra startColor, ColorBgra endColor, uint ditherSeed)
            {
                this.startColor = startColor;
                this.endColor = endColor;
                this.ditherSeed = ditherSeed;
                this.startA = startColor.A;
                this.startB = startColor.B * this.startA;
                this.startG = startColor.G * this.startA;
                this.startR = startColor.R * this.startA;
                double a = endColor.A;
                this.diffB = (endColor.B * a) - this.startB;
                this.diffG = (endColor.G * a) - this.startG;
                this.diffR = (endColor.R * a) - this.startR;
                this.diffA = endColor.A - this.startA;
                this.ditherB = !(this.diffB == 0.0);
                this.ditherG = !(this.diffG == 0.0);
                this.ditherR = !(this.diffR == 0.0);
                this.ditherA = !(this.diffA == 0.0);
            }

            public override ColorBgra GetGradientValue(double lerp, uint pixelId)
            {
                int num8;
                double num9;
                double num = this.startA + (this.diffA * lerp);
                if (num == 0.0)
                {
                    return ColorBgra.TransparentBlack;
                }
                double num2 = 1.0 / num;
                double num3 = (this.startB + (this.diffB * lerp)) * num2;
                double num4 = (this.startG + (this.diffG * lerp)) * num2;
                double num5 = (this.startR + (this.diffR * lerp)) * num2;
                uint h = pixelId * 0x5bd1e995;
                h ^= h >> 0x18;
                h *= 0x5bd1e995;
                h ^= this.ditherSeed;
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                int num7 = ((int) h) & 0xfffffff;
                if (!this.ditherB)
                {
                    num9 = 0.0;
                }
                else
                {
                    h = GradientBlender<ColorBgra>.XorShift(h);
                    num8 = ((int) h) & 0xfffffff;
                    num9 = (num7 - num8) * 3.7252903123397019E-09;
                    num7 = num8;
                }
                num3 = DoubleUtil.ClampToByte((num3 + num9) + 0.5);
                if (!this.ditherG)
                {
                    num9 = 0.0;
                }
                else
                {
                    h = GradientBlender<ColorBgra>.XorShift(h);
                    num8 = ((int) h) & 0xfffffff;
                    num9 = (num7 - num8) * 3.7252903123397019E-09;
                    num7 = num8;
                }
                num4 = DoubleUtil.ClampToByte((num4 + num9) + 0.5);
                if (!this.ditherR)
                {
                    num9 = 0.0;
                }
                else
                {
                    h = GradientBlender<ColorBgra>.XorShift(h);
                    num8 = ((int) h) & 0xfffffff;
                    num9 = (num7 - num8) * 3.7252903123397019E-09;
                    num7 = num8;
                }
                num5 = DoubleUtil.ClampToByte((num5 + num9) + 0.5);
                if (!this.ditherA)
                {
                    num9 = 0.0;
                }
                else
                {
                    num8 = ((int) GradientBlender<ColorBgra>.XorShift(h)) & 0x3fffffff;
                    num9 = (num7 - num8) * 3.7252903123397019E-09;
                    num7 = num8;
                }
                num = DoubleUtil.ClampToByte((num + num9) + 0.5);
                return ColorBgra.FromBgra((byte) num3, (byte) num4, (byte) num5, (byte) num);
            }
        }

        public sealed class RgbSolidColor : GradientBlender<ColorBgra>
        {
            private double diffB;
            private double diffG;
            private double diffR;
            private bool ditherB;
            private bool ditherG;
            private bool ditherR;
            private uint ditherSeed;
            private ColorBgra endColor;
            private double startB;
            private ColorBgra startColor;
            private double startG;
            private double startR;

            public RgbSolidColor(ColorBgra startColor, ColorBgra endColor, uint ditherSeed)
            {
                this.startColor = startColor;
                this.endColor = endColor;
                this.ditherSeed = ditherSeed;
                this.startB = startColor.B;
                this.startG = startColor.G;
                this.startR = startColor.R;
                this.diffB = endColor.B - this.startB;
                this.diffG = endColor.G - this.startG;
                this.diffR = endColor.R - this.startR;
                this.ditherB = !(this.diffB == 0.0);
                this.ditherG = !(this.diffG == 0.0);
                this.ditherR = !(this.diffR == 0.0);
            }

            public override ColorBgra GetGradientValue(double lerp, uint pixelId)
            {
                int num3;
                double num4;
                uint h = pixelId * 0x5bd1e995;
                h ^= h >> 0x18;
                h *= 0x5bd1e995;
                h ^= this.ditherSeed;
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                int num2 = ((int) h) & 0xfffffff;
                if (!this.ditherB)
                {
                    num4 = 0.0;
                }
                else
                {
                    h = GradientBlender<ColorBgra>.XorShift(h);
                    num3 = ((int) h) & 0xfffffff;
                    num4 = (num2 - num3) * 3.7252903123397019E-09;
                    num2 = num3;
                }
                double num5 = DoubleUtil.ClampToByte(((this.startB + (this.diffB * lerp)) + num4) + 0.5);
                if (!this.ditherG)
                {
                    num4 = 0.0;
                }
                else
                {
                    h = GradientBlender<ColorBgra>.XorShift(h);
                    num3 = ((int) h) & 0xfffffff;
                    num4 = (num2 - num3) * 3.7252903123397019E-09;
                    num2 = num3;
                }
                double num6 = DoubleUtil.ClampToByte(((this.startG + (this.diffG * lerp)) + num4) + 0.5);
                if (!this.ditherR)
                {
                    num4 = 0.0;
                }
                else
                {
                    num3 = ((int) GradientBlender<ColorBgra>.XorShift(h)) & 0xfffffff;
                    num4 = (num2 - num3) * 3.7252903123397019E-09;
                    num2 = num3;
                }
                double num7 = DoubleUtil.ClampToByte(((this.startR + (this.diffR * lerp)) + num4) + 0.5);
                return ColorBgra.FromBgra((byte) num5, (byte) num6, (byte) num7, 0xff);
            }
        }
    }
}

