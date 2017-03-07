namespace PaintDotNet.Brushes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class BrushStrokePath
    {
        private double length;
        private SegmentedList<double> lengthAtPoint;
        private ReadOnlyCollection<double> lengthAtPointRO;
        private BrushStrokeLengthMetric lengthMetric;
        private SegmentedList<PointDouble> points = new SegmentedList<PointDouble>();
        private ReadOnlyCollection<PointDouble> pointsRO;

        public BrushStrokePath(BrushStrokeLengthMetric lengthMetric)
        {
            this.pointsRO = new ReadOnlyCollection<PointDouble>(this.points);
            this.length = 0.0;
            this.lengthAtPoint = new SegmentedList<double>();
            this.lengthAtPointRO = new ReadOnlyCollection<double>(this.lengthAtPoint);
            this.lengthMetric = lengthMetric;
        }

        private static double GetAnamorphicLength(VectorDouble vec)
        {
            double num = Math.Abs(vec.X);
            double num2 = Math.Abs(vec.Y);
            if (num > num2)
            {
                return num;
            }
            return num2;
        }

        private double GetLength(VectorDouble vec)
        {
            switch (this.lengthMetric)
            {
                case BrushStrokeLengthMetric.Euclidean:
                    return vec.Length;

                case BrushStrokeLengthMetric.Anamorphic:
                    return GetAnamorphicLength(vec);
            }
            ExceptionUtil.ThrowInvalidEnumArgumentException<BrushStrokeLengthMetric>(this.lengthMetric, "this.lengthMetric");
            return double.NegativeInfinity;
        }

        public PointDouble GetPointAtLength(double length)
        {
            if (!length.IsFinite())
            {
                throw new ArgumentException("length is not finite");
            }
            if (this.points.Count == 0)
            {
                return PointDouble.NaN;
            }
            if (length < 0.0)
            {
                return this.points[0];
            }
            if (length >= this.length)
            {
                return this.points[this.points.Count - 1];
            }
            int num = ListUtil.BinarySearch<double, SegmentedListStruct<double>, DefaultComparerStruct<double>>(this.lengthAtPoint.AsStruct<double>(), length, new DefaultComparerStruct<double>());
            if (num >= 0)
            {
                return this.points[num];
            }
            int num2 = ~num;
            if (num2 == this.lengthAtPoint.Count)
            {
                return this.points[this.points.Count - 1];
            }
            int num3 = num2 - 1;
            PointDouble num4 = this.points[num3];
            PointDouble num5 = this.points[num2];
            double num6 = this.lengthAtPoint[num3];
            double num7 = this.lengthAtPoint[num2];
            if (num6 == num7)
            {
                return num4;
            }
            double num8 = (length - num6) / (num7 - num6);
            double num9 = 1.0 - num8;
            return new PointDouble((num4.X * num9) + (num5.X * num8), (num4.Y * num9) + (num5.Y * num8));
        }

        public bool TryAddPoint(PointDouble point)
        {
            if (this.points.Count == 0)
            {
                this.points.Add(point);
                this.lengthAtPoint.Add(0.0);
                this.length = 0.0;
                return true;
            }
            if (point == this.points[this.points.Count - 1])
            {
                return false;
            }
            PointDouble num = this.points[this.points.Count - 1];
            double num2 = this.lengthAtPoint[this.lengthAtPoint.Count - 1];
            VectorDouble vec = (VectorDouble) (point - num);
            double length = this.GetLength(vec);
            double item = num2 + length;
            this.points.Add(point);
            this.lengthAtPoint.Add(item);
            this.length = item;
            return true;
        }

        public double Length =>
            this.length;

        public IList<double> LengthAtPoint =>
            this.lengthAtPointRO;

        public BrushStrokeLengthMetric LengthMetric =>
            this.lengthMetric;

        public IList<PointDouble> Points =>
            this.pointsRO;
    }
}

