namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Controls;
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class ClearMruListAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string question = PdnResources.GetString("ClearOpenRecentList.Dialog.Text");
            if (MessageBoxUtil.AskYesNo(appWorkspace, question) == DialogResult.Yes)
            {
                MostRecentFilesService.Instance.Clear();
                MostRecentFilesService.Instance.SaveMruList();
            }
        }
    }
}

