namespace PaintDotNet.Tools.PaintBrush
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Settings;
    using PaintDotNet.Tools.BrushBase;
    using PaintDotNet.UI.Input;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class PaintBrushTool : BrushToolBase<PaintBrushTool, PaintBrushToolChanges, PaintBrushToolUI>
    {
        public PaintBrushTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.PaintBrushToolIcon.png"), PdnResources.GetString("PaintBrushTool.Name"), PdnResources.GetString("PaintBrushTool.HelpText"), 'b', true, ToolBarConfigItems.BlendMode | ToolBarConfigItems.Brush)
        {
        }

        protected override PaintBrushToolChanges CreateChanges(PaintBrushToolChanges oldChanges, IEnumerable<BrushInputPoint> inputPoints) => 
            new PaintBrushToolChanges(oldChanges, inputPoints);

        protected override PaintBrushToolChanges CreateChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints, MouseButtonState rightButtonState) => 
            new PaintBrushToolChanges(drawingSettingsValues, (rightButtonState == MouseButtonState.Pressed) ? WhichUserColor.Secondary : WhichUserColor.Primary, inputPoints);

        [IteratorStateMachine(typeof(<CreateContentRenderers>d__2))]
        protected override IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, PaintBrushToolChanges changes)
        {
            yield return new PaintBrushToolContentRenderer(layer.Width, layer.Height, changes);
        }

        protected override ContentBlendMode GetBlendMode(PaintBrushToolChanges changes) => 
            changes.BlendMode;

        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            Setting[] tails = new Setting[] { base.ToolSettings.PrimaryColor, base.ToolSettings.SecondaryColor, base.ToolSettings.Brush.Type, base.ToolSettings.Brush.HatchStyle, base.ToolSettings.BlendMode };
            return base.OnGetDrawingSettings().Concat<Setting>(tails);
        }

    }
}

