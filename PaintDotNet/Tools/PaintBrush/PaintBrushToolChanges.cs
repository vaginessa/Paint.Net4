namespace PaintDotNet.Tools.PaintBrush
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Tools.BrushBase;
    using System;
    using System.Collections.Generic;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal sealed class PaintBrushToolChanges : BrushToolChangesBase<PaintBrushToolChanges, PaintBrushTool>
    {
        public PaintBrushToolChanges(PaintBrushToolChanges oldChanges, IEnumerable<BrushInputPoint> newInputPoints) : base(oldChanges, newInputPoints)
        {
            this.WhichUserColor = oldChanges.WhichUserColor;
        }

        public PaintBrushToolChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, PaintDotNet.WhichUserColor whichUserColor, IEnumerable<BrushInputPoint> inputPoints) : base(drawingSettingsValues, inputPoints)
        {
            this.WhichUserColor = whichUserColor;
        }

        public ColorBgra BackgroundColor
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

        public ContentBlendMode BlendMode =>
            base.GetDrawingSettingValue<ContentBlendMode>(ToolSettings.Null.BlendMode);

        public PaintDotNet.BrushType BrushType =>
            base.GetDrawingSettingValue<PaintDotNet.BrushType>(ToolSettings.Null.Brush.Type);

        public ColorBgra ForegroundColor
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

        public System.Drawing.Drawing2D.HatchStyle HatchStyle =>
            base.GetDrawingSettingValue<System.Drawing.Drawing2D.HatchStyle>(ToolSettings.Null.Brush.HatchStyle);

        public ColorBgra PrimaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.PrimaryColor);

        public ColorBgra SecondaryColor =>
            base.GetDrawingSettingValue<ColorBgra32>(ToolSettings.Null.SecondaryColor);

        public PaintDotNet.WhichUserColor WhichUserColor { get; private set; }
    }
}

