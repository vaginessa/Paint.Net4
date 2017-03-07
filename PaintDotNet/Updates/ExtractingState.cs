namespace PaintDotNet.Updates
{
    using PaintDotNet.IO;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ExtractingState : UpdatesState, INewVersionInfo
    {
        private SiphonStream abortMeStream;
        private Exception exception;
        private string extractMe;
        private string installerPath;
        private PdnVersionInfo newVersionInfo;

        public ExtractingState(string extractMe, PdnVersionInfo newVersionInfo) : base(false, false, MarqueeStyle.Smooth)
        {
            this.extractMe = extractMe;
            this.newVersionInfo = newVersionInfo;
        }

        protected override void OnAbort()
        {
            SiphonStream abortMeStream = this.abortMeStream;
            if (abortMeStream != null)
            {
                abortMeStream.Abort(new Exception());
            }
            base.OnAbort();
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

        public void OnEnteredStateImpl()
        {
            FileStream stream = new FileStream(this.extractMe, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream underlyingStream = null;
            try
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    ZipArchiveEntry entry = archive.Entries.FirstOrDefault<ZipArchiveEntry>(ze => string.Compare(".exe", Path.GetExtension(ze.Name), true, CultureInfo.InvariantCulture) == 0);
                    if (entry == null)
                    {
                        this.exception = new FileNotFoundException();
                        base.StateMachine.QueueInput(PrivateInput.GoToError);
                    }
                    else
                    {
                        int maxBytes = (int) entry.Length;
                        int bytesSoFar = 0;
                        this.installerPath = Path.Combine(Path.GetDirectoryName(this.extractMe), entry.Name);
                        underlyingStream = new FileStream(this.installerPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                        SiphonStream output = new SiphonStream(underlyingStream, 0x1000);
                        this.abortMeStream = output;
                        IOEventHandler handler = delegate (object sender, IOEventArgs e) {
                            bytesSoFar += e.Count;
                            double percent = 100.0 * (((double) bytesSoFar) / ((double) maxBytes));
                            this.OnProgress(percent);
                        };
                        base.OnProgress(0.0);
                        if (maxBytes > 0)
                        {
                            output.IOFinished += handler;
                        }
                        using (Stream stream4 = entry.Open())
                        {
                            StreamUtil.CopyStream(stream4, output);
                        }
                        if (maxBytes > 0)
                        {
                            output.IOFinished -= handler;
                        }
                        this.abortMeStream = null;
                        output = null;
                        underlyingStream.Close();
                        underlyingStream = null;
                        base.StateMachine.QueueInput(PrivateInput.GoToReadyToInstall);
                    }
                }
            }
            catch (Exception exception)
            {
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
            finally
            {
                if (underlyingStream != null)
                {
                    underlyingStream.Close();
                    underlyingStream = null;
                }
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
                if (((this.exception != null) || base.AbortRequested) && (this.installerPath != null))
                {
                    bool flag = FileSystem.TryDeleteFile(this.installerPath);
                }
                if (this.extractMe != null)
                {
                    bool flag2 = FileSystem.TryDeleteFile(this.extractMe);
                }
            }
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (input.Equals(PrivateInput.GoToReadyToInstall))
            {
                newState = new ReadyToInstallState(this.installerPath, this.newVersionInfo);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string errorMessage = PdnResources.GetString("Updates.ExtractingState.GenericError");
                newState = new ErrorState(this.exception, errorMessage);
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
            this.newVersionInfo;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ExtractingState.<>c <>9 = new ExtractingState.<>c();
            public static Func<ZipArchiveEntry, bool> <>9__11_0;

            internal bool <OnEnteredStateImpl>b__11_0(ZipArchiveEntry ze) => 
                (string.Compare(".exe", Path.GetExtension(ze.Name), true, CultureInfo.InvariantCulture) == 0);
        }
    }
}

