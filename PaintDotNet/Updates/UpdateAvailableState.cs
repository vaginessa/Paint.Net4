namespace PaintDotNet.Updates
{
    using System;
    using System.Runtime.InteropServices;

    internal class UpdateAvailableState : UpdatesState, INewVersionInfo
    {
        private PdnVersionInfo newVersionInfo;

        public UpdateAvailableState(PdnVersionInfo newVersionInfo) : base(false, true, MarqueeStyle.None)
        {
            this.newVersionInfo = newVersionInfo;
        }

        public override void OnEnteredState()
        {
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new DownloadingState(this.newVersionInfo);
            }
            else
            {
                if (!input.Equals(UpdatesAction.Cancel))
                {
                    throw new ArgumentException();
                }
                newState = new DoneState();
            }
        }

        public PdnVersionInfo NewVersionInfo =>
            this.newVersionInfo;
    }
}

