namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    internal sealed class MaskedSurface : ICloneable, IDisposable, IIsDisposed
    {
        private bool disposed;
        [NonSerialized]
        private ProtectedRegion drawRegion;
        private static readonly Func<ProtectedRegion> drawRegionValueFactory = new Func<ProtectedRegion>(<>c.<>9.<.cctor>b__42_0);
        private const double fp_MaxValue = 131071.0;
        private const double fp_MultFactor = 16384.0;
        private const int fp_RoundFactor = 0x1fff;
        private const int fp_ShiftFactor = 14;
        private GeometryList geometryMask;
        [NonSerialized]
        private Lazy<ReadOnlyCollection<RectInt32>> lazyCachedGeometryMaskScans;
        [NonSerialized]
        private Lazy<RectInt32> lazyCachedGeometryMaskScansBounds;
        private PaintDotNet.Surface surface;

        private MaskedSurface()
        {
        }

        public MaskedSurface(ref PaintDotNet.Surface source, bool takeOwnership)
        {
            if (takeOwnership)
            {
                this.surface = source;
                source = null;
            }
            else
            {
                this.surface = source.Clone();
            }
            this.geometryMask = new GeometryList(this.surface.Bounds<ColorBgra>());
            this.OnConstructed();
        }

        private MaskedSurface(SizeInt32 maxSize, IRenderer<ColorBgra> source, GeometryList geometryMask)
        {
            RectInt32 num5;
            RectInt32 b = new RectInt32(PointInt32.Zero, maxSize);
            RectInt32 a = geometryMask.Bounds.Int32Bound;
            RectInt32 num4 = RectInt32.Intersect(a, b);
            if (a != num4)
            {
                GeometryList list = GeometryList.ClipToRect(geometryMask, b);
                this.geometryMask = list;
                num5 = this.geometryMask.Bounds.Int32Bound;
            }
            else
            {
                this.geometryMask = geometryMask.Clone();
                num5 = num4;
            }
            if (!num5.HasZeroArea)
            {
                this.surface = new PaintDotNet.Surface(num5.Size);
                if (source != null)
                {
                    source.Parallelize<ColorBgra>(TilingStrategy.Tiles, 7, WorkItemQueuePriority.Normal).Render(this.surface, num5.Location);
                }
            }
            this.OnConstructed();
        }

        public MaskedSurface(ref PaintDotNet.Surface source, bool takeOwnershipOfSurface, ref GeometryList geometryMaskAndOffset, bool takeOwnershipOfGMAO)
        {
            if (takeOwnershipOfSurface)
            {
                this.surface = source;
                source = null;
            }
            else
            {
                this.surface = source.Clone();
            }
            if (takeOwnershipOfGMAO)
            {
                this.geometryMask = geometryMaskAndOffset;
                geometryMaskAndOffset = null;
            }
            else
            {
                this.geometryMask = geometryMaskAndOffset.CloneT<GeometryList>();
            }
            this.OnConstructed();
        }

        public MaskedSurface Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            MaskedSurface surface = new MaskedSurface {
                geometryMask = this.geometryMask.Clone()
            };
            if (this.surface != null)
            {
                surface.surface = this.surface.Clone();
            }
            surface.OnConstructed();
            return surface;
        }

        private ReadOnlyCollection<RectInt32> ComputeCachedGeometryMaskScans() => 
            new ReadOnlyCollection<RectInt32>(this.geometryMask.GetInteriorScans<SegmentedList<RectInt32>>());

        private RectInt32 ComputeCachedGeometryMaskScansBounds() => 
            ((IList<RectInt32>) this.GetCachedGeometryMaskScans()).Bounds();

        public static MaskedSurface CopyFrom(IRenderer<ColorBgra> source, GeometryList geometryMask) => 
            new MaskedSurface(source.Size<ColorBgra>(), source, geometryMask);

        public static MaskedSurface Create(SizeInt32 maxSize, GeometryList geometryMask) => 
            new MaskedSurface(maxSize, null, geometryMask);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.surface = null;
            this.geometryMask = null;
            this.disposed = true;
        }

        public void Draw(PaintDotNet.Surface dst)
        {
            this.Draw(dst, 0, 0);
        }

        public void Draw(PaintDotNet.Surface dst, Matrix3x2Double transform, ResamplingAlgorithm sampling)
        {
            Action<int> action;
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            if ((this.surface == null) || !transform.HasInverse)
            {
                return;
            }
            if ((((sampling == ResamplingAlgorithm.Bilinear) && (transform.M11 == 1.0)) && ((transform.M12 == 0.0) && (transform.M21 == 0.0))) && (((transform.M22 == 1.0) && transform.OffsetX.IsInteger()) && transform.OffsetY.IsInteger()))
            {
                this.Draw(dst, transform, ResamplingAlgorithm.NearestNeighbor);
                return;
            }
            RectInt32 num = this.geometryMask.Bounds.Int32Bound;
            RectInt32[] interiorScans = GeometryList.Transform(this.geometryMask, transform).GetInteriorScans();
            DrawContext context = new DrawContext {
                boundsX = num.X,
                boundsY = num.Y,
                inverse = transform
            };
            context.inverse = context.inverse.Inverse;
            VectorDouble[] vectors = new VectorDouble[] { new VectorDouble(1.0, 0.0), new VectorDouble(0.0, 1.0) };
            context.inverse.Transform(vectors);
            context.dsxddx = vectors[0].X;
            if (Math.Abs(context.dsxddx) > 131071.0)
            {
                context.dsxddx = 0.0;
            }
            context.dsyddx = vectors[0].Y;
            if (Math.Abs(context.dsyddx) > 131071.0)
            {
                context.dsyddx = 0.0;
            }
            context.dsxddy = vectors[1].X;
            if (Math.Abs(context.dsxddy) > 131071.0)
            {
                context.dsxddy = 0.0;
            }
            context.dsyddy = vectors[1].Y;
            if (Math.Abs(context.dsyddy) > 131071.0)
            {
                context.dsyddy = 0.0;
            }
            context.fp_dsxddx = (int) (context.dsxddx * 16384.0);
            context.fp_dsyddx = (int) (context.dsyddx * 16384.0);
            context.fp_dsxddy = (int) (context.dsxddy * 16384.0);
            context.fp_dsyddy = (int) (context.dsyddy * 16384.0);
            context.dst = dst;
            context.src = this.surface;
            if (interiorScans.Length == 1)
            {
                context.dstScans = new RectInt32[Processor.LogicalCpuCount * 4];
                RectInt32Util.Split(interiorScans[0], context.dstScans);
            }
            else
            {
                context.dstScans = interiorScans;
            }
            if (sampling != ResamplingAlgorithm.NearestNeighbor)
            {
                if (sampling != ResamplingAlgorithm.Bilinear)
                {
                    throw ExceptionUtil.InvalidEnumArgumentException<ResamplingAlgorithm>(sampling, "sampling");
                }
            }
            else
            {
                action = new Action<int>(context.DrawScansNearestNeighbor);
                goto Label_0300;
            }
            action = new Action<int>(context.DrawScansBilinear);
        Label_0300:
            LazyInitializer.EnsureInitialized<ProtectedRegion>(ref this.drawRegion, drawRegionValueFactory);
            using (this.drawRegion.UseEnterScope())
            {
                Work.ParallelFor(WaitType.Blocking, 0, Environment.ProcessorCount, action, WorkItemQueuePriority.High, null);
            }
            context.src = null;
        }

        public void Draw(PaintDotNet.Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            Matrix3x2Double transform = Matrix3x2Double.Translation((double) tX, (double) tY);
            this.Draw(dst, transform, ResamplingAlgorithm.Bilinear);
        }

        public ReadOnlyCollection<RectInt32> GetCachedGeometryMaskScans()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.lazyCachedGeometryMaskScans.Value;
        }

        public RectInt32 GetCachedGeometryMaskScansBounds()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.lazyCachedGeometryMaskScansBounds.Value;
        }

        public GeometryList GetGeometryMaskCopy()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.geometryMask.Clone();
        }

        private void OnConstructed()
        {
            this.lazyCachedGeometryMaskScans = new Lazy<ReadOnlyCollection<RectInt32>>(new Func<ReadOnlyCollection<RectInt32>>(this.ComputeCachedGeometryMaskScans), LazyThreadSafetyMode.ExecutionAndPublication);
            this.lazyCachedGeometryMaskScansBounds = new Lazy<RectInt32>(new Func<RectInt32>(this.ComputeCachedGeometryMaskScansBounds), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.OnConstructed();
        }

        object ICloneable.Clone() => 
            this.Clone();

        public GeometryList GeometryMask =>
            this.geometryMask.AsReadOnly();

        public RectDouble GeometryMaskBounds =>
            this.geometryMask.Bounds;

        public bool IsDisposed =>
            this.disposed;

        internal PaintDotNet.Surface Surface =>
            this.surface;

        public PaintDotNet.Surface SurfaceReadOnly =>
            this.surface;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MaskedSurface.<>c <>9 = new MaskedSurface.<>c();

            internal ProtectedRegion <.cctor>b__42_0() => 
                new ProtectedRegion("Draw", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        }

        private class DrawContext
        {
            public int boundsX;
            public int boundsY;
            public Surface dst;
            public RectInt32[] dstScans;
            public double dsxddx;
            public double dsxddy;
            public double dsyddx;
            public double dsyddy;
            public int fp_dsxddx;
            public int fp_dsxddy;
            public int fp_dsyddx;
            public int fp_dsyddy;
            public Matrix3x2Double inverse;
            public Surface src;

            public void DrawScansBilinear(int cpuNumber)
            {
                int logicalCpuCount = Processor.LogicalCpuCount;
                RectInt32 b = this.dst.Bounds<ColorBgra>();
                for (int i = cpuNumber; i < this.dstScans.Length; i += logicalCpuCount)
                {
                    RectInt32 num4 = RectInt32.Intersect(this.dstScans[i], b);
                    PointDouble pt = new PointDouble((double) num4.X, (double) num4.Y);
                    pt.X += 0.5;
                    pt.Y += 0.5;
                    pt = this.inverse.Transform(pt);
                    pt.X -= this.boundsX;
                    pt.Y -= this.boundsY;
                    pt.X -= 0.5;
                    pt.Y -= 0.5;
                    PointDouble num6 = pt;
                    for (int j = num4.Y; j < (num4.Y + num4.Height); j++)
                    {
                        PointDouble num8 = num6;
                        if (j >= 0)
                        {
                            for (int k = num4.X; k < (num4.X + num4.Width); k++)
                            {
                                float x = (float) num8.X;
                                float y = (float) num8.Y;
                                ColorBgra bilinearSampleClamped = this.src.GetBilinearSampleClamped(x, y);
                                *(this.dst.GetPointAddressUnchecked(k, j)) = bilinearSampleClamped;
                                num8.X += this.dsxddx;
                                num8.Y += this.dsyddx;
                            }
                        }
                        num6.X += this.dsxddy;
                        num6.Y += this.dsyddy;
                    }
                }
            }

            public unsafe void DrawScansNearestNeighbor(int cpuNumber)
            {
                int logicalCpuCount = Processor.LogicalCpuCount;
                void* voidStar = this.src.Scan0.VoidStar;
                int stride = this.src.Stride;
                RectInt32 b = this.dst.Bounds<ColorBgra>();
                for (int i = cpuNumber; i < this.dstScans.Length; i += logicalCpuCount)
                {
                    RectInt32 num5 = RectInt32.Intersect(this.dstScans[i], b);
                    if ((num5.Width != 0) && (num5.Height != 0))
                    {
                        PointDouble pt = new PointDouble((double) num5.X, (double) num5.Y);
                        pt.X += 0.5;
                        pt.Y += 0.5;
                        pt = this.inverse.Transform(pt);
                        pt.X -= this.boundsX;
                        pt.Y -= this.boundsY;
                        pt.X -= 0.5;
                        pt.Y -= 0.5;
                        int num7 = (int) (pt.X * 16384.0);
                        int num8 = (int) (pt.Y * 16384.0);
                        for (int j = num5.Y; j < (num5.Y + num5.Height); j++)
                        {
                            int num10 = num7;
                            int num11 = num8;
                            num7 += this.fp_dsxddy;
                            num8 += this.fp_dsyddy;
                            if (j >= 0)
                            {
                                int x = num5.X;
                                ColorBgra* pointAddress = this.dst.GetPointAddress(x, j);
                                ColorBgra* bgraPtr2 = pointAddress + num5.Width;
                                int num13 = num10 + (this.fp_dsxddx * (num5.Width - 1));
                                int num14 = num11 + (this.fp_dsyddx * (num5.Width - 1));
                                while (pointAddress < bgraPtr2)
                                {
                                    int num15 = (num10 + 0x1fff) >> 14;
                                    int num16 = (num11 + 0x1fff) >> 14;
                                    int num17 = Int32Util.Clamp(num15, 0, this.src.Width - 1);
                                    int y = Int32Util.Clamp(num16, 0, this.src.Height - 1);
                                    pointAddress[0] = this.src.GetPointUnchecked(num17, y);
                                    pointAddress++;
                                    num10 += this.fp_dsxddx;
                                    num11 += this.fp_dsyddx;
                                    if ((num17 == num15) && (y == num16))
                                    {
                                        break;
                                    }
                                }
                                ColorBgra* bgraPtr3 = pointAddress;
                                pointAddress = bgraPtr2 - 1;
                                while (pointAddress >= bgraPtr3)
                                {
                                    int num19 = (num13 + 0x1fff) >> 14;
                                    int num20 = (num14 + 0x1fff) >> 14;
                                    int num21 = Int32Util.Clamp(num19, 0, this.src.Width - 1);
                                    int num22 = Int32Util.Clamp(num20, 0, this.src.Height - 1);
                                    pointAddress[0] = this.src.GetPointUnchecked(num21, num22);
                                    if ((num21 == num19) && (num22 == num20))
                                    {
                                        break;
                                    }
                                    pointAddress--;
                                    num13 -= this.fp_dsxddx;
                                    num14 -= this.fp_dsyddx;
                                }
                                ColorBgra* bgraPtr4 = pointAddress;
                                while (bgraPtr3 < bgraPtr4)
                                {
                                    int num23 = (num10 + 0x1fff) >> 14;
                                    int num24 = (num11 + 0x1fff) >> 14;
                                    bgraPtr3->Bgra = (((IntPtr) (num23 * sizeof(ColorBgra))) + (voidStar + (num24 * stride))).Bgra;
                                    bgraPtr3++;
                                    num10 += this.fp_dsxddx;
                                    num11 += this.fp_dsyddx;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

