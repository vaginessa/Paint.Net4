namespace PaintDotNet.Settings
{
    using System;

    internal enum SettingScope
    {
        CurrentUser,
        CurrentUserWithSystemWideOverride,
        SystemWide,
        SystemWideWithCurrentUserOverride
    }
}

