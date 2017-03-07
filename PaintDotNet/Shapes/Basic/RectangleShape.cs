namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class RectangleShape : RectangleShapeBase
    {
        public RectangleShape() : base(PdnResources.GetString("RectangleShape.Name"))
        {
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            new RectangleGeometry(bounds).EnsureFrozen<RectangleGeometry>();

        protected override Geometry OnCreateInteriorFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            RectDouble? nullable;
            ShapeDrawType type = (ShapeDrawType) settingValues[ToolSettings.Null.Shapes.DrawType.Path];
            if ((type & ShapeDrawType.Outline) == ShapeDrawType.Outline)
            {
                float num = (float) settingValues[ToolSettings.Null.Pen.Width.Path];
                double penWidth = num;
                nullable = RectangleShapeBase.TryGetInsetInteriorFillBounds(bounds, penWidth);
            }
            else
            {
                nullable = new RectDouble?(bounds);
            }
            if (!nullable.HasValue)
            {
                return null;
            }
            return new RectangleGeometry(nullable.Value).EnsureFrozen<RectangleGeometry>();
        }

        protected override Geometry OnCreateOutlineDrawGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            float num = (float) settingValues[ToolSettings.Null.Pen.Width.Path];
            double penWidth = num;
            RectDouble? nullable = RectangleShapeBase.TryGetInsetOutlineDrawBounds(bounds, penWidth);
            if (!nullable.HasValue)
            {
                return null;
            }
            return new RectangleGeometry(nullable.Value).EnsureFrozen<RectangleGeometry>();
        }

        protected override Geometry OnCreateOutlineFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            float num = (float) settingValues[ToolSettings.Null.Pen.Width.Path];
            double penWidth = num;
            if (!RectangleShapeBase.TryGetInsetOutlineDrawBounds(bounds, penWidth).HasValue)
            {
                return new RectangleGeometry(bounds).EnsureFrozen<RectangleGeometry>();
            }
            return null;
        }
    }
}

