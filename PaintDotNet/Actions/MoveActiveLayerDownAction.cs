namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class MoveActiveLayerDownAction : DocumentWorkspaceAction
    {
        public MoveActiveLayerDownAction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento memento = null;
            int activeLayerIndex = documentWorkspace.ActiveLayerIndex;
            if (activeLayerIndex != 0)
            {
                memento = new SwapLayerHistoryMemento(StaticName, StaticImage, documentWorkspace, activeLayerIndex, activeLayerIndex - 1).PerformUndo(null);
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersMoveLayerDownIcon.png");

        public static string StaticName =>
            PdnResources.GetString("MoveLayerDown.HistoryMementoName");
    }
}

