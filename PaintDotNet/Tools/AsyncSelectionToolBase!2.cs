namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.Tasks;
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal abstract class AsyncSelectionToolBase<TDerived, TChanges> : PresentationBasedTool<TDerived, TChanges> where TDerived: AsyncSelectionToolBase<TDerived, TChanges> where TChanges: TransactedToolChanges<TChanges, TDerived>
    {
        private Action beginCreateSelection;
        private CancellationTokenSource createSelectionCancellationToken;
        private TChanges createSelectionChanges;
        private AsyncSelectionToolCreateGeometryContext createSelectionContext;
        private Action createSelectionOnBackgroundThread;
        private Action endCreateSelectionCallback;
        private ManualResetEvent endCreateSelectionEvent;
        private bool isBeginCreateSelectionQueued;
        private bool isCreatingSelection;
        private bool needToRecreateSelection;

        protected AsyncSelectionToolBase(DocumentWorkspace docWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems) : base(docWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems, true)
        {
            this.beginCreateSelection = new Action(this.BeginCreateSelection);
            this.createSelectionOnBackgroundThread = new Action(this.CreateSelectionOnBackgroundThread);
        }

        private void BeginCreateSelection()
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            if (this.isBeginCreateSelectionQueued)
            {
                this.isBeginCreateSelectionQueued = false;
                if (this.isCreatingSelection)
                {
                    this.needToRecreateSelection = true;
                    this.createSelectionCancellationToken.Cancel();
                }
                else
                {
                    this.isCreatingSelection = true;
                    this.createSelectionCancellationToken = new CancellationTokenSource();
                    this.createSelectionChanges = base.Changes;
                    this.createSelectionContext = this.GetCreateGeometryContext(this.createSelectionChanges);
                    this.endCreateSelectionEvent = new ManualResetEvent(false);
                    base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(null);
                    WorkItemDispatcher.Default.Enqueue(this.createSelectionOnBackgroundThread, WorkItemQueuePriority.High);
                }
            }
        }

        private void CancelSelection()
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            this.isBeginCreateSelectionQueued = false;
            if (this.isCreatingSelection)
            {
                this.needToRecreateSelection = false;
                this.createSelectionCancellationToken.Cancel();
            }
        }

        protected abstract GeometryList CreateSelectionGeometry(TChanges changes, AsyncSelectionToolCreateGeometryContext context, CancellationToken cancellationToken);
        private void CreateSelectionOnBackgroundThread()
        {
            Result<GeometryList> result = null;
            try
            {
                if (this.createSelectionCancellationToken.IsCancellationRequested)
                {
                    result = Result.NewError<GeometryList>(new OperationCanceledException(), false);
                }
                else
                {
                    result = () => this.CreateSelectionGeometry(base.createSelectionChanges, base.createSelectionContext, base.createSelectionCancellationToken.Token).Eval<GeometryList>();
                }
            }
            catch (Exception exception)
            {
                result = Result.NewError<GeometryList>(exception);
            }
            finally
            {
                bool endCreateSelectionCallbackExecuted = false;
                Action action = delegate {
                    ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
                    if (!endCreateSelectionCallbackExecuted)
                    {
                        endCreateSelectionCallbackExecuted = true;
                        ((AsyncSelectionToolBase<TDerived, TChanges>) this).EndCreateSelectionOnUIThread(result);
                    }
                };
                this.endCreateSelectionCallback = action;
                this.endCreateSelectionEvent.Set();
                PdnSynchronizationContext.Instance.Post(action);
            }
        }

        private void EndCreateSelectionOnUIThread(Result<GeometryList> result)
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            this.isCreatingSelection = false;
            this.createSelectionChanges = default(TChanges);
            DisposableUtil.Free<AsyncSelectionToolCreateGeometryContext>(ref this.createSelectionContext);
            DisposableUtil.Free<ManualResetEvent>(ref this.endCreateSelectionEvent);
            this.endCreateSelectionCallback = null;
            if (!base.IsCommitting && ((this.State == TransactedToolState.Inactive) || !base.Active))
            {
                if (result.NeedsObservation)
                {
                    result.Observe();
                }
                return;
            }
            if (result.IsError && (result.Error is OperationCanceledException))
            {
                this.createSelectionCancellationToken.Cancel();
            }
            bool isCancellationRequested = this.createSelectionCancellationToken.IsCancellationRequested;
            DisposableUtil.Free<CancellationTokenSource>(ref this.createSelectionCancellationToken);
            if (this.needToRecreateSelection)
            {
                this.needToRecreateSelection = false;
                this.InvalidateSelection();
            }
            else if (isCancellationRequested)
            {
                base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?((double) 100));
                base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            }
            else
            {
                if (result.IsError)
                {
                    base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?((double) 100));
                    base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    ExceptionDialog.ShowErrorDialog(base.DocumentWorkspace, result.Error);
                    using (new PresentationBasedTool<TDerived, TChanges>.AllowSelectionChangesScope((TDerived) this))
                    {
                        using (base.Selection.UseChangeScope())
                        {
                            base.Selection.Reset();
                            goto Label_0222;
                        }
                    }
                }
                GeometryList geometry = result.Value;
                using (new PresentationBasedTool<TDerived, TChanges>.AllowSelectionChangesScope((TDerived) this))
                {
                    using (base.Selection.UseChangeScope())
                    {
                        base.Selection.SetContinuation(geometry, SelectionCombineMode.Replace);
                        base.Selection.CommitContinuation();
                    }
                }
                base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?((double) 100));
                base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            }
        Label_0222:
            base.InvalidateStatus();
        }

        protected abstract string GetCommitChangesHistoryMementoName(TChanges changes);
        protected abstract AsyncSelectionToolCreateGeometryContext GetCreateGeometryContext(TChanges changes);
        protected SelectionHistoryMemento GetSelectionHistoryMementoAndPrepareForBeginDrawing()
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            base.VerifyState(TransactedToolState.Idle);
            SelectionHistoryMemento memento = new SelectionHistoryMemento(base.Name, base.Image, base.DocumentWorkspace);
            using (new PresentationBasedTool<TDerived, TChanges>.AllowSelectionChangesScope((TDerived) this))
            {
                using (base.Selection.UseChangeScope())
                {
                    if (!base.Selection.GetInterimTransform().IsIdentity)
                    {
                        base.Selection.CommitInterimTransform();
                        return memento;
                    }
                    base.Selection.CommitContinuation();
                }
            }
            return memento;
        }

        private void InvalidateSelection()
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            if (this.isCreatingSelection)
            {
                this.needToRecreateSelection = true;
                this.createSelectionCancellationToken.Cancel();
            }
            else
            {
                this.isBeginCreateSelectionQueued = true;
                PdnSynchronizationContext.Instance.EnsurePosted(this.beginCreateSelection);
            }
        }

        protected override void OnActivated()
        {
            base.DocumentWorkspace.EnableSelectionTinting = true;
            base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?((double) 100));
            base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            base.OnActivated();
        }

        protected sealed override void OnChangesChanged(TChanges oldChanges, TChanges newChanges)
        {
            if (newChanges == null)
            {
                this.CancelSelection();
            }
            else if (newChanges != null)
            {
                this.InvalidateSelection();
            }
            base.OnChangesChanged(oldChanges, newChanges);
        }

        protected sealed override HistoryMemento OnCommitChanges(TChanges changes)
        {
            string commitChangesHistoryMementoName = this.GetCommitChangesHistoryMementoName(changes);
            if (this.isCreatingSelection && (changes != this.createSelectionChanges))
            {
                this.InvalidateSelection();
            }
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan span = TimeSpan.FromSeconds(1.0);
            bool flag = false;
            do
            {
                if (this.PollForSelection(1))
                {
                    flag = true;
                    break;
                }
            }
            while ((DateTime.UtcNow - utcNow) < span);
            if (!flag)
            {
                VirtualTask<Unit> selectionTask = base.DocumentWorkspace.TaskManager.CreateVirtualTask(TaskState.Running);
                selectionTask.Progress = null;
                using (TaskProgressDialog dialog = new TaskProgressDialog())
                {
                    dialog.Task = selectionTask;
                    dialog.CloseOnFinished = true;
                    dialog.ShowCancelButton = false;
                    dialog.Text = base.Name;
                    dialog.Icon = base.Image.Reference.ToIcon();
                    dialog.HeaderText = PdnResources.GetString("SaveConfigDialog.Finishing.Text");
                    using (System.Windows.Forms.Timer pollTimer = new System.Windows.Forms.Timer())
                    {
                        <>c__DisplayClass15_2<TDerived, TChanges> class_2;
                        bool isPollTimerTickExecuting = false;
                        pollTimer.Enabled = false;
                        pollTimer.Interval = 10;
                        pollTimer.Tick += delegate (object sender, EventArgs e) {
                            if (!isPollTimerTickExecuting)
                            {
                                isPollTimerTickExecuting = true;
                                try
                                {
                                    if ((selectionTask.State != TaskState.Finished) && ((AsyncSelectionToolBase<TDerived, TChanges>) this).PollForSelection(1))
                                    {
                                        pollTimer.Enabled = false;
                                        selectionTask.SetState(TaskState.Finished);
                                    }
                                }
                                finally
                                {
                                    isPollTimerTickExecuting = false;
                                }
                            }
                        };
                        dialog.Shown += new EventHandler(class_2.<OnCommitChanges>b__1);
                        dialog.ShowDialog(base.DocumentWorkspace);
                    }
                }
            }
            return new EmptyHistoryMemento(commitChangesHistoryMementoName, base.Image);
        }

        protected override void OnDeactivated()
        {
            base.DocumentWorkspace.EnableSelectionTinting = false;
            base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(new double?((double) 100));
            base.DocumentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            base.OnDeactivated();
        }

        private bool PollForSelection(int waitTimeoutMs)
        {
            ((AsyncSelectionToolBase<TDerived, TChanges>) this).VerifyAccess<AsyncSelectionToolBase<TDerived, TChanges>>();
            if (!this.isCreatingSelection)
            {
                return true;
            }
            if (this.endCreateSelectionEvent.WaitOne(waitTimeoutMs))
            {
                this.endCreateSelectionCallback();
                return true;
            }
            return false;
        }

        private void WaitForSelection()
        {
            this.WaitForSelection(100, null);
        }

        private void WaitForSelection(Action pollCallback)
        {
            this.WaitForSelection(100, pollCallback);
        }

        private void WaitForSelection(int pollIntervalMs, Action pollCallback)
        {
            int num = 0;
            while (true)
            {
                while (this.PollForSelection(pollIntervalMs))
                {
                }
                num++;
                if (pollCallback != null)
                {
                    pollCallback();
                }
            }
        }
    }
}

