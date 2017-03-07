namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LineSegmentDouble : IEquatable<LineSegmentDouble>
    {
        internal PointDouble startPoint;
        internal PointDouble endPoint;
        public PointDouble StartPoint =>
            this.startPoint;
        public PointDouble EndPoint =>
            this.endPoint;
        public double X0 =>
            this.startPoint.X;
        public double Y0 =>
            this.startPoint.Y;
        public double X1 =>
            this.endPoint.X;
        public double Y1 =>
            this.endPoint.Y;
        public double Length
        {
            get
            {
                VectorDouble num = (VectorDouble) (this.endPoint - this.startPoint);
                return num.Length;
            }
        }
        public double LengthSquared
        {
            get
            {
                VectorDouble num = (VectorDouble) (this.endPoint - this.startPoint);
                return num.LengthSquared;
            }
        }
        public LineSegmentDouble(PointDouble startPoint, PointDouble endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public LineSegmentDouble(double x0, double y0, double x1, double y1)
        {
            this.startPoint = new PointDouble(x0, y0);
            this.endPoint = new PointDouble(x1, y1);
        }

        public static bool operator ==(LineSegmentDouble x, LineSegmentDouble y) => 
            x.Equals(y);

        public static bool operator !=(LineSegmentDouble x, LineSegmentDouble y) => 
            !x.Equals(y);

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<LineSegmentDouble, object>(this, obj);

        public bool Equals(LineSegmentDouble other) => 
            ((this.startPoint == other.startPoint) && (this.endPoint == other.endPoint));

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.startPoint.GetHashCode(), this.endPoint.GetHashCode());
    }
}

