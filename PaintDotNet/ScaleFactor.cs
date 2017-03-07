namespace PaintDotNet
{
    using PaintDotNet.Resources;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{Numerator}:{Denominator}, {Ratio * 100}%")]
    internal struct ScaleFactor : IEquatable<ScaleFactor>
    {
        private int denominator;
        private int numerator;
        public static readonly ScaleFactor OneToOne;
        public static readonly ScaleFactor MinValue;
        public static readonly ScaleFactor MaxValue;
        private static string percentageFormat;
        private static readonly ScaleFactor[] scales;
        public int Denominator =>
            this.denominator;
        public int Numerator =>
            this.numerator;
        public double Ratio =>
            (((double) this.numerator) / ((double) this.denominator));
        private void Clamp()
        {
            if (this < MinValue)
            {
                this = MinValue;
            }
            else if (this > MaxValue)
            {
                this = MaxValue;
            }
        }

        public static ScaleFactor UseIfValid(int numerator, int denominator, ScaleFactor lastResort)
        {
            if ((numerator > 0) && (denominator > 0))
            {
                return new ScaleFactor(numerator, denominator);
            }
            return lastResort;
        }

        public static ScaleFactor Min(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor lhs = UseIfValid(n1, d1, lastResort);
            ScaleFactor rhs = UseIfValid(n2, d2, lastResort);
            return Min(lhs, rhs);
        }

        public static ScaleFactor Max(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor lhs = UseIfValid(n1, d1, lastResort);
            ScaleFactor rhs = UseIfValid(n2, d2, lastResort);
            return Max(lhs, rhs);
        }

        public static ScaleFactor Min(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs < rhs)
            {
                return lhs;
            }
            return rhs;
        }

        public static ScaleFactor Max(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs > rhs)
            {
                return lhs;
            }
            return rhs;
        }

        public static bool operator ==(ScaleFactor lhs, ScaleFactor rhs) => 
            lhs.Equals(rhs);

        public static bool operator !=(ScaleFactor lhs, ScaleFactor rhs) => 
            !(lhs == rhs);

        public static bool operator <(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) < (rhs.numerator * lhs.denominator));

        public static bool operator <=(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) <= (rhs.numerator * lhs.denominator));

        public static bool operator >(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) > (rhs.numerator * lhs.denominator));

        public static bool operator >=(ScaleFactor lhs, ScaleFactor rhs) => 
            ((lhs.numerator * rhs.denominator) >= (rhs.numerator * lhs.denominator));

        public bool Equals(ScaleFactor other) => 
            ((this.numerator * other.denominator) == (other.numerator * this.denominator));

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<ScaleFactor, object>(this, obj);

        public override int GetHashCode() => 
            this.Ratio.GetHashCode();

        public override string ToString()
        {
            try
            {
                return string.Format(percentageFormat, Math.Round((double) (100.0 * this.Ratio), MidpointRounding.AwayFromZero));
            }
            catch (ArithmeticException)
            {
                return "--";
            }
        }

        public static ScaleFactor[] PresetValues
        {
            get
            {
                ScaleFactor[] array = new ScaleFactor[scales.Length];
                scales.CopyTo(array, 0);
                return array;
            }
        }
        public ScaleFactor GetNextLarger()
        {
            double ratio = this.Ratio + 0.005;
            int length = Array.FindIndex<ScaleFactor>(scales, scale => ratio <= scale.Ratio);
            if (length == -1)
            {
                length = scales.Length;
            }
            length = Math.Min(length, scales.Length - 1);
            return scales[length];
        }

        public ScaleFactor GetNextSmaller()
        {
            double ratio = this.Ratio - 0.005;
            int num = Array.FindIndex<ScaleFactor>(scales, scale => ratio <= scale.Ratio) - 1;
            if (num == -1)
            {
                num = 0;
            }
            num = Math.Max(num, 0);
            return scales[num];
        }

        private static ScaleFactor Reduce(int numerator, int denominator)
        {
            int num = 2;
            while ((num < denominator) && (num < numerator))
            {
                if (((numerator % num) == 0) && ((denominator % num) == 0))
                {
                    numerator /= num;
                    denominator /= num;
                }
                else
                {
                    num++;
                }
            }
            return new ScaleFactor(numerator, denominator);
        }

        public static ScaleFactor FromRatio(double ratio)
        {
            int numerator = (int) Math.Floor((double) (ratio * 10000.0));
            int denominator = 0x2710;
            return Reduce(numerator, denominator);
        }

        public ScaleFactor(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }
            if (numerator < 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }
            this.numerator = numerator;
            this.denominator = denominator;
            this.Clamp();
        }

        public static bool TryParse(string text, out ScaleFactor scaleFactor)
        {
            double num;
            if (string.IsNullOrWhiteSpace(text))
            {
                scaleFactor = new ScaleFactor();
                return false;
            }
            if (!double.TryParse(text.Replace("%", string.Empty).Replace(" ", string.Empty), out num))
            {
                scaleFactor = new ScaleFactor();
                return false;
            }
            if (num.IsFinite() && (num > 0.0))
            {
                scaleFactor = FromRatio(num / 100.0);
                return true;
            }
            scaleFactor = new ScaleFactor();
            return false;
        }

        static ScaleFactor()
        {
            OneToOne = new ScaleFactor(1, 1);
            MinValue = new ScaleFactor(1, 100);
            MaxValue = new ScaleFactor(0x20, 1);
            percentageFormat = PdnResources.GetString("ScaleFactor.Percentage.Format");
            scales = new ScaleFactor[] { 
                new ScaleFactor(1, 0x20), new ScaleFactor(1, 0x18), new ScaleFactor(1, 0x10), new ScaleFactor(1, 12), new ScaleFactor(1, 8), new ScaleFactor(1, 6), new ScaleFactor(1, 5), new ScaleFactor(1, 4), new ScaleFactor(1, 3), new ScaleFactor(1, 2), new ScaleFactor(2, 3), new ScaleFactor(1, 1), new ScaleFactor(3, 2), new ScaleFactor(2, 1), new ScaleFactor(3, 1), new ScaleFactor(4, 1),
                new ScaleFactor(5, 1), new ScaleFactor(6, 1), new ScaleFactor(8, 1), new ScaleFactor(12, 1), new ScaleFactor(0x10, 1), new ScaleFactor(0x18, 1), new ScaleFactor(0x20, 1)
            };
        }
    }
}

