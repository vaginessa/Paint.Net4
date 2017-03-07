namespace PaintDotNet.Updates
{
    using System;

    internal enum PrivateInput
    {
        GoToReadyToCheck,
        GoToChecking,
        GoToUpdateAvailable,
        GoToDownloading,
        GoToExtracting,
        GoToReadyToInstall,
        GoToInstalling,
        GoToDone,
        GoToError,
        GoToAborted
    }
}

