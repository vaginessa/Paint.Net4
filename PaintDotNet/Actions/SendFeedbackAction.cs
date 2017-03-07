namespace PaintDotNet.Actions
{
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Resources;
    using System;
    using System.Diagnostics;

    internal sealed class SendFeedbackAction : AppWorkspaceAction
    {
        private string GetEmailLaunchString(string email, string subject, string body)
        {
            string str = body.Replace("\r\n", "%0D%0A");
            return $"mailto:{email}?subject={subject}&body={str}";
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string email = "feedback4@getpaint.net";
            string subject = string.Format(PdnResources.GetString("SendFeedback.Email.Subject.Format"), PdnInfo.FullAppName);
            string body = PdnResources.GetString("SendFeedback.Email.Body");
            string fileName = this.GetEmailLaunchString(email, subject, body);
            fileName = fileName.Substring(0, Math.Min(0x400, fileName.Length));
            try
            {
                Process.Start(fileName);
            }
            catch (Exception exception)
            {
                ExceptionDialog.ShowErrorDialog(appWorkspace, PdnResources.GetString("SendFeedbackAction.Error"), exception);
            }
        }
    }
}

