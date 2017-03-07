namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Runtime.CompilerServices;

    internal static class VectorDoubleExtensions
    {
        public static bool IsCloseToZero(this VectorDouble vec) => 
            (DoubleUtil.IsCloseToZero(vec.X) && DoubleUtil.IsCloseToZero(vec.Y));

        public static void NormalizeInPlace(this VectorDouble[] vecs)
        {
            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = VectorDouble.Normalize(vecs[i]);
            }
        }

        public static VectorDouble NormalizeOrZeroCopy(this VectorDouble vec)
        {
            if (vec.Length == 0.0)
            {
                return new VectorDouble(0.0, 0.0);
            }
            return VectorDouble.Normalize(vec);
        }

        private static VectorDouble RotateCopy(this VectorDouble vec, double angle)
        {
            double num = (angle * 3.1415926535897931) / 180.0;
            double length = vec.Length;
            double d = num + Math.Atan2(vec.Y, vec.X);
            double x = Math.Cos(d);
            return new VectorDouble(x, Math.Sin(d));
        }

        public static void RotateInPlace(this VectorDouble[] vecs, double angle)
        {
            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = vecs[i].RotateCopy(angle);
            }
        }
    }
}

