namespace PaintDotNet.Settings.App
{
    using System;

    [Flags]
    internal enum AppErrorFlags
    {
        None,
        DisabledHardwareAccelerationDueToCreateHwndRenderTargetAccessViolation
    }
}

