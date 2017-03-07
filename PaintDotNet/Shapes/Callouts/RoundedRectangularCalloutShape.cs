namespace PaintDotNet.Shapes.Callouts
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class RoundedRectangularCalloutShape : PdnGeometryShapeBase
    {
        private static readonly Geometry unitGeometry = Geometry.Parse("F1 M 743.813,569C 739.498,569 736,565.502 736,561.188L 736,501.813C 736,497.498 739.498,494 743.813,494L 828.188,494C 832.502,494 836,497.498 836,501.813L 836,561.188C 836,565.502 832.502,569 828.188,569L 764.152,569L 736,594L 746.704,569L 743.813,569 Z").EnsureFrozen<Geometry>();

        public RoundedRectangularCalloutShape() : base(PdnResources.GetString("RoundedRectangularCalloutShape.Name"), ShapeCategory.Callouts)
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            unitGeometry;
    }
}

