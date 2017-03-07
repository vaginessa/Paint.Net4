namespace PaintDotNet.Tools
{
    using System;

    internal abstract class TransactedToolChangesToken<TChanges> : TransactedToolToken
    {
        protected TransactedToolChangesToken()
        {
        }

        protected abstract TChanges OnGetChanges();
        protected abstract void OnSetChanges(TChanges newChanges);

        public TChanges Changes
        {
            get => 
                this.OnGetChanges();
            set
            {
                this.OnSetChanges(value);
            }
        }
    }
}

