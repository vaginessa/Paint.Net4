namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.IO;
    using System.Windows.Forms;

    internal sealed class OpenFileAction : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (appWorkspace.CanSetActiveWorkspace)
            {
                string directoryName;
                string[] strArray;
                if (appWorkspace.ActiveDocumentWorkspace == null)
                {
                    directoryName = null;
                }
                else
                {
                    string str2;
                    FileType type;
                    SaveConfigToken token;
                    appWorkspace.ActiveDocumentWorkspace.GetDocumentSaveOptions(out str2, out type, out token);
                    directoryName = Path.GetDirectoryName(str2);
                }
                if (DocumentWorkspace.ChooseFiles(appWorkspace, out strArray, true, directoryName) == DialogResult.OK)
                {
                    appWorkspace.OpenFilesInNewWorkspace(strArray);
                }
            }
        }
    }
}

