namespace PaintDotNet.Tools.CloneStamp
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.BrushBase;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class CloneStampToolChanges : BrushToolChangesBase<CloneStampToolChanges, CloneStampTool>
    {
        public CloneStampToolChanges(CloneStampToolChanges oldChanges, IEnumerable<BrushInputPoint> newInputPoints) : base(oldChanges, newInputPoints)
        {
            this.WhichUserColor = oldChanges.WhichUserColor;
            this.SourceLayerIndex = oldChanges.SourceLayerIndex;
            this.SourceSamplingOffset = oldChanges.SourceSamplingOffset;
        }

        public CloneStampToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PaintDotNet.WhichUserColor whichUserColor, IEnumerable<BrushInputPoint> inputPoints, int sourceLayerIndex, PointInt32 sourceSamplingOffset) : base(drawingSettingsValues, inputPoints)
        {
            this.WhichUserColor = whichUserColor;
            this.SourceLayerIndex = sourceLayerIndex;
            this.SourceSamplingOffset = sourceSamplingOffset;
        }

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public ColorBgra Color
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.SecondaryColor;
                }
                return this.PrimaryColor;
            }
        }

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public int SourceLayerIndex { get; private set; }

        public PointInt32 SourceSamplingOffset { get; private set; }

        public PaintDotNet.WhichUserColor WhichUserColor { get; private set; }
    }
}

