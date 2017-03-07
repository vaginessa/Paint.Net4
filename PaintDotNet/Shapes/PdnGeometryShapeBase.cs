namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;

    internal abstract class PdnGeometryShapeBase : PdnShapeBase
    {
        protected PdnGeometryShapeBase(string displayName, ShapeCategory category) : base(displayName, category)
        {
        }

        protected abstract Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues);
        protected virtual Geometry OnCreateImageGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.OnCreateGuideGeometry(bounds, settingValues);

        protected sealed override ShapeRenderData OnCreateImageRenderData(ShapeRenderParameters renderParams)
        {
            RectDouble bounds = RectDouble.FromCorners(renderParams.StartPoint, renderParams.EndPoint);
            return new ShapeRenderData(this.OnCreateImageGeometry(bounds, renderParams.SettingValues));
        }

        protected virtual Geometry OnCreateInteriorFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.OnCreateGuideGeometry(bounds, settingValues);

        protected virtual Geometry OnCreateOutlineDrawGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.OnCreateGuideGeometry(bounds, settingValues);

        protected virtual Geometry OnCreateOutlineFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            null;

        protected sealed override ShapeRenderData OnCreateRenderData(ShapeRenderParameters renderParams)
        {
            ShapeDrawType type = (ShapeDrawType) renderParams.SettingValues[ToolSettings.Null.Shapes.DrawType.Path];
            RectDouble bounds = RectDouble.FromCorners(renderParams.StartPoint, renderParams.EndPoint);
            Geometry guideGeometry = this.OnCreateGuideGeometry(bounds, renderParams.SettingValues);
            Geometry interiorFillGeometry = null;
            if ((type & ShapeDrawType.Interior) == ShapeDrawType.Interior)
            {
                interiorFillGeometry = this.OnCreateInteriorFillGeometry(bounds, renderParams.SettingValues);
            }
            Geometry outlineDrawGeometry = null;
            Geometry outlineFillGeometry = null;
            if ((type & ShapeDrawType.Outline) == ShapeDrawType.Outline)
            {
                outlineDrawGeometry = this.OnCreateOutlineDrawGeometry(bounds, renderParams.SettingValues);
                outlineFillGeometry = this.OnCreateOutlineFillGeometry(bounds, renderParams.SettingValues);
            }
            return new ShapeRenderData(guideGeometry, interiorFillGeometry, outlineDrawGeometry, outlineFillGeometry);
        }

        protected override IEnumerable<string> OnGetRenderSettingPaths()
        {
            string[] tails = new string[] { ToolSettings.Null.Pen.Width.Path, ToolSettings.Null.Pen.DashStyle.Path, ToolSettings.Null.Shapes.DrawType.Path };
            return base.OnGetRenderSettingPaths().Concat<string>(tails);
        }
    }
}

