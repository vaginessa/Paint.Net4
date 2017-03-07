namespace PaintDotNet
{
    using PaintDotNet.Controls;
    using System;

    internal sealed class PushNullToolMode : IDisposable
    {
        private DocumentWorkspace documentWorkspace;

        public PushNullToolMode(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            this.documentWorkspace.PushNullTool();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && (this.documentWorkspace != null))
            {
                this.documentWorkspace.PopNullTool();
                this.documentWorkspace = null;
            }
        }

        ~PushNullToolMode()
        {
            this.Dispose(false);
        }
    }
}

