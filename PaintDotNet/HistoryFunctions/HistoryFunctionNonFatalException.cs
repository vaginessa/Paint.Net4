namespace PaintDotNet.HistoryFunctions
{
    using System;

    internal class HistoryFunctionNonFatalException : Exception
    {
        private string localizedErrorText;
        private const string message = "Non-fatal exception encountered";

        public HistoryFunctionNonFatalException()
        {
            this.localizedErrorText = null;
        }

        public HistoryFunctionNonFatalException(string localizedErrorText) : base("Non-fatal exception encountered")
        {
            this.localizedErrorText = localizedErrorText;
        }

        public HistoryFunctionNonFatalException(string localizedErrorText, Exception innerException) : base("Non-fatal exception encountered", innerException)
        {
            this.localizedErrorText = localizedErrorText;
        }

        public string LocalizedErrorText =>
            this.localizedErrorText;
    }
}

