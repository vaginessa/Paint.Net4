namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal sealed class EmptyHistoryMemento : HistoryMemento
    {
        public EmptyHistoryMemento(string name, ImageResource image) : base(name, image)
        {
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback) => 
            new EmptyHistoryMemento(base.Name, base.Image);
    }
}

