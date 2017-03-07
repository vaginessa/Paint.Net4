namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct Int32RgbColor : IEquatable<Int32RgbColor>
    {
        private int red;
        private int green;
        private int blue;
        private static void VerifyColorValue(int value, string valueName)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw new ArgumentOutOfRangeException(valueName, "must be in the range [0, 255]");
            }
        }

        public int Red
        {
            get => 
                this.red;
            set
            {
                VerifyColorValue(value, "value");
                this.red = value;
            }
        }
        public int Green
        {
            get => 
                this.green;
            set
            {
                VerifyColorValue(value, "value");
                this.green = value;
            }
        }
        public int Blue
        {
            get => 
                this.blue;
            set
            {
                VerifyColorValue(value, "value");
                this.blue = value;
            }
        }
        public Int32RgbColor(int r, int g, int b)
        {
            VerifyColorValue(r, "r");
            VerifyColorValue(g, "g");
            VerifyColorValue(b, "b");
            this.red = r;
            this.green = g;
            this.blue = b;
        }

        public bool Equals(Int32RgbColor other) => 
            (((this.red == other.red) && (this.green == other.green)) && (this.blue == other.blue));

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<Int32RgbColor, object>(this, obj);

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.red.GetHashCode(), this.green.GetHashCode(), this.blue.GetHashCode());

        public static Int32RgbColor FromHsv(Int32HsvColor hsv) => 
            hsv.ToRgb();

        public static Int32RgbColor Ceiling(ColorRgb96Float rgbF) => 
            new Int32RgbColor((int) FloatUtil.Clamp((float) Math.Ceiling((double) (rgbF.R * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Ceiling((double) (rgbF.G * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Ceiling((double) (rgbF.B * 255f)), 0f, 255f));

        public static Int32RgbColor Floor(ColorRgb96Float rgbF) => 
            new Int32RgbColor((int) FloatUtil.Clamp((float) Math.Floor((double) (rgbF.R * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Floor((double) (rgbF.G * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Floor((double) (rgbF.B * 255f)), 0f, 255f));

        public static Int32RgbColor Round(ColorRgb96Float rgbF) => 
            new Int32RgbColor((int) FloatUtil.Clamp((float) Math.Round((double) (rgbF.R * 255f), MidpointRounding.AwayFromZero), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Round((double) (rgbF.G * 255f), MidpointRounding.AwayFromZero), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Round((double) (rgbF.B * 255f), MidpointRounding.AwayFromZero), 0f, 255f));

        public static Int32RgbColor Truncate(ColorRgb96Float rgbF) => 
            new Int32RgbColor((int) FloatUtil.Clamp((float) Math.Truncate((double) (rgbF.R * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Truncate((double) (rgbF.G * 255f)), 0f, 255f), (int) FloatUtil.Clamp((float) Math.Truncate((double) (rgbF.B * 255f)), 0f, 255f));

        public Color ToGdipColor() => 
            Color.FromArgb(this.Red, this.Green, this.Blue);

        public Int32HsvColor ToHsv()
        {
            double num7;
            double num8;
            double num4 = ((double) this.Red) / 255.0;
            double num5 = ((double) this.Green) / 255.0;
            double num6 = ((double) this.Blue) / 255.0;
            double num = Math.Min(Math.Min(num4, num5), num6);
            double num2 = Math.Max(Math.Max(num4, num5), num6);
            double num9 = num2;
            double num3 = num2 - num;
            if ((num2 == 0.0) || (num3 == 0.0))
            {
                num8 = 0.0;
                num7 = 0.0;
            }
            else
            {
                num8 = num3 / num2;
                if (num4 == num2)
                {
                    num7 = (num5 - num6) / num3;
                }
                else if (num5 == num2)
                {
                    num7 = 2.0 + ((num6 - num4) / num3);
                }
                else
                {
                    num7 = 4.0 + ((num4 - num5) / num3);
                }
            }
            num7 *= 60.0;
            if (num7 < 0.0)
            {
                num7 += 360.0;
            }
            return new Int32HsvColor((int) num7, (int) (num8 * 100.0), (int) (num9 * 100.0));
        }

        public override string ToString() => 
            $"({this.Red}, {this.Green}, {this.Blue})";
    }
}

