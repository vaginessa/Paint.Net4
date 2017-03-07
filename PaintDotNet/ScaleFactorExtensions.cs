namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal static class ScaleFactorExtensions
    {
        public static PointDouble Scale(this ScaleFactor sf, PointDouble p) => 
            new PointDouble(sf.Scale(p.X), sf.Scale(p.Y));

        public static PointDouble Scale(this ScaleFactor sf, PointInt32 p) => 
            new PointDouble(sf.Scale((double) p.X), sf.Scale((double) p.Y));

        public static RectDouble Scale(this ScaleFactor sf, RectDouble rect) => 
            new RectDouble(sf.Scale(rect.X), sf.Scale(rect.Y), sf.Scale(rect.Width), sf.Scale(rect.Height));

        public static RectDouble Scale(this ScaleFactor sf, RectInt32 rect) => 
            new RectDouble(sf.Scale((double) rect.X), sf.Scale((double) rect.Y), sf.Scale((double) rect.Width), sf.Scale((double) rect.Height));

        public static SizeDouble Scale(this ScaleFactor sf, SizeDouble size) => 
            new SizeDouble(sf.Scale(size.Width), sf.Scale(size.Height));

        public static SizeDouble Scale(this ScaleFactor sf, SizeInt32 size) => 
            new SizeDouble(sf.Scale((double) size.Width), sf.Scale((double) size.Height));

        public static VectorDouble Scale(this ScaleFactor sf, VectorDouble vec) => 
            new VectorDouble(sf.Scale(vec.X), sf.Scale(vec.Y));

        public static double Scale(this ScaleFactor sf, double x) => 
            ((x * sf.Numerator) / ((double) sf.Denominator));

        public static PointDouble Unscale(this ScaleFactor sf, PointDouble p) => 
            new PointDouble(sf.Unscale(p.X), sf.Unscale(p.Y));

        public static PointDouble Unscale(this ScaleFactor sf, PointInt32 p) => 
            new PointDouble(sf.Unscale((double) p.X), sf.Unscale((double) p.Y));

        public static RectDouble Unscale(this ScaleFactor sf, RectDouble rect) => 
            new RectDouble(sf.Unscale(rect.X), sf.Unscale(rect.Y), sf.Unscale(rect.Width), sf.Unscale(rect.Height));

        public static RectDouble Unscale(this ScaleFactor sf, RectInt32 rect) => 
            new RectDouble(sf.Unscale((double) rect.X), sf.Unscale((double) rect.Y), sf.Unscale((double) rect.Width), sf.Unscale((double) rect.Height));

        public static SizeDouble Unscale(this ScaleFactor sf, SizeDouble size) => 
            new SizeDouble(sf.Unscale(size.Width), sf.Unscale(size.Height));

        public static SizeDouble Unscale(this ScaleFactor sf, SizeInt32 size) => 
            new SizeDouble(sf.Unscale((double) size.Width), sf.Unscale((double) size.Height));

        public static VectorDouble Unscale(this ScaleFactor sf, VectorDouble vec) => 
            new VectorDouble(sf.Unscale(vec.X), sf.Unscale(vec.Y));

        public static double Unscale(this ScaleFactor sf, double x) => 
            ((x * sf.Denominator) / ((double) sf.Numerator));
    }
}

