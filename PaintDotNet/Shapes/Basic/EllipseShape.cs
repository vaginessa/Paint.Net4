namespace PaintDotNet.Shapes.Basic
{
    using PaintDotNet.Drawing;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Shapes;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;
    using System.Drawing.Drawing2D;

    internal sealed class EllipseShape : PdnGeometryShapeBase
    {
        private EllipseGeometry cachedEllipseGeometry;
        private FlattenedGeometry cachedFlattenedGeometry;
        private const double flatteningTolerance = 0.0001;
        private object sync;

        public EllipseShape() : base(PdnResources.GetString("EllipseShape.Name"), ShapeCategory.Basic)
        {
            this.sync = new object();
        }

        private EllipseGeometry GetEllipseGeometry(RectDouble bounds)
        {
            object sync = this.sync;
            lock (sync)
            {
                if ((this.cachedEllipseGeometry == null) || (this.cachedEllipseGeometry.Bounds != bounds))
                {
                    this.cachedEllipseGeometry = new EllipseGeometry(bounds).EnsureFrozen<EllipseGeometry>();
                }
                return this.cachedEllipseGeometry;
            }
        }

        private FlattenedGeometry GetFlattenedEllipseGeometry(RectDouble bounds)
        {
            object sync = this.sync;
            lock (sync)
            {
                Geometry ellipseGeometry = this.GetEllipseGeometry(bounds);
                if ((this.cachedFlattenedGeometry == null) || (this.cachedFlattenedGeometry.Geometry != ellipseGeometry))
                {
                    this.cachedFlattenedGeometry = new FlattenedGeometry { 
                        Geometry = ellipseGeometry,
                        FlatteningTolerance = 0.0001
                    }.EnsureFrozen<FlattenedGeometry>();
                }
                return this.cachedFlattenedGeometry;
            }
        }

        protected override Geometry OnCreateGuideGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.GetEllipseGeometry(bounds);

        protected override Geometry OnCreateInteriorFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.GetFlattenedEllipseGeometry(bounds);

        protected override Geometry OnCreateOutlineDrawGeometry(RectDouble bounds, IDictionary<string, object> settingValues) => 
            this.GetFlattenedEllipseGeometry(bounds);

        protected override Geometry OnCreateOutlineFillGeometry(RectDouble bounds, IDictionary<string, object> settingValues)
        {
            Geometry ellipseGeometry = this.GetEllipseGeometry(bounds);
            float num = (float) settingValues[ToolSettings.Null.Pen.Width.Path];
            System.Drawing.Drawing2D.DashStyle gdipDashStyle = (System.Drawing.Drawing2D.DashStyle) settingValues[ToolSettings.Null.Pen.DashStyle.Path];
            PaintDotNet.UI.Media.DashStyle style2 = DashStyleUtil.ToMedia(gdipDashStyle);
            StrokeStyle style3 = new StrokeStyle { DashStyle = style2 }.EnsureFrozen<StrokeStyle>();
            WidenedGeometry freezable = new WidenedGeometry {
                Geometry = ellipseGeometry,
                Thickness = num,
                StrokeStyle = style3,
                FlatteningTolerance = 0.0001
            };
            return freezable.EnsureFrozen<WidenedGeometry>();
        }
    }
}

