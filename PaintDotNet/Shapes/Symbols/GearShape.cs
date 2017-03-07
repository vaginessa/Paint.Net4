namespace PaintDotNet.Shapes.Symbols
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class GearShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = new FlattenedGeometry { 
            FlatteningTolerance=0.0001,
            Geometry=Geometry.Parse("F1\r\n                      M 37.6289,0 \r\n                      L 49.6289,-0.000976563 \r\n                      L 52.8506,10.8311 \r\n                      C 55.3838,11.5439 \r\n                        57.7979,12.542 \r\n                        60.0557,13.79 \r\n                      L 70.0654,8.37012\r\n                      L 78.5518,16.8545 \r\n                      L 73.1934,26.749 \r\n                      C 74.4844,29.0117 \r\n                        75.5234,31.4365 \r\n                        76.2725,33.9863 \r\n                      L 87.251,37.252 \r\n                      L 87.251,49.252 \r\n                      L 76.4746,52.457\r\n                      C 75.7588,55.1113 \r\n                        74.7295,57.6377 \r\n                        73.4297,59.9932 \r\n                      L 78.8838,70.0645 \r\n                      L 70.3994,78.5518 \r\n                      L 60.4404,73.1582 \r\n                      C 58.0811,74.5029 \r\n                        55.5439,75.5732 \r\n                        52.875,76.3252 \r\n                      L 49.624,87.2549\r\n                      L 37.624,87.2549 \r\n                      L 34.373,76.3232 \r\n                      C 31.7061,75.5713 \r\n                        29.1729,74.501 \r\n                        26.8145,73.1572 \r\n                      L 16.8574,78.5488 \r\n                      L 8.37207,70.0635 \r\n                      L 13.8262,59.9912\r\n                      C 12.5264,57.6357 \r\n                        11.498,55.1113 \r\n                        10.7822,52.458 \r\n                      L 0,49.251 \r\n                      L 0,37.251 \r\n                      L 10.9854,33.9834 \r\n                      C 11.7334,31.4365 \r\n                        12.7715,29.0137 \r\n                        14.0615,26.7529\r\n                      L 8.70313,16.8564 \r\n                      L 17.1885,8.37012 \r\n                      L 27.1982,13.791 \r\n                      C 29.4561,12.542 \r\n                        31.8721,11.543 \r\n                        34.4072,10.8311 ZM\t43.62644869,59.92630997\r\n                      C\t52.62744869,59.92630997\r\n                        59.92434869,52.62740997\r\n                        59.92434869,43.62740997\r\n                      C\t59.92434869,34.62650997\r\n                        52.62744869,27.32760997\r\n                        43.62644869,27.32760997\r\n                      C\t34.62644869,27.32760997\r\n                        27.32664869,34.62650997\r\n                        27.32664869,43.62740997\r\n                      C\t27.32664869,52.62740997\r\n                        34.62644869,59.92630997\r\n                        43.62644869,59.92630997 Z")
        }.EnsureFrozen<FlattenedGeometry>();

        public GearShape() : base(PdnResources.GetString("GearShape.Name"), ShapeCategory.Symbols)
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

