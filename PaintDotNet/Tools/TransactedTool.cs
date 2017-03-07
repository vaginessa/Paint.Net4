namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;

    internal abstract class TransactedTool : Tool
    {
        protected TransactedTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems)
        {
        }

        public abstract void CancelChanges();
        internal void ForceCancelDrawingOrEditing()
        {
            if ((this.State == TransactedToolState.Drawing) && !this.RequestCancelDrawing())
            {
                ExceptionUtil.ThrowInternalErrorException("TransactedTool did not allow us to cancel the Drawing state");
            }
            if ((this.State == TransactedToolState.Editing) && !this.RequestCancelEditing())
            {
                ExceptionUtil.ThrowInternalErrorException("TransactedTool did not allow us to cancel the Editing state");
            }
        }

        internal void ForceCancelDrawingOrEditingAndDirty()
        {
            this.ForceCancelDrawingOrEditing();
            if (this.State == TransactedToolState.Dirty)
            {
                this.CancelChanges();
            }
        }

        protected abstract ReferenceValue GetChanges();
        protected abstract void OnRestoreChanges(ReferenceValue changes);
        public abstract bool RequestCancelDrawing();
        public abstract bool RequestCancelEditing();
        public abstract bool RequestEndDrawing();
        public abstract bool RequestEndEditing();
        public void RestoreChanges(ReferenceValue changes)
        {
            this.OnRestoreChanges(changes);
        }

        public ReferenceValue Changes =>
            this.GetChanges();

        public abstract TransactedToolState State { get; }
    }
}

