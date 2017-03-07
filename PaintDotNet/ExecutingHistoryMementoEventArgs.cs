namespace PaintDotNet
{
    using PaintDotNet.HistoryMementos;
    using System;

    internal class ExecutingHistoryMementoEventArgs : EventArgs
    {
        private PaintDotNet.HistoryMementos.HistoryMemento historyMemento;
        private bool mayAlterSuspendToolProperty;
        private bool suspendTool;

        public ExecutingHistoryMementoEventArgs(PaintDotNet.HistoryMementos.HistoryMemento historyMemento, bool mayAlterSuspendToolProperty, bool suspendTool)
        {
            this.historyMemento = historyMemento;
            this.mayAlterSuspendToolProperty = mayAlterSuspendToolProperty;
            this.suspendTool = suspendTool;
        }

        public PaintDotNet.HistoryMementos.HistoryMemento HistoryMemento =>
            this.historyMemento;

        public bool MayAlterSuspendTool =>
            this.mayAlterSuspendToolProperty;

        public bool SuspendTool
        {
            get => 
                this.suspendTool;
            set
            {
                if (!this.mayAlterSuspendToolProperty)
                {
                    ExceptionUtil.ThrowInvalidOperationException("May not alter the SuspendTool property when MayAlterSuspendToolProperty is false");
                }
                this.suspendTool = value;
            }
        }
    }
}

