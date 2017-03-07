namespace PaintDotNet.Tools.Recolor
{
    using PaintDotNet;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.BrushBase;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class RecolorToolChanges : BrushToolChangesBase<RecolorToolChanges, RecolorTool>
    {
        public RecolorToolChanges(RecolorToolChanges oldChanges, IEnumerable<BrushInputPoint> newInputPoints) : base(oldChanges, newInputPoints)
        {
            this.WhichUserColor = oldChanges.WhichUserColor;
        }

        public RecolorToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PaintDotNet.WhichUserColor whichUserColor, IEnumerable<BrushInputPoint> inputPoints) : base(drawingSettingsValues, inputPoints)
        {
            this.WhichUserColor = whichUserColor;
        }

        public ColorBgra BasisColor
        {
            get
            {
                if (this.WhichUserColor != PaintDotNet.WhichUserColor.Primary)
                {
                    return this.PrimaryColor;
                }
                return this.SecondaryColor;
            }
        }

        public ColorBgra FillColor
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

        public RecolorToolSamplingMode SamplingMode =>
            base.GetDrawingSettingValue<RecolorToolSamplingMode>(ToolSettings.Null.RecolorToolSamplingMode);

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public float Tolerance =>
            base.GetDrawingSettingValue<float>(ToolSettings.Null.Tolerance);

        public PaintDotNet.WhichUserColor WhichUserColor { get; private set; }
    }
}

