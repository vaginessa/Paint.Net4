namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.IO;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class DownloadingState : UpdatesState, INewVersionInfo
    {
        private SiphonStream abortMeStream;
        private PdnVersionInfo downloadMe;
        private Exception exception;
        private string zipTempName;

        public DownloadingState(PdnVersionInfo downloadMe) : base(false, false, MarqueeStyle.Smooth)
        {
            this.downloadMe = downloadMe;
        }

        protected override void OnAbort()
        {
            SiphonStream abortMeStream = this.abortMeStream;
            if (abortMeStream != null)
            {
                abortMeStream.Abort(new Exception());
            }
        }

        public override void OnEnteredState()
        {
            this.zipTempName = Path.GetTempFileName() + ".zip";
            try
            {
                bool flag;
                if (OS.VerifyFrameworkVersion(this.downloadMe.NetFxMajorVersion, this.downloadMe.NetFxMinorVersion, this.downloadMe.NetFxServicePack, true))
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }
                base.OnProgress(0.0);
                FileStream underlyingStream = new FileStream(this.zipTempName, FileMode.Create, FileAccess.Write, FileShare.Read);
                try
                {
                    SiphonStream output = new SiphonStream(underlyingStream);
                    this.abortMeStream = output;
                    ProgressEventHandler progressCallback = (sender, e) => base.OnProgress(e.Percent);
                    WebHelpers.DownloadFile(new Uri(this.downloadMe.ChooseDownloadUrl(flag)), output, progressCallback);
                    output.Flush();
                    this.abortMeStream = null;
                    output = null;
                }
                finally
                {
                    if (underlyingStream != null)
                    {
                        underlyingStream.Close();
                        underlyingStream = null;
                    }
                }
                base.StateMachine.QueueInput(PrivateInput.GoToExtracting);
            }
            catch (Exception exception)
            {
                this.exception = exception;
                if (base.AbortRequested)
                {
                    base.StateMachine.QueueInput(PrivateInput.GoToAborted);
                }
                else
                {
                    this.exception = exception;
                    base.StateMachine.QueueInput(PrivateInput.GoToError);
                }
            }
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (input.Equals(PrivateInput.GoToExtracting))
            {
                newState = new ExtractingState(this.zipTempName, this.downloadMe);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string str;
                if (this.exception is WebException)
                {
                    str = UpdatesState.WebExceptionToErrorMessage((WebException) this.exception);
                }
                else
                {
                    str = PdnResources.GetString("Updates.DownloadingState.GenericError");
                }
                newState = new ErrorState(this.exception, str);
            }
            else
            {
                if (!input.Equals(PrivateInput.GoToAborted))
                {
                    throw new ArgumentException();
                }
                newState = new AbortedState();
            }
        }

        public override bool CanAbort =>
            true;

        public PdnVersionInfo NewVersionInfo =>
            this.downloadMe;
    }
}

