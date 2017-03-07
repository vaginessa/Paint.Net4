namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Updates;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class AppWorkspaceUpdatesServiceHost : Disposable, IUpdatesServiceHost, IUpdatesUISite, IUpdatesMainFormSite
    {
        private AppWorkspace appWorkspace;

        [field: CompilerGenerated]
        public event EventHandler HandleCreated;

        event FormClosingEventHandler IUpdatesMainFormSite.FormClosing
        {
            add
            {
                PdnBaseForm form = this.appWorkspace.FindForm() as PdnBaseForm;
                if (form == null)
                {
                    throw new InternalErrorException();
                }
                form.FormClosing += value;
            }
            remove
            {
                PdnBaseForm form = this.appWorkspace.FindForm() as PdnBaseForm;
                if (form == null)
                {
                    throw new InternalErrorException();
                }
                form.FormClosing -= value;
            }
        }

        event EventHandler IUpdatesUISite.HandleCreated
        {
            add
            {
                this.HandleCreated += value;
            }
            remove
            {
                this.HandleCreated -= value;
            }
        }

        public AppWorkspaceUpdatesServiceHost(AppWorkspace appWorkspace)
        {
            Validate.IsNotNull<AppWorkspace>(appWorkspace, "appWorkspace");
            this.appWorkspace = appWorkspace;
            this.appWorkspace.HandleCreated += new EventHandler(this.OnAppWorkspaceHandleCreated);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.appWorkspace != null))
            {
                this.appWorkspace.HandleCreated -= new EventHandler(this.OnAppWorkspaceHandleCreated);
                this.appWorkspace = null;
            }
            base.Dispose(disposing);
        }

        private void OnAppWorkspaceHandleCreated(object sender, EventArgs e)
        {
            this.RaiseHandleCreated();
        }

        void IUpdatesServiceHost.CloseAllWorkspaces(out bool cancelled)
        {
            CloseAllWorkspacesAction performMe = new CloseAllWorkspacesAction();
            this.appWorkspace.PerformAction(performMe);
            cancelled = performMe.Cancelled;
        }

        IUpdatesMainFormSite IUpdatesServiceHost.FindMainForm()
        {
            if (this.appWorkspace.FindForm() == null)
            {
                return null;
            }
            return this;
        }

        private void RaiseHandleCreated()
        {
            this.HandleCreated.Raise(this);
        }

        bool IUpdatesMainFormSite.Enabled
        {
            get
            {
                PdnBaseForm form = this.appWorkspace.FindForm() as PdnBaseForm;
                return form?.Enabled;
            }
        }

        bool IUpdatesMainFormSite.IsCurrentModalForm
        {
            get
            {
                PdnBaseForm form = this.appWorkspace.FindForm() as PdnBaseForm;
                return form?.IsCurrentModalForm;
            }
        }

        ISynchronizeInvoke IUpdatesServiceHost.SyncContext =>
            this.appWorkspace;

        IUpdatesUISite IUpdatesServiceHost.UISite =>
            this;

        Font IUpdatesUISite.Font =>
            this.appWorkspace.Font;

        bool IUpdatesUISite.IsHandleCreated =>
            this.appWorkspace.IsHandleCreated;

        IWin32Window IUpdatesUISite.Win32Window =>
            this.appWorkspace;
    }
}

