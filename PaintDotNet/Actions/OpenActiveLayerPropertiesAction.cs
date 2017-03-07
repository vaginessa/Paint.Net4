namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Windows.Forms;

    internal sealed class OpenActiveLayerPropertiesAction : DocumentWorkspaceAction
    {
        public OpenActiveLayerPropertiesAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            bool dirty = documentWorkspace.Document.Dirty;
            if (!(documentWorkspace.ActiveLayer is BitmapLayer))
            {
                throw new InternalErrorException(new InvalidOperationException("Layer isn't a bitmap layer"));
            }
            using (BitmapLayerPropertiesDialog dialog = new BitmapLayerPropertiesDialog())
            {
                dialog.Layer = documentWorkspace.ActiveLayer;
                if (dialog.ShowDialog(documentWorkspace.AppWorkspace) == DialogResult.Cancel)
                {
                    documentWorkspace.Document.Dirty = dirty;
                }
            }
            return null;
        }
    }
}

