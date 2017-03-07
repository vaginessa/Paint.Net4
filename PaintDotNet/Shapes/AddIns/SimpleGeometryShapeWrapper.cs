namespace PaintDotNet.Shapes.AddIns
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class SimpleGeometryShapeWrapper : Shape
    {
        private ConcurrentDictionary<ShapeDrawType, ShapeRenderData> renderDataCache;
        private SimpleGeometryShape source;
        private string sourcePath;

        public SimpleGeometryShapeWrapper(string sourcePath, SimpleGeometryShape source) : base(source.DisplayName, ShapeCategory.Custom, ShapeOptions.Default)
        {
            this.renderDataCache = new ConcurrentDictionary<ShapeDrawType, ShapeRenderData>();
            Validate.IsNotNullOrWhiteSpace(sourcePath, "sourcePath");
            this.sourcePath = sourcePath;
            this.source = source;
        }

        protected sealed override ShapeRenderData OnCreateImageRenderData(ShapeRenderParameters renderParams) => 
            new ShapeRenderData(this.source.Geometry);

        protected override ShapeRenderData OnCreateRenderData(ShapeRenderParameters renderParams) => 
            this.renderDataCache.GetOrAdd((ShapeDrawType) renderParams.SettingValues[ToolSettings.Null.Shapes.DrawType.Path], delegate (ShapeDrawType drawType) {
                Geometry geometry = this.source.Geometry;
                Geometry interiorFillGeometry = ((drawType & ShapeDrawType.Interior) == ShapeDrawType.Interior) ? this.source.Geometry : null;
                Geometry outlineDrawGeometry = ((drawType & ShapeDrawType.Outline) == ShapeDrawType.Outline) ? this.source.Geometry : null;
                return new ShapeRenderData(geometry, interiorFillGeometry, outlineDrawGeometry, null);
            });

        protected override double OnGetAspectRatio()
        {
            RectDouble bounds = this.source.Geometry.Bounds;
            if ((bounds.Width != 0.0) && (bounds.Height != 0.0))
            {
                return (bounds.Width / bounds.Height);
            }
            return 1.0;
        }

        protected override IEnumerable<string> OnGetRenderSettingPaths()
        {
            string[] tails = new string[] { ToolSettings.Null.Pen.Width.Path, ToolSettings.Null.Pen.DashStyle.Path, ToolSettings.Null.Shapes.DrawType.Path };
            return base.OnGetRenderSettingPaths().Concat<string>(tails);
        }

        public override string ID =>
            this.sourcePath;

        public sealed override string ToolTipText
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(base.DisplayName);
                string str2 = string.Format(PdnResources.GetString("Effect.PluginToolTip.Location.Format"), this.sourcePath);
                builder.AppendLine(str2);
                return builder.ToString();
            }
        }
    }
}

