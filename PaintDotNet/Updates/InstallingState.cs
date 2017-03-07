namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class InstallingState : UpdatesState
    {
        private Exception exception;
        private bool finishing;
        private bool haveFinished;
        private string installerPath;

        public InstallingState(string installerPath) : base(false, false, MarqueeStyle.None)
        {
            this.installerPath = installerPath;
        }

        public void Finish(IUpdatesServiceHost host)
        {
            if (!this.finishing)
            {
                try
                {
                    if (this.haveFinished)
                    {
                        throw new ApplicationException("already called Finish()");
                    }
                    this.finishing = true;
                    this.haveFinished = true;
                    bool flag = Security.VerifySignedFile(base.StateMachine.UIContext, this.installerPath, false, false);
                    bool cancelled = false;
                    host.CloseAllWorkspaces(out cancelled);
                    if (flag && !cancelled)
                    {
                        AppSettings.Instance.Updates.PackageFileName.Value = this.installerPath;
                        if (string.Compare(Path.GetExtension(this.installerPath), ".exe", true) == 0)
                        {
                            ShellUtil.IsActivityQueuedForRestart = false;
                            Form parent = new Form {
                                BackColor = Color.Red
                            };
                            parent.TransparencyKey = parent.BackColor;
                            parent.ShowInTaskbar = false;
                            parent.FormBorderStyle = FormBorderStyle.None;
                            parent.StartPosition = FormStartPosition.CenterScreen;
                            parent.Show();
                            ShellUtil.Execute(parent, this.installerPath, "/skipConfig /restartPdnOnExit", ExecutePrivilege.RequireAdmin, ExecuteWaitType.ReturnImmediately);
                            parent.Close();
                            Startup.CloseApplication();
                        }
                    }
                    else
                    {
                        bool flag3 = FileSystem.TryDeleteFile(this.installerPath);
                    }
                }
                finally
                {
                    this.finishing = false;
                }
            }
        }

        public override void OnEnteredState()
        {
            try
            {
                this.OnEnteredStateImpl();
            }
            catch (Exception exception)
            {
                this.exception = exception;
                base.StateMachine.QueueInput(PrivateInput.GoToError);
            }
        }

        private void OnEnteredStateImpl()
        {
            string extension = Path.GetExtension(this.installerPath);
            if (string.Compare(".exe", Path.GetExtension(extension), true) != 0)
            {
                ExceptionUtil.ThrowInvalidOperationException("installerPath does not end in .exe: " + this.installerPath);
            }
            string fileName = Path.GetFileName(this.installerPath);
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (!input.Equals(UpdatesAction.Continue))
            {
                throw new ArgumentException();
            }
            newState = new DoneState();
        }
    }
}

