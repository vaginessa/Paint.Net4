namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal sealed class RoundedRectangleShape : RectangleShapeBase
    {
        private FlattenedGeometry cachedFlattenedGeometry;
        private RectangleGeometry cachedRoundedRectGeometry;
        private const double flatteningTolerance = 0.0001;
        private const double imageCornerRadiusDivisor = 4.0;
        private object sync;

        public RoundedRectangleShape() : base(PdnResources.GetString("RoundedRectangleShape.Name"))
        {
            this.sync = new object();
        }

        private FlattenedGeometry GetFlattenedRoundedRectangleGeometry(RectDouble bounds, double cornerRadiusX, double cornerRadiusY)
        {
            object sync = this.sync;
            lock (sync)
            {
                RectangleGeometry geometry = this.GetRoundedRectangleGeometry(bounds, cornerRadiusX, cornerRadiusY);
                if ((this.cachedFlattenedGeometry == null) || (this.cachedFlattenedGeometry.Geometry != geometry))
                {
                    this.cachedFlattenedGeometry = new FlattenedGeometry { 
                        Geometry = geometry,
                        FlatteningTolerance = 0.0001
                    }.EnsureFrozen<FlattenedGeometry>();
                }
                return this.cachedFlattenedGeometry;
            }
        }

        private RectangleGeometry GetRoundedRectangleGeometry(RectDouble bounds, double cornerRadiusX, double cornerRadiusY)
        {
            object sync = this.sync;
            lock (sync)
            {
                if (((this.cachedRoundedRectGeometry == null) || (this.cachedRoundedRectGeometry.Bounds != bounds)) || ((this.cachedRoundedRectGeometry.RadiusX != cornerRadiusX) || (this.cachedRoundedRectGeometry.RadiusY != cornerRadiusY)))
                {
                    this.cachedRoundedRectGeometry = new RectangleGeometry(bounds, cornerRadiusX, cornerRadiusY).EnsureFrozen<RectangleGeometry>();
                }
                return this.cachedRoundedRectGeometry;
            }
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            float num = (float) settingValues[ToolSettings.Null.Radius.Path];
            return this.GetRoundedRectangleGeometry(bounds, (double) num, (double) num).EnsureFrozen<RectangleGeometry>();
        }

        protected override Geometry OnCreateImageGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.GetRoundedRectangleGeometry(bounds, bounds.Width / 4.0, bounds.Height / 4.0).EnsureFrozen<RectangleGeometry>();

        protected override Geometry OnCreateInteriorFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            float num = (float) settingValues[ToolSettings.Null.Radius.Path];
            ShapeDrawType type = (ShapeDrawType) settingValues[ToolSettings.Null.Shapes.DrawType.Path];
            if ((type & ShapeDrawType.Outline) != ShapeDrawType.Outline)
            {
                return this.GetFlattenedRoundedRectangleGeometry(bounds, (double) num, (double) num);
            }
            return this.OnCreateOutlineDrawGeometry(bounds, settingValues);
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
            float num3 = (float) settingValues[ToolSettings.Null.Radius.Path];
            return this.GetFlattenedRoundedRectangleGeometry(nullable.Value, (double) num3, (double) num3);
        }

        protected override Geometry OnCreateOutlineFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            float num = (float) settingValues[ToolSettings.Null.Pen.Width.Path];
            double penWidth = num;
            if (RectangleShapeBase.TryGetInsetOutlineDrawBounds(bounds, penWidth).HasValue)
            {
                return null;
            }
            float num3 = (float) settingValues[ToolSettings.Null.Radius.Path];
            return this.GetFlattenedRoundedRectangleGeometry(bounds, (double) num3, (double) num3);
        }

        protected override IEnumerable<string> OnGetRenderSettingPaths() => 
            base.OnGetRenderSettingPaths().Concat<string>(ToolSettings.Null.Radius.Path);
    }
}

