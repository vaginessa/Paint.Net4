namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;

    internal class MouseEventArgsF : MouseEventArgs
    {
        private double fx;
        private double fy;
        private PointDouble[] intermediatePoints;
        private ReadOnlyCollection<PointDouble> intermediatePointsRO;

        public MouseEventArgsF(MouseButtons buttons, int clicks, double fx, double fy, int delta) : this(buttons, clicks, fx, fy, delta, numArray1)
        {
            PointDouble[] numArray1 = new PointDouble[] { new PointDouble(fx, fy) };
        }

        public MouseEventArgsF(MouseButtons buttons, int clicks, double fx, double fy, int delta, IEnumerable<PointDouble> intermediatePoints) : base(buttons, clicks, (int) Math.Floor(fx), (int) Math.Floor(fy), delta)
        {
            Validate.IsNotNull<IEnumerable<PointDouble>>(intermediatePoints, "intermediatePoints");
            this.fx = fx;
            this.fy = fy;
            this.intermediatePoints = intermediatePoints.ToArrayEx<PointDouble>();
            if (!this.intermediatePoints.Any<PointDouble>())
            {
                throw new ArgumentException("intermediatePoints may not be an empty list", "intermediatePoints");
            }
            if (this.intermediatePoints.Last<PointDouble>() != this.Point)
            {
                throw new ArgumentException("The last value in intermediatePoints must be equal to the coalesced point (fx, fy) that is also passed in");
            }
            this.intermediatePointsRO = new ReadOnlyCollection<PointDouble>(this.intermediatePoints);
        }

        public double Fx =>
            this.fx;

        public double Fy =>
            this.fy;

        public IList<PointDouble> IntermediatePoints =>
            this.intermediatePointsRO;

        public PointDouble Point =>
            new PointDouble(this.fx, this.fy);
    }
}

