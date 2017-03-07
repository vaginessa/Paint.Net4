namespace PaintDotNet.Tools
{
    using System;

    internal enum TransactedToolState
    {
        Inactive,
        Idle,
        Drawing,
        Dirty,
        Editing
    }
}

