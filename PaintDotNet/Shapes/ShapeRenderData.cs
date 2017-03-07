namespace PaintDotNet.Shapes
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class ShapeRenderData
    {
        private readonly CachedGeometry guide;
        private readonly CachedGeometry interiorFill;
        private readonly CachedGeometry outlineDraw;
        private readonly CachedGeometry outlineFill;

        public ShapeRenderData(Geometry geometry)
        {
            Validate.IsNotNull<Geometry>(geometry, "geometry");
            CachedGeometry geometry3 = new CachedGeometry(geometry.ToFrozen<Geometry>());
            this.guide = geometry3;
            this.interiorFill = geometry3;
            this.outlineDraw = geometry3;
            this.outlineFill = null;
        }

        private ShapeRenderData(CachedGeometry guide, CachedGeometry interiorFill, CachedGeometry outlineDraw, CachedGeometry outlineFill)
        {
            Validate.IsNotNull<CachedGeometry>(guide, "guide");
            this.guide = guide;
            this.interiorFill = interiorFill;
            this.outlineDraw = outlineDraw;
            this.outlineFill = outlineFill;
        }

        public ShapeRenderData(Geometry guideGeometry, Geometry interiorFillGeometry, Geometry outlineDrawGeometry, Geometry outlineFillGeometry)
        {
            Validate.IsNotNull<Geometry>(guideGeometry, "guideGeometry");
            this.guide = new CachedGeometry(guideGeometry.ToFrozen<Geometry>());
            if (interiorFillGeometry == guideGeometry)
            {
                this.interiorFill = this.Guide;
            }
            else
            {
                this.interiorFill = (interiorFillGeometry == null) ? null : new CachedGeometry(interiorFillGeometry);
            }
            if (outlineDrawGeometry == guideGeometry)
            {
                this.outlineDraw = this.Guide;
            }
            else if (outlineDrawGeometry == interiorFillGeometry)
            {
                this.outlineDraw = this.InteriorFill;
            }
            else
            {
                this.outlineDraw = (outlineDrawGeometry == null) ? null : new CachedGeometry(outlineDrawGeometry);
            }
            if (outlineFillGeometry == guideGeometry)
            {
                this.outlineFill = this.Guide;
            }
            else if (outlineFillGeometry == interiorFillGeometry)
            {
                this.outlineFill = this.InteriorFill;
            }
            else if (outlineFillGeometry == outlineDrawGeometry)
            {
                this.outlineFill = this.OutlineDraw;
            }
            else
            {
                this.outlineFill = (outlineFillGeometry == null) ? null : new CachedGeometry(outlineFillGeometry);
            }
        }

        public static ShapeRenderData Transform(ShapeRenderData renderData, Matrix3x2Double matrix)
        {
            Validate.IsNotNull<ShapeRenderData>(renderData, "renderData");
            if (matrix.IsIdentity)
            {
                return renderData;
            }
            CachedGeometry transformed = renderData.Guide.GetTransformed(matrix);
            CachedGeometry interiorFill = null;
            if (renderData.InteriorFill != null)
            {
                if (renderData.InteriorFill.Equals(renderData.Guide))
                {
                    interiorFill = transformed;
                }
                else
                {
                    interiorFill = renderData.InteriorFill.GetTransformed(matrix);
                }
            }
            CachedGeometry outlineDraw = null;
            if (renderData.OutlineDraw != null)
            {
                if (renderData.OutlineDraw.Equals(renderData.Guide))
                {
                    outlineDraw = transformed;
                }
                else if (renderData.OutlineDraw.Equals(renderData.InteriorFill))
                {
                    outlineDraw = interiorFill;
                }
                else
                {
                    outlineDraw = renderData.OutlineDraw.GetTransformed(matrix);
                }
            }
            CachedGeometry outlineFill = null;
            if (renderData.OutlineFill != null)
            {
                if (renderData.OutlineFill.Equals(renderData.Guide))
                {
                    outlineFill = transformed;
                }
                else if (renderData.OutlineFill.Equals(renderData.InteriorFill))
                {
                    outlineFill = interiorFill;
                }
                else if (renderData.OutlineFill.Equals(renderData.OutlineDraw))
                {
                    outlineFill = outlineDraw;
                }
                else
                {
                    outlineFill = renderData.OutlineFill.GetTransformed(matrix);
                }
            }
            return new ShapeRenderData(transformed, interiorFill, outlineDraw, outlineFill);
        }

        public CachedGeometry Guide =>
            this.guide;

        public CachedGeometry InteriorFill =>
            this.interiorFill;

        public CachedGeometry OutlineDraw =>
            this.outlineDraw;

        public CachedGeometry OutlineFill =>
            this.outlineFill;
    }
}

