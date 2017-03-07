namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.Resources;
    using System;
    using System.Runtime.CompilerServices;

    internal static class MeasurementUnitExtensions
    {
        public static string GetAbbreviation(this MeasurementUnit units)
        {
            switch (units)
            {
                case MeasurementUnit.Pixel:
                    return string.Empty;

                case MeasurementUnit.Inch:
                    return PdnResources.GetString("MeasurementUnit.Inch.Abbreviation");

                case MeasurementUnit.Centimeter:
                    return PdnResources.GetString("MeasurementUnit.Centimeter.Abbreviation");
            }
            throw ExceptionUtil.InvalidEnumArgumentException<MeasurementUnit>(units, "units");
        }
    }
}

