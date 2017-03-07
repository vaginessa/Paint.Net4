namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class StateMachineExecutor : IDisposable
    {
        private bool disposed;
        private ManualResetEvent inputAvailable = new ManualResetEvent(false);
        private bool isStarted;
        private volatile bool pleaseAbort;
        private object queuedInput;
        private StateMachine stateMachine;
        private ManualResetEvent stateMachineInitialized = new ManualResetEvent(false);
        private ManualResetEvent stateMachineNotBusy = new ManualResetEvent(false);
        private Thread stateMachineThread;
        private ISynchronizeInvoke syncContext;
        private Exception threadException;

        [field: CompilerGenerated]
        public event ValueEventHandler<PaintDotNet.Updates.State> StateBegin;

        [field: CompilerGenerated]
        public event EventHandler StateMachineBegin;

        [field: CompilerGenerated]
        public event EventHandler StateMachineFinished;

        [field: CompilerGenerated]
        public event ProgressEventHandler StateProgress;

        [field: CompilerGenerated]
        public event ValueEventHandler<PaintDotNet.Updates.State> StateWaitingForInput;

        public StateMachineExecutor(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Abort()
        {
            if (!this.disposed)
            {
                this.pleaseAbort = true;
                PaintDotNet.Updates.State currentState = this.stateMachine.CurrentState;
                if ((currentState != null) && currentState.CanAbort)
                {
                    this.stateMachine.CurrentState.Abort();
                }
                this.stateMachineNotBusy.WaitOne();
                this.inputAvailable.Set();
                this.stateMachineThread.Join();
                if (this.threadException != null)
                {
                    throw new WorkerThreadException("State machine thread threw an exception", this.threadException);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Abort();
                if (this.stateMachineInitialized != null)
                {
                    this.stateMachineInitialized.Close();
                    this.stateMachineInitialized = null;
                }
                if (this.stateMachineNotBusy != null)
                {
                    this.stateMachineNotBusy.Close();
                    this.stateMachineNotBusy = null;
                }
                if (this.inputAvailable != null)
                {
                    this.inputAvailable.Close();
                    this.inputAvailable = null;
                }
            }
            this.disposed = true;
        }

        ~StateMachineExecutor()
        {
            this.Dispose(false);
        }

        private void OnStateBegin(PaintDotNet.Updates.State state)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                object[] args = new object[] { state };
                this.syncContext.BeginInvoke(new Action<PaintDotNet.Updates.State>(this.OnStateBegin), args);
            }
            else
            {
                this.StateBegin.Raise<PaintDotNet.Updates.State>(this, state);
            }
        }

        private void OnStateMachineBegin()
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action(this.OnStateMachineBegin), null);
            }
            else
            {
                this.StateMachineBegin.Raise(this);
            }
        }

        private void OnStateMachineFinished()
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action(this.OnStateMachineFinished), null);
            }
            else
            {
                this.StateMachineFinished.Raise(this);
            }
        }

        private void OnStateProgress(double percent)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                object[] args = new object[] { percent };
                this.syncContext.BeginInvoke(new Action<double>(this.OnStateProgress), args);
            }
            else if (this.StateProgress != null)
            {
                this.StateProgress(this, new ProgressEventArgs(percent));
            }
        }

        private void OnStateWaitingForInput(PaintDotNet.Updates.State state)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                object[] args = new object[] { state };
                this.syncContext.BeginInvoke(new Action<PaintDotNet.Updates.State>(this.OnStateWaitingForInput), args);
            }
            else
            {
                this.StateWaitingForInput.Raise<PaintDotNet.Updates.State>(this, state);
            }
        }

        public void ProcessInput(object input)
        {
            this.stateMachineNotBusy.WaitOne();
            this.stateMachineNotBusy.Reset();
            this.queuedInput = input;
            this.inputAvailable.Set();
        }

        public void Start()
        {
            if (this.isStarted)
            {
                ExceptionUtil.ThrowInvalidOperationException("State machine thread is already executing");
            }
            this.isStarted = true;
            this.stateMachineThread = new Thread(new ThreadStart(this.StateMachineThread));
            this.stateMachineInitialized.Reset();
            this.stateMachineThread.Start();
            this.stateMachineInitialized.WaitOne();
        }

        private void StateMachineThread()
        {
            this.threadException = null;
            ValueEventHandler<PaintDotNet.Updates.State> handler = delegate (object sender, ValueEventArgs<PaintDotNet.Updates.State> e) {
                this.stateMachineInitialized.Set();
                this.OnStateBegin(e.Value);
            };
            ProgressEventHandler handler2 = (sender, e) => this.OnStateProgress(e.Percent);
            try
            {
                this.stateMachineNotBusy.Set();
                this.OnStateMachineBegin();
                this.stateMachineNotBusy.Reset();
                this.stateMachine.NewState += handler;
                this.stateMachine.StateProgress += handler2;
                this.stateMachine.Start();
                do
                {
                    this.stateMachineNotBusy.Set();
                    this.OnStateWaitingForInput(this.stateMachine.CurrentState);
                    this.inputAvailable.WaitOne();
                    this.inputAvailable.Reset();
                    if (this.pleaseAbort)
                    {
                        break;
                    }
                    this.stateMachine.ProcessInput(this.queuedInput);
                }
                while (!this.stateMachine.IsInFinalState);
                this.stateMachineNotBusy.Set();
            }
            catch (Exception exception)
            {
                this.threadException = exception;
            }
            finally
            {
                this.stateMachineNotBusy.Set();
                this.stateMachineInitialized.Set();
                this.stateMachine.NewState -= handler;
                this.stateMachine.StateProgress -= handler2;
                this.OnStateMachineFinished();
            }
        }

        public PaintDotNet.Updates.State CurrentState =>
            this.stateMachine.CurrentState;

        public bool IsInFinalState =>
            this.stateMachine.IsInFinalState;

        public bool IsStarted =>
            this.isStarted;

        public ISynchronizeInvoke SyncContext
        {
            get => 
                this.syncContext;
            set
            {
                this.syncContext = value;
            }
        }
    }
}

