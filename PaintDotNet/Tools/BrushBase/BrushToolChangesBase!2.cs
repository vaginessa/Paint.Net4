namespace PaintDotNet.Tools.BrushBase
{
    using PaintDotNet;
    using PaintDotNet.Brushes;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal abstract class BrushToolChangesBase<TDerived, TTool> : TransactedToolChanges<TDerived, TTool> where TDerived: BrushToolChangesBase<TDerived, TTool> where TTool: TransactedTool<TTool, TDerived>
    {
        [NonSerialized]
        private bool isInitialized;
        [NonSerialized]
        private BrushStrokeRenderCache renderCache;
        [NonSerialized]
        private BrushStrokeRenderData renderData;
        [NonSerialized]
        private object renderDataCurrencyToken;
        [NonSerialized]
        private BrushStamp stamp;
        private static readonly WeakCache<TupleStruct<double, double, double, bool>, CircleBrushStamp> stampCache;

        static BrushToolChangesBase()
        {
            BrushToolChangesBase<TDerived, TTool>.stampCache = new WeakCache<TupleStruct<double, double, double, bool>, CircleBrushStamp>(new Func<TupleStruct<double, double, double, bool>, CircleBrushStamp>(<>c<TDerived, TTool>.<>9.<.cctor>b__34_0));
        }

        protected BrushToolChangesBase(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints) : base(drawingSettingsValues)
        {
            this.InputPoints = new ReadOnlyCollection<BrushInputPoint>(inputPoints.ToArrayEx<BrushInputPoint>());
            if (this.InputPoints.Count == 0)
            {
                throw new ArgumentException("must have at least 1 inputPoint");
            }
        }

        protected BrushToolChangesBase(TDerived oldChanges, IEnumerable<BrushInputPoint> newInputPoints) : base(oldChanges.DrawingSettingsValues)
        {
            this.InputPoints = oldChanges.InputPoints.Concat<BrushInputPoint>(newInputPoints).ToArrayEx<BrushInputPoint>();
            this.stamp = oldChanges.Stamp;
            this.renderData = oldChanges.RenderData;
            this.renderData.AddInputPoints(newInputPoints);
            this.renderData.EnsureStrokeSamplesUpdated();
            this.renderCache = oldChanges.RenderCache;
            this.renderDataCurrencyToken = this.renderData.CreateCurrencyToken();
            this.isInitialized = true;
        }

        private void EnsureInitialized()
        {
            lock (base2)
            {
                if (!this.isInitialized)
                {
                    this.Initialize();
                    this.isInitialized = true;
                }
            }
        }

        protected void Initialize()
        {
            this.OnInitializing();
            this.stamp = BrushToolChangesBase<TDerived, TTool>.stampCache.Get(TupleStruct.Create<double, double, double, bool>((double) this.PenWidth, (double) this.Hardness, 1.0, this.Antialiasing));
            double num = this.PenWidth * 0.15;
            double stampSpacingPx = Math.Max(1.0, num);
            BrushStrokeLengthMetric lengthMetric = this.Antialiasing ? BrushStrokeLengthMetric.Euclidean : BrushStrokeLengthMetric.Anamorphic;
            this.renderData = new BrushStrokeRenderData(this.stamp.Size, stampSpacingPx, 7, lengthMetric);
            this.renderData.AddInputPoints(this.InputPoints);
            this.renderData.EnsureStrokeSamplesUpdated();
            this.renderDataCurrencyToken = this.renderData.CreateCurrencyToken();
            this.renderCache = new BrushStrokeRenderCache(this.renderData, this.stamp, 7);
            this.OnInitialized();
        }

        protected override void OnClonedWithNewDrawingSettingsValues(TDerived source)
        {
            this.Initialize();
            base.OnClonedWithNewDrawingSettingsValues(source);
        }

        protected override RectInt32 OnGetMaxRenderBounds()
        {
            this.EnsureInitialized();
            return this.renderData.StrokeRenderBounds;
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnInitializing()
        {
        }

        public bool Antialiasing =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.Antialiasing);

        public float Hardness =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Pen.Hardness);

        public IList<BrushInputPoint> InputPoints { get; private set; }

        public float PenWidth =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Pen.Width);

        public BrushStrokeRenderCache RenderCache
        {
            get
            {
                this.EnsureInitialized();
                return this.renderCache;
            }
        }

        public BrushStrokeRenderData RenderData
        {
            get
            {
                this.EnsureInitialized();
                return this.renderData;
            }
        }

        public object RenderDataCurrencyToken
        {
            get
            {
                this.EnsureInitialized();
                return this.renderDataCurrencyToken;
            }
        }

        public PaintDotNet.SelectionRenderingQuality SelectionRenderingQuality =>
            base.GetDrawingSettingValue<PaintDotNet.SelectionRenderingQuality>(ToolSettings.Null.Selection.RenderingQuality);

        public BrushStamp Stamp
        {
            get
            {
                this.EnsureInitialized();
                return this.stamp;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BrushToolChangesBase<TDerived, TTool>.<>c <>9;

            static <>c()
            {
                BrushToolChangesBase<TDerived, TTool>.<>c.<>9 = new BrushToolChangesBase<TDerived, TTool>.<>c();
            }

            internal CircleBrushStamp <.cctor>b__34_0(TupleStruct<double, double, double, bool> tup) => 
                new CircleBrushStamp(tup.Item1, tup.Item2, tup.Item3, tup.Item4);
        }
    }
}

