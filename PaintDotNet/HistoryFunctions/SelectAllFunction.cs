namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class SelectAllFunction : HistoryFunction
    {
        public SelectAllFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, StaticImage, historyWorkspace);
            base.EnterCriticalRegion();
            using (historyWorkspace.Selection.UseChangeScope())
            {
                historyWorkspace.Selection.Reset();
                historyWorkspace.Selection.SetContinuation(historyWorkspace.Document.Bounds(), SelectionCombineMode.Replace);
                historyWorkspace.Selection.CommitContinuation();
            }
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource("Icons.MenuEditSelectAllIcon.png");

        public static string StaticName =>
            PdnResources.GetString("SelectAllAction.Name");
    }
}

