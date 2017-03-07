namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Functional;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class CachedGeometry : IEquatable<CachedGeometry>
    {
        private volatile bool areCachesInitialized;
        private CachedGeometry basis;
        private Matrix3x2Double? basisRelativeTx;
        private BasisUsage basisUsage;
        private readonly PaintDotNet.UI.Media.Geometry geometry;
        private int isDrawPrepared;
        private int isFillPrepared;
        private LazyResult<PathGeometry> lazyFlattenedPathGeometry;
        private LazyResult<IList<PointDouble[]>> lazyFlattenedPolyPoly;
        private LazyResult<RectDouble> lazyGeometryBounds;
        private LazyResult<GeometrySource> lazyGeometrySource;
        private ThreadLocal<IGeometry> perThreadD2DGeometry;
        private readonly object sync = new object();

        public CachedGeometry(PaintDotNet.UI.Media.Geometry geometry)
        {
            this.geometry = Validate.IsNotNull<PaintDotNet.UI.Media.Geometry>(geometry, "geometry").ToFrozen<PaintDotNet.UI.Media.Geometry>();
        }

        private RectDouble ComputeBounds()
        {
            RectDouble? nullable = this.geometry.TryGetCachedBounds();
            if (nullable.HasValue)
            {
                return nullable.GetValueOrDefault();
            }
            if (this.geometry.MayHaveHollowSegments())
            {
                return this.GetPerThreadDirect2DGeometry().GetWidenedBounds(0f, null, null, null);
            }
            return this.GetPerThreadDirect2DGeometry().GetBounds(null);
        }

        public void Draw(IDrawingContext dc, IBrush brush, float strokeWidth, IStrokeStyle strokeStyle)
        {
            if ((this.basis != null) && ((this.basisUsage & BasisUsage.DrawWithRelativeTransform) == BasisUsage.DrawWithRelativeTransform))
            {
                using (dc.UseTransformMultiply((Matrix3x2Float) this.basisRelativeTx.Value, MatrixMultiplyOrder.Prepend))
                {
                    this.basis.Draw(dc, brush, strokeWidth, strokeStyle);
                    return;
                }
            }
            dc.DrawGeometry(this.GetPerThreadDirect2DGeometry(), brush, strokeWidth, strokeStyle);
        }

        private void EnsureCachesInitialized()
        {
            if (!this.areCachesInitialized)
            {
                object sync = this.sync;
                lock (sync)
                {
                    if (!this.areCachesInitialized)
                    {
                        this.lazyFlattenedPathGeometry = this.lazyFlattenedPathGeometry ?? LazyResult.New<PathGeometry>(() => this.geometry.GetFlattenedPathGeometry().EnsureFrozen<PathGeometry>(), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                        this.lazyFlattenedPolyPoly = this.lazyFlattenedPolyPoly ?? LazyResult.New<IList<PointDouble[]>>(() => (from p in PathGeometryUtil.EnumeratePathGeometryPolygons(this.lazyFlattenedPathGeometry.Value) select p.ToArrayEx<PointDouble>()).ToArrayEx<PointDouble[]>(), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                        this.lazyGeometrySource = this.lazyGeometrySource ?? LazyResult.New<GeometrySource>(() => GeometrySource.Create(this.geometry), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                        this.perThreadD2DGeometry = this.perThreadD2DGeometry ?? new ThreadLocal<IGeometry>(() => this.lazyGeometrySource.Value.ToDirect2DGeometry(Direct2DFactory.PerThread));
                        this.lazyGeometryBounds = this.lazyGeometryBounds ?? LazyResult.New<RectDouble>(new Func<RectDouble>(this.ComputeBounds), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());
                        this.areCachesInitialized = true;
                    }
                }
            }
        }

        public bool EnsureDrawPrepared()
        {
            if (Interlocked.Increment(ref this.isDrawPrepared) != 1)
            {
                return false;
            }
            if ((this.basis != null) && ((this.basisUsage & BasisUsage.DrawWithRelativeTransform) == BasisUsage.DrawWithRelativeTransform))
            {
                return this.basis.EnsureDrawPrepared();
            }
            this.EnsureCachesInitialized();
            return this.lazyGeometrySource.EnsureEvaluated();
        }

        public bool EnsureFillPrepared()
        {
            if (Interlocked.Increment(ref this.isFillPrepared) != 1)
            {
                return false;
            }
            if ((this.basis != null) && ((this.basisUsage & BasisUsage.FillWithRelativeTransform) == BasisUsage.FillWithRelativeTransform))
            {
                return this.basis.EnsureFillPrepared();
            }
            this.EnsureCachesInitialized();
            return this.lazyGeometrySource.EnsureEvaluated();
        }

        public bool Equals(CachedGeometry other)
        {
            if (other == null)
            {
                return false;
            }
            return ((this == other) || (this.geometry == other.geometry));
        }

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<CachedGeometry, object>(this, obj);

        public void Fill(IDrawingContext dc, IBrush brush)
        {
            if ((this.basis != null) && ((this.basisUsage & BasisUsage.FillWithRelativeTransform) == BasisUsage.FillWithRelativeTransform))
            {
                using (dc.UseTransformMultiply((Matrix3x2Float) this.basisRelativeTx.Value, MatrixMultiplyOrder.Prepend))
                {
                    this.basis.Fill(dc, brush);
                    return;
                }
            }
            dc.FillGeometry(this.GetPerThreadDirect2DGeometry(), brush, null);
        }

        public IList<PointDouble[]> GetFlattenedPolyPoly()
        {
            this.EnsureCachesInitialized();
            return this.lazyFlattenedPolyPoly.Value;
        }

        public override int GetHashCode() => 
            this.geometry.GetHashCode();

        public float GetLength() => 
            this.GetPerThreadDirect2DGeometry().ComputeLength(null, null);

        public IGeometry GetPerThreadDirect2DGeometry()
        {
            this.EnsureCachesInitialized();
            return this.perThreadD2DGeometry.Value;
        }

        public PointAndTangentFloat GetPointAtLength(double length) => 
            this.GetPerThreadDirect2DGeometry().ComputePointAtLength((float) length, null, null);

        public RectFloat GetRenderBounds(float strokeWidth, IStrokeStyleSource strokeStyle)
        {
            using (IStrokeStyle style = (strokeStyle == null) ? null : strokeStyle.CreateResource<IStrokeStyle>())
            {
                return this.GetPerThreadDirect2DGeometry().GetWidenedBounds(strokeWidth, style, null, null);
            }
        }

        public CachedGeometry GetTransformed(Matrix3x2Double matrix)
        {
            if (matrix.IsIdentity)
            {
                return this;
            }
            bool flag = matrix.IsScaling();
            if (this.basis == null)
            {
                CachedGeometry geometry = new CachedGeometry(this.geometry.GetTransformedGeometry(matrix)) {
                    basis = this
                };
                if (flag)
                {
                    geometry.basisUsage = BasisUsage.FillWithRelativeTransform;
                }
                else
                {
                    geometry.basisUsage = BasisUsage.FillWithRelativeTransform | BasisUsage.DrawWithRelativeTransform;
                }
                geometry.basisRelativeTx = new Matrix3x2Double?(matrix);
                return geometry;
            }
            if (!flag && this.basisRelativeTx.Value.IsScaling())
            {
                return new CachedGeometry(this.geometry.GetTransformedGeometry(matrix)) { 
                    basis = this,
                    basisUsage = BasisUsage.FillWithRelativeTransform | BasisUsage.DrawWithRelativeTransform,
                    basisRelativeTx = new Matrix3x2Double?(matrix)
                };
            }
            return this.basis.GetTransformed(this.basisRelativeTx.Value * matrix);
        }

        public RectDouble Bounds
        {
            get
            {
                this.EnsureCachesInitialized();
                return this.lazyGeometryBounds.Value;
            }
        }

        public RectDouble FastMaxBounds
        {
            get
            {
                if (this.basis == null)
                {
                    return this.Bounds;
                }
                RectDouble bounds = this.basis.Bounds;
                return this.basisRelativeTx.Value.Transform(bounds);
            }
        }

        public PaintDotNet.UI.Media.Geometry Geometry =>
            this.geometry;

        public bool IsEmpty =>
            this.geometry.IsEmpty;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly CachedGeometry.<>c <>9 = new CachedGeometry.<>c();
            public static Func<IEnumerable<PointDouble>, PointDouble[]> <>9__37_2;

            internal PointDouble[] <EnsureCachesInitialized>b__37_2(IEnumerable<PointDouble> p) => 
                p.ToArrayEx<PointDouble>();
        }

        [Flags]
        private enum BasisUsage
        {
            DrawWithRelativeTransform = 1,
            FillWithRelativeTransform = 2
        }
    }
}

