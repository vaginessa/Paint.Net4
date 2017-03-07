namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal sealed class SelectionHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        public SelectionHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : this(name, image, historyWorkspace, historyWorkspace.Selection.Save())
        {
        }

        public SelectionHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, SelectionData selectionData) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            base.Data = new SelectionHistoryMementoData(selectionData);
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            SelectionHistoryMemento memento = new SelectionHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            SelectionHistoryMementoData data = (SelectionHistoryMementoData) base.Data;
            SelectionData savedSelectionData = data.SavedSelectionData;
            this.historyWorkspace.Selection.Restore(savedSelectionData);
            return memento;
        }

        [Serializable]
        private sealed class SelectionHistoryMementoData : HistoryMementoData
        {
            private SelectionData savedSelectionData;

            public SelectionHistoryMementoData(SelectionData savedSelectionData)
            {
                this.savedSelectionData = savedSelectionData;
            }

            protected override void Dispose(bool disposing)
            {
                this.savedSelectionData = null;
                base.Dispose(disposing);
            }

            public SelectionData SavedSelectionData =>
                this.savedSelectionData;
        }
    }
}

