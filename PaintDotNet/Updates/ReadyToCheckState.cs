namespace PaintDotNet.Updates
{
    using System;
    using System.Runtime.InteropServices;

    internal class ReadyToCheckState : UpdatesState
    {
        public ReadyToCheckState() : base(false, true, MarqueeStyle.None)
        {
        }

        public override void OnEnteredState()
        {
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (!input.Equals(UpdatesAction.Continue))
            {
                throw new ArgumentException();
            }
            newState = new CheckingState();
        }
    }
}

