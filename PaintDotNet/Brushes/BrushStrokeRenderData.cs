namespace PaintDotNet.Brushes
{
    using PaintDotNet.Collections;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class BrushStrokeRenderData
    {
        private SegmentedList<BrushInputPoint> inputPoints;
        private ReadOnlyCollection<BrushInputPoint> inputPointsRO;
        private ReaderWriterLock rwLock;
        private SpriteTileSorter<int> spriteTileSorter;
        private SizeDouble stampSize;
        private double stampSpacingPx;
        private BrushStrokePath strokePath;
        private SegmentedList<BrushStrokeSample> strokeSamples;
        private ReadOnlyCollection<BrushStrokeSample> strokeSamplesRO;

        public BrushStrokeRenderData(SizeDouble stampSize, double stampSpacingPx, int tileEdgeLog2, BrushStrokeLengthMetric lengthMetric)
        {
            Validate.Begin().IsFinite(stampSize.Width, "stampSize.Width").IsFinite(stampSize.Height, "stampSize.Height").IsPositive(stampSize.Width, "stampSize.Width").IsPositive(stampSize.Height, "stampSize.Height").IsPositive(stampSpacingPx, "stampSpacingPx").IsNotNegative(tileEdgeLog2, "tileEdgeLog2").Check();
            this.rwLock = new ReaderWriterLock();
            this.spriteTileSorter = new SpriteTileSorter<int>(TransactedToolChanges.MaxMaxRenderBounds.Size, tileEdgeLog2);
            this.strokePath = new BrushStrokePath(lengthMetric);
            this.stampSize = stampSize;
            this.stampSpacingPx = stampSpacingPx;
            this.inputPoints = new SegmentedList<BrushInputPoint>();
            this.inputPointsRO = new ReadOnlyCollection<BrushInputPoint>(this.inputPoints);
            this.strokeSamples = new SegmentedList<BrushStrokeSample>();
            this.strokeSamplesRO = new ReadOnlyCollection<BrushStrokeSample>(this.strokeSamples);
        }

        public void AddInputPoint(BrushInputPoint inputPoint)
        {
            using (this.rwLock.UseWriteLock())
            {
                this.AddInputPointWhileLocked(inputPoint);
            }
        }

        public void AddInputPoints(IEnumerable<BrushInputPoint> inputPoints)
        {
            using (this.rwLock.UseWriteLock())
            {
                foreach (BrushInputPoint point in inputPoints)
                {
                    this.AddInputPointWhileLocked(point);
                }
            }
        }

        private void AddInputPointWhileLocked(BrushInputPoint inputPoint)
        {
            this.inputPoints.Add(inputPoint);
            this.strokePath.TryAddPoint(inputPoint.Location);
        }

        public object CreateCurrencyToken()
        {
            using (this.rwLock.UseReadLock())
            {
                return this.CreateCurrencyTokenWhileLocked();
            }
        }

        private CurrencyToken CreateCurrencyTokenWhileLocked() => 
            new CurrencyToken(this, this.inputPoints.Count, this.strokeSamples.Count);

        public void EnsureStrokeSamplesUpdated()
        {
            using (this.rwLock.UseWriteLock())
            {
                int num;
                if (this.strokePath.Points.Count == 0)
                {
                    num = 0;
                }
                else
                {
                    double d = this.strokePath.Length / this.stampSpacingPx;
                    int num4 = (int) Math.Floor(d);
                    num = num4 + 1;
                }
                for (int i = this.strokeSamples.Count; i < num; i++)
                {
                    double length = i * this.stampSpacingPx;
                    PointDouble pointAtLength = this.strokePath.GetPointAtLength(length);
                    BrushStrokeSample item = new BrushStrokeSample(pointAtLength, 1.0);
                    this.strokeSamples.Add(item);
                    SizeDouble size = new SizeDouble(this.stampSize.Width * item.StampScale, this.stampSize.Height * item.StampScale);
                    RectDouble bounds = RectDouble.FromCenter(item.Center, size);
                    Sprite<int> sprite = new Sprite<int>(bounds, i);
                    this.spriteTileSorter.Add(sprite);
                }
            }
        }

        private IList<int> EnumerateStrokeSampleIndicesBetweenCurrencyTokens(CurrencyToken oldCurrencyToken, CurrencyToken newCurrencyToken)
        {
            Validate.Begin().IsNotNull<CurrencyToken>(oldCurrencyToken, "oldCurrencyToken").IsNotNull<CurrencyToken>(newCurrencyToken, "newCurrencyToken").Check();
            int count = newCurrencyToken.StrokeSamplesCount - oldCurrencyToken.StrokeSamplesCount;
            int startIndex = oldCurrencyToken.StrokeSamplesCount;
            return new FuncList<int>(count, i => i + startIndex);
        }

        public IList<int> EnumerateStrokeSampleIndicesBetweenCurrencyTokens(object oldCurrencyToken, object newCurrencyToken) => 
            this.EnumerateStrokeSampleIndicesBetweenCurrencyTokens((CurrencyToken) oldCurrencyToken, (CurrencyToken) newCurrencyToken);

        private IList<int?> EnumerateStrokeSampleIndicesForCurrencyToken(CurrencyToken currencyToken)
        {
            Validate.IsNotNull<CurrencyToken>(currencyToken, "currencyToken");
            return new FuncList<int?>(currencyToken.StrokeSamplesCount, i => new int?(i));
        }

        public IList<int?> EnumerateStrokeSampleIndicesForCurrencyToken(object currencyToken) => 
            this.EnumerateStrokeSampleIndicesForCurrencyToken((CurrencyToken) currencyToken);

        private IList<int> EnumerateStrokeSampleIndicesInRectWhileLockedAndUpdated(RectInt32 bounds) => 
            this.spriteTileSorter.GetSpriteIndicesInRect(bounds).Select<int, int>(si => this.spriteTileSorter.Sprites[si].Data);

        private IList<int?> GetStrokeSampleIndicesInRect(RectInt32 bounds, CurrencyToken currencyToken)
        {
            Validate.IsNotNull<CurrencyToken>(currencyToken, "currencyToken");
            if (this != currencyToken.RenderData)
            {
                throw new ArgumentException();
            }
            this.EnsureStrokeSamplesUpdated();
            using (this.rwLock.UseReadLock())
            {
                int tokenStrokeSampleCount = currencyToken.StrokeSamplesCount;
                return this.EnumerateStrokeSampleIndicesInRectWhileLockedAndUpdated(bounds).Select<int, int?>(delegate (int si) {
                    if (si >= tokenStrokeSampleCount)
                    {
                        return null;
                    }
                    return new int?(si);
                });
            }
        }

        public IList<int?> GetStrokeSampleIndicesInRect(RectInt32 bounds, object currencyToken)
        {
            Validate.IsNotNull<object>(currencyToken, "currencyToken");
            return this.GetStrokeSampleIndicesInRect(bounds, (CurrencyToken) currencyToken);
        }

        public IList<int?> GetStrokeSampleIndicesInRect(RectInt32 bounds, object oldCurrencyToken, object newCurrencyToken)
        {
            if (oldCurrencyToken == null)
            {
                return this.GetStrokeSampleIndicesInRect(bounds, newCurrencyToken);
            }
            Validate.Begin().IsNotNull<object>(oldCurrencyToken, "oldCurrencyToken").IsNotNull<object>(newCurrencyToken, "newCurrencyToken").Check();
            IList<int?> strokeSampleIndicesInRect = this.GetStrokeSampleIndicesInRect(bounds, oldCurrencyToken);
            int count = strokeSampleIndicesInRect.Count;
            IList<int?> source = this.GetStrokeSampleIndicesInRect(bounds, newCurrencyToken);
            int num2 = source.Count;
            int startIndex = num2;
            int num4 = num2;
            for (int i = num2 - 1; i >= 0; i--)
            {
                if (i < count)
                {
                    int? nullable = strokeSampleIndicesInRect[i];
                    if (nullable.HasValue)
                    {
                        break;
                    }
                }
                startIndex = i;
            }
            if ((startIndex == num2) || ((num4 - startIndex) == 0))
            {
                return Array.Empty<int?>();
            }
            return new ListSegment<int?>(source, startIndex, num4 - startIndex);
        }

        public IList<BrushInputPoint> InputPoints =>
            this.inputPointsRO;

        public BrushStrokeLengthMetric LengthMetric =>
            this.strokePath.LengthMetric;

        public RectInt32 StrokeRenderBounds
        {
            get
            {
                using (this.rwLock.UseReadLock())
                {
                    RectDouble allSpriteBounds = this.spriteTileSorter.AllSpriteBounds;
                    if (allSpriteBounds.IsEmpty)
                    {
                        return RectInt32.Empty;
                    }
                    return allSpriteBounds.Int32Bound;
                }
            }
        }

        public IList<BrushStrokeSample> StrokeSamples =>
            this.strokeSamplesRO;

        public int TileEdgeLog2 =>
            this.spriteTileSorter.TileEdgeLog2;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BrushStrokeRenderData.<>c <>9 = new BrushStrokeRenderData.<>c();
            public static Func<int, int?> <>9__30_0;

            internal int? <EnumerateStrokeSampleIndicesForCurrencyToken>b__30_0(int i) => 
                new int?(i);
        }

        private sealed class CurrencyToken
        {
            private int inputPointsCount;
            private BrushStrokeRenderData renderData;
            private int strokeSamplesCount;

            public CurrencyToken(BrushStrokeRenderData renderData, int inputPointsCount, int strokeSamplesCount)
            {
                Validate.IsNotNull<BrushStrokeRenderData>(renderData, "renderData");
                this.renderData = renderData;
                this.inputPointsCount = inputPointsCount;
                this.strokeSamplesCount = strokeSamplesCount;
            }

            public int InputPointsCount =>
                this.inputPointsCount;

            public BrushStrokeRenderData RenderData =>
                this.renderData;

            public int StrokeSamplesCount =>
                this.strokeSamplesCount;
        }
    }
}

