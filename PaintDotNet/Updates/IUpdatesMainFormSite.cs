namespace PaintDotNet.Updates
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal interface IUpdatesMainFormSite
    {
        event FormClosingEventHandler FormClosing;

        bool Enabled { get; }

        bool IsCurrentModalForm { get; }
    }
}

