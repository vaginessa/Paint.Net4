namespace PaintDotNet
{
    using PaintDotNet.HistoryMementos;
    using System;

    internal class ExecutedHistoryMementoEventArgs : EventArgs
    {
        private HistoryMemento newHistoryMemento;

        public ExecutedHistoryMementoEventArgs(HistoryMemento newHistoryMemento)
        {
            this.newHistoryMemento = newHistoryMemento;
        }

        public HistoryMemento NewHistoryMemento =>
            this.newHistoryMemento;
    }
}

