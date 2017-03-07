namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Resources;
    using System;

    internal sealed class EraseSelectionFunction : FillSelectionFunction
    {
        public EraseSelectionFunction() : base(ColorBgra.FromBgra(0xff, 0xff, 0xff, 0))
        {
        }

        protected override HistoryMemento OnPostRender(IHistoryWorkspace historyWorkspace)
        {
            HistoryMemento memento = base.OnPostRender(historyWorkspace);
            historyWorkspace.Selection.Reset();
            return memento;
        }

        protected override HistoryMemento OnPreRender(IHistoryWorkspace historyWorkspace)
        {
            HistoryMemento memento = base.OnPreRender(historyWorkspace);
            SelectionHistoryMemento memento2 = new SelectionHistoryMemento(string.Empty, null, historyWorkspace);
            HistoryMemento[] mementos = new HistoryMemento[] { memento, memento2 };
            return HistoryMemento.Combine(this.HistoryMementoName, this.HistoryMementoImage, mementos);
        }

        protected override ImageResource HistoryMementoImage =>
            PdnResources.GetImageResource("Icons.MenuEditEraseSelectionIcon.png");

        protected override string HistoryMementoName =>
            PdnResources.GetString("EraseSelectionAction.Name");
    }
}

