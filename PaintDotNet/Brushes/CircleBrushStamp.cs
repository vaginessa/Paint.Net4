namespace PaintDotNet.Brushes
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Imaging;
    using PaintDotNet.MemoryManagement;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using System;
    using System.Threading;

    internal sealed class CircleBrushStamp : BrushStamp
    {
        private readonly double diameter;
        private readonly double hardness;
        private const double maxBitmapSizeInflationPercentage = 0.665;

        public CircleBrushStamp(double diameter, double hardness, double opacity, bool antialiased) : base(CalculateBitmapSize(diameter, hardness, antialiased), opacity, antialiased)
        {
            Validate.Begin().IsFinite(diameter, "diameter").IsFinite(hardness, "hardness").Check();
            if ((hardness < 0.0) || (hardness > 1.0))
            {
                throw new ArgumentOutOfRangeException("hardness");
            }
            this.diameter = diameter;
            this.hardness = hardness;
        }

        private static SizeInt32 CalculateBitmapSize(double diameter, double hardness, bool antialiased)
        {
            int num3 = GetRoundedDiameter(GetInflatedDiameter(diameter, hardness, antialiased)) | (antialiased ? 1 : 0);
            int width = num3 + 2;
            return new SizeInt32(width, width);
        }

        private Brush CreateBrush(double centerX, double centerY, double radiusX, double radiusY)
        {
            if (!base.Antialiased)
            {
                return SolidColorBrushCache.Get((ColorRgba128Float) Colors.White);
            }
            GradientStopCollection gradientStopCollection = new GradientStopCollection(0x20);
            for (int i = 0; i < 0x20; i++)
            {
                double num2;
                switch (i)
                {
                    case 0:
                        num2 = 0.0;
                        break;

                    case 0x1f:
                        num2 = 1.0;
                        break;

                    default:
                        num2 = ((double) i) / 31.0;
                        break;
                }
                double gradientStopAlpha = this.GetGradientStopAlpha(num2);
                GradientStop item = new GradientStop(new ColorRgba128Float((ColorRgba128Float) Colors.White, (float) gradientStopAlpha), num2);
                gradientStopCollection.Add(item);
            }
            gradientStopCollection.Freeze();
            RadialGradientBrush brush = new RadialGradientBrush(gradientStopCollection) {
                ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation,
                GradientOrigin = PointDouble.Zero,
                RadiusX = radiusX,
                RadiusY = radiusY,
                Center = new PointDouble(centerX, centerY)
            };
            brush.Freeze();
            return brush;
        }

        private static double GetEffectiveHardness(double hardness, bool antialiased)
        {
            if (!antialiased)
            {
                return 1.0;
            }
            return hardness;
        }

        private double GetGradientStopAlpha(double offset)
        {
            double num5;
            if ((offset < 0.0) || (offset > 1.0))
            {
                throw new ArgumentOutOfRangeException();
            }
            double effectiveHardness = GetEffectiveHardness(this.hardness, base.Antialiased);
            double hardnessInflationFactor = GetHardnessInflationFactor(this.hardness, base.Antialiased);
            double num3 = effectiveHardness;
            double num4 = 1.0 - num3;
            if (offset <= num3)
            {
                num5 = 1.0;
            }
            else
            {
                double x = (offset - num3) / num4;
                num5 = MathUtil.Gaussian(x, 1.0, 0.0, 0.275);
            }
            return DoubleUtil.Clamp(num5, 0.0, 1.0);
        }

        private static double GetHardnessInflationFactor(double hardness, bool antialiased)
        {
            double effectiveHardness = GetEffectiveHardness(hardness, antialiased);
            return (1.0 + ((1.0 - effectiveHardness) * 0.665));
        }

        private static double GetInflatedDiameter(double diameter, double hardness, bool antialiased)
        {
            double hardnessInflationFactor = GetHardnessInflationFactor(hardness, antialiased);
            return (diameter * hardnessInflationFactor);
        }

        private static int GetRoundedDiameter(double diameter) => 
            ((int) Math.Ceiling(diameter));

        protected override unsafe void OnRender(IBitmap<ColorAlpha8> dstMask)
        {
            <>c__DisplayClass3_0 class_;
            SizeInt32 size = dstMask.Size;
            double centerX = ((double) size.Width) / 2.0;
            double centerY = ((double) size.Height) / 2.0;
            int roundedDiameter = GetRoundedDiameter(this.diameter);
            if (roundedDiameter == 1)
            {
                int num5 = size.Width / 2;
                int num6 = size.Height / 2;
                using (IBitmapLock<ColorAlpha8> @lock = dstMask.Lock<ColorAlpha8>(BitmapLockOptions.ReadWrite))
                {
                    @lock.Clear<ColorAlpha8>();
                    ColorAlpha8* alphaPtr = (ColorAlpha8*) (((void*) @lock.Scan0) + (num6 * @lock.Stride));
                    ColorAlpha8* alphaPtr2 = alphaPtr + num5;
                    alphaPtr2[0] = ColorAlpha8.Opaque;
                    return;
                }
            }
            double num7 = GetInflatedDiameter((double) roundedDiameter, this.hardness, base.Antialiased);
            double radiusX = num7 / 2.0;
            double radiusY = num7 / 2.0;
            Brush brush = this.CreateBrush(centerX, centerY, radiusX, radiusY);
            Geometry geometry = new EllipseGeometry(new PointDouble(centerX, centerY), radiusX, radiusY).EnsureFrozen<EllipseGeometry>().GetFlattenedPathGeometry((double) 0.0001).EnsureFrozen<PathGeometry>();
            GeometrySource fillGeometrySource = GeometrySource.Create(geometry);
            using (ThreadLocal<IGeometry> perThreadD2DFillGeometry = new ThreadLocal<IGeometry>(new Func<IGeometry>(class_.<OnRender>b__0), true))
            {
                using (IBitmapLock<ColorAlpha8> lock2 = dstMask.Lock<ColorAlpha8>(BitmapLockOptions.Write))
                {
                    byte* dstMaskLockScan0 = (byte*) lock2.Scan0;
                    int dstMaskLockStride = lock2.Stride;
                    Work.ParallelForEach<RectInt32>(WaitType.Pumping, new TileMathHelper(size, 7).EnumerateTiles(), delegate (RectInt32 tileRect) {
                        byte* numPtr = (dstMaskLockScan0 + (tileRect.Top * dstMaskLockStride)) + tileRect.Left;
                        SharedSurfaceStruct<ColorAlpha8> dst = new SharedSurfaceStruct<ColorAlpha8>(tileRect.Width, tileRect.Height, dstMaskLockStride, (IntPtr) numPtr);
                        RenderTile(dst, tileRect.Location, perThreadD2DFillGeometry.Value, brush, this.Antialiased);
                    }, WorkItemQueuePriority.Normal, null);
                }
                foreach (IDisposable disposable in perThreadD2DFillGeometry.Values)
                {
                    disposable.Dispose();
                }
            }
        }

        private static unsafe void RenderTile(ISurface<ColorAlpha8> dst, PointInt32 renderOffset, IGeometry fillGeometry, Brush brush, bool isAntialiased)
        {
            SizeInt32 size = new SizeInt32(dst.Width, dst.Height);
            using (IBitmap<ColorPbgra32> bitmap = BitmapAllocator.Pbgra32.Allocate(size, AllocationOptions.ZeroFillNotRequired))
            {
                using (IDrawingContext context = DrawingContext.FromBitmap(bitmap, FactorySource.PerThread))
                {
                    using (context.UseAntialiasMode(isAntialiased ? AntialiasMode.PerPrimitive : AntialiasMode.Aliased))
                    {
                        context.Transform = Matrix3x2Float.Translation((float) -renderOffset.X, (float) -renderOffset.Y);
                        context.Clear(null);
                        IBrush cachedOrCreateResource = context.GetCachedOrCreateResource<IBrush>(brush);
                        context.FillGeometry(fillGeometry, cachedOrCreateResource, null);
                    }
                }
                using (IBitmapLock<ColorPbgra32> @lock = bitmap.Lock<ColorPbgra32>(BitmapLockOptions.Read))
                {
                    int stride = @lock.Stride;
                    int num3 = dst.Stride;
                    byte* numPtr = (byte*) @lock.Scan0;
                    byte* numPtr2 = (byte*) dst.Scan0;
                    ColorPbgra32* pbgraPtr = (ColorPbgra32*) numPtr;
                    byte* numPtr3 = numPtr2;
                    for (int i = 0; i < size.Height; i++)
                    {
                        for (int j = 0; j < size.Width; j++)
                        {
                            numPtr3[j] = (pbgraPtr + j).B;
                        }
                        pbgraPtr += stride;
                        numPtr3 += num3;
                    }
                }
            }
        }
    }
}

