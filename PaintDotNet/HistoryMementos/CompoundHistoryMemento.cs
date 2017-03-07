namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;
    using System.Collections.Generic;

    internal class CompoundHistoryMemento : HistoryMemento
    {
        private List<HistoryMemento> mementos;

        public CompoundHistoryMemento(string name, ImageResource image) : this(name, image, Array.Empty<HistoryMemento>())
        {
        }

        public CompoundHistoryMemento(string name, ImageResource image, IEnumerable<HistoryMemento> actions) : base(name, image)
        {
            this.mementos = new List<HistoryMemento>(actions);
        }

        public CompoundHistoryMemento(string name, ImageResource image, params HistoryMemento[] actions) : base(name, image)
        {
            this.mementos = new List<HistoryMemento>(actions);
        }

        public void AddMemento(HistoryMemento newHA)
        {
            this.mementos.Add(newHA);
        }

        protected override void OnFlush()
        {
            for (int i = 0; i < this.mementos.Count; i++)
            {
                if (this.mementos[i] != null)
                {
                    this.mementos[i].Flush();
                }
            }
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            List<HistoryMemento> actions = new List<HistoryMemento>(this.mementos.Count);
            for (int i = 0; i < this.mementos.Count; i++)
            {
                HistoryMemento memento2 = this.mementos[(this.mementos.Count - i) - 1];
                HistoryMemento item = null;
                if (memento2 != null)
                {
                    item = memento2.PerformUndo(progressCallback);
                }
                actions.Add(item);
            }
            return new CompoundHistoryMemento(base.Name, base.Image, actions);
        }

        public int MementoCount =>
            this.mementos.Count;
    }
}

