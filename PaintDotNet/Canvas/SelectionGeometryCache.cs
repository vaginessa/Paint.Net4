namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Threading;

    internal sealed class SelectionGeometryCache : Disposable
    {
        private IDirect2DFactory factory;
        private LazyResult<IGeometry> geometry;
        private LazyResult<IGeometry> pixelatedGeometry;
        private PaintDotNet.Canvas.SelectionSnapshot selectionSnapshot;

        public SelectionGeometryCache(IDirect2DFactory factory, PaintDotNet.Canvas.SelectionSnapshot selectionSnapshot)
        {
            this.factory = factory;
            this.selectionSnapshot = selectionSnapshot;
            this.geometry = LazyResult.New<IGeometry>(() => this.factory.CreateGeometry(selectionSnapshot.GeometryList.Value), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
            this.pixelatedGeometry = LazyResult.New<IGeometry>(() => GeometryHelpers.ToDirect2DGeometryDestructive(this.factory, ScansHelpers.ConvertNonOverlappingScansToPolygons(selectionSnapshot.PixelatedScans.Value), FillMode.Alternate, FigureBegin.Filled, FigureEnd.Closed), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
        }

        protected override void Dispose(bool disposing)
        {
            this.selectionSnapshot = null;
            if (disposing)
            {
                LazyResult<IGeometry> geometry = this.geometry;
                this.geometry = null;
                if ((geometry != null) && geometry.IsEvaluated)
                {
                    geometry.Value.Dispose();
                }
                LazyResult<IGeometry> pixelatedGeometry = this.pixelatedGeometry;
                this.pixelatedGeometry = null;
                if ((pixelatedGeometry != null) && pixelatedGeometry.IsEvaluated)
                {
                    pixelatedGeometry.Value.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public Result<IGeometry> Geometry =>
            this.geometry;

        public Result<IGeometry> PixelatedGeometry =>
            this.pixelatedGeometry;

        public PaintDotNet.Canvas.SelectionSnapshot SelectionSnapshot =>
            this.selectionSnapshot;
    }
}

