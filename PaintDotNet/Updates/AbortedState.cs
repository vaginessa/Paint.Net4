namespace PaintDotNet.Updates
{
    using System;
    using System.Runtime.InteropServices;

    internal class AbortedState : UpdatesState
    {
        public AbortedState() : base(true, false, MarqueeStyle.None)
        {
        }

        public override void OnEnteredState()
        {
            base.OnEnteredState();
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

