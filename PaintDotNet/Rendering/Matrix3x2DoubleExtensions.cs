namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Runtime.CompilerServices;

    public static class Matrix3x2DoubleExtensions
    {
        public static double GetRotationAngle(this Matrix3x2Double matrix)
        {
            if (matrix.IsIdentity)
            {
                return 0.0;
            }
            return MathUtil.RadiansToDegrees(matrix.GetRotationRadians());
        }

        public static double GetRotationRadians(this Matrix3x2Double matrix)
        {
            if (matrix.IsIdentity)
            {
                return 0.0;
            }
            VectorDouble vec = new VectorDouble(1.0, 0.0);
            VectorDouble num2 = matrix.Transform(vec);
            return Math.Atan2(num2.Y, num2.X);
        }

        public static bool IsFlipped(this Matrix3x2Double matrix)
        {
            if (matrix.IsIdentity)
            {
                return false;
            }
            VectorDouble vec = new VectorDouble(1.0, 0.0);
            VectorDouble num2 = matrix.Transform(vec);
            double num4 = Math.Atan2(num2.Y, num2.X) * 57.295779513082323;
            VectorDouble num5 = new VectorDouble(0.0, 1.0);
            VectorDouble num6 = matrix.Transform(num5);
            double num8 = (Math.Atan2(num6.Y, num6.X) * 57.295779513082323) - 90.0;
            while (num4 < 0.0)
            {
                num4 += 360.0;
            }
            while (num8 < 0.0)
            {
                num8 += 360.0;
            }
            double num9 = Math.Abs((double) (num4 - num8));
            return ((num9 > 1.0) && (num9 < 359.0));
        }

        public static void VerifyIsFinite(this Matrix3x2Double m)
        {
            if (!m.IsFinite)
            {
                throw new ArgumentException("matrix is not finite, " + m.ToString());
            }
        }
    }
}

