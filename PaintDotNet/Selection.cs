namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.Imaging;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class Selection : ThreadAffinitizedObjectBase
    {
        private int alreadyChanging = 0;
        private WeakCache<Matrix3x2Double, GeometryList> cachedClippingMask;
        private WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>> cachedClippingMaskBiLevelCoverageScans;
        private WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IRenderer<ColorAlpha8>> cachedClippingMaskRenderers;
        private WeakCache<Matrix3x2Double, IReadOnlyList<RectInt32>> cachedClippingMaskScans;
        private WeakCache<Matrix3x2Double, GeometryList> cachedGeometryList;
        private WeakCache<Matrix3x2Double, IReadOnlyList<RectInt32>> cachedGeometryListScans;
        private WeakCache<Matrix3x2Double, Result<GeometryList>> cachedLazyClippingMask;
        private WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IRenderer<ColorAlpha8>>> cachedLazyClippingMaskRenderers;
        private WeakCache<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>> cachedLazyClippingMaskScans;
        private WeakCache<Matrix3x2Double, Result<GeometryList>> cachedLazyGeometryList;
        private WeakCache<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>> cachedLazyGeometryListScans;
        private WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IReadOnlyList<RectInt32>>> cachedLazyScaledClippingMaskScans;
        private WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>> cachedScaledClippingMaskScans;
        private SelectionChangeFlags changes = SelectionChangeFlags.All;
        private RectInt32 clipRectangle = new RectInt32(0, 0, 0xffff, 0xffff);
        private readonly ProtectedRegion commitInterimTransformRegion = new ProtectedRegion("CommitInterimTransform", ProtectedRegionOptions.ErrorOnPerThreadReentrancy | ProtectedRegionOptions.DisablePumpingWhenEntered);
        private SelectionData data = new SelectionData();

        [field: CompilerGenerated]
        public event EventHandler<SelectionChangedEventArgs> Changed;

        [field: CompilerGenerated]
        public event EventHandler Changing;

        public Selection()
        {
            this.ResetCaches();
        }

        private void ClearBaseGeometry()
        {
            base.VerifyAccess();
            if (!this.data.BaseGeometry.IsEmpty)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.BaseGeometry);
                    this.data.BaseGeometry = new GeometryList();
                    this.data.BaseGeometry.Freeze();
                }
            }
        }

        private void ClearContinuationGeometry()
        {
            base.VerifyAccess();
            if (!this.data.ContinuationGeometry.IsEmpty)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.ContinuationGeometry);
                    this.data.ContinuationGeometry = new GeometryList();
                    this.data.ContinuationGeometry.Freeze();
                }
            }
        }

        public void CommitContinuation()
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.ReportChanging(SelectionChangeFlags.BaseGeometry);
                GeometryList cachedGeometryList = this.GetCachedGeometryList(this.GetInterimTransform());
                this.data.BaseGeometry = cachedGeometryList;
                this.ClearContinuationGeometry();
                this.SetContinuationCombineMode(SelectionCombineMode.Xor);
            }
        }

        public void CommitInterimTransform()
        {
            base.VerifyAccess();
            if (!this.data.InterimTransform.IsIdentity)
            {
                using (this.commitInterimTransformRegion.UseEnterScope())
                {
                    using (this.UseChangeScope())
                    {
                        this.ReportChanging(SelectionChangeFlags.BaseGeometry);
                        this.ReportChanging(SelectionChangeFlags.ContinuationGeometry);
                        Task<GeometryList> task = Task.Factory.StartNew<GeometryList>(delegate {
                            GeometryList list = this.data.BaseGeometry.Clone();
                            list.Transform(this.data.InterimTransform);
                            list.Freeze();
                            return list;
                        }, TaskCreationOptions.LongRunning);
                        Task<GeometryList> task2 = Task.Factory.StartNew<GeometryList>(delegate {
                            GeometryList list = this.data.ContinuationGeometry.Clone();
                            list.Transform(this.data.InterimTransform);
                            list.Freeze();
                            return list;
                        }, TaskCreationOptions.LongRunning);
                        GeometryList result = task.Result;
                        GeometryList list2 = task2.Result;
                        this.data.BaseGeometry = result;
                        this.data.ContinuationGeometry = list2;
                        Matrix3x2Double m = Matrix3x2Double.Multiply(this.data.CumulativeTransform, this.data.InterimTransform);
                        this.SetCumulativeTransform(m);
                        this.ResetInterimTransform();
                    }
                }
            }
        }

        public GeometryList CreateClippingMask() => 
            this.CreateClippingMask(true);

        public GeometryList CreateClippingMask(Matrix3x2Double transform) => 
            this.CreateClippingMask(transform, false);

        public GeometryList CreateClippingMask(bool applyInterimTransform) => 
            this.CreateClippingMask(this.GetTransform(applyInterimTransform));

        private GeometryList CreateClippingMask(Matrix3x2Double transform, bool? frozenResult) => 
            this.CreateLazyClippingMask(transform, frozenResult).Value;

        public IReadOnlyList<RectInt32> CreateClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality) => 
            this.CreateClippingMaskBiLevelCoverageScans(quality, true);

        private IReadOnlyList<RectInt32> CreateClippingMaskBiLevelCoverageScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.CreateClippingMaskBiLevelCoverageScans(context.Item1, context.Item2);

        public IReadOnlyList<RectInt32> CreateClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality, Matrix3x2Double transform)
        {
            if (this.IsEmpty)
            {
                return new RectInt32[] { this.ClipRectangle };
            }
            int sampleFactor = GetSampleFactor(quality);
            if ((sampleFactor < 1) || (sampleFactor > 3))
            {
                ExceptionUtil.ThrowInternalErrorException("sampleFactor is not in the range [1, 3]");
            }
            if (sampleFactor == 1)
            {
                return this.GetCachedClippingMaskScans(transform);
            }
            GeometryList cachedClippingMask = this.GetCachedClippingMask(transform);
            IReadOnlyList<RectInt32> cachedScaledClippingMaskScans = this.GetCachedScaledClippingMaskScans(quality, transform);
            UnsafeList<RectInt32> list = cachedScaledClippingMaskScans as UnsafeList<RectInt32>;
            if (list != null)
            {
                using (FastUncheckedRectInt32ArrayWrapper wrapper = new FastUncheckedRectInt32ArrayWrapper(list))
                {
                    UnsafeList<RectInt32> list5 = new UnsafeList<RectInt32>();
                    GetCoverageScansFromScaledScans<FastUncheckedRectInt32ArrayWrapper, UnsafeListStruct<RectInt32>>(wrapper, sampleFactor, list5.AsStruct<RectInt32>());
                    return list5;
                }
            }
            SegmentedList<RectInt32> source = cachedScaledClippingMaskScans as SegmentedList<RectInt32>;
            if (source != null)
            {
                UncheckedSegmentedListStruct<RectInt32> scaledScans = new UncheckedSegmentedListStruct<RectInt32>(source);
                SegmentedList<RectInt32> list7 = new SegmentedList<RectInt32>();
                GetCoverageScansFromScaledScans<UncheckedSegmentedListStruct<RectInt32>, SegmentedListStruct<RectInt32>>(scaledScans, sampleFactor, list7.AsStruct<RectInt32>());
                return list7;
            }
            if (Memory.AreSmallAllocationsPreferred)
            {
                SegmentedList<RectInt32> list8 = new SegmentedList<RectInt32>();
                GetCoverageScansFromScaledScans<IReadOnlyList<RectInt32>, SegmentedListStruct<RectInt32>>(cachedScaledClippingMaskScans, sampleFactor, list8.AsStruct<RectInt32>());
                return list8;
            }
            UnsafeList<RectInt32> list9 = new UnsafeList<RectInt32>();
            GetCoverageScansFromScaledScans<IReadOnlyList<RectInt32>, UnsafeListStruct<RectInt32>>(cachedScaledClippingMaskScans, sampleFactor, list9.AsStruct<RectInt32>());
            return list9;
        }

        public IReadOnlyList<RectInt32> CreateClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.CreateClippingMaskBiLevelCoverageScans(quality, this.GetTransform(applyInterimTransform));

        public IRenderer<ColorAlpha8> CreateClippingMaskRenderer(SelectionRenderingQuality quality) => 
            this.CreateClippingMaskRenderer(quality, true);

        private IRenderer<ColorAlpha8> CreateClippingMaskRenderer(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.CreateClippingMaskRenderer(context.Item1, context.Item2);

        public IRenderer<ColorAlpha8> CreateClippingMaskRenderer(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.CreateLazyClippingMaskRenderer(quality, transform).Value;

        public IRenderer<ColorAlpha8> CreateClippingMaskRenderer(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.CreateClippingMaskRenderer(quality, this.GetTransform(applyInterimTransform));

        public IReadOnlyList<RectInt32> CreateClippingMaskScans() => 
            this.CreateClippingMaskScans(true);

        public IReadOnlyList<RectInt32> CreateClippingMaskScans(Matrix3x2Double transform) => 
            this.CreateLazyClippingMaskScans(transform).Value;

        public IReadOnlyList<RectInt32> CreateClippingMaskScans(bool applyInterimTransform) => 
            this.CreateClippingMaskScans(this.GetTransform(applyInterimTransform));

        public GeometryList CreateGeometryList() => 
            this.CreateGeometryList(true);

        public GeometryList CreateGeometryList(Matrix3x2Double transform) => 
            this.CreateGeometryList(transform, false);

        public GeometryList CreateGeometryList(bool applyInterimTransform) => 
            this.CreateGeometryList(this.GetTransform(applyInterimTransform));

        private GeometryList CreateGeometryList(Matrix3x2Double transform, bool? frozenResult) => 
            this.CreateLazyGeometryList(transform, frozenResult).Value;

        public IReadOnlyList<RectInt32> CreateGeometryListScans() => 
            this.CreateGeometryListScans(true);

        public IReadOnlyList<RectInt32> CreateGeometryListScans(Matrix3x2Double transform) => 
            this.CreateLazyGeometryListScans(transform).Value;

        public IReadOnlyList<RectInt32> CreateGeometryListScans(bool applyInterimTransform) => 
            this.CreateGeometryListScans(this.GetTransform(applyInterimTransform));

        public Result<GeometryList> CreateLazyClippingMask() => 
            this.CreateLazyClippingMask(true);

        public Result<GeometryList> CreateLazyClippingMask(Matrix3x2Double transform) => 
            this.CreateLazyClippingMask(transform, false);

        public Result<GeometryList> CreateLazyClippingMask(bool applyInterimTransform) => 
            this.CreateLazyClippingMask(this.GetTransform(applyInterimTransform));

        private Result<GeometryList> CreateLazyClippingMask(Matrix3x2Double transform, bool? frozenResult)
        {
            base.VerifyAccess();
            bool isEmpty = this.IsEmpty;
            RectInt32 clipRect = this.ClipRectangle;
            Result<GeometryList> lazyGeometry = this.GetCachedLazyGeometryList(transform);
            Func<GeometryList> valueFactory = delegate {
                GeometryList list;
                if (isEmpty)
                {
                    list = new GeometryList();
                    list.AddRect(clipRect);
                }
                else
                {
                    GeometryList geometry = lazyGeometry.Value;
                    if (clipRect.Contains(geometry.Bounds))
                    {
                        list = geometry;
                    }
                    else
                    {
                        list = GeometryList.ClipToRect(geometry, clipRect);
                    }
                }
                if (!frozenResult.HasValue)
                {
                    return list;
                }
                if (!frozenResult.Value)
                {
                    return list.EnsureUnfrozen();
                }
                return list.EnsureFrozen();
            };
            return CreateLazyResult<GeometryList>(valueFactory);
        }

        private Result<IRenderer<ColorAlpha8>> CreateLazyClippingMaskRenderer(SelectionRenderingQuality quality) => 
            this.CreateLazyClippingMaskRenderer(quality, true);

        private Result<IRenderer<ColorAlpha8>> CreateLazyClippingMaskRenderer(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.CreateLazyClippingMaskRenderer(context.Item1, context.Item2);

        private Result<IRenderer<ColorAlpha8>> CreateLazyClippingMaskRenderer(SelectionRenderingQuality quality, Matrix3x2Double transform)
        {
            if (this.IsEmpty)
            {
                IRenderer<ColorAlpha8> renderer = new FillRendererAlpha8(this.clipRectangle.Width, this.clipRectangle.Height, ColorAlpha8.Opaque);
                return CreateLazyResult<IRenderer<ColorAlpha8>>(renderer);
            }
            RectInt32 clipRectangle = this.ClipRectangle;
            Result<GeometryList> lazyClipGeometry = this.GetCachedLazyClippingMask(transform);
            Result<IReadOnlyList<RectInt32>> lazyNormalScans = this.GetCachedLazyScaledClippingMaskScans(quality, transform);
            Result<IReadOnlyList<RectInt32>> lazyAliasedScans = this.GetCachedLazyScaledClippingMaskScans(SelectionRenderingQuality.Aliased, transform);
            Func<IRenderer<ColorAlpha8>> valueFactory = delegate {
                Result<IReadOnlyList<RectInt32>> result1;
                SelectionRenderingQuality aliased;
                IRenderer<ColorAlpha8> renderer;
                IRenderer<ColorAlpha8> renderer2;
                GeometryList list = lazyClipGeometry.Value;
                if (list.IsEmpty || list.IsPixelated)
                {
                    result1 = lazyAliasedScans;
                    aliased = SelectionRenderingQuality.Aliased;
                }
                else
                {
                    result1 = lazyNormalScans;
                    aliased = quality;
                }
                int scaleX = GetSampleFactor(aliased);
                RectInt32 bounds = RectInt32.Scale(clipRectangle, scaleX, scaleX);
                IReadOnlyList<RectInt32> sortedScans = result1.Value;
                UnsafeList<RectInt32> list3 = sortedScans as UnsafeList<RectInt32>;
                if (list3 != null)
                {
                    FastUncheckedRectInt32ArrayWrapper wrapper = new FastUncheckedRectInt32ArrayWrapper(list3);
                    renderer = new MaskFromScansRenderer<FastUncheckedRectInt32ArrayWrapper>(wrapper, bounds);
                }
                else
                {
                    SegmentedList<RectInt32> source = sortedScans as SegmentedList<RectInt32>;
                    if (source != null)
                    {
                        UncheckedSegmentedListStruct<RectInt32> struct2 = new UncheckedSegmentedListStruct<RectInt32>(source);
                        renderer = new MaskFromScansRenderer<UncheckedSegmentedListStruct<RectInt32>>(struct2, bounds);
                    }
                    else
                    {
                        renderer = new MaskFromScansRenderer<IReadOnlyList<RectInt32>>(sortedScans, bounds);
                    }
                }
                switch (scaleX)
                {
                    case 1:
                        renderer2 = renderer;
                        break;

                    case 3:
                        renderer2 = new ResizeBoxFilterOneThirdAlpha8(renderer);
                        break;

                    default:
                        renderer2 = new ResizeSuperSamplingRendererAlpha8(renderer, clipRectangle.Width, clipRectangle.Height);
                        break;
                }
                return new TileCachedRenderer<ColorAlpha8>(renderer2, 7, SimpleSurfaceAlpha8Allocator.Instance);
            };
            return CreateLazyResult<IRenderer<ColorAlpha8>>(valueFactory);
        }

        private Result<IRenderer<ColorAlpha8>> CreateLazyClippingMaskRenderer(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.CreateLazyClippingMaskRenderer(quality, this.GetTransform(applyInterimTransform));

        public Result<IReadOnlyList<RectInt32>> CreateLazyClippingMaskScans() => 
            this.CreateLazyClippingMaskScans(true);

        public Result<IReadOnlyList<RectInt32>> CreateLazyClippingMaskScans(Matrix3x2Double transform)
        {
            base.VerifyAccess();
            if (this.IsEmpty)
            {
                RectInt32[] numArray = new RectInt32[] { this.ClipRectangle };
                IReadOnlyList<RectInt32> list = numArray;
                return CreateLazyResult<IReadOnlyList<RectInt32>>(list);
            }
            Result<GeometryList> cachedLazyClippingMask = this.GetCachedLazyClippingMask(transform);
            Func<IReadOnlyList<RectInt32>> valueFactory = delegate {
                EdgeTable table = cachedLazyClippingMask.Value.CreateEdgeTable(Matrix3x2Double.Identity);
                if (Memory.AreSmallAllocationsPreferred)
                {
                    SegmentedList<RectInt32> list3 = new SegmentedList<RectInt32>();
                    SegmentedListStruct<RectInt32> struct2 = list3.AsStruct<RectInt32>();
                    table.GetScans<SegmentedListStruct<RectInt32>>(ref struct2);
                    return list3;
                }
                UnsafeList<RectInt32> source = new UnsafeList<RectInt32>();
                UnsafeListStruct<RectInt32> scansOutput = source.AsStruct<RectInt32>();
                table.GetScans<UnsafeListStruct<RectInt32>>(ref scansOutput);
                return source;
            };
            return CreateLazyResult<IReadOnlyList<RectInt32>>(valueFactory);
        }

        public Result<IReadOnlyList<RectInt32>> CreateLazyClippingMaskScans(bool applyInterimTransform) => 
            this.CreateLazyClippingMaskScans(this.GetTransform(applyInterimTransform));

        public Result<GeometryList> CreateLazyGeometryList() => 
            this.CreateLazyGeometryList(true);

        public Result<GeometryList> CreateLazyGeometryList(Matrix3x2Double transform) => 
            this.CreateLazyGeometryList(transform, false);

        public Result<GeometryList> CreateLazyGeometryList(bool applyInterimTransform) => 
            this.CreateLazyGeometryList(this.GetTransform(applyInterimTransform));

        private Result<GeometryList> CreateLazyGeometryList(Matrix3x2Double transform, bool? frozenResult)
        {
            base.VerifyAccess();
            SelectionCombineMode combineMode = this.data.ContinuationCombineMode;
            GeometryList baseGeometry = this.data.BaseGeometry;
            GeometryList continuationGeometry = this.data.ContinuationGeometry;
            Func<GeometryList> valueFactory = delegate {
                GeometryList list1;
                GeometryList list2;
                if (combineMode == SelectionCombineMode.Replace)
                {
                    list1 = continuationGeometry;
                }
                else
                {
                    list1 = GeometryList.Combine(baseGeometry, combineMode.ToGeometryCombineMode(), continuationGeometry);
                }
                if (transform.IsIdentity)
                {
                    list2 = list1;
                }
                else
                {
                    list2 = list1.EnsureUnfrozen();
                    list2.Transform(transform);
                }
                if (!frozenResult.HasValue)
                {
                    return list2;
                }
                if (!frozenResult.Value)
                {
                    return list2.EnsureUnfrozen();
                }
                return list2.EnsureFrozen();
            };
            return CreateLazyResult<GeometryList>(valueFactory);
        }

        public Result<IReadOnlyList<RectInt32>> CreateLazyGeometryListScans() => 
            this.CreateLazyGeometryListScans(true);

        public Result<IReadOnlyList<RectInt32>> CreateLazyGeometryListScans(Matrix3x2Double transform)
        {
            base.VerifyAccess();
            Result<GeometryList> lazyGeometry = this.GetCachedLazyGeometryList(transform);
            Func<IReadOnlyList<RectInt32>> valueFactory = delegate {
                GeometryList list = lazyGeometry.Value;
                if (Memory.AreSmallAllocationsPreferred)
                {
                    SegmentedList<RectInt32> list3 = new SegmentedList<RectInt32>();
                    SegmentedListStruct<RectInt32> struct2 = list3.AsStruct<RectInt32>();
                    list.GetInteriorScans<SegmentedListStruct<RectInt32>>(struct2);
                    return list3;
                }
                UnsafeList<RectInt32> source = new UnsafeList<RectInt32>();
                UnsafeListStruct<RectInt32> output = source.AsStruct<RectInt32>();
                list.GetInteriorScans<UnsafeListStruct<RectInt32>>(output);
                return source;
            };
            return CreateLazyResult<IReadOnlyList<RectInt32>>(valueFactory);
        }

        public Result<IReadOnlyList<RectInt32>> CreateLazyGeometryListScans(bool applyInterimTransform) => 
            this.CreateLazyGeometryListScans(this.GetTransform(applyInterimTransform));

        private static Result<T> CreateLazyResult<T>(T value) => 
            Result.New<T>(value);

        private static Result<T> CreateLazyResult<T>(Func<T> valueFactory) => 
            LazyResult.New<T>(() => valueFactory(), LazyThreadSafetyMode.ExecutionAndPublication, new SingleUseCriticalSection());

        private Result<IReadOnlyList<RectInt32>> CreateLazyScaledClippingMaskScans(SelectionRenderingQuality quality) => 
            this.CreateLazyScaledClippingMaskScans(quality, true);

        private Result<IReadOnlyList<RectInt32>> CreateLazyScaledClippingMaskScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.CreateLazyScaledClippingMaskScans(context.Item1, context.Item2);

        private Result<IReadOnlyList<RectInt32>> CreateLazyScaledClippingMaskScans(SelectionRenderingQuality quality, Matrix3x2Double transform)
        {
            int sampleFactor = GetSampleFactor(quality);
            if (sampleFactor == 1)
            {
                return this.GetCachedLazyClippingMaskScans(transform);
            }
            Result<GeometryList> lazyGeometry = this.GetCachedLazyClippingMask(transform);
            Func<IReadOnlyList<RectInt32>> valueFactory = delegate {
                GeometryList list = lazyGeometry.Value;
                Matrix3x2Double transform = Matrix3x2Double.Scaling((double) sampleFactor, (double) sampleFactor);
                EdgeTable table = list.CreateEdgeTable(transform);
                if (Memory.AreSmallAllocationsPreferred)
                {
                    SegmentedList<RectInt32> list3 = new SegmentedList<RectInt32>();
                    SegmentedListStruct<RectInt32> struct2 = list3.AsStruct<RectInt32>();
                    table.GetScans<SegmentedListStruct<RectInt32>>(ref struct2);
                    return list3;
                }
                UnsafeList<RectInt32> source = new UnsafeList<RectInt32>();
                UnsafeListStruct<RectInt32> scansOutput = source.AsStruct<RectInt32>();
                table.GetScans<UnsafeListStruct<RectInt32>>(ref scansOutput);
                return source;
            };
            return CreateLazyResult<IReadOnlyList<RectInt32>>(valueFactory);
        }

        private Result<IReadOnlyList<RectInt32>> CreateLazyScaledClippingMaskScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.CreateLazyScaledClippingMaskScans(quality, this.GetTransform(applyInterimTransform));

        internal PdnRegion CreateRegion()
        {
            base.VerifyAccess();
            if (this.IsEmpty)
            {
                return new PdnRegion(this.clipRectangle);
            }
            return PdnRegion.FromRectangles(this.GetCachedClippingMaskScans());
        }

        private IReadOnlyList<RectInt32> CreateScaledClippingMaskScans(SelectionRenderingQuality quality) => 
            this.CreateScaledClippingMaskScans(quality, true);

        private IReadOnlyList<RectInt32> CreateScaledClippingMaskScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.CreateScaledClippingMaskScans(context.Item1, context.Item2);

        private IReadOnlyList<RectInt32> CreateScaledClippingMaskScans(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.CreateLazyScaledClippingMaskScans(quality, transform).Value;

        private IReadOnlyList<RectInt32> CreateScaledClippingMaskScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.CreateScaledClippingMaskScans(quality, this.GetTransform(applyInterimTransform));

        public RectInt32 GetBounds()
        {
            base.VerifyAccess();
            return this.GetBounds(true);
        }

        public RectInt32 GetBounds(Matrix3x2Double transform)
        {
            base.VerifyAccess();
            return this.GetBoundsF(transform).Int32Bound;
        }

        public RectInt32 GetBounds(bool applyInterimTransform) => 
            this.GetBounds(this.GetTransform(applyInterimTransform));

        public RectDouble GetBoundsF()
        {
            base.VerifyAccess();
            return this.GetBoundsF(true);
        }

        public RectDouble GetBoundsF(Matrix3x2Double transform)
        {
            base.VerifyAccess();
            return this.GetCachedGeometryList(transform).Bounds;
        }

        public RectDouble GetBoundsF(bool applyInterimTransform) => 
            this.GetBoundsF(this.GetTransform(applyInterimTransform));

        public GeometryList GetCachedClippingMask() => 
            this.GetCachedClippingMask(true);

        public GeometryList GetCachedClippingMask(Matrix3x2Double transform) => 
            this.cachedClippingMask[transform];

        public GeometryList GetCachedClippingMask(bool applyInterimTransform) => 
            this.GetCachedClippingMask(this.GetTransform(applyInterimTransform));

        public IReadOnlyList<RectInt32> GetCachedClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality) => 
            this.GetCachedClippingMaskBiLevelCoverageScans(quality, true);

        private IReadOnlyList<RectInt32> GetCachedClippingMaskBiLevelCoverageScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.cachedClippingMaskBiLevelCoverageScans[context];

        public IReadOnlyList<RectInt32> GetCachedClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.GetCachedClippingMaskBiLevelCoverageScans(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(quality, transform));

        public IReadOnlyList<RectInt32> GetCachedClippingMaskBiLevelCoverageScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.GetCachedClippingMaskBiLevelCoverageScans(quality, this.GetTransform(applyInterimTransform));

        public IRenderer<ColorAlpha8> GetCachedClippingMaskRenderer(SelectionRenderingQuality quality) => 
            this.GetCachedClippingMaskRenderer(quality, true);

        private IRenderer<ColorAlpha8> GetCachedClippingMaskRenderer(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.cachedClippingMaskRenderers[context];

        public IRenderer<ColorAlpha8> GetCachedClippingMaskRenderer(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.GetCachedClippingMaskRenderer(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(quality, transform));

        public IRenderer<ColorAlpha8> GetCachedClippingMaskRenderer(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.GetCachedClippingMaskRenderer(quality, this.GetTransform(applyInterimTransform));

        public IReadOnlyList<RectInt32> GetCachedClippingMaskScans() => 
            this.GetCachedClippingMaskScans(true);

        public IReadOnlyList<RectInt32> GetCachedClippingMaskScans(Matrix3x2Double transform) => 
            this.cachedClippingMaskScans[transform];

        public IReadOnlyList<RectInt32> GetCachedClippingMaskScans(bool applyInterimTransform) => 
            this.GetCachedClippingMaskScans(this.GetTransform(applyInterimTransform));

        public GeometryList GetCachedGeometryList() => 
            this.GetCachedGeometryList(true);

        public GeometryList GetCachedGeometryList(Matrix3x2Double transform) => 
            this.cachedGeometryList[transform];

        public GeometryList GetCachedGeometryList(bool applyInterimTransform) => 
            this.GetCachedGeometryList(this.GetTransform(applyInterimTransform));

        public IReadOnlyList<RectInt32> GetCachedGeometryListScans() => 
            this.GetCachedGeometryListScans(true);

        public IReadOnlyList<RectInt32> GetCachedGeometryListScans(Matrix3x2Double transform) => 
            this.cachedGeometryListScans[transform];

        public IReadOnlyList<RectInt32> GetCachedGeometryListScans(bool applyInterimTransform) => 
            this.GetCachedGeometryListScans(this.GetTransform(applyInterimTransform));

        public Result<GeometryList> GetCachedLazyClippingMask() => 
            this.GetCachedLazyClippingMask(true);

        public Result<GeometryList> GetCachedLazyClippingMask(Matrix3x2Double transform) => 
            this.cachedLazyClippingMask[transform];

        public Result<GeometryList> GetCachedLazyClippingMask(bool applyInterimTransform) => 
            this.GetCachedLazyClippingMask(this.GetTransform(applyInterimTransform));

        public Result<IRenderer<ColorAlpha8>> GetCachedLazyClippingMaskRenderer(SelectionRenderingQuality quality) => 
            this.GetCachedLazyClippingMaskRenderer(quality, true);

        private Result<IRenderer<ColorAlpha8>> GetCachedLazyClippingMaskRenderer(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.cachedLazyClippingMaskRenderers[context];

        public Result<IRenderer<ColorAlpha8>> GetCachedLazyClippingMaskRenderer(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.GetCachedLazyClippingMaskRenderer(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(quality, transform));

        public Result<IRenderer<ColorAlpha8>> GetCachedLazyClippingMaskRenderer(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.GetCachedLazyClippingMaskRenderer(quality, this.GetTransform(applyInterimTransform));

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyClippingMaskScans() => 
            this.GetCachedLazyClippingMaskScans(true);

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyClippingMaskScans(Matrix3x2Double transform) => 
            this.cachedLazyClippingMaskScans[transform];

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyClippingMaskScans(bool applyInterimTransform) => 
            this.GetCachedLazyClippingMaskScans(this.GetTransform(applyInterimTransform));

        public Result<GeometryList> GetCachedLazyGeometryList() => 
            this.GetCachedLazyGeometryList(true);

        public Result<GeometryList> GetCachedLazyGeometryList(Matrix3x2Double transform) => 
            this.cachedLazyGeometryList[transform];

        public Result<GeometryList> GetCachedLazyGeometryList(bool applyInterimTransform) => 
            this.GetCachedLazyGeometryList(this.GetTransform(applyInterimTransform));

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyGeometryListScans() => 
            this.GetCachedLazyGeometryListScans(true);

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyGeometryListScans(Matrix3x2Double transform) => 
            this.cachedLazyGeometryListScans[transform];

        public Result<IReadOnlyList<RectInt32>> GetCachedLazyGeometryListScans(bool applyInterimTransform) => 
            this.GetCachedLazyGeometryListScans(this.GetTransform(applyInterimTransform));

        private Result<IReadOnlyList<RectInt32>> GetCachedLazyScaledClippingMaskScans(SelectionRenderingQuality quality) => 
            this.GetCachedLazyScaledClippingMaskScans(quality, true);

        private Result<IReadOnlyList<RectInt32>> GetCachedLazyScaledClippingMaskScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.cachedLazyScaledClippingMaskScans[context];

        private Result<IReadOnlyList<RectInt32>> GetCachedLazyScaledClippingMaskScans(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.GetCachedLazyScaledClippingMaskScans(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(quality, transform));

        private Result<IReadOnlyList<RectInt32>> GetCachedLazyScaledClippingMaskScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.GetCachedLazyScaledClippingMaskScans(quality, this.GetTransform(applyInterimTransform));

        private IReadOnlyList<RectInt32> GetCachedScaledClippingMaskScans(SelectionRenderingQuality quality) => 
            this.GetCachedScaledClippingMaskScans(quality, true);

        private IReadOnlyList<RectInt32> GetCachedScaledClippingMaskScans(TupleStruct<SelectionRenderingQuality, Matrix3x2Double> context) => 
            this.cachedScaledClippingMaskScans[context];

        private IReadOnlyList<RectInt32> GetCachedScaledClippingMaskScans(SelectionRenderingQuality quality, Matrix3x2Double transform) => 
            this.GetCachedScaledClippingMaskScans(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(quality, transform));

        private IReadOnlyList<RectInt32> GetCachedScaledClippingMaskScans(SelectionRenderingQuality quality, bool applyInterimTransform) => 
            this.GetCachedScaledClippingMaskScans(quality, this.GetTransform(applyInterimTransform));

        private static void GetCoverageScansFromScaledScans<TListIn, TListOut>(TListIn scaledScans, int sampleFactor, TListOut coverageScans) where TListIn: IReadOnlyList<RectInt32> where TListOut: IList<RectInt32>
        {
            if (sampleFactor == 2)
            {
                UnscaleRects<TListIn, Int32Div2CeilingFunctor, Int32Div2FloorFunctor, Int32Mul2Functor, UnscaleRectBy2Functor, TListOut>(scaledScans, new Int32Div2CeilingFunctor(), new Int32Div2FloorFunctor(), new Int32Mul2Functor(), new UnscaleRectBy2Functor(), coverageScans);
            }
            else
            {
                if (sampleFactor != 3)
                {
                    throw new UnreachableCodeException();
                }
                UnscaleRects<TListIn, Int32Div3CeilingFunctor, Int32Div3FloorFunctor, Int32Mul3Functor, UnscaleRectBy3Functor, TListOut>(scaledScans, new Int32Div3CeilingFunctor(), new Int32Div3FloorFunctor(), new Int32Mul3Functor(), new UnscaleRectBy3Functor(), coverageScans);
            }
        }

        public Matrix3x2Double GetCumulativeTransform()
        {
            base.VerifyAccess();
            return this.data.CumulativeTransform;
        }

        public RectDouble GetFastMaxBounds() => 
            this.GetFastMaxBounds(true);

        public RectDouble GetFastMaxBounds(Matrix3x2Double transform)
        {
            base.VerifyAccess();
            GeometryList list = new GeometryList();
            if (!this.data.BaseGeometry.IsEmpty)
            {
                list.AddRect(this.data.BaseGeometry.Bounds);
            }
            if (!this.data.ContinuationGeometry.IsEmpty)
            {
                list.CombineWith(this.data.ContinuationGeometry.Bounds, GeometryCombineMode.Union);
            }
            if (list.IsEmpty)
            {
                return RectDouble.Empty;
            }
            if (!transform.IsIdentity)
            {
                list.Transform(transform);
            }
            return list.Bounds;
        }

        public RectDouble GetFastMaxBounds(bool applyInterimTransform) => 
            this.GetFastMaxBounds(this.GetTransform(applyInterimTransform));

        public Matrix3x2Double GetInterimTransform()
        {
            base.VerifyAccess();
            return this.data.InterimTransform;
        }

        private static int GetSampleFactor(SelectionRenderingQuality quality)
        {
            if ((quality != SelectionRenderingQuality.Aliased) && (quality != SelectionRenderingQuality.HighQualityAntialiased))
            {
                throw ExceptionUtil.InvalidEnumArgumentException<SelectionRenderingQuality>(quality, "quality");
            }
            return (int) quality;
        }

        private Matrix3x2Double GetTransform(bool applyInterimTransform)
        {
            if (!applyInterimTransform)
            {
                return Matrix3x2Double.Identity;
            }
            return this.GetInterimTransform();
        }

        private void OnChanged()
        {
            base.VerifyAccess();
            if (this.alreadyChanging <= 0)
            {
                ExceptionUtil.ThrowInvalidOperationException("Changed event was raised without corresponding Changing event beforehand");
            }
            SelectionChangeFlags changes = this.changes;
            this.alreadyChanging--;
            if (this.alreadyChanging == 0)
            {
                this.ResetCaches();
                this.Changed.RaisePooled<SelectionChangedEventArgs, SelectionChangeFlags>(this, changes);
            }
        }

        private void OnChanging()
        {
            base.VerifyAccess();
            if (this.alreadyChanging == 0)
            {
                this.Changing.Raise(this);
            }
            this.alreadyChanging++;
        }

        public void PerformChanged()
        {
            this.OnChanged();
        }

        public void PerformChanging()
        {
            this.OnChanging();
        }

        private void ReportChanging(SelectionChangeFlags changes)
        {
            this.changes |= changes;
            if ((changes & (SelectionChangeFlags.ClipRectangle | SelectionChangeFlags.CumulativeTransform | SelectionChangeFlags.ContinuationCombineMode | SelectionChangeFlags.ContinuationGeometry | SelectionChangeFlags.BaseGeometry)) != SelectionChangeFlags.None)
            {
                SelectionChangeFlags flags = this.changes;
                this.ResetCaches();
                this.changes = flags | this.changes;
            }
        }

        public void Reset()
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.ClearBaseGeometry();
                this.ClearContinuationGeometry();
                this.ResetCumulativeTransform();
                this.ResetInterimTransform();
            }
        }

        private void ResetCaches()
        {
            if (this.changes != SelectionChangeFlags.None)
            {
                if ((this.changes & (SelectionChangeFlags.ClipRectangle | SelectionChangeFlags.CumulativeTransform | SelectionChangeFlags.ContinuationCombineMode | SelectionChangeFlags.ContinuationGeometry | SelectionChangeFlags.BaseGeometry)) != SelectionChangeFlags.None)
                {
                    WeakCache.EnsureEmpty<Matrix3x2Double, GeometryList>(ref this.cachedGeometryList, () => new WeakCache<Matrix3x2Double, GeometryList>(tx => this.CreateGeometryList(tx, true)));
                    this.cachedGeometryList.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, Result<GeometryList>>(ref this.cachedLazyGeometryList, () => new WeakCache<Matrix3x2Double, Result<GeometryList>>(tx => this.CreateLazyGeometryList(tx, true)));
                    this.cachedLazyGeometryList.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, IReadOnlyList<RectInt32>>(ref this.cachedGeometryListScans, () => new WeakCache<Matrix3x2Double, IReadOnlyList<RectInt32>>(tx => this.CreateGeometryListScans(tx)));
                    this.cachedGeometryListScans.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>>(ref this.cachedLazyGeometryListScans, () => new WeakCache<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>>(tx => this.CreateLazyGeometryListScans(tx)));
                    this.cachedLazyGeometryListScans.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, GeometryList>(ref this.cachedClippingMask, () => new WeakCache<Matrix3x2Double, GeometryList>(tx => this.CreateClippingMask(tx, true)));
                    this.cachedClippingMask.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, Result<GeometryList>>(ref this.cachedLazyClippingMask, () => new WeakCache<Matrix3x2Double, Result<GeometryList>>(tx => this.CreateLazyClippingMask(tx, true)));
                    this.cachedLazyClippingMask.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, IReadOnlyList<RectInt32>>(ref this.cachedClippingMaskScans, () => new WeakCache<Matrix3x2Double, IReadOnlyList<RectInt32>>(new Func<Matrix3x2Double, IReadOnlyList<RectInt32>>(this.CreateClippingMaskScans)));
                    this.cachedClippingMaskScans.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>>(ref this.cachedLazyClippingMaskScans, () => new WeakCache<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>>(new Func<Matrix3x2Double, Result<IReadOnlyList<RectInt32>>>(this.CreateLazyClippingMaskScans)));
                    this.cachedLazyClippingMaskScans.AddAlwaysPinnedKey(Matrix3x2Double.Identity);
                    WeakCache.EnsureEmpty<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IRenderer<ColorAlpha8>>(ref this.cachedClippingMaskRenderers, () => new WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IRenderer<ColorAlpha8>>(new Func<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IRenderer<ColorAlpha8>>(this.CreateClippingMaskRenderer)));
                    this.cachedClippingMaskRenderers.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.Aliased, Matrix3x2Double.Identity));
                    this.cachedClippingMaskRenderers.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.HighQualityAntialiased, Matrix3x2Double.Identity));
                    WeakCache.EnsureEmpty<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IRenderer<ColorAlpha8>>>(ref this.cachedLazyClippingMaskRenderers, () => new WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IRenderer<ColorAlpha8>>>(new Func<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IRenderer<ColorAlpha8>>>(this.CreateLazyClippingMaskRenderer)));
                    this.cachedLazyClippingMaskRenderers.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.Aliased, Matrix3x2Double.Identity));
                    this.cachedLazyClippingMaskRenderers.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.HighQualityAntialiased, Matrix3x2Double.Identity));
                    WeakCache.EnsureEmpty<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(ref this.cachedScaledClippingMaskScans, () => new WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(new Func<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(this.CreateScaledClippingMaskScans)));
                    this.cachedScaledClippingMaskScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.Aliased, Matrix3x2Double.Identity));
                    this.cachedScaledClippingMaskScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.HighQualityAntialiased, Matrix3x2Double.Identity));
                    WeakCache.EnsureEmpty<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IReadOnlyList<RectInt32>>>(ref this.cachedLazyScaledClippingMaskScans, () => new WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IReadOnlyList<RectInt32>>>(new Func<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, Result<IReadOnlyList<RectInt32>>>(this.CreateLazyScaledClippingMaskScans)));
                    this.cachedLazyScaledClippingMaskScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.Aliased, Matrix3x2Double.Identity));
                    this.cachedLazyScaledClippingMaskScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.HighQualityAntialiased, Matrix3x2Double.Identity));
                    WeakCache.EnsureEmpty<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(ref this.cachedClippingMaskBiLevelCoverageScans, () => new WeakCache<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(new Func<TupleStruct<SelectionRenderingQuality, Matrix3x2Double>, IReadOnlyList<RectInt32>>(this.CreateClippingMaskBiLevelCoverageScans)));
                    this.cachedClippingMaskBiLevelCoverageScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.Aliased, Matrix3x2Double.Identity));
                    this.cachedClippingMaskBiLevelCoverageScans.AddAlwaysPinnedKey(TupleStruct.Create<SelectionRenderingQuality, Matrix3x2Double>(SelectionRenderingQuality.HighQualityAntialiased, Matrix3x2Double.Identity));
                }
                this.changes = SelectionChangeFlags.None;
            }
        }

        public void ResetContinuation()
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.CommitInterimTransform();
                this.ResetCumulativeTransform();
                this.ClearContinuationGeometry();
            }
        }

        private void ResetCumulativeTransform()
        {
            base.VerifyAccess();
            if (!this.data.CumulativeTransform.IsIdentity)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.CumulativeTransform);
                    this.data.CumulativeTransform = Matrix3x2Double.Identity;
                }
            }
        }

        public void ResetInterimTransform()
        {
            base.VerifyAccess();
            if (!this.data.InterimTransform.IsIdentity)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.InterimTransform);
                    this.data.InterimTransform = Matrix3x2Double.Identity;
                }
            }
        }

        public void Restore(SelectionData state)
        {
            this.Restore(ref state, false);
        }

        private void Restore(ref SelectionData state, bool takeOwnership)
        {
            Validate.IsNotNull<SelectionData>(state, "state");
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.ReportChanging(SelectionChangeFlags.All);
                if (takeOwnership)
                {
                    this.data = state;
                    state = null;
                }
                else
                {
                    this.data = state.Clone();
                }
                this.data.BaseGeometry.Freeze();
                this.data.ContinuationGeometry.Freeze();
            }
        }

        public SelectionData Save()
        {
            base.VerifyAccess();
            return this.data.Clone();
        }

        public void SetContinuation(PointDouble[] polygon, SelectionCombineMode combineMode)
        {
            this.SetContinuation(ref polygon, combineMode, false);
        }

        public void SetContinuation(GeometryList geometry, SelectionCombineMode combineMode)
        {
            this.SetContinuation(ref geometry, geometry.IsFrozen, combineMode);
        }

        public void SetContinuation(RectDouble rect, SelectionCombineMode combineMode)
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.CommitInterimTransform();
                this.ResetCumulativeTransform();
                this.SetContinuationCombineMode(combineMode);
                this.ReportChanging(SelectionChangeFlags.ContinuationGeometry);
                this.data.ContinuationGeometry = new GeometryList();
                this.data.ContinuationGeometry.AddRect(rect);
                this.data.ContinuationGeometry.Freeze();
            }
        }

        public void SetContinuation(RectInt32 rect, SelectionCombineMode combineMode)
        {
            this.SetContinuation((RectDouble) rect, combineMode);
        }

        public void SetContinuation(ref PointDouble[] polygon, SelectionCombineMode combineMode, bool takeOwnership)
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.CommitInterimTransform();
                this.ResetCumulativeTransform();
                this.SetContinuationCombineMode(combineMode);
                this.ReportChanging(SelectionChangeFlags.ContinuationGeometry);
                this.data.ContinuationGeometry = new GeometryList();
                this.data.ContinuationGeometry.AddPolygon(ref polygon, takeOwnership);
                this.data.ContinuationGeometry.Freeze();
            }
        }

        private void SetContinuation(ref GeometryList geometry, bool takeOwnership, SelectionCombineMode combineMode)
        {
            base.VerifyAccess();
            using (this.UseChangeScope())
            {
                this.CommitInterimTransform();
                this.ResetCumulativeTransform();
                this.SetContinuationCombineMode(combineMode);
                if (takeOwnership && (geometry == this.data.ContinuationGeometry))
                {
                    geometry = null;
                }
                else
                {
                    this.ReportChanging(SelectionChangeFlags.ContinuationGeometry);
                    if (takeOwnership)
                    {
                        this.data.ContinuationGeometry = geometry;
                        this.data.ContinuationGeometry.Freeze();
                        geometry = null;
                    }
                    else
                    {
                        this.data.ContinuationGeometry = geometry.Clone();
                        this.data.ContinuationGeometry.Freeze();
                    }
                }
            }
        }

        private void SetContinuationCombineMode(SelectionCombineMode combineMode)
        {
            base.VerifyAccess();
            if (this.data.ContinuationCombineMode != combineMode)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.ContinuationCombineMode);
                    this.data.ContinuationCombineMode = combineMode;
                }
            }
        }

        private void SetCumulativeTransform(Matrix3x2Double m)
        {
            base.VerifyAccess();
            VerifyIsFinite(m);
            if (this.data.CumulativeTransform != m)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.CumulativeTransform);
                    this.data.CumulativeTransform = m;
                }
            }
        }

        public void SetInterimTransform(Matrix3x2Double m)
        {
            base.VerifyAccess();
            VerifyIsFinite(m);
            if (this.data.InterimTransform != m)
            {
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.InterimTransform);
                    this.data.InterimTransform = m;
                }
            }
        }

        [IteratorStateMachine(typeof(<UnscaleAndOrganizeRects>d__12))]
        private static IEnumerable<RectInt32> UnscaleAndOrganizeRects<TList, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn>(TList rects, TInt32DivFloorFn int32DivFloorFn, TInt32DivCeilingFn int32DivCeilingFn, TInt32MulFn int32MulFn, TRectInt32UnscaleFn rectInt32UnscaleFn) where TList: IReadOnlyList<RectInt32> where TInt32DivFloorFn: IFunc<int, int> where TInt32DivCeilingFn: IFunc<int, int> where TInt32MulFn: IFunc<int, int> where TRectInt32UnscaleFn: IFunc<RectInt32, RectInt32>
        {
            this.<count>5__5 = rects.Count;
            if (this.<count>5__5 != 0)
            {
                RectInt32 rect = rects[0];
                VerifyRectIsNonNegative(rect);
                RectInt32 num3 = rectInt32UnscaleFn.Invoke(rect);
                this.<i>5__4 = 1;
                while (this.<i>5__4 < this.<count>5__5)
                {
                    RectInt32 num4 = rects[this.<i>5__4];
                    VerifyRectIsNonNegative(num4);
                    this.<currentUnscaled>5__1 = rectInt32UnscaleFn.Invoke(num4);
                    if ((!this.<currentUnscaled>5__1.HasZeroArea && (num3 != this.<currentUnscaled>5__1)) && !num3.Contains(this.<currentUnscaled>5__1))
                    {
                        if ((num3.Top == this.<currentUnscaled>5__1.Top) && (num3.Bottom == this.<currentUnscaled>5__1.Bottom))
                        {
                            yield return num3;
                            num3 = this.<currentUnscaled>5__1;
                        }
                        else if (num3.Bottom <= this.<currentUnscaled>5__1.Top)
                        {
                            yield return num3;
                            num3 = this.<currentUnscaled>5__1;
                        }
                        else if ((num3.Bottom - 1) == this.<currentUnscaled>5__1.Top)
                        {
                            if (num3.Height > 1)
                            {
                                RectInt32 num5 = RectInt32.FromEdges(num3.Left, num3.Top, num3.Right, num3.Bottom - 1);
                                this.<previousBottomUnscaled>5__2 = RectInt32.FromEdges(num3.Left, num3.Bottom - 1, num3.Right, num3.Bottom);
                                yield return num5;
                                num3 = this.<previousBottomUnscaled>5__2;
                                this.<previousBottomUnscaled>5__2 = new RectInt32();
                            }
                            if (this.<currentUnscaled>5__1.Height == 1)
                            {
                                yield return num3;
                                num3 = this.<currentUnscaled>5__1;
                            }
                            else
                            {
                                yield return num3;
                                this.<currentTopUnscaled>5__3 = RectInt32.FromEdges(this.<currentUnscaled>5__1.Left, this.<currentUnscaled>5__1.Top, this.<currentUnscaled>5__1.Right, this.<currentUnscaled>5__1.Top + 1);
                                yield return this.<currentTopUnscaled>5__3;
                                num3 = RectInt32.FromEdges(this.<currentUnscaled>5__1.Left, this.<currentTopUnscaled>5__3.Bottom, this.<currentUnscaled>5__1.Right, this.<currentUnscaled>5__1.Bottom);
                                this.<currentTopUnscaled>5__3 = new RectInt32();
                            }
                        }
                        else
                        {
                            ExceptionUtil.ThrowUnreachableCodeException();
                        }
                        this.<currentUnscaled>5__1 = new RectInt32();
                    }
                    int num7 = this.<i>5__4 + 1;
                    this.<i>5__4 = num7;
                }
                yield return num3;
            }
        }

        private static RectInt32 UnscaleRectBy2(RectInt32 rect) => 
            RectInt32.FromEdges((int) UInt32Util.Div2Floor((uint) rect.Left), (int) UInt32Util.Div2Floor((uint) rect.Top), (int) UInt32Util.Div2Ceiling((uint) rect.Right), (int) UInt32Util.Div2Ceiling((uint) rect.Bottom));

        private static RectInt32 UnscaleRectBy3(RectInt32 rect) => 
            RectInt32.FromEdges((int) UInt32Util.Div3Floor((uint) rect.Left), (int) UInt32Util.Div3Floor((uint) rect.Top), (int) UInt32Util.Div3Ceiling((uint) rect.Right), (int) UInt32Util.Div3Ceiling((uint) rect.Bottom));

        private static void UnscaleRects<TListIn, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn, TListOut>(TListIn rectsIn, TInt32DivFloorFn int32DivFloorFn, TInt32DivCeilingFn int32DivCeilingFn, TInt32MulFn int32MulFn, TRectInt32UnscaleFn rectInt32UnscaleFn, TListOut rectsOut) where TListIn: IReadOnlyList<RectInt32> where TInt32DivFloorFn: IFunc<int, int> where TInt32DivCeilingFn: IFunc<int, int> where TInt32MulFn: IFunc<int, int> where TRectInt32UnscaleFn: IFunc<RectInt32, RectInt32> where TListOut: IList<RectInt32>
        {
            if (rectsOut.Count > 0)
            {
                ExceptionUtil.ThrowArgumentException("rectsOut must be empty (Count == 0)", "rectsOut");
            }
            if (rectsIn.Count != 0)
            {
                rectsOut.AddRange<RectInt32>(UnscaleAndOrganizeRects<TListIn, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn>(rectsIn, int32DivFloorFn, int32DivCeilingFn, int32MulFn, rectInt32UnscaleFn));
                ScansHelpers.SortScanBeamsByLeftEdgeInPlace<TListOut>(rectsOut);
                ScansHelpers.ConsolidateSortedScansInPlace<TListOut>(rectsOut, 0, rectsOut.Count);
            }
        }

        public ChangeScope UseChangeScope() => 
            new ChangeScope(this);

        private static void VerifyIsFinite(Matrix3x2Double m)
        {
            if (!m.IsFinite)
            {
                throw new ArgumentException("matrix isn't finite, " + m.ToString());
            }
        }

        private static void VerifyRectIsNonNegative(RectInt32 rect)
        {
            if ((rect.Top < 0) || (rect.Left < 0))
            {
                ExceptionUtil.ThrowArgumentException("Rectangles must have non-negative coordinates");
            }
        }

        public RectInt32 ClipRectangle
        {
            get
            {
                base.VerifyAccess();
                return this.clipRectangle;
            }
            set
            {
                base.VerifyAccess();
                using (this.UseChangeScope())
                {
                    this.ReportChanging(SelectionChangeFlags.ClipRectangle);
                    this.clipRectangle = value;
                }
            }
        }

        public bool IsChanging =>
            (this.alreadyChanging > 0);

        public bool IsEmpty
        {
            get
            {
                base.VerifyAccess();
                return (this.data.BaseGeometry.IsEmpty && this.data.ContinuationGeometry.IsEmpty);
            }
        }

        [CompilerGenerated]
        private sealed class <UnscaleAndOrganizeRects>d__12<TList, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn> : IEnumerable<RectInt32>, IEnumerable, IEnumerator<RectInt32>, IDisposable, IEnumerator where TList: IReadOnlyList<RectInt32> where TInt32DivFloorFn: IFunc<int, int> where TInt32DivCeilingFn: IFunc<int, int> where TInt32MulFn: IFunc<int, int> where TRectInt32UnscaleFn: IFunc<RectInt32, RectInt32>
        {
            private int <>1__state;
            private RectInt32 <>2__current;
            public TRectInt32UnscaleFn <>3__rectInt32UnscaleFn;
            public TList <>3__rects;
            private int <>l__initialThreadId;
            private int <count>5__5;
            private RectInt32 <currentTopUnscaled>5__3;
            private RectInt32 <currentUnscaled>5__1;
            private int <i>5__4;
            private RectInt32 <previousBottomUnscaled>5__2;
            private TRectInt32UnscaleFn rectInt32UnscaleFn;
            private TList rects;

            [DebuggerHidden]
            public <UnscaleAndOrganizeRects>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                RectInt32 num3;
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<count>5__5 = this.rects.Count;
                        if (this.<count>5__5 != 0)
                        {
                            RectInt32 rect = this.rects[0];
                            Selection.VerifyRectIsNonNegative(rect);
                            num3 = this.rectInt32UnscaleFn.Invoke(rect);
                            this.<i>5__4 = 1;
                            while (this.<i>5__4 < this.<count>5__5)
                            {
                                int num7;
                                RectInt32 num4 = this.rects[this.<i>5__4];
                                Selection.VerifyRectIsNonNegative(num4);
                                this.<currentUnscaled>5__1 = this.rectInt32UnscaleFn.Invoke(num4);
                                if ((this.<currentUnscaled>5__1.HasZeroArea || (num3 == this.<currentUnscaled>5__1)) || num3.Contains(this.<currentUnscaled>5__1))
                                {
                                    goto Label_0308;
                                }
                                if ((num3.Top == this.<currentUnscaled>5__1.Top) && (num3.Bottom == this.<currentUnscaled>5__1.Bottom))
                                {
                                    this.<>2__current = num3;
                                    this.<>1__state = 1;
                                    return true;
                                }
                                if (num3.Bottom <= this.<currentUnscaled>5__1.Top)
                                {
                                    this.<>2__current = num3;
                                    this.<>1__state = 2;
                                    return true;
                                }
                                if ((num3.Bottom - 1) != this.<currentUnscaled>5__1.Top)
                                {
                                    goto Label_02F7;
                                }
                                if (num3.Height <= 1)
                                {
                                    goto Label_0216;
                                }
                                RectInt32 num5 = RectInt32.FromEdges(num3.Left, num3.Top, num3.Right, num3.Bottom - 1);
                                this.<previousBottomUnscaled>5__2 = RectInt32.FromEdges(num3.Left, num3.Bottom - 1, num3.Right, num3.Bottom);
                                this.<>2__current = num5;
                                this.<>1__state = 3;
                                return true;
                            Label_01FC:
                                this.<>1__state = -1;
                                num3 = this.<previousBottomUnscaled>5__2;
                                this.<previousBottomUnscaled>5__2 = new RectInt32();
                            Label_0216:
                                if (this.<currentUnscaled>5__1.Height == 1)
                                {
                                    this.<>2__current = num3;
                                    this.<>1__state = 4;
                                    return true;
                                }
                                this.<>2__current = num3;
                                this.<>1__state = 5;
                                return true;
                            Label_02F7:
                                ExceptionUtil.ThrowUnreachableCodeException();
                            Label_02FC:
                                this.<currentUnscaled>5__1 = new RectInt32();
                            Label_0308:
                                num7 = this.<i>5__4 + 1;
                                this.<i>5__4 = num7;
                            }
                            this.<>2__current = num3;
                            this.<>1__state = 7;
                            return true;
                        }
                        return false;

                    case 1:
                        this.<>1__state = -1;
                        num3 = this.<currentUnscaled>5__1;
                        goto Label_02FC;

                    case 2:
                        this.<>1__state = -1;
                        num3 = this.<currentUnscaled>5__1;
                        goto Label_02FC;

                    case 3:
                        goto Label_01FC;

                    case 4:
                        this.<>1__state = -1;
                        num3 = this.<currentUnscaled>5__1;
                        goto Label_02FC;

                    case 5:
                        this.<>1__state = -1;
                        this.<currentTopUnscaled>5__3 = RectInt32.FromEdges(this.<currentUnscaled>5__1.Left, this.<currentUnscaled>5__1.Top, this.<currentUnscaled>5__1.Right, this.<currentUnscaled>5__1.Top + 1);
                        this.<>2__current = this.<currentTopUnscaled>5__3;
                        this.<>1__state = 6;
                        return true;

                    case 6:
                        this.<>1__state = -1;
                        num3 = RectInt32.FromEdges(this.<currentUnscaled>5__1.Left, this.<currentTopUnscaled>5__3.Bottom, this.<currentUnscaled>5__1.Right, this.<currentUnscaled>5__1.Bottom);
                        this.<currentTopUnscaled>5__3 = new RectInt32();
                        goto Label_02FC;

                    case 7:
                        this.<>1__state = -1;
                        return false;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<RectInt32> IEnumerable<RectInt32>.GetEnumerator()
            {
                Selection.<UnscaleAndOrganizeRects>d__12<TList, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn> d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (Selection.<UnscaleAndOrganizeRects>d__12<TList, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn>) this;
                }
                else
                {
                    d__ = new Selection.<UnscaleAndOrganizeRects>d__12<TList, TInt32DivFloorFn, TInt32DivCeilingFn, TInt32MulFn, TRectInt32UnscaleFn>(0);
                }
                d__.rects = this.<>3__rects;
                d__.rectInt32UnscaleFn = this.<>3__rectInt32UnscaleFn;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Rendering.RectInt32>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            RectInt32 IEnumerator<RectInt32>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ChangeScope : IDisposable
        {
            private Selection owner;
            internal ChangeScope(Selection owner)
            {
                Validate.IsNotNull<Selection>(owner, "owner");
                this.owner = owner;
                this.owner.PerformChanging();
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    Selection owner = this.owner;
                    this.owner = null;
                    owner.PerformChanged();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Div2CeilingFunctor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                ((int) UInt32Util.Div2Ceiling((uint) x));
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Div2FloorFunctor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                ((int) UInt32Util.Div2Floor((uint) x));
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Div3CeilingFunctor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                ((int) UInt32Util.Div3Ceiling((uint) x));
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Div3FloorFunctor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                ((int) UInt32Util.Div3Floor((uint) x));
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Mul2Functor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                (x << 1);
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct Int32Mul3Functor : IFunc<int, int>
        {
            public int Invoke(int x) => 
                (x * 3);
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct UnscaleRectBy2Functor : IFunc<RectInt32, RectInt32>
        {
            public RectInt32 Invoke(RectInt32 x) => 
                Selection.UnscaleRectBy2(x);
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct UnscaleRectBy3Functor : IFunc<RectInt32, RectInt32>
        {
            public RectInt32 Invoke(RectInt32 x) => 
                Selection.UnscaleRectBy3(x);
        }
    }
}

