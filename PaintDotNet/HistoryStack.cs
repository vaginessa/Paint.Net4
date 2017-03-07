namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Runtime;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class HistoryStack
    {
        private readonly ProtectedRegion disallowPushNewMementoRegion = new ProtectedRegion("DisallowPushNewMemento", ProtectedRegionOptions.ErrorOnMultithreadedAccess);
        private DocumentWorkspace documentWorkspace;
        private int isExecutingMemento;
        private SegmentedList<HistoryMemento> redoStack;
        private ReadOnlyCollection<HistoryMemento> redoStackRO;
        private int stepGroupDepth;
        private readonly ProtectedRegion syncRegion = new ProtectedRegion("HistoryStack", ProtectedRegionOptions.ErrorOnMultithreadedAccess);
        private SegmentedList<HistoryMemento> undoStack;
        private ReadOnlyCollection<HistoryMemento> undoStackRO;

        [field: CompilerGenerated]
        public event EventHandler Changed;

        [field: CompilerGenerated]
        public event EventHandler Changing;

        [field: CompilerGenerated]
        public event ExecutedHistoryMementoEventHandler ExecutedHistoryMemento;

        [field: CompilerGenerated]
        public event ExecutingHistoryMementoEventHandler ExecutingHistoryMemento;

        [field: CompilerGenerated]
        public event EventHandler FinishedStepGroup;

        [field: CompilerGenerated]
        public event EventHandler HistoryFlushed;

        [field: CompilerGenerated]
        public event EventHandler NewHistoryMemento;

        [field: CompilerGenerated]
        public event EventHandler SteppedBackward;

        [field: CompilerGenerated]
        public event EventHandler SteppedForward;

        public HistoryStack(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            this.undoStack = new SegmentedList<HistoryMemento>();
            this.redoStack = new SegmentedList<HistoryMemento>();
        }

        public void BeginStepGroup()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.stepGroupDepth++;
            }
        }

        public void ClearAll()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.OnChanging();
                foreach (HistoryMemento memento in this.undoStack)
                {
                    memento.Flush();
                }
                foreach (HistoryMemento memento2 in this.redoStack)
                {
                    memento2.Flush();
                }
                this.undoStack.Clear();
                this.redoStack.Clear();
                this.OnChanged();
                this.OnHistoryFlushed();
            }
        }

        public void ClearRedoStack()
        {
            using (this.syncRegion.UseEnterScope())
            {
                foreach (HistoryMemento memento in this.redoStack)
                {
                    memento.Flush();
                }
                this.OnChanging();
                this.redoStack.Clear();
                this.OnChanged();
            }
        }

        public void EndStepGroup()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.stepGroupDepth--;
                if (this.stepGroupDepth == 0)
                {
                    this.OnFinishedStepGroup();
                }
            }
        }

        private void OnChanged()
        {
            this.Changed.Raise(this);
        }

        private void OnChanging()
        {
            this.Changing.Raise(this);
        }

        private void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (this.ExecutedHistoryMemento != null)
            {
                this.ExecutedHistoryMemento(this, e);
            }
        }

        private void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            if (this.ExecutingHistoryMemento != null)
            {
                this.ExecutingHistoryMemento(this, e);
            }
        }

        private void OnFinishedStepGroup()
        {
            this.FinishedStepGroup.Raise(this);
        }

        private void OnHistoryFlushed()
        {
            this.HistoryFlushed.Raise(this);
        }

        private void OnNewHistoryMemento()
        {
            this.NewHistoryMemento.Raise(this);
        }

        private void OnSteppedBackward()
        {
            this.SteppedBackward.Raise(this);
        }

        private void OnSteppedForward()
        {
            this.SteppedForward.Raise(this);
        }

        public void PerformChanged()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.OnChanged();
            }
        }

        private void PopExecutingMemento()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.isExecutingMemento--;
            }
        }

        private void PushExecutingMemento()
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.isExecutingMemento++;
            }
        }

        public void PushNewMemento(HistoryMemento value)
        {
            using (this.syncRegion.UseEnterScope())
            {
                if (this.disallowPushNewMementoRegion.IsThreadEntered)
                {
                    throw new InternalErrorException();
                }
                using (this.disallowPushNewMementoRegion.UseEnterScope())
                {
                    CleanupManager.RequestCleanup();
                    this.OnChanging();
                    this.ClearRedoStack();
                    this.undoStack.Add(value);
                    this.OnNewHistoryMemento();
                    this.OnChanged();
                    value.Flush();
                    CleanupManager.RequestCleanup();
                }
            }
        }

        public void StepBackward(IWin32Window owner)
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.PushExecutingMemento();
                try
                {
                    this.StepBackwardImpl(owner);
                }
                finally
                {
                    this.PopExecutingMemento();
                }
            }
        }

        private void StepBackwardImpl(IWin32Window owner)
        {
            HistoryMemento historyMemento = this.undoStack[this.undoStack.Count - 1];
            ToolHistoryMemento memento2 = historyMemento as ToolHistoryMemento;
            if ((memento2 != null) && (memento2.ToolType != this.documentWorkspace.GetToolType()))
            {
                this.documentWorkspace.SetToolFromType(memento2.ToolType);
                this.StepBackwardImpl(owner);
            }
            else
            {
                this.OnChanging();
                ExecutingHistoryMementoEventArgs e = new ExecutingHistoryMementoEventArgs(historyMemento, true, false);
                if ((memento2 == null) && (historyMemento.SeriesGuid == Guid.Empty))
                {
                    e.SuspendTool = true;
                }
                this.OnExecutingHistoryMemento(e);
                ReferenceValue changes = null;
                System.Type type = null;
                if (e.SuspendTool)
                {
                    TransactedTool tool = this.documentWorkspace.Tool as TransactedTool;
                    if (tool != null)
                    {
                        type = tool.GetType();
                        tool.ForceCancelDrawingOrEditing();
                        if (tool.State == TransactedToolState.Dirty)
                        {
                            changes = tool.Changes;
                            tool.CancelChanges();
                        }
                    }
                    this.documentWorkspace.PushNullTool();
                }
                HistoryMemento memento3 = this.undoStack[this.undoStack.Count - 1];
                ExecutingHistoryMementoEventArgs args2 = new ExecutingHistoryMementoEventArgs(memento3, false, e.SuspendTool);
                this.OnExecutingHistoryMemento(args2);
                using (this.disallowPushNewMementoRegion.UseEnterScope())
                {
                    HistoryMemento item = this.undoStack[this.undoStack.Count - 1].PerformUndo(null);
                    this.undoStack.RemoveAt(this.undoStack.Count - 1);
                    this.redoStack.Insert(0, item);
                    ExecutedHistoryMementoEventArgs args3 = new ExecutedHistoryMementoEventArgs(item);
                    this.OnExecutedHistoryMemento(args3);
                    this.OnChanged();
                    this.OnSteppedBackward();
                    item.Flush();
                }
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                    if (changes != null)
                    {
                        ((TransactedTool) this.documentWorkspace.Tool).RestoreChanges(changes);
                    }
                }
            }
            if (this.stepGroupDepth == 0)
            {
                this.OnFinishedStepGroup();
            }
        }

        public void StepForward(IWin32Window owner)
        {
            using (this.syncRegion.UseEnterScope())
            {
                this.PushExecutingMemento();
                try
                {
                    this.StepForwardImpl(owner);
                }
                finally
                {
                    this.PopExecutingMemento();
                }
            }
        }

        private void StepForwardImpl(IWin32Window owner)
        {
            HistoryMemento historyMemento = this.redoStack[0];
            ToolHistoryMemento memento2 = historyMemento as ToolHistoryMemento;
            if ((memento2 != null) && (memento2.ToolType != this.documentWorkspace.GetToolType()))
            {
                this.documentWorkspace.SetToolFromType(memento2.ToolType);
                this.StepForwardImpl(owner);
            }
            else
            {
                this.OnChanging();
                ExecutingHistoryMementoEventArgs e = new ExecutingHistoryMementoEventArgs(historyMemento, true, false);
                if ((memento2 == null) && (historyMemento.SeriesGuid != Guid.Empty))
                {
                    e.SuspendTool = true;
                }
                this.OnExecutingHistoryMemento(e);
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }
                HistoryMemento memento3 = this.redoStack[0];
                ExecutingHistoryMementoEventArgs args2 = new ExecutingHistoryMementoEventArgs(memento3, false, e.SuspendTool);
                this.OnExecutingHistoryMemento(args2);
                using (this.disallowPushNewMementoRegion.UseEnterScope())
                {
                    HistoryMemento item = memento3.PerformUndo(null);
                    this.redoStack.RemoveAt(0);
                    this.undoStack.Add(item);
                    ExecutedHistoryMementoEventArgs args3 = new ExecutedHistoryMementoEventArgs(item);
                    this.OnExecutedHistoryMemento(args3);
                    this.OnChanged();
                    this.OnSteppedForward();
                    item.Flush();
                }
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }
            }
            if (this.stepGroupDepth == 0)
            {
                this.OnFinishedStepGroup();
            }
        }

        public bool IsExecutingMemento =>
            (this.isExecutingMemento > 0);

        public IList<HistoryMemento> RedoStack
        {
            get
            {
                using (this.syncRegion.UseEnterScope())
                {
                    if (this.redoStackRO == null)
                    {
                        this.redoStackRO = new ReadOnlyCollection<HistoryMemento>(this.redoStack);
                    }
                    return this.redoStackRO;
                }
            }
        }

        public IList<HistoryMemento> UndoStack
        {
            get
            {
                using (this.syncRegion.UseEnterScope())
                {
                    if (this.undoStackRO == null)
                    {
                        this.undoStackRO = new ReadOnlyCollection<HistoryMemento>(this.undoStack);
                    }
                    return this.undoStackRO;
                }
            }
        }
    }
}

