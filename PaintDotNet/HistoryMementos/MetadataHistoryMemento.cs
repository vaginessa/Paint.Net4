namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class MetadataHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        public MetadataHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            Document document = new Document(1, 1);
            document.ReplaceMetadataFrom(historyWorkspace.Document);
            MetadataHistoryMementoData data = new MetadataHistoryMementoData(document);
            base.Data = data;
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            MetadataHistoryMemento memento = new MetadataHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            MetadataHistoryMementoData data = (MetadataHistoryMementoData) base.Data;
            this.historyWorkspace.Document.ReplaceMetadataFrom(data.Document);
            return memento;
        }

        [Serializable]
        private class MetadataHistoryMementoData : HistoryMementoData
        {
            private PaintDotNet.Document document;

            public MetadataHistoryMementoData(PaintDotNet.Document document)
            {
                this.document = document;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.document != null))
                {
                    this.document.Dispose();
                    this.document = null;
                }
                base.Dispose(disposing);
            }

            public PaintDotNet.Document Document =>
                this.document;
        }
    }
}

