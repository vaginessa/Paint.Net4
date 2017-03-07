namespace PaintDotNet.Updates
{
    using PaintDotNet.Resources;
    using System;
    using System.Net;

    internal abstract class UpdatesState : PaintDotNet.Updates.State
    {
        private bool continueButtonVisible;
        private PaintDotNet.Updates.MarqueeStyle marqueeStyle;

        public UpdatesState(bool isFinalState, bool continueButtonVisible, PaintDotNet.Updates.MarqueeStyle marqueeStyle) : base(isFinalState)
        {
            this.continueButtonVisible = continueButtonVisible;
            this.marqueeStyle = marqueeStyle;
        }

        protected static string WebExceptionToErrorMessage(WebException wex)
        {
            if (wex.Status == WebExceptionStatus.ProtocolError)
            {
                string format = PdnResources.GetString("WebExceptionStatus.ProtocolError.Format");
                HttpStatusCode statusCode = ((HttpWebResponse) wex.Response).StatusCode;
                return string.Format(format, statusCode.ToString(), (int) statusCode);
            }
            return PdnResources.GetString("WebExceptionStatus." + wex.Status.ToString());
        }

        public string ContinueButtonText =>
            PdnResources.GetString("UpdatesDialog.ContinueButton.Text." + base.GetType().Name);

        public bool ContinueButtonVisible =>
            this.continueButtonVisible;

        public virtual string InfoText =>
            PdnResources.GetString("UpdatesDialog.InfoText.Text." + base.GetType().Name);

        public PaintDotNet.Updates.MarqueeStyle MarqueeStyle =>
            this.marqueeStyle;

        public UpdatesStateMachine StateMachine =>
            ((UpdatesStateMachine) base.StateMachine);
    }
}

