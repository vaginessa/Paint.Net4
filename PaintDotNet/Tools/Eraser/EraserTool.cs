namespace PaintDotNet.Tools.Eraser
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

    internal sealed class EraserTool : BrushToolBase<EraserTool, EraserToolChanges, EraserToolUI>
    {
        public EraserTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.EraserToolIcon.png"), PdnResources.GetString("EraserTool.Name"), PdnResources.GetString("EraserTool.HelpText"), 'e', true, ToolBarConfigItems.None)
        {
        }

        protected override EraserToolChanges CreateChanges(EraserToolChanges oldChanges, IEnumerable<BrushInputPoint> inputPoints) => 
            new EraserToolChanges(oldChanges, inputPoints);

        protected override EraserToolChanges CreateChanges(IEnumerable<KeyValuePair<string, object>> drawingSettingsValues, IEnumerable<BrushInputPoint> inputPoints, MouseButtonState rightButtonState) => 
            new EraserToolChanges(drawingSettingsValues, (rightButtonState == MouseButtonState.Pressed) ? WhichUserColor.Secondary : WhichUserColor.Primary, inputPoints);

        [IteratorStateMachine(typeof(<CreateContentRenderers>d__2))]
        protected override IEnumerable<IMaskedRenderer<ColorBgra, ColorAlpha8>> CreateContentRenderers(BitmapLayer layer, EraserToolChanges changes)
        {
            yield return new EraserToolContentRenderer(layer.Width, layer.Height, changes);
        }

        protected override ContentBlendMode GetBlendMode(EraserToolChanges changes) => 
            ContentBlendMode.Overwrite;

        protected override IEnumerable<Setting> OnGetDrawingSettings()
        {
            Setting[] tails = new Setting[] { base.ToolSettings.PrimaryColor, base.ToolSettings.SecondaryColor };
            return base.OnGetDrawingSettings().Concat<Setting>(tails);
        }

    }
}

