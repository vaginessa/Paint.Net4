namespace PaintDotNet.Settings
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class SettingExtensions
    {
        public static ReadOnlySetting<T> AsReadOnly<T>(this Setting<T> setting) => 
            new ReadOnlySetting<T>(setting);
    }
}

