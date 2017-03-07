namespace PaintDotNet.Updates
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal interface IUpdatesServiceHost
    {
        void CloseAllWorkspaces(out bool cancelled);
        IUpdatesMainFormSite FindMainForm();

        ISynchronizeInvoke SyncContext { get; }

        IUpdatesUISite UISite { get; }
    }
}

