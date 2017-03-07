namespace PaintDotNet.Rendering
{
    using System;

    internal abstract class GradientShape
    {
        protected readonly double endPointX;
        protected readonly double endPointY;
        protected readonly double startPointX;
        protected readonly double startPointY;

        public GradientShape(PointDouble startPoint, PointDouble endPoint)
        {
            this.startPointX = startPoint.X;
            this.startPointY = startPoint.Y;
            this.endPointX = endPoint.X;
            this.endPointY = endPoint.Y;
        }

        public virtual double ComputeLerp(double x, double y)
        {
            throw new NotImplementedException();
        }
    }
}

