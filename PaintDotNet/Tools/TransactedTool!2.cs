namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Settings;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal abstract class TransactedTool<TDerived, TChanges> : TransactedTool where TDerived: TransactedTool<TDerived, TChanges> where TChanges: TransactedToolChanges<TChanges, TDerived>
    {
        private HistoryMemento beforeDrawingMemento;
        private TChanges changes;
        private TChanges changesBeforeEditing;
        private readonly ProtectedRegion commitChangesRegion;
        private TransactedToolDrawingAgent<TChanges> drawingAgent;
        private HashSet<string> drawingSettingPaths;
        private KeyValuePair<string, object>[] drawingSettingValuesBackup;
        private TransactedToolEditingAgent<TChanges> editingAgent;
        private int ignoreDrawingSettingsValueChangedCount;
        private bool isCommitting;
        private TransactedToolState state;
        private static readonly TimeSpan toolSettingsDebounceExpireInterval;
        private DateTime toolSettingsDebounceExpireTime;
        private Timer toolSettingsDebounceTimer;
        private TransactedToolEditingAgent<TChanges> toolSettingsEditingAgent;
        private readonly Action updateStatusCallback;

        static TransactedTool()
        {
            TransactedTool<TDerived, TChanges>.toolSettingsDebounceExpireInterval = TimeSpan.FromMilliseconds(500.0);
        }

        protected TransactedTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems, bool isCommitSupported) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems)
        {
            this.commitChangesRegion = new ProtectedRegion("CommitChanges", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
            this.updateStatusCallback = new Action(this.UpdateStatus);
            this.state = TransactedToolState.Inactive;
            base.IsCommitSupported = isCommitSupported;
        }

        public void BeginDrawing(TransactedToolDrawingAgent<TChanges> agent, TChanges initialChanges)
        {
            this.BeginDrawing(agent, initialChanges, base.Name, base.Image);
        }

        public void BeginDrawing(TransactedToolDrawingAgent<TChanges> agent, TChanges initialChanges, HistoryMemento beforeDrawingMemento)
        {
            Validate.Begin().IsNotNull<TransactedToolDrawingAgent<TChanges>>(agent, "agent").IsNotNull<TChanges>(initialChanges, "changes").IsNotNull<HistoryMemento>(beforeDrawingMemento, "beforeDrawingMemento").Check();
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            agent.VerifyIsNotActive();
            if (this.State != TransactedToolState.Idle)
            {
                throw new InvalidOperationException($"Can only BeginDrawing when State is Idle (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.beforeDrawingMemento != null)
            {
                throw new InternalErrorException($"this.beforeDrawingMemento != null (Tool={base.GetType().Name})");
            }
            this.beforeDrawingMemento = beforeDrawingMemento;
            TransactedToolDrawingTransactionTokenPrivate<TDerived, TChanges> @private = new TransactedToolDrawingTransactionTokenPrivate<TDerived, TChanges>((TDerived) this);
            agent.TransactionToken = @private;
            this.drawingAgent = agent;
            this.SetState(TransactedToolState.Drawing);
            this.Changes = initialChanges.Clone();
        }

        public void BeginDrawing(TransactedToolDrawingAgent<TChanges> agent, TChanges initialChanges, string mementoName, ImageResource mementoImage)
        {
            this.BeginDrawing(agent, initialChanges, new EmptyHistoryMemento(mementoName, mementoImage));
        }

        public void BeginEditing(TransactedToolEditingAgent<TChanges> agent)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            Validate.IsNotNull<TransactedToolEditingAgent<TChanges>>(agent, "agent");
            agent.VerifyIsNotActive();
            if (this.State != TransactedToolState.Dirty)
            {
                throw new InvalidOperationException($"Can only BeginEditing when State is Dirty (State={this.State}, Tool={base.GetType().Name})");
            }
            TransactedToolEditingTransactionTokenPrivate<TDerived, TChanges> @private = new TransactedToolEditingTransactionTokenPrivate<TDerived, TChanges>((TDerived) this);
            agent.TransactionToken = @private;
            this.editingAgent = agent;
            this.drawingSettingValuesBackup = null;
            this.changesBeforeEditing = this.Changes.CloneT<TChanges>();
            this.SetState(TransactedToolState.Editing);
        }

        public sealed override void CancelChanges()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Dirty)
            {
                throw new InvalidOperationException($"Can only CancelChanges when State is Dirty (State={this.State}, Tool={base.GetType().Name})");
            }
            this.SetState(TransactedToolState.Idle);
            this.Changes = default(TChanges);
            this.TryRestoreDrawingSettingsValuesBackup();
        }

        private void CancelDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Drawing)
            {
                throw new InvalidOperationException($"Can only CancelDrawing when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
            }
            if (!this.drawingAgent.TransactionToken.IsCanceling)
            {
                this.drawingAgent.TransactionToken.Cancel();
            }
            else
            {
                this.OnCancelingDrawing();
                this.drawingAgent.TransactionToken = null;
                this.drawingAgent = null;
                this.SetState(TransactedToolState.Idle);
                this.Changes = default(TChanges);
                HistoryMemento beforeDrawingMemento = this.beforeDrawingMemento;
                this.beforeDrawingMemento = null;
                beforeDrawingMemento.PerformUndo(null);
            }
        }

        private void CancelEditing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Editing)
            {
                throw new InvalidOperationException($"Can only CancelEditing when State is Editing (State={this.State}, Tool={base.GetType().Name})");
            }
            if (!this.editingAgent.TransactionToken.IsCanceling)
            {
                this.editingAgent.TransactionToken.Cancel();
            }
            else
            {
                this.OnCancelingEditing();
                this.editingAgent.TransactionToken = null;
                this.editingAgent = null;
                this.SetState(TransactedToolState.Dirty);
                this.Changes = this.changesBeforeEditing;
                this.changesBeforeEditing = default(TChanges);
            }
        }

        private bool CoerceChangesAfterEndDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            this.VerifyState(TransactedToolState.Dirty);
            TChanges changes = this.Changes;
            TChanges local2 = this.OnCoerceChangesAfterEndDrawing(changes);
            if (local2 == null)
            {
                throw new InternalErrorException();
            }
            if (changes != local2)
            {
                this.Changes = local2;
                return true;
            }
            return false;
        }

        protected void CommitChanges()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            TransactedToolUndoCommitHistoryMemento<TDerived, TChanges> memento = this.TryCommitChanges();
            if (memento == null)
            {
                throw new InternalErrorException($"TryCommitChanges() returned null (Tool={base.GetType().Name})");
            }
            base.HistoryStack.PushNewMemento(memento);
        }

        internal HistoryMemento CommitChangesInner(TChanges changes)
        {
            HistoryMemento memento2;
            Validate.IsNotNull<TChanges>(changes, "changes");
            this.VerifyState(TransactedToolState.Dirty);
            if (this.State != TransactedToolState.Dirty)
            {
                throw new InvalidOperationException($"Can only call CommitChangesInner() when State is Dirty (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.IsCommitting)
            {
                throw new InvalidOperationException($"Cannot commit while IsCommitting is true (Tool={base.GetType().Name})");
            }
            try
            {
                this.IsCommitting = true;
                using (this.commitChangesRegion.UseEnterScope())
                {
                    HistoryMemento memento = this.OnCommitChanges(changes);
                    if (memento == null)
                    {
                        throw new InternalErrorException($"OnCommitChanges() returned null (Tool={base.GetType().Name})");
                    }
                    this.SetState(TransactedToolState.Idle);
                    this.Changes = default(TChanges);
                    this.changesBeforeEditing = default(TChanges);
                    this.TryRestoreDrawingSettingsValuesBackup();
                    memento2 = memento;
                }
            }
            finally
            {
                if (!this.IsCommitting)
                {
                    throw new InternalErrorException($"this.IsCommitting = false (Tool={base.GetType().Name})");
                }
                this.IsCommitting = false;
            }
            return memento2;
        }

        private void CommitDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Drawing)
            {
                throw new InvalidOperationException($"Can only CommitDrawing when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.beforeDrawingMemento == null)
            {
                throw new InternalErrorException($"this.beforeDrawingMemento == null (Tool={base.GetType().Name})");
            }
            if (!this.drawingAgent.TransactionToken.IsCommitting)
            {
                this.drawingAgent.TransactionToken.Commit();
            }
            else
            {
                this.OnEndingDrawing();
                this.drawingAgent.TransactionToken = null;
                this.drawingAgent = null;
                this.SetState(TransactedToolState.Dirty);
                this.CoerceChangesAfterEndDrawing();
                TChanges newChanges = this.Changes.Clone();
                HistoryMemento beforeDrawingMemento = this.beforeDrawingMemento;
                this.beforeDrawingMemento = null;
                TChanges oldChanges = default(TChanges);
                string historyMementoNameForChanges = this.GetHistoryMementoNameForChanges(oldChanges, newChanges);
                ImageResource historyMementoImageForChanges = this.GetHistoryMementoImageForChanges(default(TChanges), newChanges);
                TransactedToolUndoDrawHistoryMemento<TDerived, TChanges> memento2 = new TransactedToolUndoDrawHistoryMemento<TDerived, TChanges>(base.DocumentWorkspace, historyMementoNameForChanges, historyMementoImageForChanges, beforeDrawingMemento);
                TransactedToolUndoCommitHistoryMemento<TDerived, TChanges> memento3 = this.TryCommitChanges();
                if (!(beforeDrawingMemento is EmptyHistoryMemento) || !(memento3.InnerMemento is EmptyHistoryMemento))
                {
                    HistoryMemento[] actions = new HistoryMemento[] { memento2, memento3 };
                    CompoundHistoryMemento memento4 = new CompoundHistoryMemento(memento3.Name, memento3.Image, actions);
                    base.HistoryStack.PushNewMemento(memento4);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<Timer>(ref this.toolSettingsDebounceTimer);
            }
            base.Dispose(disposing);
        }

        private void EndDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Drawing)
            {
                throw new InvalidOperationException($"Can only EndDrawing when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.beforeDrawingMemento == null)
            {
                throw new InternalErrorException($"this.beforeDrawingMemento == null (Tool={base.GetType().Name})");
            }
            if (!this.drawingAgent.TransactionToken.IsEnding)
            {
                this.drawingAgent.TransactionToken.End();
            }
            else
            {
                string historyMementoNameForChanges;
                ImageResource historyMementoImageForChanges;
                this.OnEndingDrawing();
                this.drawingAgent.TransactionToken = null;
                this.drawingAgent = null;
                this.SetState(TransactedToolState.Dirty);
                this.CoerceChangesAfterEndDrawing();
                if ((this.beforeDrawingMemento == null) && !(this.beforeDrawingMemento is EmptyHistoryMemento))
                {
                    TChanges oldChanges = default(TChanges);
                    historyMementoNameForChanges = this.GetHistoryMementoNameForChanges(oldChanges, this.Changes);
                    historyMementoImageForChanges = this.GetHistoryMementoImageForChanges(default(TChanges), this.Changes);
                }
                else
                {
                    historyMementoNameForChanges = this.beforeDrawingMemento.Name;
                    historyMementoImageForChanges = this.beforeDrawingMemento.Image;
                }
                TransactedToolUndoDrawHistoryMemento<TDerived, TChanges> memento = new TransactedToolUndoDrawHistoryMemento<TDerived, TChanges>(base.DocumentWorkspace, historyMementoNameForChanges, historyMementoImageForChanges, this.beforeDrawingMemento);
                this.beforeDrawingMemento = null;
                base.HistoryStack.PushNewMemento(memento);
            }
        }

        private void EndEditing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Editing)
            {
                throw new InvalidOperationException($"Can only EndEditing when State is Editing (State={this.State}, Tool={base.GetType().Name})");
            }
            if (!this.editingAgent.TransactionToken.IsEnding)
            {
                this.editingAgent.TransactionToken.End();
            }
            else
            {
                this.OnEndingEditing();
                if (!this.Changes.Equals((ReferenceValue) this.changesBeforeEditing))
                {
                    string historyMementoNameForChanges = this.GetHistoryMementoNameForChanges(this.changesBeforeEditing, this.Changes);
                    ImageResource historyMementoImageForChanges = this.GetHistoryMementoImageForChanges(this.changesBeforeEditing, this.Changes);
                    TChanges previousChanges = this.changesBeforeEditing.Clone();
                    TransactedToolEditHistoryMemento<TDerived, TChanges> memento = new TransactedToolEditHistoryMemento<TDerived, TChanges>(base.DocumentWorkspace, historyMementoNameForChanges, historyMementoImageForChanges, previousChanges);
                    base.HistoryStack.PushNewMemento(memento);
                }
                this.editingAgent.TransactionToken = null;
                this.editingAgent = null;
                this.SetState(TransactedToolState.Dirty);
                this.changesBeforeEditing = default(TChanges);
            }
        }

        protected static string FoldHistoryMementoName(string historyMementoName, string genericHistoryMementoName, string predicateHistoryMementoName)
        {
            if (historyMementoName != null)
            {
                return genericHistoryMementoName;
            }
            return predicateHistoryMementoName;
        }

        protected sealed override ReferenceValue GetChanges() => 
            this.Changes;

        private ImageResource GetHistoryMementoImageForChanges(TChanges oldChanges, TChanges newChanges)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if ((oldChanges == null) && (newChanges == null))
            {
                throw new InvalidOperationException($"Cannot get a HistoryMemento image for a null->null transition for TChanges (Tool={base.GetType().Name})");
            }
            return this.OnGetHistoryMementoImageForChanges(oldChanges, newChanges);
        }

        private string GetHistoryMementoNameForChanges(TChanges oldChanges, TChanges newChanges)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if ((oldChanges == null) && (newChanges == null))
            {
                throw new InvalidOperationException($"Cannot get a HistoryMemento name for a null->null transition for TChanges (Tool={base.GetType().Name})");
            }
            return this.OnGetHistoryMementoNameForChanges(oldChanges, newChanges);
        }

        protected void InvalidateStatus()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (base.Active)
            {
                PdnSynchronizationContext.Instance.EnsurePosted(this.updateStatusCallback);
            }
        }

        protected virtual bool IsSelectionChangeAllowed() => 
            false;

        protected sealed override void OnActivate()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Inactive)
            {
                throw new InvalidOperationException($"Can only Activate when State is Inactive (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.changes != null)
            {
                throw new InternalErrorException($"this.changes != null (Tool={base.GetType().Name})");
            }
            if (this.changesBeforeEditing != null)
            {
                throw new InternalErrorException($"this.changesBeforeEditing != null (Tool={base.GetType().Name})");
            }
            this.toolSettingsEditingAgent = new TransactedToolEditingAgent<TChanges>("TransactedTool.toolSettingsEditingAgent");
            this.toolSettingsEditingAgent.CancelRequested += new HandledEventHandler(this.OnToolSettingsEditingAgentCancelRequested);
            this.toolSettingsEditingAgent.EndRequested += new HandledEventHandler(this.OnToolSettingsEditingAgentEndRequested);
            this.toolSettingsDebounceTimer = new Timer();
            this.toolSettingsDebounceTimer.Interval = 100;
            this.toolSettingsDebounceTimer.Tick += new EventHandler(this.OnToolSettingsDebounceTimerTick);
            this.toolSettingsDebounceTimer.Enabled = false;
            this.drawingSettingPaths = new HashSet<string>();
            foreach (Setting setting in this.OnGetDrawingSettings())
            {
                if (!this.drawingSettingPaths.Add(setting.Path))
                {
                    throw new InternalErrorException($"The setting was specified twice ({setting.Path}) (Tool={base.GetType().Name})");
                }
                base.ToolSettings[setting.Path].ValueChanged += new ValueChangedEventHandler<object>(this.OnDrawingSettingValueChanged);
            }
            this.SetState(TransactedToolState.Idle);
            this.OnActivated();
            AppSettings.Instance.Workspace.MeasurementUnit.ValueChanged += new ValueChangedEventHandler<object>(this.OnAppSettingsWorkspaceMeasurementUnitValueChanged);
            this.UpdateStatus();
            base.OnActivate();
        }

        protected virtual void OnActivated()
        {
        }

        private void OnAppSettingsWorkspaceMeasurementUnitValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            this.InvalidateStatus();
        }

        protected virtual void OnCancelingDrawing()
        {
        }

        protected virtual void OnCancelingEditing()
        {
        }

        protected virtual void OnChangesChanged(TChanges oldChanges, TChanges newChanges)
        {
        }

        protected virtual TChanges OnCoerceChanges(TChanges newChanges) => 
            newChanges;

        protected virtual TChanges OnCoerceChangesAfterEndDrawing(TChanges endDrawingChanges) => 
            endDrawingChanges;

        protected override void OnCommit()
        {
            this.CommitChanges();
            base.OnCommit();
        }

        protected abstract HistoryMemento OnCommitChanges(TChanges changes);
        protected sealed override void OnDeactivate()
        {
            this.OnDeactivating();
            if (this.toolSettingsEditingAgent.IsActive)
            {
                this.toolSettingsEditingAgent.TransactionToken.End();
            }
            this.toolSettingsEditingAgent = null;
            if (this.State == TransactedToolState.Drawing)
            {
                this.EndDrawing();
            }
            else if (this.State == TransactedToolState.Editing)
            {
                this.EndEditing();
            }
            if (this.State == TransactedToolState.Dirty)
            {
                this.CommitChanges();
            }
            if (this.State != TransactedToolState.Idle)
            {
                throw new InvalidOperationException($"Can only Deactivate when CurrentState is Idle (State={this.State}, Tool={base.GetType().Name})");
            }
            foreach (string str in this.drawingSettingPaths)
            {
                base.ToolSettings[str].ValueChanged -= new ValueChangedEventHandler<object>(this.OnDrawingSettingValueChanged);
            }
            this.drawingSettingPaths = null;
            this.toolSettingsDebounceTimer.Enabled = false;
            DisposableUtil.Free<Timer>(ref this.toolSettingsDebounceTimer);
            this.SetState(TransactedToolState.Inactive);
            AppSettings.Instance.Workspace.MeasurementUnit.ValueChanged -= new ValueChangedEventHandler<object>(this.OnAppSettingsWorkspaceMeasurementUnitValueChanged);
            this.UpdateStatus();
            this.OnDeactivated();
            base.OnDeactivate();
        }

        protected virtual void OnDeactivated()
        {
        }

        protected virtual void OnDeactivating()
        {
        }

        private void OnDrawingSettingValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (!this.IgnoreDrawingSettingsValueChanges)
            {
                switch (this.State)
                {
                    case TransactedToolState.Inactive:
                    case TransactedToolState.Idle:
                        break;

                    case TransactedToolState.Drawing:
                    case TransactedToolState.Editing:
                    {
                        TChanges local2 = this.Changes.CloneWithNewDrawingSettingsValues(this.DrawingSettingsValues);
                        this.Changes = local2;
                        if (!this.toolSettingsEditingAgent.IsActive)
                        {
                            break;
                        }
                        this.toolSettingsDebounceExpireTime = DateTime.Now + TransactedTool<TDerived, TChanges>.toolSettingsDebounceExpireInterval;
                        return;
                    }
                    case TransactedToolState.Dirty:
                    {
                        this.BeginEditing(this.toolSettingsEditingAgent);
                        TChanges local4 = this.Changes.CloneWithNewDrawingSettingsValues(this.DrawingSettingsValues);
                        this.toolSettingsEditingAgent.TransactionToken.Changes = local4;
                        this.toolSettingsDebounceTimer.Enabled = true;
                        this.toolSettingsDebounceExpireTime = DateTime.Now + TransactedTool<TDerived, TChanges>.toolSettingsDebounceExpireInterval;
                        return;
                    }
                    default:
                        throw ExceptionUtil.InvalidEnumArgumentException<TransactedToolState>(this.State, "this.State");
                }
            }
        }

        protected virtual void OnEndingDrawing()
        {
        }

        protected virtual void OnEndingEditing()
        {
        }

        protected override void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            if ((this.State == TransactedToolState.Drawing) && !this.RequestCancelDrawing())
            {
                throw new InternalErrorException($"Couldn't cancel Drawing state (Tool={base.GetType().Name})");
            }
            if ((this.State == TransactedToolState.Editing) && !this.RequestCancelEditing())
            {
                throw new InternalErrorException($"Couldn't cancel Editing state (Tool={base.GetType().Name})");
            }
            base.OnExecutingHistoryMemento(e);
        }

        protected abstract IEnumerable<Setting> OnGetDrawingSettings();
        protected virtual ImageResource OnGetHistoryMementoImageForChanges(TChanges oldChanges, TChanges newChanges) => 
            base.Image;

        protected abstract string OnGetHistoryMementoNameForChanges(TChanges oldChanges, TChanges newChanges);
        protected virtual void OnGetStatus(out ImageResource image, out string text)
        {
            image = base.Image;
            text = base.HelpText;
        }

        protected virtual void OnIsCommittingChanged()
        {
            this.UpdateCanCommit();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && !this.IsCommitting)
            {
                switch (e.KeyChar)
                {
                    case '\r':
                        if (this.State == TransactedToolState.Dirty)
                        {
                            this.CommitChanges();
                            this.VerifyState(TransactedToolState.Idle);
                        }
                        else
                        {
                            base.DocumentWorkspace.ApplyFunction(new DeselectFunction());
                        }
                        e.Handled = true;
                        break;

                    case '\x001b':
                        if ((base.ModifierKeys & (Keys.Alt | Keys.Control | Keys.Shift)) == Keys.None)
                        {
                            if (this.State == TransactedToolState.Drawing)
                            {
                                this.RequestCancelDrawing();
                            }
                            else if ((this.State == TransactedToolState.Editing) && (this.EditingTransactionAgent != this.toolSettingsEditingAgent))
                            {
                                this.RequestCancelEditing();
                            }
                            else if (!this.IsCommitting && (this.State == TransactedToolState.Dirty))
                            {
                                this.CommitChanges();
                                this.VerifyState(TransactedToolState.Idle);
                            }
                            else if (this.State == TransactedToolState.Idle)
                            {
                                base.DocumentWorkspace.ApplyFunction(new DeselectFunction());
                            }
                            e.Handled = true;
                        }
                        break;
                }
            }
            base.OnKeyPress(e);
        }

        protected sealed override void OnRestoreChanges(ReferenceValue changes)
        {
            this.RestoreChanges((TChanges) changes);
        }

        protected override void OnSelectionChanging()
        {
            switch (this.State)
            {
                case TransactedToolState.Drawing:
                case TransactedToolState.Dirty:
                case TransactedToolState.Editing:
                    if (!this.IsSelectionChangeAllowed())
                    {
                        throw new InternalErrorException($"Cannot change the selection while the tool's state is Drawing, Editing, or Dirty (State={this.State}, Tool={base.GetType().Name})");
                    }
                    break;
            }
            base.OnSelectionChanging();
        }

        protected virtual void OnStateChanged(TransactedToolState oldValue, TransactedToolState newValue)
        {
            if ((oldValue == TransactedToolState.Drawing) && (this.drawingAgent != null))
            {
                throw new InternalErrorException($"this.drawingTransactionAgent must be set to null before leaving the Drawing state (Tool={base.GetType().Name})");
            }
            if ((oldValue == TransactedToolState.Editing) && (this.editingAgent != null))
            {
                throw new InternalErrorException($"this.editingTransactionAgent must be set to null before leaving the Editing state (Tool={base.GetType().Name})");
            }
            if ((newValue == TransactedToolState.Drawing) && (this.drawingAgent == null))
            {
                throw new InternalErrorException($"this.drawingTransactionAgent must be set before entering the Drawing state (Tool={base.GetType().Name})");
            }
            if ((newValue == TransactedToolState.Editing) && (this.editingAgent == null))
            {
                throw new InternalErrorException($"this.editingTransactionAgent must be set before entering the Editing state (Tool={base.GetType().Name})");
            }
            this.UpdateCanCommit();
        }

        private void OnToolSettingsDebounceTimerTick(object sender, EventArgs e)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (((this.toolSettingsDebounceTimer != null) && this.toolSettingsDebounceTimer.Enabled) && (((this.toolSettingsEditingAgent != null) && this.toolSettingsEditingAgent.IsActive) && ((DateTime.Now >= this.toolSettingsDebounceExpireTime) && (Control.MouseButtons == MouseButtons.None))))
            {
                this.toolSettingsDebounceTimer.Enabled = false;
                this.toolSettingsEditingAgent.TransactionToken.End();
            }
        }

        private void OnToolSettingsEditingAgentCancelRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.toolSettingsEditingAgent.TransactionToken.Cancel();
                e.Handled = true;
            }
        }

        private void OnToolSettingsEditingAgentEndRequested(object sender, HandledEventArgs e)
        {
            if (!e.Handled)
            {
                this.toolSettingsDebounceTimer.Enabled = false;
                this.toolSettingsEditingAgent.TransactionToken.End();
                e.Handled = true;
            }
        }

        private void PopIgnoreDrawingSettingsValueChanged()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            this.ignoreDrawingSettingsValueChangedCount--;
        }

        private void PushIgnoreDrawingSettingsValueChanged()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            this.ignoreDrawingSettingsValueChangedCount++;
        }

        public sealed override bool RequestCancelDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Drawing)
            {
                throw new InvalidOperationException($"Can only RequestCancelDrawing when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
            }
            bool flag = this.drawingAgent.RequestCancelFromTool();
            if (flag)
            {
                this.VerifyState(TransactedToolState.Idle);
            }
            return flag;
        }

        public sealed override bool RequestCancelEditing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Editing)
            {
                throw new InvalidOperationException($"Can only RequestCancelEditing when State is Editing (State={this.State}, Tool={base.GetType().Name})");
            }
            bool flag = this.editingAgent.RequestCancelFromTool();
            if (flag)
            {
                this.VerifyState(TransactedToolState.Dirty);
            }
            return flag;
        }

        public sealed override bool RequestEndDrawing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Drawing)
            {
                throw new InvalidOperationException($"Can only RequestEndDrawing when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
            }
            bool flag = this.drawingAgent.RequestEndFromTool();
            if (flag)
            {
                this.VerifyState(TransactedToolState.Dirty);
            }
            return flag;
        }

        public sealed override bool RequestEndEditing()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != TransactedToolState.Editing)
            {
                throw new InvalidOperationException($"Can only RequestEndEditing when State is Editing (State={this.State}, Tool={base.GetType().Name})");
            }
            bool flag = this.editingAgent.RequestEndFromTool();
            if (flag)
            {
                this.VerifyState(TransactedToolState.Dirty);
            }
            return flag;
        }

        public void RestoreChanges(TChanges changes)
        {
            Validate.IsNotNull<TChanges>(changes, "changes");
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if ((this.State != TransactedToolState.Idle) && (this.State != TransactedToolState.Dirty))
            {
                throw new InvalidOperationException($"Can only RestoreChanges when State is Idle or Dirty (State={this.State}, Tool={base.GetType().Name})");
            }
            if (this.State == TransactedToolState.Idle)
            {
                this.SetState(TransactedToolState.Dirty);
                this.drawingSettingValuesBackup = this.DrawingSettingsValues.ToArrayEx<KeyValuePair<string, object>>();
            }
            this.Changes = changes;
        }

        private void SetChanges(TChanges newChanges)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            TChanges changes = this.changes;
            if (newChanges == null)
            {
                if (((this.State == TransactedToolState.Dirty) || (this.State == TransactedToolState.Drawing)) || (this.State == TransactedToolState.Editing))
                {
                    throw new InvalidOperationException($"May not set Changes to null when State is Dirty, Drawing, or Editing (State={this.State}, Tool={base.GetType().Name})");
                }
                this.changes = default(TChanges);
            }
            else
            {
                if ((this.State == TransactedToolState.Inactive) || (this.State == TransactedToolState.Idle))
                {
                    throw new InvalidOperationException($"May not set Changes to non-null when State is Inactive or Idle (State={this.State}, Tool={base.GetType().Name})");
                }
                TChanges local2 = this.OnCoerceChanges(newChanges);
                if (local2 == null)
                {
                    throw new InternalErrorException($"OnCoerceChanges() returned null (Tool={base.GetType().Name})");
                }
                this.changes = local2;
                this.PushIgnoreDrawingSettingsValueChanged();
                try
                {
                    foreach (KeyValuePair<string, object> pair in this.changes.DrawingSettingsValues)
                    {
                        base.ToolSettings[pair.Key].Value = pair.Value;
                    }
                }
                finally
                {
                    this.PopIgnoreDrawingSettingsValueChanged();
                }
            }
            this.OnChangesChanged(changes, this.changes);
            this.InvalidateStatus();
        }

        private void SetState(TransactedToolState newState)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (newState == this.state)
            {
                throw new InvalidOperationException($"Cannot transition to the same state (State={newState}, Tool={base.GetType().Name})");
            }
            TransactedToolState oldState = this.state;
            this.VerifyStateTransition(oldState, newState);
            this.state = newState;
            this.OnStateChanged(oldState, newState);
            this.InvalidateStatus();
        }

        private TransactedToolUndoCommitHistoryMemento<TDerived, TChanges> TryCommitChanges()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            this.VerifyState(TransactedToolState.Dirty);
            OperationCanceledException exception = null;
            TransactedToolUndoCommitHistoryMemento<TDerived, TChanges> memento = null;
            TChanges changes = this.Changes.Clone();
            try
            {
                this.drawingSettingValuesBackup = null;
                HistoryMemento innerCommitHM = this.CommitChangesInner(changes);
                if (innerCommitHM == null)
                {
                    throw new InternalErrorException($"CommitChangesInner() returned null (Tool={base.GetType().Name})");
                }
                memento = new TransactedToolUndoCommitHistoryMemento<TDerived, TChanges>(base.DocumentWorkspace, changes, innerCommitHM);
            }
            catch (OperationCanceledException exception2)
            {
                exception = exception2;
            }
            if (exception != null)
            {
                this.VerifyState(TransactedToolState.Dirty);
                ExceptionDialog.ShowErrorDialog(base.DocumentWorkspace, exception.InnerException ?? exception);
                return null;
            }
            this.VerifyState(TransactedToolState.Idle);
            if (this.changes != null)
            {
                throw new InternalErrorException($"this.Changes is not null (Tool={base.GetType().Name})");
            }
            if (this.changesBeforeEditing != null)
            {
                throw new InternalErrorException($"this.changesBeforeEditing is not null (Tool={base.GetType().Name})");
            }
            return memento;
        }

        private bool TryRestoreDrawingSettingsValuesBackup()
        {
            if (this.drawingSettingValuesBackup == null)
            {
                return false;
            }
            this.PushIgnoreDrawingSettingsValueChanged();
            try
            {
                foreach (KeyValuePair<string, object> pair in this.drawingSettingValuesBackup)
                {
                    base.ToolSettings[pair.Key].Value = pair.Value;
                }
                this.drawingSettingValuesBackup = null;
            }
            finally
            {
                this.PopIgnoreDrawingSettingsValueChanged();
            }
            return true;
        }

        private void UpdateCanCommit()
        {
            if (this.IsCommitting)
            {
                base.CanCommit = false;
            }
            else
            {
                switch (this.State)
                {
                    case TransactedToolState.Dirty:
                        base.CanCommit = true;
                        return;
                }
                base.CanCommit = false;
            }
        }

        protected void UpdateStatus()
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (base.Active)
            {
                ImageResource resource;
                string str;
                this.OnGetStatus(out resource, out str);
                base.SetStatus(resource, str);
            }
        }

        public void VerifyState(TransactedToolState requiredState)
        {
            ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
            if (this.State != requiredState)
            {
                throw new InvalidOperationException($"The tool must be in the {requiredState} state, but is actually in the {this.State} state (Tool={base.GetType().Name})");
            }
        }

        public void VerifyStateTransition(TransactedToolState oldState, TransactedToolState newState)
        {
            if ((((((oldState != TransactedToolState.Inactive) || (newState != TransactedToolState.Idle)) && ((oldState != TransactedToolState.Idle) || (newState != TransactedToolState.Inactive))) && (((oldState != TransactedToolState.Idle) || (newState != TransactedToolState.Drawing)) && ((oldState != TransactedToolState.Idle) || (newState != TransactedToolState.Dirty)))) && ((((oldState != TransactedToolState.Drawing) || (newState != TransactedToolState.Idle)) && ((oldState != TransactedToolState.Drawing) || (newState != TransactedToolState.Dirty))) && (((oldState != TransactedToolState.Dirty) || (newState != TransactedToolState.Idle)) && ((oldState != TransactedToolState.Dirty) || (newState != TransactedToolState.Editing))))) && ((oldState != TransactedToolState.Editing) || (newState != TransactedToolState.Dirty)))
            {
                throw new InvalidOperationException($"Illegal state transition, from {oldState} to {newState} (Tool={base.GetType().Name})");
            }
        }

        public TChanges Changes
        {
            get => 
                this.changes;
            private set
            {
                this.SetChanges(value);
            }
        }

        public TChanges ChangesBeforeEditing =>
            this.changesBeforeEditing;

        public IEnumerable<Setting> DrawingSettings =>
            (from path in this.drawingSettingPaths select base.ToolSettings[path]);

        public IEnumerable<KeyValuePair<string, object>> DrawingSettingsValues =>
            (from path in this.drawingSettingPaths select new KeyValuePair<string, object>(path, base.ToolSettings[path].Value));

        protected TransactedToolDrawingAgent<TChanges> DrawingTransactionAgent
        {
            get
            {
                ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
                if (this.State != TransactedToolState.Drawing)
                {
                    throw new InvalidOperationException($"Can only get the DrawingTransactionAgent when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
                }
                return this.drawingAgent;
            }
        }

        protected TransactedToolEditingAgent<TChanges> EditingTransactionAgent
        {
            get
            {
                ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
                if (this.State != TransactedToolState.Editing)
                {
                    throw new InvalidOperationException($"Can only get the EditingTransactionAgent when State is Drawing (State={this.State}, Tool={base.GetType().Name})");
                }
                return this.editingAgent;
            }
        }

        private bool IgnoreDrawingSettingsValueChanges =>
            (this.ignoreDrawingSettingsValueChangedCount > 0);

        protected bool IsCommitting
        {
            get => 
                this.isCommitting;
            private set
            {
                ((TransactedTool<TDerived, TChanges>) this).VerifyAccess<TransactedTool<TDerived, TChanges>>();
                if (value != this.isCommitting)
                {
                    this.isCommitting = value;
                    this.OnIsCommittingChanged();
                }
            }
        }

        public sealed override TransactedToolState State =>
            this.state;

        private sealed class TransactedToolDrawingTransactionTokenPrivate : TransactedToolDrawingToken<TChanges>
        {
            private TDerived tool;

            public TransactedToolDrawingTransactionTokenPrivate(TDerived tool)
            {
                this.tool = tool;
            }

            protected override void OnCancel()
            {
                this.tool.CancelDrawing();
            }

            protected override void OnCommit()
            {
                this.tool.CommitDrawing();
            }

            protected override void OnEnd()
            {
                this.tool.EndDrawing();
            }

            protected override TChanges OnGetChanges() => 
                this.tool.Changes;

            protected override void OnSetChanges(TChanges newChanges)
            {
                if (!object.Equals(this.tool.Changes, newChanges))
                {
                    this.tool.Changes = newChanges;
                }
            }
        }

        private sealed class TransactedToolEditingTransactionTokenPrivate : TransactedToolEditingToken<TChanges>
        {
            private TDerived tool;

            public TransactedToolEditingTransactionTokenPrivate(TDerived tool)
            {
                this.tool = tool;
            }

            protected override void OnCancel()
            {
                this.tool.CancelEditing();
            }

            protected override void OnEnd()
            {
                this.tool.EndEditing();
            }

            protected override TChanges OnGetChanges() => 
                this.tool.Changes;

            protected override void OnSetChanges(TChanges newChanges)
            {
                this.tool.Changes = newChanges;
            }
        }
    }
}

