namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Threading;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal abstract class TransactedToolToken : Disposable
    {
        private readonly ProtectedRegion cancelRegion = new ProtectedRegion("Cancel", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
        private StackTrace ctorStackTrace = new StackTrace(true);
        private readonly ProtectedRegion endRegion = new ProtectedRegion("End", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);
        private bool hasResponded;
        private bool isResponding;
        private readonly ProtectedRegion respondingRegion = new ProtectedRegion("Responding", ProtectedRegionOptions.ErrorOnPerThreadReentrancy);

        protected TransactedToolToken()
        {
        }

        private void AfterResponse()
        {
            if (!this.isResponding)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot call AfterResponse() until after BeforeResponse() is called");
            }
            this.isResponding = false;
            this.hasResponded = true;
            this.respondingRegion.Exit();
        }

        private void BeforeResponse()
        {
            if (this.hasResponded)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot call BeforeResponse() after AfterResponse() has been called");
            }
            if (this.isResponding)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot nest calls to BeforeResponse(). This may indicate a reentrancy problem.");
            }
            this.respondingRegion.Enter();
            this.isResponding = true;
        }

        protected RespondingScope BeginResponse() => 
            new RespondingScope(this);

        public void Cancel()
        {
            using (this.BeginResponse())
            {
                using (this.cancelRegion.UseEnterScope())
                {
                    this.OnCancel();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.hasResponded)
            {
                string[] textArray1 = new string[] { "The transaction token must be responded to before it can be disposed or finalized.", Environment.NewLine, "Type = {0}.", Environment.NewLine, "Construction stack:", Environment.NewLine, "{1}", Environment.NewLine, "------" };
                ExceptionUtil.ThrowInvalidOperationException(string.Format(string.Concat(textArray1), base.GetType().FullName, this.ctorStackTrace.ToString()));
            }
            base.Dispose(disposing);
        }

        public void End()
        {
            using (this.BeginResponse())
            {
                using (this.endRegion.UseEnterScope())
                {
                    this.OnEnd();
                }
            }
        }

        protected abstract void OnCancel();
        protected abstract void OnEnd();

        public bool HasResponded =>
            this.hasResponded;

        public bool IsCanceling =>
            this.cancelRegion.IsThreadEntered;

        public bool IsEnding =>
            this.endRegion.IsThreadEntered;

        public bool IsResponding =>
            this.isResponding;

        [StructLayout(LayoutKind.Sequential)]
        public struct RespondingScope : IDisposable
        {
            private TransactedToolToken owner;
            public RespondingScope(TransactedToolToken owner)
            {
                Validate.IsNotNull<TransactedToolToken>(owner, "owner");
                this.owner = owner;
                this.owner.BeforeResponse();
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    this.owner.AfterResponse();
                    this.owner = null;
                }
            }
        }
    }
}

