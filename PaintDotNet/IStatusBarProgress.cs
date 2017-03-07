namespace PaintDotNet
{
    using System;

    internal interface IStatusBarProgress
    {
        void EraseProgressStatusBar();
        void EraseProgressStatusBarAsync();
        double GetProgressStatusBarValue();
        void ResetProgressStatusBar();
        void SetProgressStatusBar(double? percent);
    }
}

