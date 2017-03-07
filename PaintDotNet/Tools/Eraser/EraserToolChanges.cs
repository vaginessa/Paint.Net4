namespace PaintDotNet.Tools.Eraser
{
    using PaintDotNet;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.BrushBase;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class EraserToolChanges : BrushToolChangesBase<EraserToolChanges, EraserTool>
    {
        public EraserToolChanges(EraserToolChanges oldChanges, IEnumerable<BrushInputPoint> newInputPoints) : base(oldChanges, newInputPoints)
        {
            this.WhichUserColor = oldChanges.WhichUserColor;
        }

        public EraserToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PaintDotNet.WhichUserColor whichUserColor, IEnumerable<BrushInputPoint> inputPoints) : base(drawingSettingsValues, inputPoints)
        {
            this.WhichUserColor = whichUserColor;
        }

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

        public PaintDotNet.WhichUserColor WhichUserColor { get; private set; }
    }
}

