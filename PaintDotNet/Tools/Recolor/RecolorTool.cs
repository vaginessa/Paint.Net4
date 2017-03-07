namespace PaintDotNet.Tools.Recolor
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

    internal sealed class RecolorTool : BrushToolBase<RecolorTool, RecolorToolChanges, RecolorToolUI>
    {
        public RecolorTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.RecoloringToolIcon.png"), PdnResources.GetString("RecolorTool.Name"), PdnResources.GetString("RecolorTool.HelpText"), 'r', true, ToolBarConfigItems.None | ToolBarConfigItems.RecolorToolSamplingMode | ToolBarConfigItems.Tolerance)
        {
        }

        protected override RecolorToolChanges CreateChanges(RecolorToolChanges oldChanges, IEnumerable<BrushInputPoint> inputPoints) => 
            new RecolorToolChanges(oldChanges, inputPoints);

        protected override RecolorToolChanges CreateChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints, MouseButtonState rightButtonState) => 
            new RecolorToolChanges(drawingSettingsValues, (rightButtonState == MouseButtonState.Pressed) ? WhichUserColor.Secondary : WhichUserColor.Primary, inputPoints);

        [IteratorStateMachine(typeof(<CreateContentRenderers>d__2))]
        protected override IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, RecolorToolChanges changes)
        {
            yield return new RecolorToolContentRenderer(layer, changes);
        }

        protected override ContentBlendMode GetBlendMode(RecolorToolChanges changes) => 
            ContentBlendMode.Normal;

        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            Setting[] tails = new Setting[] { base.ToolSettings.PrimaryColor, base.ToolSettings.SecondaryColor, base.ToolSettings.Tolerance, base.ToolSettings.RecolorToolSamplingMode };
            return base.OnGetDrawingSettings().Concat<Setting>(tails);
        }

    }
}

