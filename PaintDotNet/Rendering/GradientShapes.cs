namespace PaintDotNet.Rendering
{
    using System;

    internal static class GradientShapes
    {
        public class Conical : GradientShape
        {
            private const double invPi = 0.31830988618379069;
            private double thetaOffset;

            public Conical(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                double x = base.startPointX - endPoint.X;
                double y = base.startPointY - endPoint.Y;
                this.thetaOffset = Math.Atan2(y, x);
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                double num3 = Math.Atan2(num2, num) - this.thetaOffset;
                return (num3 * 0.31830988618379069);
            }
        }

        public sealed class ConicalNoRepeat : GradientShape
        {
            private const double inv2Pi = 0.15915494309189535;
            private double tOffset;

            public ConicalNoRepeat(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                this.tOffset = -this.ComputeLerp((double) ((int) base.endPointX), (double) ((int) base.endPointY));
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                double num3 = Math.Atan2(num2, num) + 3.1415926535897931;
                double num4 = num3 * 0.15915494309189535;
                num4 += this.tOffset;
                if (num4 < 0.0)
                {
                    num4++;
                    return num4;
                }
                if (num4 > 1.0)
                {
                    num4--;
                }
                return num4;
            }
        }

        public abstract class LinearBase : GradientShape
        {
            protected readonly double dtdx;
            protected readonly double dtdy;

            protected LinearBase(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                VectorDouble num = (VectorDouble) (endPoint - startPoint);
                double length = num.Length;
                if (base.endPointX == base.startPointX)
                {
                    this.dtdx = 0.0;
                }
                else
                {
                    this.dtdx = num.X / (length * length);
                }
                if (base.endPointY == base.startPointY)
                {
                    this.dtdy = 0.0;
                }
                else
                {
                    this.dtdy = num.Y / (length * length);
                }
            }
        }

        public sealed class LinearDiamond : GradientShapes.LinearStraight
        {
            public LinearDiamond(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                double num3 = (num * base.dtdx) + (num2 * base.dtdy);
                double num4 = (num * base.dtdy) - (num2 * base.dtdx);
                double num5 = Math.Abs(num3);
                double num6 = Math.Abs(num4);
                return (num5 + num6);
            }
        }

        public sealed class LinearReflected : GradientShapes.LinearStraight
        {
            public LinearReflected(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
            }

            public override double ComputeLerp(double x, double y) => 
                Math.Abs(base.ComputeLerp(x, y));
        }

        public class LinearStraight : GradientShapes.LinearBase
        {
            public LinearStraight(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                return ((num * base.dtdx) + (num2 * base.dtdy));
            }
        }

        public sealed class Radial : GradientShape
        {
            private double invDistanceScale;

            public Radial(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                VectorDouble num2 = (VectorDouble) (endPoint - startPoint);
                double length = num2.Length;
                if (length == 0.0)
                {
                    this.invDistanceScale = 0.0;
                }
                else
                {
                    this.invDistanceScale = 1.0 / length;
                }
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                return (Math.Sqrt((num * num) + (num2 * num2)) * this.invDistanceScale);
            }
        }

        public class Spiral : GradientShape
        {
            protected double invDistanceScale;
            private double tOffset;

            public Spiral(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                double x = endPoint.X - startPoint.X;
                double y = endPoint.Y - startPoint.Y;
                this.tOffset = 3.1415926535897931 - Math.Atan2(y, x);
                this.invDistanceScale = 1.0 / Math.Sqrt((x * x) + (y * y));
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                double num3 = 3.1415926535897931 - Math.Atan2(num2, num);
                num3 -= this.tOffset;
                if (num3 < 0.0)
                {
                    num3 += 6.2831853071795862;
                }
                double num4 = num3 * 0.15915494309189535;
                double num5 = Math.Sqrt((num * num) + (num2 * num2)) * this.invDistanceScale;
                return (num4 + num5);
            }
        }

        public class SpiralCCW : GradientShape
        {
            protected double invDistanceScale;
            private double tOffset;

            public SpiralCCW(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                double x = endPoint.X - startPoint.X;
                double y = endPoint.Y - startPoint.Y;
                this.tOffset = Math.Atan2(y, x);
                this.invDistanceScale = 1.0 / Math.Sqrt((x * x) + (y * y));
            }

            public override double ComputeLerp(double x, double y)
            {
                double num = x - base.startPointX;
                double num2 = y - base.startPointY;
                double num3 = Math.Atan2(num2, num) - this.tOffset;
                if (num3 < 0.0)
                {
                    num3 += 6.2831853071795862;
                }
                double num4 = num3 * 0.15915494309189535;
                double num5 = Math.Sqrt((num * num) + (num2 * num2)) * this.invDistanceScale;
                return (num4 + num5);
            }
        }

        public sealed class SpiralReflected : GradientShapes.Spiral
        {
            public SpiralReflected(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                base.invDistanceScale *= 0.5;
            }

            public override double ComputeLerp(double x, double y) => 
                (base.ComputeLerp(x, y) * 2.0);
        }

        public sealed class SpiralReflectedCCW : GradientShapes.SpiralCCW
        {
            public SpiralReflectedCCW(PointDouble startPoint, PointDouble endPoint) : base(startPoint, endPoint)
            {
                base.invDistanceScale *= 0.5;
            }

            public override double ComputeLerp(double x, double y) => 
                (base.ComputeLerp(x, y) * 2.0);
        }
    }
}

