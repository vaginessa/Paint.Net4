namespace PaintDotNet.Canvas
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Threading;

    internal sealed class SelectionSnapshot
    {
        private RectDouble fastMaxBounds;
        private int geometryVersion;
        private Matrix3x2Double interimTransform;
        private bool isEmpty;
        private Result<bool> isPixelated;
        private Result<bool> isRectilinear;
        private Result<PaintDotNet.Rendering.GeometryList> lazyGeometryList;
        private Result<IReadOnlyList<RectInt32>> lazyPixelatedScans;

        public SelectionSnapshot(Result<PaintDotNet.Rendering.GeometryList> lazyGeometryList, Result<IReadOnlyList<RectInt32>> lazyPixelatedScans, Matrix3x2Double interimTransform, RectDouble fastMaxBounds, bool isEmpty, int geometryVersion)
        {
            Validate.Begin().IsNotNull<Result<PaintDotNet.Rendering.GeometryList>>(lazyGeometryList, "lazyGeometryList").IsNotNull<Result<IReadOnlyList<RectInt32>>>(lazyPixelatedScans, "lazyPixelatedScans").Check();
            this.lazyGeometryList = lazyGeometryList;
            this.lazyPixelatedScans = lazyPixelatedScans;
            this.interimTransform = interimTransform;
            this.fastMaxBounds = fastMaxBounds;
            this.isEmpty = isEmpty;
            this.geometryVersion = geometryVersion;
            this.isRectilinear = LazyResult.New<bool>(() => this.GeometryList.Value.IsRectilinear, LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
            this.isPixelated = LazyResult.New<bool>(() => this.GeometryList.Value.IsPixelated, LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
        }

        public RectDouble FastMaxBounds =>
            this.fastMaxBounds;

        public Result<PaintDotNet.Rendering.GeometryList> GeometryList =>
            this.lazyGeometryList;

        public int GeometryVersion =>
            this.geometryVersion;

        public Matrix3x2Double InterimTransform =>
            this.interimTransform;

        public bool IsEmpty =>
            this.isEmpty;

        public Result<bool> IsPixelated =>
            this.isPixelated;

        public Result<bool> IsRectilinear =>
            this.isRectilinear;

        public Result<IReadOnlyList<RectInt32>> PixelatedScans =>
            this.lazyPixelatedScans;
    }
}

