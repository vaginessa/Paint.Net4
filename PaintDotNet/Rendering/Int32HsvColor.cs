namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Imaging;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Int32HsvColor : IEquatable<Int32HsvColor>
    {
        public const int HueMinValue = 0;
        public const int HueMaxValue = 360;
        public const int SaturationMinValue = 0;
        public const int SaturationMaxValue = 100;
        public const int ValueMinValue = 0;
        public const int ValueMaxValue = 100;
        private int hue;
        private int saturation;
        private int value;
        public int Hue
        {
            get => 
                this.hue;
            set
            {
                this.hue = value;
            }
        }
        public int Saturation
        {
            get => 
                this.saturation;
            set
            {
                this.saturation = value;
            }
        }
        public int Value
        {
            get => 
                this.value;
            set
            {
                this.value = value;
            }
        }
        public static bool operator ==(Int32HsvColor lhs, Int32HsvColor rhs) => 
            lhs.Equals(rhs);

        public static bool operator !=(Int32HsvColor lhs, Int32HsvColor rhs) => 
            !(lhs == rhs);

        public bool Equals(Int32HsvColor other) => 
            (((this.Hue == other.Hue) && (this.Saturation == other.Saturation)) && (this.Value == other.Value));

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<Int32HsvColor, object>(this, obj);

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.Hue, this.Saturation, this.Value);

        public Int32HsvColor(int hue, int saturation, int value)
        {
            if ((hue < 0) || (hue > 360))
            {
                throw new ArgumentOutOfRangeException("hue", "must be in the range [0, 360]");
            }
            if ((saturation < 0) || (saturation > 100))
            {
                throw new ArgumentOutOfRangeException("saturation", "must be in the range [0, 100]");
            }
            if ((value < 0) || (value > 100))
            {
                throw new ArgumentOutOfRangeException("value", "must be in the range [0, 100]");
            }
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public static Int32HsvColor Ceiling(ColorHsv96Float hsvF) => 
            new Int32HsvColor((int) DoubleUtil.Clamp(Math.Ceiling((double) hsvF.Hue), 0.0, 360.0), (int) DoubleUtil.Clamp(Math.Ceiling((double) hsvF.Saturation), 0.0, 100.0), (int) DoubleUtil.Clamp(Math.Ceiling((double) hsvF.Value), 0.0, 100.0));

        public static Int32HsvColor Floor(ColorHsv96Float hsvF) => 
            new Int32HsvColor((int) DoubleUtil.Clamp(Math.Floor((double) hsvF.Hue), 0.0, 360.0), (int) DoubleUtil.Clamp(Math.Floor((double) hsvF.Saturation), 0.0, 100.0), (int) DoubleUtil.Clamp(Math.Floor((double) hsvF.Value), 0.0, 100.0));

        public static Int32HsvColor Round(ColorHsv96Float hsvF) => 
            new Int32HsvColor((int) DoubleUtil.Clamp(Math.Round((double) hsvF.Hue, MidpointRounding.AwayFromZero), 0.0, 360.0), (int) DoubleUtil.Clamp(Math.Round((double) hsvF.Saturation, MidpointRounding.AwayFromZero), 0.0, 100.0), (int) DoubleUtil.Clamp(Math.Round((double) hsvF.Value, MidpointRounding.AwayFromZero), 0.0, 100.0));

        public static Int32HsvColor Truncate(ColorHsv96Float hsvF) => 
            new Int32HsvColor((int) DoubleUtil.Clamp(Math.Truncate((double) hsvF.Hue), 0.0, 360.0), (int) DoubleUtil.Clamp(Math.Truncate((double) hsvF.Saturation), 0.0, 100.0), (int) DoubleUtil.Clamp(Math.Truncate((double) hsvF.Value), 0.0, 100.0));

        public static Int32HsvColor FromGdipColor(Color color)
        {
            Int32RgbColor color2 = new Int32RgbColor(color.R, color.G, color.B);
            return color2.ToHsv();
        }

        public Color ToGdipColor()
        {
            Int32RgbColor color = this.ToRgb();
            return Color.FromArgb(color.Red, color.Green, color.Blue);
        }

        public Int32RgbColor ToRgb()
        {
            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num = ((double) this.Hue) % 360.0;
            double num2 = ((double) this.Saturation) / 100.0;
            double num3 = ((double) this.Value) / 100.0;
            if (num2 == 0.0)
            {
                num4 = num3;
                num5 = num3;
                num6 = num3;
            }
            else
            {
                double d = num / 60.0;
                int num11 = (int) Math.Floor(d);
                double num10 = d - num11;
                double num7 = num3 * (1.0 - num2);
                double num8 = num3 * (1.0 - (num2 * num10));
                double num9 = num3 * (1.0 - (num2 * (1.0 - num10)));
                switch (num11)
                {
                    case 0:
                        num4 = num3;
                        num5 = num9;
                        num6 = num7;
                        break;

                    case 1:
                        num4 = num8;
                        num5 = num3;
                        num6 = num7;
                        break;

                    case 2:
                        num4 = num7;
                        num5 = num3;
                        num6 = num9;
                        break;

                    case 3:
                        num4 = num7;
                        num5 = num8;
                        num6 = num3;
                        break;

                    case 4:
                        num4 = num9;
                        num5 = num7;
                        num6 = num3;
                        break;

                    case 5:
                        num4 = num3;
                        num5 = num7;
                        num6 = num8;
                        break;
                }
            }
            return new Int32RgbColor((int) (num4 * 255.0), (int) (num5 * 255.0), (int) (num6 * 255.0));
        }

        public override string ToString() => 
            $"({this.Hue}, {this.Saturation}, {this.Value})";
    }
}

