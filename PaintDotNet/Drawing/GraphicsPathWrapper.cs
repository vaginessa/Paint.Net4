namespace PaintDotNet.Drawing
{
    using PaintDotNet;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    [Serializable]
    internal class GraphicsPathWrapper
    {
        private FillMode fillMode;
        private PointF[] points;
        private byte[] types;

        public GraphicsPathWrapper(PdnGraphicsPath path)
        {
            this.points = (PointF[]) path.PathPoints.Clone();
            this.types = (byte[]) path.PathTypes.Clone();
            this.fillMode = path.FillMode;
        }

        public PdnGraphicsPath CreateGraphicsPath() => 
            new PdnGraphicsPath(this.points, this.types, this.fillMode);
    }
}

