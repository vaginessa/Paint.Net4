namespace PaintDotNet.Updates
{
    using PaintDotNet.Resources;
    using System;
    using System.Runtime.InteropServices;

    internal class ErrorState : UpdatesState
    {
        private string errorMessage;
        private System.Exception exception;

        public ErrorState(System.Exception exception, string errorMessage) : base(true, false, MarqueeStyle.None)
        {
            this.exception = exception;
            this.errorMessage = errorMessage;
        }

        public override void OnEnteredState()
        {
            base.OnEnteredState();
        }

        public override void ProcessInput(object input, out PaintDotNet.Updates.State newState)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public string ErrorMessage =>
            this.errorMessage;

        public System.Exception Exception =>
            this.exception;

        public override string InfoText =>
            string.Format(PdnResources.GetString("UpdatesDialog.InfoText.Text.ErrorState.Format"), this.errorMessage);
    }
}

