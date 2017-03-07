namespace PaintDotNet.Tools
{
    using PaintDotNet.Threading;
    using System;

    internal abstract class TransactedToolDrawingToken<TChanges> : TransactedToolChangesToken<TChanges>
    {
        private readonly ProtectedRegion commitRegion;

        protected TransactedToolDrawingToken()
        {
            this.commitRegion = new ProtectedRegion("Commit", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
        }

        public void Commit()
        {
            using (base.BeginResponse())
            {
                using (this.commitRegion.UseEnterScope())
                {
                    this.OnCommit();
                }
            }
        }

        protected abstract void OnCommit();

        public bool IsCommitting =>
            this.commitRegion.IsThreadEntered;
    }
}

