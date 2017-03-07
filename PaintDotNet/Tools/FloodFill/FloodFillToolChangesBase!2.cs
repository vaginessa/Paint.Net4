namespace PaintDotNet.Tools.FloodFill
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal abstract class FloodFillToolChangesBase<TDerived, TTool> : TransactedToolChanges<TDerived, TTool> where TDerived: FloodFillToolChangesBase<TDerived, TTool> where TTool: TransactedTool<TTool, TDerived>
    {
        private PaintDotNet.FloodMode? floodModeOverride;
        private PointDouble originPoint;

        public FloodFillToolChangesBase(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PointDouble originPoint, PaintDotNet.FloodMode? floodModeOverride) : base(drawingSettingsValues)
        {
            this.originPoint = originPoint;
            this.floodModeOverride = floodModeOverride;
        }

        protected override void OnClonedWithNewDrawingSettingsValues(TDerived source)
        {
            if (this.floodModeOverride.HasValue && (source.FloodModeBase != this.FloodModeBase))
            {
                using (base.UseChangeScope())
                {
                    this.floodModeOverride = null;
                }
            }
            base.OnClonedWithNewDrawingSettingsValues(source);
        }

        public PaintDotNet.FloodMode FloodMode
        {
            get
            {
                PaintDotNet.FloodMode? floodModeOverride = this.FloodModeOverride;
                if (!floodModeOverride.HasValue)
                {
                    return this.FloodModeBase;
                }
                return floodModeOverride.GetValueOrDefault();
            }
        }

        public PaintDotNet.FloodMode FloodModeBase =>
            base.GetDrawingSettingValue<PaintDotNet.FloodMode>(ToolSettings.Null.FloodMode);

        public PaintDotNet.FloodMode? FloodModeOverride =>
            this.floodModeOverride;

        public PointDouble OriginPoint =>
            this.originPoint;

        public PointInt32 OriginPointInt32 =>
            PointDouble.Floor(this.originPoint);

        public bool SampleAllLayers =>
            base.GetDrawingSettingValue<bool>(ToolSettings.Null.SampleAllLayers);

        public float Tolerance =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Tolerance);
    }
}

