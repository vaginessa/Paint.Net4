namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class MoveActiveLayerToTopAction : DocumentWorkspaceAction
    {
        public MoveActiveLayerToTopAction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento memento = null;
            int activeLayerIndex = documentWorkspace.ActiveLayerIndex;
            if (activeLayerIndex != (documentWorkspace.Document.Layers.Count - 1))
            {
                HistoryMemento memento2 = new MoveLayerFunction(activeLayerIndex, documentWorkspace.Document.Layers.Count - 1).Execute(documentWorkspace);
                HistoryMemento[] actions = new HistoryMemento[] { memento2 };
                memento = new CompoundHistoryMemento(StaticName, StaticImage, actions);
                documentWorkspace.ActiveLayer = (Layer) documentWorkspace.Document.Layers[documentWorkspace.Document.Layers.Count - 1];
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.png");

        public static string StaticName =>
            PdnResources.GetString("MoveLayerToTop.HistoryMementoName");
    }
}

