namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class TransactedToolEditingAgent<TChanges> : TransactedToolAgent<TransactedToolEditingToken<TChanges>>
    {
        [field: CompilerGenerated]
        public event HandledEventHandler CancelRequested;

        [field: CompilerGenerated]
        public event HandledEventHandler EndRequested;

        public TransactedToolEditingAgent(string name) : base(name)
        {
        }

        protected virtual void OnCancelRequested(ref bool handled)
        {
            this.CancelRequested.Raise(this, out handled, handled);
        }

        protected virtual void OnEndRequested(ref bool handled)
        {
            this.EndRequested.Raise(this, out handled, handled);
        }

        public bool RequestCancelFromTool()
        {
            base.VerifyIsActive();
            bool handled = false;
            this.OnCancelRequested(ref handled);
            return handled;
        }

        public bool RequestEndFromTool()
        {
            base.VerifyIsActive();
            bool handled = false;
            this.OnEndRequested(ref handled);
            return handled;
        }
    }
}

