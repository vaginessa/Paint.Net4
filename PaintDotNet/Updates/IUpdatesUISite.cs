namespace PaintDotNet.Updates
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal interface IUpdatesUISite
    {
        event EventHandler HandleCreated;

        System.Drawing.Font Font { get; }

        bool IsHandleCreated { get; }

        IWin32Window Win32Window { get; }
    }
}

