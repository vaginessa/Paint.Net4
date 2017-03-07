namespace PaintDotNet.Tools.Move
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.Tools;
    using PaintDotNet.Tools.Controls;
    using System;
    using System.Collections.Generic;

    internal sealed class MoveSelectedPixelsTool : MoveTool
    {
        public MoveSelectedPixelsTool(DocumentWorkspace docWorkspace) : base(docWorkspace, PdnResources.GetImageResource("Icons.MoveToolIcon.png"), PdnResources.GetString("MoveTool.Name"), PdnResources.GetString("MoveTool.HelpText"), 'm', false, ToolBarConfigItems.None | ToolBarConfigItems.Resampling, true)
        {
        }

        protected override IEnumerable<Setting> OnGetDrawingSettings() => 
            base.OnGetDrawingSettings().Concat<Setting>(base.ToolSettings.SecondaryColor);

        protected override string OnGetHistoryMementoNameForChanges(MoveToolChanges oldChanges, MoveToolChanges newChanges)
        {
            string name = base.Name;
            string historyMementoName = null;
            if ((oldChanges != null) && (newChanges != null))
            {
                if (oldChanges.SecondaryColor != newChanges.SecondaryColor)
                {
                    historyMementoName = TransactedTool<MoveTool, MoveToolChanges>.FoldHistoryMementoName(historyMementoName, name, PdnResources.GetString("TransactedTool.Changed.PrimaryColorOrSecondaryColor.HistoryMementoName"));
                }
                if (oldChanges.SelectionRenderingQuality != newChanges.SelectionRenderingQuality)
                {
                    historyMementoName = TransactedTool<MoveTool, MoveToolChanges>.FoldHistoryMementoName(historyMementoName, name, PdnResources.GetString("TransactedTool.Changed.SelectionRenderingQuality.HistoryMementoName"));
                }
                if (oldChanges.MoveToolResamplingAlgorithm != newChanges.MoveToolResamplingAlgorithm)
                {
                    historyMementoName = TransactedTool<MoveTool, MoveToolChanges>.FoldHistoryMementoName(historyMementoName, name, PdnResources.GetString("TransactedTool.Changed.SampleAllLayers.HistoryMementoName"));
                }
            }
            if (historyMementoName == null)
            {
                switch (((newChanges != null) ? newChanges.EditingMode : oldChanges.EditingMode))
                {
                    case TransformEditingMode.None:
                    case TransformEditingMode.Custom:
                        historyMementoName = base.Name;
                        goto Label_0111;

                    case TransformEditingMode.Rotate:
                        historyMementoName = PdnResources.GetString("MoveTool.HistoryMemento.Rotate");
                        goto Label_0111;

                    case TransformEditingMode.Scale:
                        historyMementoName = PdnResources.GetString("MoveTool.HistoryMemento.Scale");
                        goto Label_0111;

                    case TransformEditingMode.Translate:
                        historyMementoName = PdnResources.GetString("MoveTool.HistoryMemento.Translate");
                        goto Label_0111;

                    case TransformEditingMode.MoveRotationAnchor:
                        historyMementoName = PdnResources.GetString("MoveSelectionTool.HistoryMemento.MoveRotationAnchor");
                        goto Label_0111;
                }
                throw new InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(newChanges.EditingMode, "newChanges.EditingMode"));
            }
        Label_0111:;
            return (historyMementoName ?? name);
        }
    }
}

