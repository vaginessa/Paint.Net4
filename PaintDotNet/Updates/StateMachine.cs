namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class StateMachine
    {
        private PaintDotNet.Updates.State currentState;
        private PaintDotNet.Updates.State initialState;
        private ArrayList inputAlphabet;
        private Queue inputQueue = new Queue();
        private bool processingInput;

        [field: CompilerGenerated]
        public event ValueEventHandler<PaintDotNet.Updates.State> NewState;

        [field: CompilerGenerated]
        public event ProgressEventHandler StateProgress;

        public StateMachine(PaintDotNet.Updates.State initialState, IEnumerable inputAlphabet)
        {
            this.initialState = initialState;
            this.inputAlphabet = new ArrayList();
            foreach (object obj2 in inputAlphabet)
            {
                this.inputAlphabet.Add(obj2);
            }
        }

        private void OnNewState(PaintDotNet.Updates.State newState)
        {
            this.NewState.Raise<PaintDotNet.Updates.State>(this, newState);
        }

        public void OnStateProgress(double percent)
        {
            if (this.StateProgress != null)
            {
                this.StateProgress(this, new ProgressEventArgs(percent));
            }
        }

        public void ProcessInput(object input)
        {
            if (this.processingInput)
            {
                ExceptionUtil.ThrowInvalidOperationException("already processing input");
            }
            if (this.currentState.IsFinalState)
            {
                ExceptionUtil.ThrowInvalidOperationException("state machine is already in a final state");
            }
            if (!this.inputAlphabet.Contains(input))
            {
                throw new ArgumentOutOfRangeException("must be contained in the input alphabet set", "input");
            }
            this.inputQueue.Enqueue(input);
            this.ProcessQueuedInput();
        }

        private void ProcessQueuedInput()
        {
            while (this.inputQueue.Count > 0)
            {
                PaintDotNet.Updates.State state;
                object input = this.inputQueue.Dequeue();
                this.currentState.ProcessInput(input, out state);
                if (state == this.currentState)
                {
                    ExceptionUtil.ThrowInvalidOperationException("must provide a clean, newly constructed state");
                }
                this.SetCurrentState(state);
            }
        }

        public void QueueInput(object input)
        {
            this.inputQueue.Enqueue(input);
        }

        private void SetCurrentState(PaintDotNet.Updates.State newState)
        {
            if ((this.currentState != null) && this.currentState.IsFinalState)
            {
                ExceptionUtil.ThrowInvalidOperationException("state machine is already in a final state");
            }
            this.currentState = newState;
            this.currentState.StateMachine = this;
            this.OnNewState(this.currentState);
            this.currentState.OnEnteredState();
            if (!this.currentState.IsFinalState)
            {
                this.ProcessQueuedInput();
            }
        }

        public void Start()
        {
            if (this.currentState != null)
            {
                ExceptionUtil.ThrowInvalidOperationException("may only call Start() once after construction");
            }
            this.SetCurrentState(this.initialState);
        }

        public PaintDotNet.Updates.State CurrentState =>
            this.currentState;

        public bool IsInFinalState =>
            this.currentState.IsFinalState;
    }
}

