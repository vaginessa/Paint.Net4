namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class CutAction
    {
        public void PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento memento;
            if (documentWorkspace.Selection.IsEmpty)
            {
                memento = null;
            }
            else
            {
                CopyToClipboardAction action = new CopyToClipboardAction(documentWorkspace);
                if (!action.PerformAction())
                {
                    memento = null;
                }
                else
                {
                    using (new PushNullToolMode(documentWorkspace))
                    {
                        HistoryMemento memento2 = new EraseSelectionFunction().Execute(documentWorkspace);
                        HistoryMemento[] actions = new HistoryMemento[] { memento2 };
                        CompoundHistoryMemento memento3 = new CompoundHistoryMemento(StaticName, StaticImage, actions);
                        memento = memento3;
                    }
                }
            }
            if (memento != null)
            {
                documentWorkspace.History.PushNewMemento(memento);
            }
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuEditCutIcon.png");

        public static string StaticName =>
            PdnResources.GetString("CutAction.Name");
    }
}

