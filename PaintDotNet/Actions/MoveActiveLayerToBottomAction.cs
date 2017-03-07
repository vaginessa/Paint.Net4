namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class MoveActiveLayerToBottomAction : DocumentWorkspaceAction
    {
        public MoveActiveLayerToBottomAction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento memento = null;
            int activeLayerIndex = documentWorkspace.ActiveLayerIndex;
            if (activeLayerIndex != 0)
            {
                HistoryMemento memento2 = new MoveLayerFunction(activeLayerIndex, 0).Execute(documentWorkspace);
                HistoryMemento[] actions = new HistoryMemento[] { memento2 };
                memento = new CompoundHistoryMemento(StaticName, StaticImage, actions);
                documentWorkspace.ActiveLayer = (Layer) documentWorkspace.Document.Layers[0];
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersMoveLayerDownIcon.png");

        public static string StaticName =>
            PdnResources.GetString("MoveLayerToBottom.HistoryMementoName");
    }
}

