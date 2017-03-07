namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal sealed class CloseAllWorkspacesAction : AppWorkspaceAction
    {
        private bool cancelled = false;

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (!appWorkspace.CanSetActiveWorkspace)
            {
                this.cancelled = true;
                return;
            }
            DocumentWorkspace activeDocumentWorkspace = appWorkspace.ActiveDocumentWorkspace;
            int? nullable = null;
            try
            {
                nullable = new int?(appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency);
                appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency = 0;
            }
            catch (NullReferenceException)
            {
            }
            List<DocumentWorkspace> unsavedDocs = new List<DocumentWorkspace>();
            foreach (DocumentWorkspace workspace2 in appWorkspace.DocumentWorkspaces)
            {
                if ((workspace2.Document != null) && workspace2.Document.Dirty)
                {
                    unsavedDocs.Add(workspace2);
                }
            }
            if (unsavedDocs.Count == 1)
            {
                CloseWorkspaceAction action = new CloseWorkspaceAction(unsavedDocs[0]);
                action.PerformAction(appWorkspace);
                this.cancelled = action.Cancelled;
            }
            else if (unsavedDocs.Count > 1)
            {
                using (UnsavedChangesDialog dialog = new UnsavedChangesDialog())
                {
                    <>c__DisplayClass3_0 class_;
                    dialog.DocumentClicked += new ValueEventHandler<DocumentWorkspace>(class_.<PerformAction>b__0);
                    dialog.Shown += delegate (object s, EventArgs e) {
                        dialog.Documents = unsavedDocs.ToArrayEx<DocumentWorkspace>();
                        if (appWorkspace.ActiveDocumentWorkspace.Document.Dirty)
                        {
                            dialog.SelectedDocument = appWorkspace.ActiveDocumentWorkspace;
                        }
                    };
                    Form form = appWorkspace.FindForm();
                    if ((form != null) && (form.WindowState == FormWindowState.Minimized))
                    {
                        PdnBaseForm form2 = form as PdnBaseForm;
                        if (form2 != null)
                        {
                            form2.RestoreWindow();
                        }
                    }
                    DialogResult result = dialog.ShowDialog(appWorkspace);
                    if (result != DialogResult.Cancel)
                    {
                        if (result != DialogResult.Yes)
                        {
                            if (result != DialogResult.No)
                            {
                                throw ExceptionUtil.InvalidEnumArgumentException<DialogResult>(result, "dr");
                            }
                        }
                        else
                        {
                            foreach (DocumentWorkspace workspace3 in unsavedDocs)
                            {
                                appWorkspace.ActiveDocumentWorkspace = workspace3;
                                if (workspace3.DoSave())
                                {
                                    appWorkspace.RemoveDocumentWorkspace(workspace3);
                                }
                                else
                                {
                                    this.cancelled = true;
                                    break;
                                }
                            }
                            goto Label_0272;
                        }
                        this.cancelled = false;
                    }
                    else
                    {
                        this.cancelled = true;
                    }
                }
            }
        Label_0272:;
            try
            {
                if (nullable.HasValue)
                {
                    appWorkspace.Widgets.DocumentStrip.ThumbnailUpdateLatency = nullable.Value;
                }
            }
            catch (NullReferenceException)
            {
            }
            if (this.cancelled)
            {
                if ((appWorkspace.ActiveDocumentWorkspace != activeDocumentWorkspace) && !activeDocumentWorkspace.IsDisposed)
                {
                    appWorkspace.ActiveDocumentWorkspace = activeDocumentWorkspace;
                }
            }
            else
            {
                UIUtil.SuspendControlPainting(appWorkspace);
                foreach (DocumentWorkspace workspace4 in appWorkspace.DocumentWorkspaces)
                {
                    if ((workspace4.Tool != null) && (workspace4.Tool is TransactedTool))
                    {
                        ((TransactedTool) workspace4.Tool).ForceCancelDrawingOrEditingAndDirty();
                    }
                    appWorkspace.RemoveDocumentWorkspace(workspace4);
                }
                UIUtil.ResumeControlPainting(appWorkspace);
                appWorkspace.Invalidate(true);
            }
        }

        public bool Cancelled =>
            this.cancelled;
    }
}

