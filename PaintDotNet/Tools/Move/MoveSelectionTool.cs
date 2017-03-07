namespace PaintDotNet.Tools.Move
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using PaintDotNet.Tools.Controls;
    using System;

    internal sealed class MoveSelectionTool : MoveTool
    {
        public MoveSelectionTool(DocumentWorkspace docWorkspace) : base(docWorkspace, PdnResources.GetImageResource("Icons.MoveSelectionToolIcon.png"), PdnResources.GetString("MoveSelectionTool.Name"), PdnResources.GetString("MoveSelectionTool.HelpText"), 'm', false, ToolBarConfigItems.None, false)
        {
        }

        protected override string OnGetHistoryMementoNameForChanges(MoveToolChanges oldChanges, MoveToolChanges newChanges)
        {
            switch (((newChanges != null) ? newChanges.EditingMode : oldChanges.EditingMode))
            {
                case TransformEditingMode.None:
                case TransformEditingMode.Custom:
                    return base.Name;

                case TransformEditingMode.Rotate:
                    return PdnResources.GetString("MoveSelectionTool.HistoryMemento.Rotate");

                case TransformEditingMode.Scale:
                    return PdnResources.GetString("MoveSelectionTool.HistoryMemento.Scale");

                case TransformEditingMode.Translate:
                    return PdnResources.GetString("MoveSelectionTool.HistoryMemento.Translate");

                case TransformEditingMode.MoveRotationAnchor:
                    return PdnResources.GetString("MoveSelectionTool.HistoryMemento.MoveRotationAnchor");
            }
            throw new InternalErrorException(ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(newChanges.EditingMode, "newChanges.EditingMode"));
        }
    }
}

