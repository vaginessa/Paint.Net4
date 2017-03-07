namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class DocumentExtensions
    {
        public static RectInt32 Bounds(this Document doc) => 
            new RectInt32(0, 0, doc.Width, doc.Height);

        public static void CoordinatesToStrings(this Document document, MeasurementUnit units, int x, int y, out string xString, out string yString, out string unitsString)
        {
            string abbreviation = units.GetAbbreviation();
            unitsString = units.GetAbbreviation();
            if (units == MeasurementUnit.Pixel)
            {
                xString = x.ToString();
                yString = y.ToString();
            }
            else
            {
                xString = document.PixelToPhysicalX((double) x, units).ToString("F2");
                yString = document.PixelToPhysicalY((double) y, units).ToString("F2");
            }
        }

        public static SizeInt32 Size(this Document doc) => 
            new SizeInt32(doc.Width, doc.Height);
    }
}

