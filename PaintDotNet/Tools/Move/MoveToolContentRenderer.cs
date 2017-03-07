namespace PaintDotNet.Tools.Move
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Functional;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class MoveToolContentRenderer : CancellableMaskedRendererBgraBase
    {
        private BitmapLayer activeLayer;
        private static readonly ISurfaceAllocator<ColorAlpha8> alpha8Allocator = SurfaceAllocator.Alpha8;
        private static readonly ISurfaceAllocator<ColorBgra> bgraAllocator = SurfaceAllocator.Bgra;
        private MoveToolChanges changes;
        private int isLazyDeltaSelectionMaskFirstTimePrefetched;
        private int isLazyFinalSelectionMaskFirstTimePrefetched;
        private Result<IRenderer<ColorAlpha8>> lazyDeltaSelectionMask;
        private Result<IRenderer<ColorAlpha8>> lazyFinalSelectionMask;
        private ISurface<ColorBgra> source;
        private CancellableMaskedRendererBgraBase sourceTx;

        public MoveToolContentRenderer(BitmapLayer activeLayer, MoveToolChanges changes, Result<IRenderer<ColorAlpha8>> lazyDeltaSelectionMask, Result<IRenderer<ColorAlpha8>> lazyFinalSelectionMask) : base(activeLayer.Width, activeLayer.Height, false)
        {
            Validate.Begin().IsNotNull<BitmapLayer>(activeLayer, "activeLayer").IsNotNull<MoveToolChanges>(changes, "changes").IsNotNull<Result<IRenderer<ColorAlpha8>>>(lazyDeltaSelectionMask, "lazyDeltaSelectionMask").IsNotNull<Result<IRenderer<ColorAlpha8>>>(lazyFinalSelectionMask, "lazyFinalSelectionMask").Check();
            this.activeLayer = activeLayer;
            this.changes = changes;
            this.lazyDeltaSelectionMask = lazyDeltaSelectionMask;
            this.lazyFinalSelectionMask = lazyFinalSelectionMask;
            switch (changes.PixelSource)
            {
                case MoveToolPixelSource.ActiveLayer:
                    this.source = activeLayer.Surface;
                    break;

                case MoveToolPixelSource.Bitmap:
                    this.source = changes.BitmapSource.Object;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<MoveToolPixelSource>(changes.PixelSource, "changes.PixelSource");
            }
            Matrix3x2Double matrix = changes.DeltaTransform * changes.EditTransform;
            if ((changes.MoveToolResamplingAlgorithm == ResamplingAlgorithm.NearestNeighbor) || changes.FinalTransform.IsIntegerTranslation)
            {
                this.sourceTx = new TransformedNearestNeighborContentRenderer(activeLayer.Size(), this.source, matrix);
            }
            else
            {
                RectDouble baseBounds = changes.BaseBounds;
                RectInt32 srcCoverageBounds = changes.BaseTransform.Transform(baseBounds).Int32Bound;
                this.sourceTx = new TransformedBilinearContentRenderer(activeLayer.Size(), this.source, srcCoverageBounds, matrix);
            }
        }

        protected override void OnCancelled()
        {
            this.sourceTx.Cancel();
            base.OnCancelled();
        }

        protected override unsafe void OnRender(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
        {
            base.ThrowIfCancellationRequested();
            int width = dstContent.Width;
            int height = dstContent.Height;
            ColorBgra* bgraPtr = (ColorBgra*) dstContent.Scan0;
            int num3 = dstContent.Stride - (width * 4);
            RectInt32 sourceRect = new RectInt32(renderOffset.X, renderOffset.Y, width, height);
            if (this.changes.PixelSource == MoveToolPixelSource.Bitmap)
            {
                ZeroMaskSampler baseMaskSampler = new ZeroMaskSampler();
                this.OnRenderImpl<ZeroMaskSampler>(dstContent, renderOffset, ref baseMaskSampler);
            }
            else if (this.changes.LeaveCopyBehind)
            {
                ZeroMaskSampler sampler2 = new ZeroMaskSampler();
                this.OnRenderImpl<ZeroMaskSampler>(dstContent, renderOffset, ref sampler2);
            }
            else
            {
                using (BaseMaskSampler sampler3 = new BaseMaskSampler(this, sourceRect))
                {
                    this.OnRenderImpl<BaseMaskSampler>(dstContent, renderOffset, ref sampler3);
                }
            }
        }

        private unsafe void OnRenderImpl<TMaskSampler>(ISurface<ColorBgra> dstContent, PointInt32 renderOffset, ref TMaskSampler baseMaskSampler) where TMaskSampler: IMaskSampler
        {
            if (Interlocked.Exchange(ref this.isLazyDeltaSelectionMaskFirstTimePrefetched, 1) == 0)
            {
                this.lazyDeltaSelectionMask.EnsureEvaluated();
            }
            else if (Interlocked.Exchange(ref this.isLazyFinalSelectionMaskFirstTimePrefetched, 1) == 0)
            {
                this.lazyFinalSelectionMask.EnsureEvaluated();
            }
            base.ThrowIfCancellationRequested();
            int width = dstContent.Width;
            int height = dstContent.Height;
            ColorBgra* bgraPtr = (ColorBgra*) dstContent.Scan0;
            int stride = dstContent.Stride;
            int num4 = stride - (width * 4);
            RectInt32 bounds = new RectInt32(renderOffset.X, renderOffset.Y, width, height);
            uint num6 = this.changes.BackFillColor.Bgra;
            using (ISurface<ColorAlpha8> surface = alpha8Allocator.Allocate(width, height, AllocationOptions.ZeroFillNotRequired))
            {
                base.ThrowIfCancellationRequested();
                this.sourceTx.Render(dstContent, surface, renderOffset);
                base.ThrowIfCancellationRequested();
                using (ISurface<ColorAlpha8> surface2 = this.lazyFinalSelectionMask.Value.UseTileOrToSurface(bounds))
                {
                    base.ThrowIfCancellationRequested();
                    ColorBgra* pointAddress = this.activeLayer.Surface.GetPointAddress(renderOffset.X, renderOffset.Y);
                    int num7 = this.activeLayer.Surface.Stride - (width * 4);
                    byte* numPtr = (byte*) surface2.Scan0;
                    int num8 = surface2.Stride - bounds.Width;
                    byte* numPtr2 = (byte*) surface.Scan0;
                    int num9 = surface.Stride - bounds.Width;
                    ColorBgra* bgraPtr3 = bgraPtr + width;
                    baseMaskSampler.Initialize();
                    for (int i = 0; i < height; i++)
                    {
                        for (ColorBgra* bgraPtr4 = bgraPtr3 - width; bgraPtr4 < bgraPtr3; bgraPtr4++)
                        {
                            byte next = baseMaskSampler.GetNext();
                            byte num12 = numPtr[0];
                            byte num13 = numPtr2[0];
                            byte num14 = Math.Min(num13, num12);
                            if (num14 == 0)
                            {
                                byte frac = (byte) (0xff - next);
                                if (frac == 0)
                                {
                                    bgraPtr4->Bgra = num6;
                                }
                                else
                                {
                                    ColorBgra bgra = pointAddress[0];
                                    if (frac != 0xff)
                                    {
                                        bgra.A = ByteUtil.FastScale(bgra.A, frac);
                                    }
                                    bgraPtr4->Bgra = bgra.Bgra;
                                }
                            }
                            else if (num14 != 0xff)
                            {
                                byte num16 = Math.Min((byte) (0xff - num14), (byte) (0xff - next));
                                ColorBgra bgra2 = bgraPtr4[0];
                                ColorBgra bgra3 = pointAddress[0];
                                ushort d = (ushort) ((bgra3.A * num16) + (bgra2.A * num14));
                                if (d == 0)
                                {
                                    bgraPtr4->Bgra = num6;
                                }
                                else
                                {
                                    int num18 = UInt16Util.FastDivideBy255((ushort) ((bgra3.A * num16) + (bgra2.A * num14)));
                                    uint num19 = UInt32Util.FastDivideByUInt16((uint) (((bgra3.A * num16) * bgra3.B) + ((bgra2.A * num14) * bgra2.B)), d);
                                    uint num20 = UInt32Util.FastDivideByUInt16((uint) (((bgra3.A * num16) * bgra3.G) + ((bgra2.A * num14) * bgra2.G)), d);
                                    uint num21 = UInt32Util.FastDivideByUInt16((uint) (((bgra3.A * num16) * bgra3.R) + ((bgra2.A * num14) * bgra2.R)), d);
                                    bgraPtr4->Bgra = ColorBgra.BgraToUInt32((byte) num19, (byte) num20, (byte) num21, (byte) num18);
                                }
                            }
                            pointAddress++;
                            numPtr++;
                            numPtr2++;
                        }
                        pointAddress += num7;
                        baseMaskSampler.MoveToNextRow();
                        numPtr += num8;
                        numPtr2 += num9;
                        bgraPtr3 += stride;
                        base.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BaseMaskSampler : MoveToolContentRenderer.IMaskSampler, IDisposable
        {
            private MoveToolContentRenderer owner;
            private RectInt32 sourceRect;
            private ISurface<ColorAlpha8> baseMaskWindow;
            private bool isInitialized;
            private unsafe byte* ptr;
            private int margin;
            public unsafe BaseMaskSampler(MoveToolContentRenderer owner, RectInt32 sourceRect)
            {
                this.owner = owner;
                this.sourceRect = sourceRect;
                this.baseMaskWindow = null;
                this.isInitialized = false;
                this.ptr = null;
                this.margin = -1;
            }

            public unsafe void Initialize()
            {
                IRenderer<ColorAlpha8> renderer = this.owner.lazyDeltaSelectionMask.Value;
                this.owner.ThrowIfCancellationRequested();
                this.baseMaskWindow = renderer.UseTileOrToSurface(this.sourceRect);
                this.owner.ThrowIfCancellationRequested();
                byte* numPtr = (byte*) this.baseMaskWindow.Scan0;
                int num = this.baseMaskWindow.Stride - this.baseMaskWindow.Width;
                this.ptr = numPtr;
                this.margin = num;
                this.isInitialized = true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe byte GetNext()
            {
                byte* ptr = this.ptr;
                this.ptr = ptr + 1;
                return ptr[0];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void MoveToNextRow()
            {
                this.ptr += this.margin;
            }

            public void Dispose()
            {
                DisposableUtil.Free<ISurface<ColorAlpha8>>(ref this.baseMaskWindow);
            }
        }

        private interface IMaskSampler
        {
            byte GetNext();
            void Initialize();
            void MoveToNextRow();
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct ZeroMaskSampler : MoveToolContentRenderer.IMaskSampler
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize()
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte GetNext() => 
                0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void MoveToNextRow()
            {
            }
        }
    }
}

