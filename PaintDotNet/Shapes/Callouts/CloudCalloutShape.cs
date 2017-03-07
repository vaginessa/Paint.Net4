﻿namespace PaintDotNet.Shapes.Callouts
{
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class CloudCalloutShape : PdnGeometryShapeBase
    {
        private static readonly Geometry path1 = Geometry.Parse("F1 M 29.8114,6.86053C 32.6865,6.86053 35.3553,7.63245 37.561,8.95203C 38.8107,5.08148 43.0224,2.23468 48.0257,2.23468C 51.3842,2.23468 54.386,3.51746 56.3747,5.53149C 57.7646,2.53638 60.4959,0.5 63.638,0.5C 66.7857,0.5 69.5212,2.5437 70.9087,5.54773C 72.5835,2.72839 76.078,0.789063 80.1176,0.789063C 85.786,0.789063 90.3812,4.6076 90.3812,9.31799L 90.3795,9.47687C 95.0489,9.78705 98.7656,15.0986 98.7656,21.6055C 98.7656,22.9968 98.5956,24.3336 98.2828,25.5785C 99.6871,28.1501 100.5,31.2003 100.5,34.4711C 100.5,42.6901 95.3656,49.5161 88.6192,50.8566L 88.6465,51.6735C 88.6465,58.6992 82.6275,64.3947 75.2026,64.3947C 72.9284,64.3947 70.7861,63.8603 68.9082,62.9171C 68.1762,68.1365 62.4068,72.2007 55.3982,72.2007C 49.8579,72.2007 45.092,69.6611 42.9766,66.0188C 40.5288,67.3732 37.6553,68.1531 34.5818,68.1531C 27.846,68.1531 22.0709,64.407 19.6314,59.0776C 19.1799,59.152 18.7175,59.1906 18.2467,59.1906C 13.217,59.1906 9.13957,54.7896 9.13957,49.3606C 9.13957,46.6221 10.1771,44.1451 11.851,42.3626C 9.22155,40.8277 7.40488,37.5473 7.40488,33.7484C 7.40488,28.4791 10.8998,24.2075 15.211,24.2075C 16.1401,24.2075 17.0314,24.4059 17.8578,24.7699C 16.7251,23.0251 16.0784,21.008 16.0784,18.8588C 16.0784,12.2324 22.2268,6.86053 29.8114,6.86053 Z ").EnsureFrozen<Geometry>();
        private static readonly Geometry path2 = Geometry.Parse("F1 M 19.9099,64.5927C 24.1911,64.4238 27.7611,67.2875 27.8812,70.9274C 28.0014,74.5673 24.6274,77.6326 20.3451,77.774C 16.0628,77.9154 12.4939,75.0793 12.3738,71.4393C 12.2536,67.7994 15.2289,64.7773 19.9099,64.5927 Z ").EnsureFrozen<Geometry>();
        private static readonly Geometry path3 = Geometry.Parse("F1 M 11.0507,75.5271C 15.2486,75.5865 18.6535,77.7501 18.6535,80.4922C 18.6535,83.2344 15.2496,85.4573 11.0507,85.4573C 6.85179,85.4573 3.44791,83.2344 3.44791,80.4922C 3.44791,77.7501 6.60393,75.4642 11.0507,75.5271 Z ").EnsureFrozen<Geometry>();
        private static readonly Geometry path4 = Geometry.Parse("F1 M 4.06865,83.7512C 6.03956,83.756 7.6373,85.0016 7.6373,86.5441C 7.6373,88.0865 6.03956,89.3369 4.06865,89.3369C 2.09774,89.3369 0.49999,88.0865 0.49999,86.5441C 0.49999,85.0016 1.94781,83.7461 4.06865,83.7512 Z").EnsureFrozen<Geometry>();
        private static readonly GeometryGroup unitGeometry;

        static CloudCalloutShape()
        {
            Geometry[] children = new Geometry[] { path1, path2, path3, path4 };
            unitGeometry = new GeometryGroup(children).EnsureFrozen<GeometryGroup>();
        }

        public CloudCalloutShape() : base(PdnResources.GetString("CloudCalloutShape.Name"), ShapeCategory.Callouts)
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

